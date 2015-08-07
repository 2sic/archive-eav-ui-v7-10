using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using ToSic.Eav.Persistence;

namespace ToSic.Eav.Import
{
    /// <summary>
    /// Import Schema and Entities to the EAV SqlStore
    /// </summary>
    public class Import
    {
        #region Private Fields
        private readonly EavContext _db;
        private readonly DbShortcuts DbS;
        private readonly int _zoneId;
        private readonly int _appId;
        private readonly bool _leaveExistingValuesUntouched;
        private readonly bool _preserveUndefinedValues;
        private readonly List<LogItem> _importLog = new List<LogItem>();
        #endregion

        #region Properties
        /// <summary>
        /// Get the Import Log
        /// </summary>
        public List<LogItem> ImportLog
        {
            get { return _importLog; }
        }
        #endregion

        /// <summary>
        /// Initializes a new instance of the Import class.
        /// </summary>
        public Import(int? zoneId, int? appId, string userName, bool leaveExistingValuesUntouched = true, bool preserveUndefinedValues = true)
        {
            _db = EavContext.Instance(zoneId, appId);
            DbS = new DbShortcuts(_db);

            _db.UserName = userName;
            _zoneId = _db.ZoneId;
            _appId = _db.AppId;
            _leaveExistingValuesUntouched = leaveExistingValuesUntouched;
            _preserveUndefinedValues = preserveUndefinedValues;
        }

        /// <summary>
        /// Import AttributeSets and Entities
        /// </summary>
        public DbTransaction RunImport(IEnumerable<ImportAttributeSet> newAttributeSets, IEnumerable<ImportEntity> newEntities, bool commitTransaction = true, bool purgeCache = true)
        {
            // 2dm 2015-06-21: this doesn't seem to be used anywhere else in the entire code!
            _db.PurgeCacheOnSave = false;

            // Make sure the connection is open - because on multiple calls it's not clear if it was already opened or not
            if (_db.Connection.State != ConnectionState.Open)
                _db.Connection.Open();

            var transaction = _db.Connection.BeginTransaction();

			// Enhance the SQL timeout for imports
			_db.CommandTimeout = 3600;

            // import AttributeSets if any were included
            if (newAttributeSets != null)
            {
                foreach (var attributeSet in newAttributeSets)
                    ImportAttributeSet(attributeSet);

                _db.ImportEntityRelationshipsQueue();

				_db.EnsureSharedAttributeSets();

                _db.SaveChanges();
            }

            // import Entities
            if (newEntities != null)
            {
                foreach (var entity in newEntities)
                    PersistOneImportEntity(entity);

                _db.ImportEntityRelationshipsQueue();

                _db.SaveChanges();
            }

            // Commit DB Transaction
            if (commitTransaction)
            {
                transaction.Commit();
                _db.Connection.Close();
            }

            // Purge Cache
            if (purgeCache)
                DataSource.GetCache(_db.ZoneId, _db.AppId).PurgeCache(_db.ZoneId, _db.AppId);

            return transaction;
        }

        /// <summary>
        /// Import an AttributeSet with all Attributes and AttributeMetaData
        /// </summary>
        private void ImportAttributeSet(ImportAttributeSet importAttributeSet)
        {
            var destinationSet = DbS.GetAttributeSet(importAttributeSet.StaticName);
            // add new AttributeSet
            if (destinationSet == null)
                destinationSet = _db.AddAttributeSet(importAttributeSet.Name, importAttributeSet.Description, importAttributeSet.StaticName, importAttributeSet.Scope, false);
            else	// use/update existing attribute Set
            {
                if (destinationSet.UsesConfigurationOfAttributeSet.HasValue)
                {
                    _importLog.Add(new LogItem(EventLogEntryType.Error, "Not allowed to import/extend an AttributeSet which uses Configuration of another AttributeSet.") { ImportAttributeSet = importAttributeSet });
                    return;
                }

                _importLog.Add(new LogItem(EventLogEntryType.Information, "AttributeSet already exists") { ImportAttributeSet = importAttributeSet });
            }

	        destinationSet.AlwaysShareConfiguration = importAttributeSet.AlwaysShareConfiguration;
	        if (destinationSet.AlwaysShareConfiguration)
	        {
		        _db.EnsureSharedAttributeSets();
	        }
	        _db.SaveChanges();

            // append all Attributes
            foreach (var importAttribute in importAttributeSet.Attributes)
            {
                Eav.Attribute destinationAttribute;
                var isNewAttribute = false;
                try	// try to add new Attribute
                {
                    var isTitle = importAttribute == importAttributeSet.TitleAttribute;
                    destinationAttribute = _db.AppendAttribute(destinationSet, importAttribute.StaticName, importAttribute.Type, isTitle, false);
                    isNewAttribute = true;
                }
				catch (ArgumentException ex)	// Attribute already exists
                {
					_importLog.Add(new LogItem(EventLogEntryType.Warning, "Attribute already exists") { ImportAttribute = importAttribute, Exception = ex });
                    destinationAttribute = destinationSet.AttributesInSets.Single(a => a.Attribute.StaticName == importAttribute.StaticName).Attribute;
                }

                // Insert AttributeMetaData
                if (isNewAttribute && importAttribute.AttributeMetaData != null)
                {
                    foreach (var entity in importAttribute.AttributeMetaData)
                    {
                        // Validate Entity
                        entity.AssignmentObjectTypeId = DataSource.AssignmentObjectTypeIdFieldProperties;

                        // Set KeyNumber
                        if (destinationAttribute.AttributeID == 0)
                            _db.SaveChanges();
                        entity.KeyNumber = destinationAttribute.AttributeID;

                        PersistOneImportEntity(entity);
                    }
                }
            }
        }

        /// <summary>
        /// Import an Entity with all values
        /// </summary>
        private void PersistOneImportEntity(ImportEntity importEntity)
        {
            #region try to get AttributeSet or otherwise cancel & log error

            // todo: tag:optimize try to cache the attribute-set definition, because this causes DB calls for no reason on each and every entity
            var attributeSet = DbS.GetAttributeSet(importEntity.AttributeSetStaticName);
            if (attributeSet == null)	// AttributeSet not Found
            {
                _importLog.Add(new LogItem(EventLogEntryType.Error, "AttributeSet not found") { ImportEntity = importEntity, ImportAttributeSet = new ImportAttributeSet { StaticName = importEntity.AttributeSetStaticName } });
                return;
            }
            #endregion

            // Update existing Entity
            if (importEntity.EntityGuid.HasValue && DbS.EntityExists(importEntity.EntityGuid.Value))
            {
                #region Do Various Error checking like: Does it really exist, is it not draft, ensure we have the correct Content-Type
                // Get existing, published Entity
                var existingEntities = DbS.GetEntitiesByGuid(importEntity.EntityGuid.Value);
                Entity existingEntity;
                try
                {
                    existingEntity = existingEntities.Count() == 1 ? existingEntities.First() : existingEntities.Single(e => e.IsPublished);
                }
                catch (Exception ex)
                {
                    _importLog.Add(new LogItem(EventLogEntryType.Error, "Unable find existing published Entity. " + ex.Message) { ImportEntity = importEntity, });
                    return;
                }

                // Prevent updating Draft-Entity
                if (!existingEntity.IsPublished)
                {
                    _importLog.Add(new LogItem(EventLogEntryType.Error, "Importing a Draft-Entity is not allowed") { ImportEntity = importEntity, });
                    return;
                }

                // Ensure entity has same AttributeSet
                if (existingEntity.Set.StaticName != importEntity.AttributeSetStaticName)
                {
                    _importLog.Add(new LogItem(EventLogEntryType.Error, "EntityGuid already exists in different AttributeSet") { ImportEntity = importEntity, });
                    return;
                }

                _importLog.Add(new LogItem(EventLogEntryType.Information, "Entity already exists") { ImportEntity = importEntity });
                #endregion

                #region Delete Draft-Entity (if any)
                var draftEntityId = _db.GetDraftEntityId(existingEntity.EntityID);
                if (draftEntityId.HasValue)
                {
                    _importLog.Add(new LogItem(EventLogEntryType.Information, "Draft-Entity deleted") { ImportEntity = importEntity, });
                    _db.DeleteEntity(draftEntityId.Value);
                }
                #endregion

                var newValues = importEntity.Values;
                if (_leaveExistingValuesUntouched)	// Skip values that are already present in existing Entity
                    newValues = newValues.Where(v => existingEntity.Values.All(ev => ev.Attribute.StaticName != v.Key)).ToDictionary(v => v.Key, v => v.Value);

                _db.UpdateEntity(existingEntity.EntityID, newValues, updateLog: _importLog, preserveUndefinedValues: _preserveUndefinedValues, isPublished: importEntity.IsPublished);
            }
            // Add new Entity
            else
            {
                _db.AddEntity(attributeSet.AttributeSetID, importEntity, _importLog, importEntity.IsPublished);
            }
        }
    }
}