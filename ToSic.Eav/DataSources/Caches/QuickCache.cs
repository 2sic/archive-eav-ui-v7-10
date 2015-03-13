using System.Collections.Generic;
using ToSic.Eav.Data;

namespace ToSic.Eav.DataSources.Caches
{
	/// <summary>
	/// simple, quick cache
	/// </summary>
	public class QuickCache : BaseCache
	{
		private static Dictionary<int, Data.Zone> _zoneApps;

		public QuickCache()
		{
			Cache = this;
		}

		public override Dictionary<int, Data.Zone> ZoneApps
		{
			get { return _zoneApps; }
			protected set { _zoneApps = value; }
		}

		private static Dictionary<int, string> _assignmentObjectTypes;
		public override Dictionary<int, string> AssignmentObjectTypes
		{
			get { return _assignmentObjectTypes; }
			protected set { _assignmentObjectTypes = value; }
		}

		private const string _cacheKeySchema = "Z{0}A{1}";
		public override string CacheKeySchema { get { return _cacheKeySchema; } }

		private static readonly IDictionary<string, CacheItem> Caches = new Dictionary<string, CacheItem>();

		protected override bool HasCacheItem(string cacheKey)
		{
			return Caches.ContainsKey(cacheKey);
		}

		protected override void SetCacheItem(string cacheKey, CacheItem item)
		{
			Caches[cacheKey] = item;
		}

		protected override CacheItem GetCacheItem(string cacheKey)
		{
			return Caches[cacheKey];
		}

		protected override void RemoveCacheItem(string cacheKey)
		{
			Caches.Remove(cacheKey);	// returns false if key was not found (no Exception)
		}
	}
}