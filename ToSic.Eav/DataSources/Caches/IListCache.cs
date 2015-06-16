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

	    IEnumerable<IEntity> GetList(string key);

	    void SetList(string key, IEnumerable<IEntity> list);
	    void RemoveList(string key);
	    bool HasList(string key);
	}
}
