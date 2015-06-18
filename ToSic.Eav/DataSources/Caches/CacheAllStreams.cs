using System;
using System.Collections.Generic;
using System.Threading;
using System.Web.UI.WebControls;
using ToSic.Eav.DataSources.Caches;

namespace ToSic.Eav.DataSources.Caches
{
	/// <summary>
	/// Return all Entities from a specific App
	/// </summary>
	[PipelineDesigner]
	public class CacheAllStreams : BaseDataSource
	{

        // Todo: caching parameters
        // Refresh when Source Refreshes ...? todo!
        // Time
        // Reload in BG

		#region Configuration-properties
        private const string RefreshOnSourceRefreshKey = "RefreshOnSourceRefresh";
        private const string CacheDurationInSecondsKey = "CacheDurationInSeconds";
	    // private const string ReturnCacheWhileRefreshingKey = "ReturnCacheWhileRefreshing";
	    private bool EnforceUniqueCache = false;

		/// <summary>
		/// An alternate app to switch to
		/// </summary>
        public int CacheDurationInSeconds
		{
            get { return Int32.Parse(Configuration[CacheDurationInSecondsKey]); }
		    set { Configuration[CacheDurationInSecondsKey] = value.ToString(); }
		}

        public bool RefreshOnSourceRefresh
	    {
            get { return Convert.ToBoolean(Configuration[RefreshOnSourceRefreshKey]); }
            set { Configuration[RefreshOnSourceRefreshKey] = value.ToString(); }
	    }

        //public bool ReturnCacheWhileRefreshing
        //{
        //    get { return Convert.ToBoolean(Configuration[ReturnCacheWhileRefreshingKey]); }
        //    set { Configuration[ReturnCacheWhileRefreshingKey] = value.ToString(); }
        //}


		private IDictionary<string, IDataStream> _Out = new Dictionary<string, IDataStream>(StringComparer.OrdinalIgnoreCase);
		private bool _requiresRebuildOfOut = true;
		public override IDictionary<string, IDataStream> Out
		{
			get
			{
				if (_requiresRebuildOfOut)
				{
					// now create all streams
					CreateOutWithAllStreams();
					_requiresRebuildOfOut = false;
				}
				return _Out;
			}
		}
		#endregion

		/// <summary>
		/// Constructs a new App DataSource
		/// </summary>
		public CacheAllStreams()
		{
			// this one is unusual, so don't pre-attach a default data stream

			// Set default switch-keys to 0 = no switch
            Configuration.Add(RefreshOnSourceRefreshKey, "[Settings:" + RefreshOnSourceRefreshKey + "||True]");
			Configuration.Add(CacheDurationInSecondsKey, "[Settings:" + CacheDurationInSecondsKey + "||3600]");
            // Configuration.Add(ReturnCacheWhileRefreshingKey, "[Settings:" + ReturnCacheWhileRefreshingKey + "||False]");
        }

		/// <summary>
		/// Create a stream for each data-type
		/// </summary>
		private void CreateOutWithAllStreams()
		{
            EnsureConfigurationIsLoaded();

		    var cache = Cache as QuickCache;
			_Out.Clear();
		    foreach (var dataStream in In)
		    {
		        var inName = dataStream.Key;
		        var inStream = dataStream.Value;

		        var useCache = cache.HasList(inStream);
                var cacheItem = cache.GetList(inStream);
                
                var refresh = false;
		        if (useCache)
		        {
                    // check if upstream has newer data
		            refresh = RefreshOnSourceRefresh && inStream.Source.CacheLastRefresh > cacheItem.SourceRefresh;
		            useCache = !refresh;
		        }

		        _Out.Add(dataStream.Key,
                    useCache
                        ? new DataStream(this, inName, null, () => cacheItem.LightList )
                        : inStream);

                

                // todo: ensure it lands it the cache - but only if ever accessed?
		        if (!cache.HasList(inStream))
		            cache.SetList(inStream, durationInSeconds: CacheDurationInSeconds);
		    }
		}
	}

}