using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ToSic.Eav.Persistence;

namespace ToSic.Eav.BLL
{
    class EavDataController
    {
        #region Extracted, now externalized objects with actions and private fields

        public DbShortcuts DbS { get; private set; }

        public DbVersioning Versioning { get; private set; }
        public DbEntityCommands EntCommands { get; private set; }
        public DbValueCommands ValCommands { get; private set; }
        public DbAttributeCommands AttrCommands { get; private set; }
        public DbRelationshipCommands RelCommands { get; private set; }
        public DbAttributeSetCommands AttSetCommands { get; private set; }
        public DbPublishing PubCommands { get; private set; }

        public int _appId;
        internal int _zoneId;
        #endregion

        #region Properties like AppId, ZoneId, UserName etc.

        // todo: remove the get/set because the default is already set in initialization...


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

        #endregion
        
        
        #region new stuff

        public EavContext Context { get; private set; }
        private EavDataController(EavContext cntx)
        {
            Context = cntx;
        }

        #endregion

        #region Constructor and Init
        /// <summary>
        /// Returns a new instace of the Eav Context. InitZoneApp must be called afterward.
        /// </summary>
        private static EavDataController Instance()
        {
            var connectionString = Configuration.GetConnectionString();
            var context = new EavContext(connectionString);
            var dataController = new EavDataController(context);
            dataController.DbS = new DbShortcuts(context);
            dataController.Versioning = new DbVersioning(context);
            dataController.EntCommands = new DbEntityCommands(context);
            dataController.ValCommands = new DbValueCommands(context);
            dataController.AttrCommands = new DbAttributeCommands(context);
            dataController.RelCommands = new DbRelationshipCommands(context);
            dataController.AttSetCommands = new DbAttributeSetCommands(context);
            dataController.PubCommands = new DbPublishing(context);
            return dataController;
        }

        /// <summary>
        /// Returns a new instace of the Eav Context on specified ZoneId and/or AppId
        /// </summary>
        public static EavDataController Instance(int? zoneId = null, int? appId = null)
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
            // If nothing is supplied, use defaults
            if (!zoneId.HasValue && !appId.HasValue)
            {
                _zoneId = Constants.DefaultZoneId;
                _appId = Constants.MetaDataAppId;
                return;
            }

            // If only AppId is supplied, look up it's zone and use that
            if (!zoneId.HasValue && appId.HasValue)
            {
                var zoneIdOfApp = Context.Apps.Where(a => a.AppID == appId.Value).Select(a => (int?)a.ZoneID).SingleOrDefault();
                if (!zoneIdOfApp.HasValue)
                    throw new ArgumentException("App with id " + appId.Value + " doesn't exist.", "appId");
                _appId = appId.Value;
                _zoneId = zoneIdOfApp.Value;
                return;
            }

            // if only ZoneId was supplied, use that...
            _zoneId = zoneId.Value;

            // ...and try to find the best match for App-ID
            // var zone = ((DataSources.Caches.BaseCache)DataSource.GetCache(_zoneId, null)).ZoneApps[_zoneId];

            if (appId.HasValue)
            {
                var foundApp = Context.Apps.FirstOrDefault(a => a.ZoneID == _zoneId && a.AppID == appId.Value);
                if (foundApp == null)
                    throw new ArgumentException("App with id " + appId.Value + " doesn't exist.", "appId");
                _appId = appId.Value;
            }
            else
                //_appId = zone.Apps.Where(a => a.Value == Constants.DefaultAppName).Select(a => a.Key).Single();
                _appId = Context.Apps.First(a => a.Name == Constants.DefaultAppName).AppID;

            #region code before refactoring 2015-08-12 - 2dm wanted to get rid of depenedncy on the the DataSource - Cache

            //// If nothing is supplied, use defaults
            //if (!zoneId.HasValue && !appId.HasValue)
            //{
            //    _zoneId = Constants.DefaultZoneId;
            //    _appId = Constants.MetaDataAppId;
            //    return;
            //}

            //// If only AppId is supplied, look up it's zone and use that
            //if (!zoneId.HasValue && appId.HasValue)
            //{
            //    var zoneIdOfApp = Apps.Where(a => a.AppID == appId.Value).Select(a => (int?)a.ZoneID).SingleOrDefault();
            //    if (!zoneIdOfApp.HasValue)
            //        throw new ArgumentException("App with id " + appId.Value + " doesn't exist.", "appId");
            //    _appId = appId.Value;
            //    _zoneId = zoneIdOfApp.Value;
            //    return;
            //}

            //// if only ZoneId was supplied, use that...
            //_zoneId = zoneId.Value;

            //// ...and try to find the best match for App-ID
            //var zone = ((DataSources.Caches.BaseCache)DataSource.GetCache(_zoneId, null)).ZoneApps[_zoneId];

            //if (appId.HasValue)
            //{
            //    // Set AppId and validate AppId exists with specified ZoneId
            //    //var foundAppId = zone.Apps.Where(a => a.Key == appId.Value).Select(a => (int?)a.Key).SingleOrDefault();
            //    //if (!foundAppId.HasValue)
            //    //    throw new ArgumentException("App with id " + appId.Value + " doesn't exist.", "appId");
            //    //_appId = foundAppId.Value;
            //    var foundApp = Apps.FirstOrDefault(a => a.ZoneID == _zoneId && a.AppID == appId.Value);
            //    if (foundApp == null)
            //        throw new ArgumentException("App with id " + appId.Value + " doesn't exist.", "appId");
            //    _appId = foundApp.AppID;
            //}
            //else
            //    _appId = zone.Apps.Where(a => a.Value == Constants.DefaultAppName).Select(a => a.Key).Single();

            #endregion
        }

        #endregion
    }
}
