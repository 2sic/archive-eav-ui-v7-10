using System;
using System.Collections.Generic;
using ToSic.Eav.Data;
using ToSic.Eav.DataSources.Caches;
using ToSic.Eav.ValueProvider;

namespace ToSic.Eav.DataSources
{
	/// <summary>
	/// Return all Entities from a specific App
	/// </summary>
	public class DeferredPipelineQuery : BaseDataSource
	{
		#region Configuration-properties

	    public IEntity QueryDefinition;

		private IDictionary<string, IDataStream> _Out = new Dictionary<string, IDataStream>();
		private bool _requiresRebuildOfOut = true;

        /// <summary>
        /// Ensures that the Out doesn't need assembling till accessed, and then auto-assembles it all
        /// </summary>
		public override IDictionary<string, IDataStream> Out
		{
			get
			{
				if (_requiresRebuildOfOut)
				{
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
		public DeferredPipelineQuery(int zoneId, int appId, IEntity queryDef, IValueCollectionProvider config)
		{
		    ZoneId = zoneId;
		    AppId = appId;
		    QueryDefinition = queryDef;
		    ConfigurationProvider = config;

		    // this one is unusual, so don't pre-attach a default data stream
		    //Out.Add(DataSource.DefaultStreamName, new DataStream(this, DataSource.DefaultStreamName, GetEntities));
		}

		/// <summary>
		/// Create a stream for each data-type
		/// </summary>
		private void CreateOutWithAllStreams()
		{
		    var pipeln = DataPipelineFactory.GetDataSource(AppId, QueryDefinition.EntityId, ConfigurationProvider as ValueCollectionProvider);
		    _Out = pipeln.Out;
		}
	}

}