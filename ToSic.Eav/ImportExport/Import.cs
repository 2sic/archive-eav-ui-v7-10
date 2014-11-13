using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;

namespace ToSic.Eav.Import
{
    /// <summary>
    /// Import Schema and Entities to the EAV SqlStore
    /// </summary>
    public class Import
    {
        #region Private Fields
        private readonly EavContext _db;
        private readonly int _zoneId;
        private readonly int _appId;
        private readonly bool _overwriteExistingEntityValues;
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
        public Import(int? zoneId, int? appId, string userName, bool overwriteExistingEntityValues = false, bool preserveUndefinedValues = true)
        {
            _db = EavContext.Instance(zoneId, appId);

            _db.UserName = userName;
            _zoneId = _db.ZoneId;
            _appId = _db.AppId;
            _overwriteExistingEntityValues = overwriteExistingEntityValues;
            _preserveUndefinedValues = preserveUndefinedValues;
        }

        /// <summary>
        /// Import AttributeSets and Entities
        /// </summary>
        public DbTransaction RunImport(IEnumerable<AttributeSet> attributeSets, IEnumerable<Entity> entities, bool commitTransaction = true, bool purgeCache = true)
        {
            _db.PurgeCacheOnSave = false;
            if (_db.Connection.State != ConnectionState.Open)
                _db.Connection.Open();

            var transaction = _db.Connection.BeginTransaction();

            // import AttributeSets
            if (attributeSets != null)
            {
                foreach (var attributeSet in attributeSets)
                    ImportAttributeSet(attributeSet);

                _db.ImportEntityRelationshipsQueue();

                _db.SaveChanges();
            }

            // import Entities
            if (entities != null)
            {
                foreach (var entity in entities)
                    ImportEntity(entity);

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
        private void ImportAttributeSet(AttributeSet attributeSet)
        {
            var destinationSet = _db.GetAttributeSet(attributeSet.StaticName);
            // add new AttributeSet
            if (destinationSet == null)
                destinationSet = _db.AddAttributeSet(attributeSet.Name, attributeSet.Description, attributeSet.StaticName, attributeSet.Scope, false);
            else	// use/update existing attribute Set
            {
                if (destinationSet.UsesConfigurationOfAttributeSet.HasValue)
                {
                    _importLog.Add(new LogItem(EventLogEntryType.Error, "Not allowed to import/extend an AttributeSet which uses Configuration of another AttributeSet.") { AttributeSet = attributeSet });
                    return;
                }

                _importLog.Add(new LogItem(EventLogEntryType.Information, "AttributeSet already exists") { AttributeSet = attributeSet });
            }

            // append all Attributes
            foreach (var importAttribute in attributeSet.Attributes)
            {
                Eav.Attribute destinationAttribute;
                var isNewAttribute = false;
                try	// try to add new Attribute
                {
                    var isTitle = importAttribute == attributeSet.TitleAttribute;
                    destinationAttribute = _db.AppendAttribute(destinationSet, importAttribute.StaticName, importAttribute.Type, isTitle, false);
                    isNewAttribute = true;
                }
                catch (ArgumentException ex)	// Attribute already exists
                {
                    _importLog.Add(new LogItem(EventLogEntryType.Warning, "Attribute already exists") { Attribute = importAttribute, Exception = ex });
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

                        ImportEntity(entity);
                    }
                }
            }
        }

        /// <summary>
        /// Import an Entity with all values
        /// </summary>
        private void ImportEntity(Entity entity)
        {
            // get AttributeSet
            var attributeSet = _db.GetAttributeSet(entity.AttributeSetStaticName);
            if (attributeSet == null)	// AttributeSet not Found
            {
                _importLog.Add(new LogItem(EventLogEntryType.Error, "AttributeSet not found") { Entity = entity, AttributeSet = new AttributeSet { StaticName = entity.AttributeSetStaticName } });
                return;
            }

            // Update existing Entity
            if (entity.EntityGuid.HasValue && _db.EntityExists(entity.EntityGuid.Value))
            {
                // Get existing, published Entity
                var existingEntities = _db.GetEntitiesByGuid(entity.EntityGuid.Value);
                Eav.Entity existingEntity;
                try
                {
                    existingEntity = existingEntities.Count() == 1 ? existingEntities.First() : existingEntities.Single(e => e.IsPublished);
                }
                catch (Exception ex)
                {
                    _importLog.Add(new LogItem(EventLogEntryType.Error, "Unable find existing published Entity. " + ex.Message) { Entity = entity, });
                    return;
                }

                // Prevent updating Draft-Entity
                if (!existingEntity.IsPublished)
                {
                    _importLog.Add(new LogItem(EventLogEntryType.Error, "Importing a Draft-Entity is not allowed") { Entity = entity, });
                    return;
                }

                // Ensure entity has same AttributeSet
                if (existingEntity.Set.StaticName != entity.AttributeSetStaticName)
                {
                    _importLog.Add(new LogItem(EventLogEntryType.Error, "EntityGuid already exists in different AttributeSet") { Entity = entity, });
                    return;
                }

                _importLog.Add(new LogItem(EventLogEntryType.Information, "Entity already exists") { Entity = entity });

                // Delete Draft-Entity (if any)
                var draftEntityId = _db.GetDraftEntityId(existingEntity.EntityID);
                if (draftEntityId.HasValue)
                {
                    _importLog.Add(new LogItem(EventLogEntryType.Information, "Draft-Entity deleted") { Entity = entity, });
                    _db.DeleteEntity(draftEntityId.Value);
                }

                var newValues = entity.Values;
                if (!_overwriteExistingEntityValues)	// Skip values that are already present in existing Entity
                    newValues = entity.Values.Where(v => existingEntity.Values.All(ev => ev.Attribute.StaticName != v.Key)).ToDictionary(v => v.Key, v => v.Value);

                _db.UpdateEntity(existingEntity.EntityID, newValues, updateLog: _importLog, preserveUndefinedValues: _preserveUndefinedValues, isPublished: entity.IsPublished);
            }
            // Add new Entity
            else
            {
                _db.ImportEntity(attributeSet.AttributeSetID, entity, _importLog, entity.IsPublished);
            }
        }
    }
}