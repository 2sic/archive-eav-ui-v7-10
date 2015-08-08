using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.Import;
using ToSic.Eav.Persistence;

namespace ToSic.Eav
{
	public partial class EavContext
    {
        #region Extracted, now externalized objects with actions

	    private DbShortcuts _dbs;

	    public DbShortcuts DbS
	    {
	        get
	        {
	            if(_dbs == null)
                    _dbs = new DbShortcuts(this);
	            return _dbs;
	        }
	    }

        public DbVersioning Versioning { get; private set; }
        public DbEntityCommands EntCommands { get; private set; }
        public DbValueCommands ValCommands { get; private set; }
        public DbAttributeCommands AttrCommands { get; private set; }
        public DbRelationshipCommands RelCommands { get; private set; }
        public DbAttributeSetCommands AttSetCommands { get; private set; }

	    #endregion



		#region Private Fields
		public int _appId;
		internal int _zoneId;
		/// <summary>caches all AttributeSets for each App</summary>
		internal readonly Dictionary<int, Dictionary<int, IContentType>> _contentTypes = new Dictionary<int, Dictionary<int, IContentType>>();
		/// <summary>SaveChanges() assigns all Changes to this ChangeLog</summary>
		public int MainChangeLogId;
		#endregion

		#region Properties like AppId, ZoneId, UserName etc.
		/// <summary>
		/// AppId of this whole Context
		/// </summary>
		public int AppId
		{
            get { return _appId == 0 ? Constants.MetaDataAppId : _appId; }
			set { _appId = value; }
		}

		/// <summary>
		/// ZoneId of this whole Context
		/// </summary>
		public int ZoneId
		{
            get { return _zoneId == 0 ? Constants.DefaultZoneId : _zoneId; }
			set { _zoneId = value; }
		}

		/// <summary>
		/// Current UserName. Used for ChangeLog
		/// </summary>
		public string UserName { get; set; }

		/// <summary>
		/// Get or seth whether SaveChanges() should automatically purge cache.
		/// </summary>
		/// <remarks>Usefull if many changes are made in a batch and Cache should be purged after that batch</remarks>
        internal bool PurgeAppCacheOnSavex = true;

	    public bool PurgeAppCacheOnSave
		{
			get { return PurgeAppCacheOnSavex; }
			set { PurgeAppCacheOnSavex = value; }
		}
		#endregion

		#region Constructor and Init
		/// <summary>
		/// Returns a new instace of the Eav Context. InitZoneApp must be called afterward.
		/// </summary>
		private static EavContext Instance()
		{
			var connectionString = Configuration.GetConnectionString();
			var x = new EavContext(connectionString);
            x.Versioning = new DbVersioning(x);
            x.EntCommands = new DbEntityCommands(x);
            x.ValCommands = new DbValueCommands(x);
            x.AttrCommands = new DbAttributeCommands(x);
            x.RelCommands = new DbRelationshipCommands(x);
            x.AttSetCommands = new DbAttributeSetCommands(x);
		    return x;
		}

		/// <summary>
		/// Returns a new instace of the Eav Context on specified ZoneId and/or AppId
		/// </summary>
		public static EavContext Instance(int? zoneId = null, int? appId = null)
		{
			var context = Instance();
			context.InitZoneApp(zoneId, appId);

			return context;
		}

		/// <summary>
		/// Set ZoneId and AppId on current context.
		/// </summary>
		public void InitZoneApp(int? zoneId = null, int? appId = null)
		{
			if (zoneId.HasValue)
				_zoneId = zoneId.Value;
			else
			{
				if (appId.HasValue)
				{
					var zoneIdOfApp = Apps.Where(a => a.AppID == appId.Value).Select(a => (int?)a.ZoneID).SingleOrDefault();
					if (!zoneIdOfApp.HasValue)
						throw new ArgumentException("App with id " + appId.Value + " doesn't exist.", "appId");
					_zoneId = zoneIdOfApp.Value;
				}
				else
				{
                    _zoneId = Constants.DefaultZoneId;
                    _appId = Constants.MetaDataAppId;
					return;
				}
			}

			var zone = ((DataSources.Caches.BaseCache)DataSource.GetCache(_zoneId, null)).ZoneApps[_zoneId];

			if (appId.HasValue)
			{
				// Set AppId and validate AppId exists with specified ZoneId
				var foundAppId = zone.Apps.Where(a => a.Key == appId.Value).Select(a => (int?)a.Key).SingleOrDefault();
				if (!foundAppId.HasValue)
					throw new ArgumentException("App with id " + appId.Value + " doesn't exist.", "appId");
				_appId = foundAppId.Value;
			}
			else
				_appId = zone.Apps.Where(a => a.Value == Constants.DefaultAppName).Select(a => a.Key).Single();

		}

		#endregion


		#region Update


		/// <summary>
		/// Update an Entity when using the Import
		/// </summary>
		internal void UpdateEntityFromImportModel(Entity currentEntity, Dictionary<string, List<IValueImportModel>> newValuesImport, List<LogItem> updateLog, List<Attribute> attributeList, List<EavValue> currentValues, bool preserveUndefinedValues)
		{
			if (updateLog == null)
				throw new ArgumentNullException("updateLog", "When Calling UpdateEntity() with newValues of Type IValueImportModel updateLog must be set.");

			// track updated values to remove values that were not updated automatically
			var updatedValueIds = new List<int>();
			var updatedAttributeIds = new List<int>();
			foreach (var newValue in newValuesImport)
            {
                #region Get Attribute Definition from List (or skip this field if not found)
                var attribute = attributeList.SingleOrDefault(a => a.StaticName == newValue.Key);
				if (attribute == null) // Attribute not found
				{
					// Log Warning for all Values
					updateLog.AddRange(newValue.Value.Select(v => new LogItem(EventLogEntryType.Warning, "Attribute not found for Value")
					{
						ImportAttribute = new Import.ImportAttribute { StaticName = newValue.Key },
						Value = v,
						ImportEntity = v.ParentEntity
					}));
					continue;
                }
                #endregion

                updatedAttributeIds.Add(attribute.AttributeID);

                // Go through each value / dimensions combination
				foreach (var newSingleValue in newValue.Value)
				{
					try
					{
						var updatedValue = UpdateValueByImport(currentEntity, attribute, currentValues, newSingleValue);

						var updatedEavValue = updatedValue as EavValue;
						if (updatedEavValue != null)
							updatedValueIds.Add(updatedEavValue.ValueID);
					}
					catch (Exception ex)
					{
						updateLog.Add(new LogItem(EventLogEntryType.Error, "Update Entity-Value failed")
						{
							ImportAttribute = new ImportAttribute { StaticName = newValue.Key },
							Value = newSingleValue,
							ImportEntity = newSingleValue.ParentEntity,
							Exception = ex
						});
					}
				}
			}

			// remove all existing values that were not updated
            // Logic should be:
            // Of all values - skip the ones we just modified and those which are deleted
		    var valuesToDeleteNew = currentEntity.Values.Where(
		        v => !updatedValueIds.Contains(v.ValueID) && v.ChangeLogIDDeleted == null);

            // Clean up - sometimes the default language doesn't clean properly - so even if it's good now...
            // ...there is old data which sometimes still is duplicate and causes issues, so this clean-up is important
            // So goal: every same-attribute-ID as the updated, with the same non-language-settings, is a left-over
		    var reallyDelete = valuesToDeleteNew.Where(e => updatedAttributeIds.Contains(e.AttributeID));

		    if (preserveUndefinedValues)
		    {
		        var valuesToKeep = valuesToDeleteNew.Where(v => updatedAttributeIds.Contains(v.AttributeID));
                if(valuesToKeep.Count() > updatedAttributeIds.Count) // in this case something is bad
                    throw new Exception("have too many to keep, don't know what to do, abort...");
		        valuesToDeleteNew = valuesToDeleteNew.Where(v => !updatedAttributeIds.Contains(v.AttributeID));
		    }

		    // && (preserveUndefinedValues == false || updatedAttributeIds.Contains(v.AttributeID))).ToList();
		    var valuesToDelete = valuesToDeleteNew.ToList();
            //var valuesToDelete = currentEntity.Values.Where(
            //    v => !updatedValueIds.Contains(v.ValueID) && v.ChangeLogIDDeleted == null 
            //        && (preserveUndefinedValues == false || updatedAttributeIds.Contains(v.AttributeID))).ToList();
            valuesToDelete.ForEach(v => v.ChangeLogIDDeleted = Versioning.GetChangeLogId());
		}

		/// <summary>
		/// Publish a Draft Entity
		/// </summary>
		/// <param name="entityId">ID of the Draft-Entity</param>
		public Entity PublishEntity(int entityId)
		{
			return PublishEntity(entityId, true);
		}

		/// <summary>
		/// Publish a Draft-Entity
		/// </summary>
		/// <param name="entityId">ID of the Draft-Entity</param>
		/// <param name="autoSave">Call SaveChanges() automatically?</param>
		/// <returns>The published Entity</returns>
		internal Entity PublishEntity(int entityId, bool autoSave = true)
		{
			var unpublishedEntity = DbS.GetEntity(entityId);
			if (unpublishedEntity.IsPublished)
				throw new InvalidOperationException(string.Format("EntityId {0} is already published", entityId));

			Entity publishedEntity;

			// Publish Draft-Entity
			if (!unpublishedEntity.PublishedEntityId.HasValue)
			{
				unpublishedEntity.IsPublished = true;
				publishedEntity = unpublishedEntity;
			}
			// Replace currently published Entity with draft Entity and delete the draft
			else
			{
				publishedEntity = DbS.GetEntity(unpublishedEntity.PublishedEntityId.Value);
				ValCommands.CloneEntityValues(unpublishedEntity, publishedEntity);

				// delete the Draft Entity
				EntCommands.DeleteEntity(unpublishedEntity, false);
			}

			if (autoSave)
				SaveChanges();

			return publishedEntity;
		}

		#region Update Values
		/// <summary>
		/// Update a Value when using IValueImportModel. Returns the Updated Value (for simple Values) or null (for Entity-Values)
		/// </summary>
		private object UpdateValueByImport(Entity currentEntity, Attribute attribute, List<EavValue> currentValues, IValueImportModel newValue)
		{
			switch (attribute.Type)
			{
				// Handle Entity Relationships - they're stored in own tables
				case "Entity":
					if (newValue is ValueImportModel<List<Guid>>)
						RelCommands.UpdateEntityRelationships(attribute.AttributeID, ((ValueImportModel<List<Guid>>)newValue).Value.Select(p => (Guid?)p).ToList(), currentEntity.EntityGUID);
					if (newValue is ValueImportModel<List<Guid?>>)
						RelCommands.UpdateEntityRelationships(attribute.AttributeID, ((ValueImportModel<List<Guid?>>)newValue).Value, currentEntity.EntityGUID);
					else
						throw new NotSupportedException("UpdateValue() for Attribute " + attribute.StaticName + " with newValue of type" + newValue.GetType() + " not supported. Expected List<Guid>");

					return null;
				// Handle simple values in Values-Table
				default:
					// masterRecord can be true or false, it's not used when valueDimensions is specified
					return UpdateSimpleValue(attribute, currentEntity, null, true, GetTypedValue(newValue, attribute.Type, attribute.StaticName), null, false, currentValues, null, newValue.ValueDimensions);
			}
		}

		/// <summary>
		/// Get typed value from ValueImportModel
		/// </summary>
		/// <param name="value">Value to convert</param>
		/// <param name="attributeType">Attribute Type</param>
		/// <param name="attributeStaticName">Attribute StaticName</param>
		/// <param name="multiValuesSeparator">Indicates whehter returned value should be convertable to a human readable string - currently only used for GetEntityVersionValues()</param>
		internal object GetTypedValue(IValueImportModel value, string attributeType = null, string attributeStaticName = null, string multiValuesSeparator = null)
		{
			object typedValue;
			if (value is ValueImportModel<bool?> && (attributeType == null || attributeType == "Boolean"))
				typedValue = ((ValueImportModel<bool?>)value).Value;
			else if (value is ValueImportModel<DateTime?> && (attributeType == null || attributeType == "DateTime"))
				typedValue = ((ValueImportModel<DateTime?>)value).Value;
			else if (value is ValueImportModel<decimal?> && (attributeType == null || attributeType == "Number"))
				typedValue = ((ValueImportModel<decimal?>)value).Value;
			else if (value is ValueImportModel<string> && (attributeType == null || attributeType == "String" || attributeType == "Hyperlink"))
				typedValue = ((ValueImportModel<string>)value).Value;
			else if (value is ValueImportModel<List<Guid>> && multiValuesSeparator != null)
			{
				var entityGuids = ((ValueImportModel<List<Guid>>)value).Value;
				typedValue = EntityGuidsToString(entityGuids, multiValuesSeparator);
			}
			else
				throw new NotSupportedException(string.Format("GetTypedValue() for Attribute {0} (Type: {1}) with newValue of type {2} not supported.", attributeStaticName, attributeType, value.GetType()));
			return typedValue;
		}

		private string EntityGuidsToString(IEnumerable<Guid> entityGuids, string separator = ", ", string format = "{0} (EntityId: {1})")
		{
			var guidIds = entityGuids.ToDictionary(k => k, v => (int?)null);
			foreach (var entityGuid in guidIds.ToList())
			{
				var firstEntityId = DbS.GetEntitiesByGuid(entityGuid.Key).Select(e => (int?)e.EntityID).FirstOrDefault();
				if (firstEntityId != null)
					guidIds[entityGuid.Key] = firstEntityId;
			}
			return string.Join(separator, guidIds.Select(e => string.Format(format, e.Key, e.Value)));
		}

		/// <summary>
		/// Update a Value when using ValueViewModel
		/// </summary>
		internal void UpdateValue(Entity currentEntity, Attribute attribute, bool masterRecord, List<EavValue> currentValues, IEntity entityModel, ValueViewModel newValue, ICollection<int> dimensionIds)
		{
			switch (attribute.Type)
			{
				// Handle Entity Relationships - they're stored in own tables
				case "Entity":
					var entityIds = newValue.Value is int?[] ? (int?[])newValue.Value : ((int[])newValue.Value).Select(v => (int?)v).ToArray();
					RelCommands.UpdateEntityRelationships(attribute.AttributeID, entityIds, currentEntity);
					break;
				// Handle simple values in Values-Table
				default:
					UpdateSimpleValue(attribute, currentEntity, dimensionIds, masterRecord, newValue.Value, newValue.ValueId, newValue.ReadOnly, currentValues, entityModel);
					break;
			}
		}

		/// <summary>
		/// Update a Value in the Values-Table
		/// </summary>
		private EavValue UpdateSimpleValue(Attribute attribute, Entity entity, ICollection<int> dimensionIds, bool masterRecord, object newValue, int? valueId, bool readOnly, List<EavValue> currentValues, IEntity entityModel, IEnumerable<Import.ValueDimension> valueDimensions = null)
		{
			var newValueSerialized = HelpersToRefactor.SerializeValue(newValue);
            var changeId = Versioning.GetChangeLogId();

			// Get Value or create new one
			var value = GetOrCreateValue(attribute, entity, masterRecord, valueId, readOnly, currentValues, entityModel, newValueSerialized, changeId, valueDimensions);

			#region Update DimensionIds on this and other values

			// Update Dimensions as specified by Import
			if (valueDimensions != null)
			{
				var valueDimensionsToDelete = value.ValuesDimensions.ToList();
				// loop all specified Dimensions, add or update it for this value
				foreach (var valueDimension in valueDimensions)
				{
					// ToDo: 2bg Log Error but continue
					var dimensionId = new DbDimensions(this).GetDimensionId(null, valueDimension.DimensionExternalKey);
					if (dimensionId == 0)
						throw new Exception("Dimension " + valueDimension.DimensionExternalKey + " not found. EntityId: " + entity.EntityID + " Attribute-StaticName: " + attribute.StaticName);

					var existingValueDimension = value.ValuesDimensions.SingleOrDefault(v => v.DimensionID == dimensionId);
					if (existingValueDimension == null)
						value.ValuesDimensions.Add(new ValueDimension { DimensionID = dimensionId, ReadOnly = valueDimension.ReadOnly });
					else
					{
						valueDimensionsToDelete.Remove(valueDimensionsToDelete.Single(vd => vd.DimensionID == dimensionId));
						existingValueDimension.ReadOnly = valueDimension.ReadOnly;
					}
				}

				// remove old dimensions
				valueDimensionsToDelete.ForEach(DeleteObject);
			}
			// Update Dimensions as specified on the whole Entity
			else if (dimensionIds != null)
			{
				#region Ensure specified Dimensions are updated/added (whether Value has changed or not)
				// Update existing ValuesDimensions
				foreach (var valueDimension in value.ValuesDimensions.Where(vd => dimensionIds.Contains(vd.DimensionID)))
				{
					// ReSharper disable RedundantCheckBeforeAssignment
					// Check to prevent unneeded DB queries
					if (valueDimension.ReadOnly != readOnly)
						// ReSharper restore RedundantCheckBeforeAssignment
						valueDimension.ReadOnly = readOnly;
				}

				// Add new ValuesDimensions
				foreach (var dimensionId in dimensionIds.Where(i => value.ValuesDimensions.All(d => d.DimensionID != i)))
					value.ValuesDimensions.Add(new ValueDimension { DimensionID = dimensionId, ReadOnly = readOnly });

				#endregion

				// Remove current Dimension(s) from other Values
				if (!masterRecord)
				{
					// Get other Values for current Attribute having all Current Dimensions assigned
					var otherValuesWithCurrentDimensions = currentValues.Where(v => v.AttributeID == attribute.AttributeID && v.ValueID != value.ValueID && dimensionIds.All(d => v.ValuesDimensions.Select(vd => vd.DimensionID).Contains(d)));
					foreach (var otherValue in otherValuesWithCurrentDimensions)
					{
						foreach (var valueDimension in otherValue.ValuesDimensions.Where(vd => dimensionIds.Contains(vd.DimensionID)).ToList())
						{
							// if only one Dimension assigned, mark this value as deleted
							if (otherValue.ValuesDimensions.Count == 1)
								otherValue.ChangeLogIDDeleted = changeId;

							otherValue.ValuesDimensions.Remove(valueDimension);
						}
					}
				}
			}
			#endregion

			return value;
		}

		/// <summary>
		/// Get an EavValue for specified EntityId etc. or create a new one. Uses different mechanism when running an Import or ValueId is specified.
		/// </summary>
		private EavValue GetOrCreateValue(Attribute attribute, Entity entity, bool masterRecord, int? valueId, bool readOnly, List<EavValue> currentValues, IEntity entityModel, string newValueSerialized, int changeId, IEnumerable<Import.ValueDimension> valueDimensions)
		{
			EavValue value = null;
			// if Import-Dimension(s) are Specified
			if (valueDimensions != null)
			{
				// Get first value having first Dimension or add new value
				value = currentValues.FirstOrDefault(v => v.ChangeLogIDDeleted == null && v.Attribute.StaticName == attribute.StaticName && v.ValuesDimensions.Any(d => d.Dimension.ExternalKey.Equals(valueDimensions.First().DimensionExternalKey, StringComparison.InvariantCultureIgnoreCase))) ??
						ValCommands.AddValue(entity, attribute.AttributeID, newValueSerialized, autoSave: false);
			}
			// if ValueID & EntityId is specified, use this Value
			else if (valueId.HasValue && entity.EntityID != 0)
			{
				value = currentValues.Single(v => v.ValueID == valueId.Value && v.Attribute.StaticName == attribute.StaticName);
				// If Master, ensure ValueID is from Master!
				var attributeModel = (IAttributeManagement)entityModel.Attributes.SingleOrDefault(a => a.Key == attribute.StaticName).Value;
				if (masterRecord && value.ValueID != attributeModel.DefaultValue.ValueId)
					throw new Exception("Master Record cannot use a ValueID rather ValueID from Master. Attribute-StaticName: " + attribute.StaticName);
			}
			// Find Value (if not specified) or create new one
			else
			{
				if (masterRecord) // if true, don't create new Value (except no one exists)
					value = currentValues.Where(v => v.AttributeID == attribute.AttributeID).OrderBy(a => a.ChangeLogIDCreated).FirstOrDefault();

				// if no Value found, create new one
				if (value == null)
				{
					if (!masterRecord && currentValues.All(v => v.AttributeID != attribute.AttributeID))
						// if updating Additional-Entity but Default-Entity doesn't have any atom
						throw new Exception("Update of a \"" + attribute.StaticName +
											"\" is not allowed. You must first updated this Value for the Default-Entity.");

					value = ValCommands.AddValue(entity, attribute.AttributeID, newValueSerialized, autoSave: false);
				}
			}

			// Update old/existing Value
			if (value.ValueID != 0 || entity.EntityID == 0)
			{
				if (!readOnly)
					ValCommands.UpdateValue(value, newValueSerialized, changeId, false);
			}
			return value;
		}





        ///// <summary>
        ///// Update Relationships of an Entity
        ///// </summary>
        //private void UpdateEntityRelationships(int attributeId, IEnumerable<int?> newValue, Entity currentEntity)
        //{
        //    // remove existing Relationships that are not in new list
        //    var newEntityIds = newValue.ToList();
        //    var existingRelationships = currentEntity.EntityParentRelationships.Where(e => e.AttributeID == attributeId).ToList();

        //    // Delete all existing relationships
        //    foreach (var relationToDelete in existingRelationships)
        //        EntityRelationships.DeleteObject(relationToDelete);

        //    // Create new relationships
        //    for (int i = 0; i < newEntityIds.Count; i++)
        //    {
        //        var newEntityId = newEntityIds[i];
        //        currentEntity.EntityParentRelationships.Add(new EntityRelationship { AttributeID = attributeId, ChildEntityID = newEntityId, SortOrder = i });
        //    }
        //}

        ///// <summary>
        ///// Update Relationships of an Entity. Update isn't done until ImportEntityRelationshipsQueue() is called!
        ///// </summary>
        //private void UpdateEntityRelationships(int attributeId, List<Guid?> newValue, Guid entityGuid)
        //{
        //    _entityRelationshipsQueue.Add(new EntityRelationshipQueueItem { AttributeId = attributeId, ChildEntityGuids = newValue, ParentEntityGuid = entityGuid });
        //}

        ///// <summary>
        ///// Import Entity Relationships Queue (Populated by UpdateEntityRelationships) and Clear Queue afterward.
        ///// </summary>
        //internal void ImportEntityRelationshipsQueue()
        //{
        //    foreach (var relationship in _entityRelationshipsQueue)
        //    {
        //        var entity = DbS.GetEntity(relationship.ParentEntityGuid);
        //        var childEntityIds = new List<int?>();
        //        foreach (var childGuid in relationship.ChildEntityGuids)
        //        {
        //            try
        //            {
        //                childEntityIds.Add(childGuid.HasValue ? DbS.GetEntity(childGuid.Value).EntityID : new int?());
        //            }
        //            catch (InvalidOperationException) { }	// may occur if the child entity wasn't created successfully
        //        }

        //        UpdateEntityRelationships(relationship.AttributeId, childEntityIds, entity);
        //    }

        //    _entityRelationshipsQueue.Clear();
        //}

		#endregion

		/// <summary>
		/// Persists all updates to the data source and optionally resets change tracking in the object context.
		/// Also Creates an initial ChangeLog (used by SQL Server for Auditing).
		/// If items were modified, Cache is purged on current Zone/App
		/// </summary>
		public override int SaveChanges(System.Data.Objects.SaveOptions options)
		{
			if (_appId == 0)
				throw new Exception("SaveChanges with AppId 0 not allowed.");

			// enure changelog exists and is set to SQL CONTEXT_INFO variable
			if (MainChangeLogId == 0)
                Versioning.GetChangeLogId(UserName);

			var modifiedItems = base.SaveChanges(options);

			if (modifiedItems != 0 && PurgeAppCacheOnSave)
				DataSource.GetCache(ZoneId, AppId).PurgeCache(ZoneId, AppId);

			return modifiedItems;
		}


		/// <summary>
		/// Update AdditionalProperties of a Field
		/// </summary>
		public Entity UpdateFieldAdditionalProperties(int attributeId, bool isAllProperty, IDictionary fieldProperties)
		{
			var fieldPropertyEntity = Entities.FirstOrDefault(e => e.AssignmentObjectTypeID == Constants.AssignmentObjectTypeIdFieldProperties && e.KeyNumber == attributeId);
			if (fieldPropertyEntity != null)
				return EntCommands.UpdateEntity(fieldPropertyEntity.EntityID, fieldProperties);

			var metaDataSetName = isAllProperty ? "@All" : "@" + Attributes.Single(a => a.AttributeID == attributeId).Type;
			var systemScope = AttributeScope.System.ToString();
			var attributeSetId = AttributeSets.First(s => s.StaticName == metaDataSetName && s.Scope == systemScope && s.AppID == _appId).AttributeSetID;

			return EntCommands.AddEntity(attributeSetId, fieldProperties, null, attributeId, Constants.AssignmentObjectTypeIdFieldProperties);
		}

		#endregion

		#region Delete

		/// <summary>
		/// Remove an Attribute from an AttributeSet and delete values
		/// </summary>
		public void RemoveAttributeInSet(int attributeId, int attributeSetId)
		{
			// Delete the AttributeInSet
			DeleteObject(AttributesInSets.Single(a => a.AttributeID == attributeId && a.AttributeSetID == attributeSetId));

			// Delete all Values an their ValueDimensions
			var valuesToDelete = Values.Where(v => v.AttributeID == attributeId && v.Entity.AttributeSetID == attributeSetId).ToList();
			foreach (var valueToDelete in valuesToDelete)
			{
				valueToDelete.ValuesDimensions.ToList().ForEach(DeleteObject);
				DeleteObject(valueToDelete);
			}

			// Delete all Entity-Relationships
			var relationshipsToDelete = EntityRelationships.Where(r => r.AttributeID == attributeId).ToList(); // No Filter by AttributeSetID is needed here at the moment because attribute can't be in multiple sets currently
			relationshipsToDelete.ForEach(DeleteObject);

			SaveChanges();
		}

		/// <summary>
		/// Test whehter Entity can be deleted safe if it has no relationships
		/// </summary>
		/// <returns>Item1: Indicates whether Entity can be deleted. Item2: Messages why Entity can't be deleted safe.</returns>
		public Tuple<bool, string> CanDeleteEntity(int entityId)
		{
			var messages = new List<string>();
            var entityModel = new DbLoadAsEav(this).GetEavEntity(entityId);

			if (!entityModel.IsPublished && entityModel.GetPublished() == null)	// allow Deleting Draft-Only Entity always
				return new Tuple<bool, string>(true, null);

			var entityChild = EntityRelationships.Where(r => r.ChildEntityID == entityId).Select(r => r.ParentEntityID).ToList();
			if (entityChild.Any())
				messages.Add(string.Format("Entity has {0} Child-Relationships to Entities: {1}.", entityChild.Count, string.Join(", ", entityChild)));

			var assignedEntitiesFieldProperties = DbS.GetEntitiesInternal(Constants.AssignmentObjectTypeIdFieldProperties, entityId).Select(e => e.EntityID).ToList();
			if (assignedEntitiesFieldProperties.Any())
				messages.Add(string.Format("Entity has {0} assigned Field-Property-Entities: {1}.", assignedEntitiesFieldProperties.Count, string.Join(", ", assignedEntitiesFieldProperties)));

			var assignedEntitiesDataPipeline = DbS.GetEntitiesInternal(Constants.AssignmentObjectTypeEntity, entityId).Select(e => e.EntityID).ToList();
			if (assignedEntitiesDataPipeline.Any())
				messages.Add(string.Format("Entity has {0} assigned Data-Pipeline Entities: {1}.", assignedEntitiesDataPipeline.Count, string.Join(", ", assignedEntitiesDataPipeline)));

			return Tuple.Create(!messages.Any(), string.Join(" ", messages));
		}

        ///// <summary>
        ///// Delete an Entity
        ///// </summary>
        //public bool DeleteEntity(int repositoryId)
        //{
        //    return DeleteEntity(DbS.GetEntity(repositoryId));
        //}

        ///// <summary>
        ///// Delete an Entity
        ///// </summary>
        //public bool DeleteEntity(Guid entityGuid)
        //{
        //    return DeleteEntity(DbS.GetEntity(entityGuid));
        //}

        ///// <summary>
        ///// Delete an Entity
        ///// </summary>
        //private bool DeleteEntity(Entity entity, bool autoSave = true)
        //{
        //    if (entity == null)
        //        return false;

        //    #region Delete Related Records (Values, Value-Dimensions, Relationships)
        //    // Delete all Value-Dimensions
        //    var valueDimensions = entity.Values.SelectMany(v => v.ValuesDimensions).ToList();
        //    valueDimensions.ForEach(DeleteObject);
        //    // Delete all Values
        //    entity.Values.ToList().ForEach(DeleteObject);
        //    // Delete all Parent-Relationships
        //    entity.EntityParentRelationships.ToList().ForEach(DeleteObject);
        //    #endregion

        //    // If entity was Published, set Deleted-Flag
        //    if (entity.IsPublished)
        //    {
        //        entity.ChangeLogIDDeleted = Versioning.GetChangeLogId();
        //        // Also delete the Draft (if any)
        //        var draftEntityId = EntCommands.GetDraftEntityId(entity.EntityID);
        //        if (draftEntityId.HasValue)
        //            DeleteEntity(draftEntityId.Value);
        //    }
        //    // If entity was a Draft, really delete that Entity
        //    else
        //    {
        //        // Delete all Child-Relationships
        //        entity.EntityChildRelationships.ToList().ForEach(DeleteObject);
        //        DeleteObject(entity);
        //    }

        //    if (autoSave)
        //        SaveChanges();

        //    return true;
        //}

		#endregion



        //#region Internal Helper Classes
        //private class EntityRelationshipQueueItem
        //{
        //    public int AttributeId { get; set; }
        //    public Guid ParentEntityGuid { get; set; }
        //    public List<Guid?> ChildEntityGuids { get; set; }
        //}
        //#endregion

		#region Versioning






		#endregion
	}

}