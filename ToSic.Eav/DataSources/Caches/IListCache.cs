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

	    IEnumerable<IEntity> GetList(string key);
	    IEnumerable<IEntity> GetList(IDataSource dataSource);
	    IEnumerable<IEntity> GetList(IDataStream dataStream, bool useStreamName = true);

	    void SetList(string key, IEnumerable<IEntity> list);
	    void SetList(IDataStream dataStream, bool useStreamName = true);
	    void SetList(IDataSource dataSource);

	    void RemoveList(string key);
	    bool HasList(string key);
	    bool HasList(IDataSource dataSource);
	    bool HasList(IDataStream dataStream, bool useStreamName = true);
	}
}
