using System;
using System.Collections.Generic;

namespace ToSic.Eav.DataSources.Caches
{
	/// <summary>
	/// Caching interface for standard Eav Cache
	/// </summary>
	[PipelineDesigner]
	public interface IListCache
    {
        #region Interfaces for the List-Cache

        /// <summary>
        /// The time a list stays in the cache by default - usually 3600 = 1 hour
        /// </summary>
        int ListDefaultRetentionTimeInSeconds { get; set; }

        /// <summary>
        /// Get a list from the cache
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
	    ListCacheItem ListGet(string key);

        /// <summary>
        /// Get a list from the cache
        /// </summary>
        /// <param name="dataStream">The data stream on a data-source object</param>
        /// <param name="useStreamName"></param>
        /// <returns></returns>
	    ListCacheItem ListGet(IDataStream dataStream, bool useStreamName = true);

	    void ListSet(string key, IEnumerable<IEntity> list, DateTime sourceRefresh, int durationInSeconds = 0);
	    void ListSet(IDataStream dataStream, bool useStreamName = true, int durationInSeconds = 0);

	    void ListRemove(string key);
	    bool ListHas(string key);
	    bool ListHas(IDataStream dataStream, bool useStreamName = true);
        #endregion
    }
}
