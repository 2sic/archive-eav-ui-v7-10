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
	    // ObjectCache ListCache { get; }
        int DefaultListRetentionTimeInSeconds { get; set; }

	    ListCacheItem GetList(string key);
	    ListCacheItem GetList(IDataSource dataSource);
	    ListCacheItem GetList(IDataStream dataStream, bool useStreamName = true);

	    void SetList(string key, IEnumerable<IEntity> list, DateTime sourceRefresh, int durationInSeconds = 0);
	    void SetList(IDataStream dataStream, bool useStreamName = true, int durationInSeconds = 0);
	    // void SetList(IDataSource dataSource);

	    void RemoveList(string key);
	    bool HasList(string key);
	    bool HasList(IDataSource dataSource);
	    bool HasList(IDataStream dataStream, bool useStreamName = true);
	}
}
