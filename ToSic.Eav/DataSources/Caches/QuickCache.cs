using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using ToSic.Eav.Data;

// new 2015-06-14 for caching
using System.Runtime.Caching;

namespace ToSic.Eav.DataSources.Caches
{
	/// <summary>
	/// simple, quick cache
	/// </summary>
	public class QuickCache : BaseCache, IListCache
	{
		private static Dictionary<int, Data.Zone> _zoneApps;

		public QuickCache()
		{
			Cache = this;
		    ListDefaultRetentionTimeInSeconds = 60*60;
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


        #region The cache-variable + HasCacheItem, SetCacheItem, Get, Remove
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
        #endregion

        #region BETA Additional Stream Caching

	    private ObjectCache ListCache
	    {
	        get { return MemoryCache.Default; }
	    }

        public bool ListHas(string key)
        {
            return ListCache.Contains(key);
        }

	    public bool HasList(IDataSource dataSource)
	    {
	        return ListCache.Contains(dataSource.CacheFullKey);
	    }

	    public bool ListHas(IDataStream dataStream, bool useStreamName = true)
	    {
	        return ListHas(dataStream.Source.CacheFullKey + (useStreamName ? "|" + dataStream.Name : ""));
	    }

	    public int ListDefaultRetentionTimeInSeconds { get; set; }

	    /// <summary>
        /// Get a DataStream in the cache - will be null if not found
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public ListCacheItem ListGet(string key)
	    {
            var ds = ListCache[key] as ListCacheItem;
	        return ds;
	    }

	    public ListCacheItem GetList(IDataSource dataSource)
	    {
	        return ListGet(dataSource.CacheFullKey);
	    }

	    public ListCacheItem ListGet(IDataStream dataStream, bool useStreamName = true)
	    {
	        return ListGet(dataStream.Source.CacheFullKey + (useStreamName ? "|" + dataStream.Name : ""));
	    }


	    /// <summary>
	    /// Insert a data-stream to the cache - if it can be found
	    /// </summary>
	    /// <param name="key"></param>
	    /// <param name="list"></param>
	    /// <param name="durationInSeconds"></param>
	    public void ListSet(string key, IEnumerable<IEntity> list, DateTime sourceRefresh, int durationInSeconds = 0)
	    {
	        var policy = new CacheItemPolicy();
	        policy.SlidingExpiration = new TimeSpan(0, 0, durationInSeconds > 0 ? durationInSeconds : ListDefaultRetentionTimeInSeconds); 

	        var cache = MemoryCache.Default;
            cache.Set(key, new ListCacheItem(key, list, sourceRefresh), policy);
	    }

        public void ListSet(IDataStream dataStream, bool useStreamName = true, int durationInSeconds = 0)
        {
            ListSet(dataStream.Source.CacheFullKey + (useStreamName ? "|" + dataStream.Name : ""), dataStream.LightList, dataStream.Source.CacheLastRefresh, durationInSeconds);
        }

        public void ListRemove(string key)
        {
            var cache = MemoryCache.Default;
            cache.Remove(key);
        }
        #endregion
    }
}