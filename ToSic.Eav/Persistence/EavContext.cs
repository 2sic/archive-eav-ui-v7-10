using System;
using System.Collections.Generic;
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
        public DbPublishing PubCommands { get; private set; }

	    #endregion

		#region Private Fields
		public int _appId;
		internal int _zoneId;
		/// <summary>caches all AttributeSets for each App</summary>
		internal readonly Dictionary<int, Dictionary<int, IContentType>> _contentTypes = new Dictionary<int, Dictionary<int, IContentType>>();
		/// <summary>SaveChanges() assigns all Changes to this ChangeLog</summary>
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
            x.PubCommands = new DbPublishing(x);
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


        #region Wrappers for ASCX Data Sources (temp till replaced with angular)

	    public List<AttributeWithMetaInfo> GetAttributesWithMetaInfo(int attributeSetId, int[] dimensionIds)
	    {
	        return DbS.GetAttributesWithMetaInfo(attributeSetId, dimensionIds);
	    }

        /// <summary>
        /// Get a List of Dimensions having specified SystemKey and current ZoneId and AppId
        /// </summary>
        public List<Dimension> GetDimensionChildren(string systemKey)
        {
            return new DbDimensions(this).GetDimensionChildren(systemKey);
        }

        #endregion



        #region Update Values
        /// <summary>
		/// Update a Value when using IValueImportModel. Returns the Updated Value (for simple Values) or null (for Entity-Values)
		/// </summary>
		internal object UpdateValueByImport(Entity currentEntity, Attribute attribute, List<EavValue> currentValues, IValueImportModel newValue)
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






		#endregion


        #region Save and check if to kill cache

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
			if (Versioning.MainChangeLogId == 0)
                Versioning.GetChangeLogId(UserName);

			var modifiedItems = base.SaveChanges(options);

			if (modifiedItems != 0 && PurgeAppCacheOnSave)
				DataSource.GetCache(ZoneId, AppId).PurgeCache(ZoneId, AppId);

			return modifiedItems;
		}

		#endregion

	}

}