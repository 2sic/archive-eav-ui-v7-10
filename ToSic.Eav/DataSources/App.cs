using System;
using System.Collections.Generic;
using ToSic.Eav.DataSources.Caches;

namespace ToSic.Eav.DataSources
{
	/// <summary>
	/// Return all Entities from a specific App
	/// </summary>
	[PipelineDesigner]
	public class App : BaseDataSource
	{
		#region Configuration-properties
		private const string AppSwitchKey = "AppSwitch";
		private const string ZoneSwitchKey = "ZoneSwitch";

		/// <summary>
		/// An alternate app to switch to
		/// </summary>
		public int AppSwitch
		{
			get { return Int32.Parse(Configuration[AppSwitchKey]); }
			set
			{
				Configuration[AppSwitchKey] = value.ToString();
				AppId = value;
				_requiresRebuildOfOut = true;
			}
		}

		/// <summary>
		/// An alternate zone to switch to
		/// </summary>
		public int ZoneSwitch
		{
			get { return Int32.Parse(Configuration[ZoneSwitchKey]); }
			set
			{
				Configuration[ZoneSwitchKey] = value.ToString();
				ZoneId = value;
				_requiresRebuildOfOut = true;
			}
		}

		private IDictionary<string, IDataStream> _Out = new Dictionary<string, IDataStream>(StringComparer.OrdinalIgnoreCase);
		private bool _requiresRebuildOfOut = true;
		public override IDictionary<string, IDataStream> Out
		{
			get
			{
				if (_requiresRebuildOfOut)
				{
					// if the rebuilt is required because the app or zone are not default, then attach it first
					if (AppSwitch != 0 && ZoneSwitch != 0)
						AttachOtherDataSource();
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
		public App()
		{
			// this one is unusual, so don't pre-attach a default data stream
			//Out.Add(DataSource.DefaultStreamName, new DataStream(this, DataSource.DefaultStreamName, GetEntities));

			// Set default switch-keys to 0 = no switch
			Configuration.Add(AppSwitchKey, "0");
			Configuration.Add(ZoneSwitchKey, "0");

            CacheRelevantConfigurations = new[] { AppSwitchKey, ZoneSwitchKey };
        }

		/// <summary>
		/// Attach a different data source than is currently attached...
		/// this is needed when a zone/app change
		/// </summary>
		private void AttachOtherDataSource()
		{
			// all not-set properties will auto-initialize
			if (ZoneSwitch != 0)
				ZoneId = ZoneSwitch; //In[DataSource.DefaultStreamName].Source.ZoneId;
			if (AppSwitch != 0)
				AppId = AppSwitch; // In[DataSource.DefaultStreamName].Source.ZoneId;

			var newDs = DataSource.GetInitialDataSource(ZoneId, AppId);
			In.Remove(DataSource.DefaultStreamName);
			In.Add(DataSource.DefaultStreamName, newDs[DataSource.DefaultStreamName]);
		}

		/// <summary>
		/// Create a stream for each data-type
		/// </summary>
		private void CreateOutWithAllStreams()
		{
			IDataStream upstream;
			try
			{
				upstream = In[DataSource.DefaultStreamName];
			}
			catch (KeyNotFoundException)
			{
				throw new Exception("App DataSource must have a Default In-Stream with name " + DataSource.DefaultStreamName + ". It has " + In.Count + " In-Streams.");
			}

			var upstreamDataSource = upstream.Source;
			_Out.Clear();
			_Out.Add(DataSource.DefaultStreamName, upstreamDataSource.Out[DataSource.DefaultStreamName]);

			// now provide all data streams for all data types; only need the cache for the content-types list, don't use it as the source...
			// because the "real" source already applies filters like published
			var cache = (BaseCache)DataSource.GetCache(zoneId: ZoneId, appId: AppId);
			var listOfTypes = cache.GetContentTypes();
			foreach (var contentType in listOfTypes)
			{
				var ds = DataSource.GetDataSource<EntityTypeFilter>(ZoneId, AppId, upstreamDataSource, ConfigurationProvider);
				var typeName = contentType.Value.Name;
				if (typeName != DataSource.DefaultStreamName && !typeName.StartsWith("@"))
				{
					ds.TypeName = contentType.Value.Name;
					if (!_Out.ContainsKey(typeName))
						_Out.Add(contentType.Value.Name, ds.Out[DataSource.DefaultStreamName]);
				}
			}
		}
	}

}