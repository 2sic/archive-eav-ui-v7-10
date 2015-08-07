using System.Collections.Generic;
using ToSic.Eav.Data;
using ToSic.Eav.DataSources.Caches;
using ToSic.Eav.Persistence;

namespace ToSic.Eav.DataSources.SqlSources
{
	/// <summary>
	/// A DataSource that uses SQL Server as Backend
	/// </summary>
	public class EavSqlStore : BaseDataSource, RootSources.IRootSource
	{
		private readonly EavContext _context;
	    private readonly DbShortcuts DbS;
		private bool _ready;

        #region App/Zone
        /// <summary>
		/// Gets the ZoneId of this DataSource
		/// </summary>
		public override int ZoneId
		{
			get { return _context.ZoneId; }
		}
		/// <summary>
		/// Gets the AppId of this DataSource
		/// </summary>
		public override int AppId
		{
			get { return _context.AppId; }
		}
        #endregion

        private IDictionary<int, IEntity> GetEntities()
		{
			return _context.GetEntitiesModel(AppId, this);
		}

		/// <summary>
		/// Constructs a new EavSqlStore DataSource
		/// </summary>
		public EavSqlStore()
		{
			Out.Add(DataSource.DefaultStreamName, new DataStream(this, DataSource.DefaultStreamName, GetEntities));
			_context = EavContext.Instance();
            DbS = new DbShortcuts(_context);
		}

		/// <summary>
		/// Set Zone and App for this DataSource
		/// </summary>
		public void InitZoneApp(int zoneId, int appId)
		{
			_context.ZoneId = zoneId;
			_context.AppId = appId;

			_ready = true;
		}

		public override bool Ready { get { return _ready; } }

		public CacheItem GetDataForCache(IDataSource cache)
		{
			return _context.GetDataForCache(null, AppId, cache);
		}

		public Dictionary<int, Data.Zone> GetAllZones()
		{
			return _context.GetAllZones();
		}

		public Dictionary<int, string> GetAssignmentObjectTypes()
		{
			return DbS.GetAssignmentObjectTypes();
		}
	}
}