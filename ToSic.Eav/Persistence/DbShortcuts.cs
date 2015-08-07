using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ToSic.Eav.Data;


namespace ToSic.Eav.Persistence
{
    public class DbShortcuts
    {
        public EavContext Context { get; set; }

        public DbShortcuts(EavContext context)
        {
            Context = context;
        }

        #region Entity Reading
        
        /// <summary>
        /// Get a single Entity by EntityId
        /// </summary>
        /// <returns>Entity or throws InvalidOperationException</returns>
        public Entity GetEntity(int entityId)
        {
            return Context.Entities.Single(e => e.EntityID == entityId);
        }

        /// <summary>
        /// Get a single Entity by EntityGuid. Ensure it's not deleted and has context's AppId
        /// </summary>
        /// <returns>Entity or throws InvalidOperationException</returns>
        public Entity GetEntity(Guid entityGuid)
        {
            // GetEntity should never return a draft entity that has a published version
            return GetEntitiesByGuid(entityGuid).Single(e => !e.PublishedEntityId.HasValue);
        }

        
        internal IQueryable<Entity> GetEntitiesByGuid(Guid entityGuid)
        {
            return
                Context.Entities.Where(
                    e =>
                        e.EntityGUID == entityGuid && !e.ChangeLogIDDeleted.HasValue &&
                        !e.Set.ChangeLogIDDeleted.HasValue && e.Set.AppID == Context.AppId);// ref:extract _appId);
        }

        /// <summary>
        /// Test whether Entity exists on current App and is not deleted
        /// </summary>
        public bool EntityExists(Guid entityGuid)
        {
            return GetEntitiesByGuid(entityGuid).Any();
        }


        /// <summary>
        /// Get a List of Entities with specified assignmentObjectTypeId and Key.
        /// </summary>
        public IQueryable<Entity> GetEntities(int assignmentObjectTypeId, int keyNumber)
        {
            return GetEntitiesInternal(assignmentObjectTypeId, keyNumber);
        }

        /// <summary>
        /// Get a List of Entities with specified assignmentObjectTypeId and Key.
        /// </summary>
        public IQueryable<Entity> GetEntities(int assignmentObjectTypeId, Guid keyGuid)
        {
            return GetEntitiesInternal(assignmentObjectTypeId, null, keyGuid);
        }

        /// <summary>
        /// Get a List of Entities with specified assignmentObjectTypeId and Key.
        /// </summary>
        public IQueryable<Entity> GetEntities(int assignmentObjectTypeId, string keyString)
        {
            return GetEntitiesInternal(assignmentObjectTypeId, null, null, keyString);
        }

        /// <summary>
        /// Get a List of Entities with specified assignmentObjectTypeId and optional Key.
        /// </summary>
        internal IQueryable<Entity> GetEntitiesInternal(int assignmentObjectTypeId, int? keyNumber = null, Guid? keyGuid = null, string keyString = null)
        {
            return from e in Context.Entities
                   where e.AssignmentObjectTypeID == assignmentObjectTypeId
                   && (keyNumber.HasValue && e.KeyNumber == keyNumber.Value || keyGuid.HasValue && e.KeyGuid == keyGuid.Value || keyString != null && e.KeyString == keyString)
                   && e.ChangeLogIDDeleted == null
                   select e;
        }
        #endregion

        #region Entity Adding, Cloning etc.

        #endregion

        #region AttributeSet
        /// <summary>
        /// Get a List of all AttributeSets
        /// </summary>
        public List<AttributeSet> GetAllAttributeSets()
        {
            return Context.AttributeSets.Where(a => a.AppID == Context.AppId).ToList();
        }

        /// <summary>
        /// Get a single AttributeSet
        /// </summary>
        public AttributeSet GetAttributeSet(int attributeSetId)
        {
            return Context.AttributeSets.SingleOrDefault(a => a.AttributeSetID == attributeSetId && a.AppID == Context.AppId && !a.ChangeLogIDDeleted.HasValue);
        }
        /// <summary>
        /// Get a single AttributeSet
        /// </summary>
        public AttributeSet GetAttributeSet(string staticName)
        {
            return Context.AttributeSets.SingleOrDefault(a => a.StaticName == staticName && a.AppID == Context.AppId && !a.ChangeLogIDDeleted.HasValue);
        }



        /// <summary>
        /// Get AttributeSetId by StaticName and Scope
        /// </summary>
        /// <param name="staticName">StaticName of the AttributeSet</param>
        /// <param name="scope">Optional Filter by Scope</param>
        /// <returns>AttributeSetId or Exception</returns>
        public int GetAttributeSetId(string staticName, AttributeScope? scope)
        {
            var scopeFilter = scope.HasValue ? scope.ToString() : null;

            try
            {
                return Context.AttributeSets.Single(s => s.AppID == Context.AppId /*_appId*/  && s.StaticName == staticName && (s.Scope == scopeFilter || scopeFilter == null)).AttributeSetID;
            }
            catch (InvalidOperationException ex)
            {
                throw new Exception("Unable to get AttributeSet with StaticName \"" + staticName + "\" in Scope \"" + scopeFilter + "\".", ex);
            }
        }

        /// <summary>
        /// if AttributeSet refers another AttributeSet, get ID of the refered AttributeSet. Otherwise returns passed AttributeSetId.
        /// </summary>
        /// <param name="attributeSetId">AttributeSetId to resolve</param>
        private int ResolveAttributeSetId(int attributeSetId)
        {
            var usesConfigurationOfAttributeSet = Context.AttributeSets.Where(a => a.AttributeSetID == attributeSetId).Select(a => a.UsesConfigurationOfAttributeSet).Single();
            return usesConfigurationOfAttributeSet.HasValue ? usesConfigurationOfAttributeSet.Value : attributeSetId;
        }

        /// <summary>
        /// Test whether AttributeSet exists on specified App and is not deleted
        /// </summary>
        public bool AttributeSetExists(string staticName, int appId)
        {
            return Context.AttributeSets.Any(a => !a.ChangeLogIDDeleted.HasValue && a.AppID == appId && a.StaticName == staticName);
        }

        /// <summary>
        /// Get AttributeSets
        /// </summary>
        /// <param name="appId">Filter by AppId</param>
        /// <param name="scope">optional Filter by Scope</param>
        internal IQueryable<AttributeSet> GetAttributeSets(int appId, AttributeScope? scope)
        {
            var result = Context.AttributeSets.Where(a => a.AppID == appId && !a.ChangeLogIDDeleted.HasValue);

            if (scope != null)
            {
                var scopeString = scope.ToString();
                result = result.Where(a => a.Scope == scopeString);
            }

            return result;
        }

        /// <summary>
        /// Get Attributes of an AttributeSet
        /// </summary>
        public IQueryable<Attribute> GetAttributes(int attributeSetId)
        {
            attributeSetId = ResolveAttributeSetId(attributeSetId);

            return from ais in Context.AttributesInSets
                   where ais.AttributeSetID == attributeSetId
                   orderby ais.SortOrder
                   select ais.Attribute;
        }

        /// <summary>
        /// Get a List of AttributeWithMetaInfo of specified AttributeSet and DimensionIds
        /// </summary>
        public List<AttributeWithMetaInfo> GetAttributesWithMetaInfo(int attributeSetId, int[] dimensionIds)
        {
            var attributesInSet = Context.AttributesInSets.Where(a => a.AttributeSetID == attributeSetId).OrderBy(a => a.SortOrder).ToList();

            var systemScope = AttributeScope.System.ToString();

            return (from a in attributesInSet
                    let metaData = new Metadata().GetAttributeMetaData(a.AttributeID, Context.ZoneId, Context.AppId)
                    select new AttributeWithMetaInfo
                    {
                        AttributeID = a.AttributeID,
                        IsTitle = a.IsTitle,
                        StaticName = a.Attribute.StaticName,
                        Name = metaData.ContainsKey("Name") && metaData["Name"].Values != null ? metaData["Name"][dimensionIds].ToString() : null,
                        Notes = metaData.ContainsKey("Notes") && metaData["Notes"].Values != null ? metaData["Notes"][dimensionIds].ToString() : null,
                        Type = a.Attribute.Type,
                        HasTypeMetaData = Context.AttributesInSets.Any(s => s.Set == Context.AttributeSets.FirstOrDefault(se => se.StaticName == "@" + a.Attribute.Type && se.Scope == systemScope) && s.Attribute != null),
                        MetaData = metaData
                    }).ToList();
        }


        /// <summary>
        /// Get a List of all Attributes in specified AttributeSet
        /// </summary>
        /// <param name="attributeSet">Reference to an AttributeSet</param>
        /// <param name="includeTitleAttribute">Specify whether TitleAttribute should be included</param>
        public List<Attribute> GetAttributes(AttributeSet attributeSet, bool includeTitleAttribute = true)
        {
            var items = Context.AttributesInSets.Where(a => a.AttributeSetID == attributeSet.AttributeSetID);
            if (!includeTitleAttribute)
                items = items.Where(a => !a.IsTitle);

            return items.Select(a => a.Attribute).ToList();
        }

        /// <summary>
        /// Get Title Attribute for specified AttributeSetId
        /// </summary>
        public Attribute GetTitleAttribute(int attributeSetId)
        {
            return Context.AttributesInSets.Single(a => a.AttributeSetID == attributeSetId && a.IsTitle).Attribute;
        }

        ///// <summary>
        ///// Get Entities describing the Attribute (e.g. General and @String)
        ///// </summary>
        //public Dictionary<string, IAttribute> GetAttributeMetaData(int attributeId)
        //{
        //    return GetAttributeMetaData(attributeId, Context.ZoneId /*_zoneId*/, Context.AppId /*_appId*/);
        //}
        ///// <summary>
        ///// Get Entities describing the Attribute (e.g. General and @String)
        ///// </summary>
        //public Dictionary<string, IAttribute> GetAttributeMetaData(int attributeId, int zoneId, int appId)
        //{
        //    // Get all EntityIds describing the Attribute (e.g. General and @String)
        //    var entities = DataSource.GetMetaDataSource(zoneId, appId).GetAssignedEntities(DataSource.AssignmentObjectTypeIdFieldProperties, attributeId);
        //    // Return all Attributes of all Entities with Value
        //    return entities.SelectMany(e => e.Attributes).ToDictionary(a => a.Key, a => a.Value);
        //}

        /// <summary>
        /// Get a list of all Attributes in Set for specified AttributeSetId
        /// </summary>
        public List<AttributeInSet> GetAttributesInSet(int attributeSetId)
        {
            return Context.AttributesInSets.Where(a => a.AttributeSetID == attributeSetId).OrderBy(a => a.SortOrder).ToList();
        }

        #endregion



        #region Assignment Object Types
        /// <summary>
        /// AssignmentObjectType with specified Name 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public AssignmentObjectType GetAssignmentObjectType(string name)
        {
            return Context.AssignmentObjectTypes.Single(a => a.Name == name);
        }

        /// <summary>
        /// Get all AssignmentObjectTypes with Id and Name
        /// </summary>
        public Dictionary<int, string> GetAssignmentObjectTypes()
        {
            return (from a in Context.AssignmentObjectTypes
                    select new { a.AssignmentObjectTypeID, a.Name }).ToDictionary(a => a.AssignmentObjectTypeID, a => a.Name);
        }


        #endregion
        
        #region Zones

        /// <summary>
        /// Get all Zones
        /// </summary>
        /// <returns>Dictionary with ZoneId as Key and ZoneModel</returns>
        public Dictionary<int, Data.Zone> GetAllZones()
        {
            //var zones = (from z in Zones
            //             select
            //                 new
            //                 {
            //                     ZoneId = z.ZoneID,
            //                     DefaultAppId = z.Apps.FirstOrDefault(a => a.Name == DefaultAppName).AppID,
            //                     Apps = from a in z.Apps select new { a.AppID, a.Name }
            //                 }).ToDictionary(z => z.ZoneId,
            //                                     z =>
            //                                     new Data.Zone
            //                                     {
            //                                         ZoneId = z.ZoneId,
            //                                         Apps = z.Apps.ToDictionary(a => a.AppID, a => a.Name),
            //                                         DefaultAppId = z.DefaultAppId
            //                                     });
            var zones = Context.Zones.ToDictionary(z => z.ZoneID, z => new Data.Zone(
                        z.ZoneID,
                        z.Apps.FirstOrDefault(a => a.Name == Constants.DefaultAppName).AppID,
                        z.Apps.ToDictionary(a => a.AppID, a => a.Name)));
            return zones;
        }

        /// <summary>
        /// Get all Zones
        /// </summary>
        /// <returns></returns>
        public List<Zone> GetZones()
        {
            return Context.Zones.ToList();
        }

        /// <summary>
        /// Get a single Zone
        /// </summary>
        /// <returns>Zone or null</returns>
        public Zone GetZone(int zoneId)
        {
            return Context.Zones.SingleOrDefault(z => z.ZoneID == zoneId);
        }



        /// <summary>
        /// Creates a new Zone with a default App and Culture-Root-Dimension
        /// </summary>
        public Tuple<Zone, App> AddZone(string name)
        {
            var newZone = new Zone { Name = name };
            Context.AddToZones(newZone);

            new DbDimensions(Context).AddDimension(Constants.CultureSystemKey, "Culture Root", newZone);

            var newApp = AddApp(newZone);

            Context.SaveChanges();

            return Tuple.Create(newZone, newApp);
        }

        /// <summary>
        /// Update a Zone
        /// </summary>
        public void UpdateZone(int zoneId, string name)
        {
            var zone = Context.Zones.Single(z => z.ZoneID == zoneId);
            zone.Name = name;

            Context.SaveChanges();
        }
        #endregion




        #region Apps

        /// <summary>
        /// Add a new App
        /// </summary>
        internal App AddApp(Zone zone, string name = Constants.DefaultAppName)
        {
            var newApp = new App
            {
                Name = name,
                Zone = zone
            };
            Context.AddToApps(newApp);

            Context.SaveChanges();	// required to ensure AppId is created - required in EnsureSharedAttributeSets();

            EnsureSharedAttributeSets(newApp);

            EnsureCacheRefresh(Context.ZoneId, Context.AppId);

            return newApp;
        }

        private void EnsureCacheRefresh(int zoneId, int appId)
        {
            // todo: bad - don't want any data-source in here!
            DataSource.GetCache(zoneId, appId).PurgeGlobalCache();
        }

        /// <summary>
        /// Add a new App to the current Zone
        /// </summary>
        /// <param name="name">The name of the new App</param>
        /// <returns></returns>
        public App AddApp(string name)
        {
            return AddApp(GetZone(Context.ZoneId), name);
        }

        /// <summary>
        /// Delete an existing App with any Values and Attributes
        /// </summary>
        /// <param name="appId">AppId to delete</param>
        public void DeleteApp(int appId)
        {
            // enure changelog exists and is set to SQL CONTEXT_INFO variable
            if (Context._mainChangeLogId == 0)
                Context.GetChangeLogId(Context.UserName);

            // Delete app using StoredProcedure
            Context.DeleteAppInternal(appId);

            // Remove App from Global Cache
            EnsureCacheRefresh(Context.ZoneId, Context.AppId);
        }

        /// <summary>
        /// Get all Apps in the current Zone
        /// </summary>
        /// <returns></returns>
        public List<App> GetApps()
        {
            return Context.Apps.Where(a => a.ZoneID == Context.ZoneId).ToList();
        }

        /// <summary>
        /// Ensure all AttributeSets with AlwaysShareConfiguration=true exist on specified App. App must be saved and have an AppId
        /// </summary>
        private void EnsureSharedAttributeSets(App app, bool autoSave = true)
        {
            if (app.AppID == 0)
                throw new Exception("App must have a valid AppID");

            // todo: bad - don't want data-sources here
            var sharedAttributeSets = GetAttributeSets(DataSource.MetaDataAppId, null).Where(a => a.AlwaysShareConfiguration);
            foreach (var sharedSet in sharedAttributeSets)
            {
                // Skip if attributeSet with StaticName already exists
                if (app.AttributeSets.Any(a => a.StaticName == sharedSet.StaticName && !a.ChangeLogIDDeleted.HasValue))
                    continue;

                // create new AttributeSet
                var newAttributeSet = Context.AddAttributeSet(sharedSet.Name, sharedSet.Description, sharedSet.StaticName, sharedSet.Scope, false, app.AppID);
                newAttributeSet.UsesConfigurationOfAttributeSet = sharedSet.AttributeSetID;
            }

            // Ensure new AttributeSets are created and cache is refreshed
            if (autoSave)
                Context.SaveChanges();
        }

        /// <summary>
        /// Ensure all AttributeSets with AlwaysShareConfiguration=true exist on all Apps an Zones
        /// </summary>
        internal void EnsureSharedAttributeSets()
        {
            foreach (var app in Context.Apps)
                EnsureSharedAttributeSets(app, false);

            Context.SaveChanges();
        }

        #endregion
    }
}
