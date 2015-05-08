using System;
using System.Collections.Generic;
using System.Linq;

namespace ToSic.Eav.DataSources
{
	/// <summary>
	/// A DataSource that filters Entities by Ids
	/// </summary>
	[PipelineDesigner]
	public class StreamFallback : BaseDataSource
	{
		#region Configuration-properties (no config)

		#endregion

        #region Debug-Properties

	    public string ReturnedStreamName { get; private set; }
        #endregion


        /// <summary>
		/// Constructs a new EntityIdFilter
		/// </summary>
		public StreamFallback()
		{
			Out.Add(DataSource.DefaultStreamName, new DataStream(this, DataSource.DefaultStreamName, GetEntities));
			//Configuration.Add(EntityIdKey, "[Settings:EntityIds]");
			//Configuration.Add(PassThroughOnEmptyEntityIdsKey, "[Settings:PassThroughOnEmptyEntityIds||false]");
		}

		private IDictionary<int, IEntity> GetEntities()
		{
			EnsureConfigurationIsLoaded();
            
            // Check if there is a default-stream in with content - if yes, try to return that
            if(In.ContainsKey(DataSource.DefaultStreamName) && In[DataSource.DefaultStreamName].List.Any())
                return In[DataSource.DefaultStreamName].List;

            // Otherwise alphabetically assemble the remaining in-streams, try to return those that have content
		    var streamList = In.Where(x => x.Key != DataSource.DefaultStreamName).OrderBy(x => x.Key);
		    foreach (var stream in streamList)
		        if (stream.Value.List.Any())
		        {
		            ReturnedStreamName = stream.Key;
		            return stream.Value.List;
		        }

		    // nothing found so far, return blank
            return new Dictionary<int, IEntity>();
		}
	}
}