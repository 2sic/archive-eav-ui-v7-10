using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ToSic.Eav.AscxHelpers;
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



        #region


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

            Context.AttSetCommands.EnsureSharedAttributeSets(newApp);

            PurgeGlobalCache(Context.ZoneId, Context.AppId);

            return newApp;
        }

        private void PurgeGlobalCache(int zoneId, int appId)
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
            if (Context.Versioning.MainChangeLogId == 0)
                Context.Versioning.GetChangeLogId(Context.UserName);

            // Delete app using StoredProcedure
            Context.DeleteAppInternal(appId);

            // Remove App from Global Cache
            PurgeGlobalCache(Context.ZoneId, Context.AppId);
        }

        /// <summary>
        /// Get all Apps in the current Zone
        /// </summary>
        /// <returns></returns>
        public List<App> GetApps()
        {
            return Context.Apps.Where(a => a.ZoneID == Context.ZoneId).ToList();
        }


        #endregion
    }
}
