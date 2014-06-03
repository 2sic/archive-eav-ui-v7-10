using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.DataSources.Caches;

namespace ToSic.Eav.DataSources
{
	/// <summary>
	/// Return only entities of a specific type
	/// </summary>
	public class App : BaseDataSource
	{
		#region Configuration-properties
		private const string AppIdKey = "AppId";
		private const string ZoneIdKey = "ZoneId";

		/// <summary>
		/// The name of the type to filter for. 
		/// </summary>
		public int AppId
		{
			get { return Int32.Parse(Configuration[AppIdKey]); }
			set { Configuration[AppIdKey] = value.ToString(); }
		}
		public int ZoneId
		{
			get { return Int32.Parse(Configuration[ZoneIdKey]); }
			set { Configuration[ZoneIdKey] = value.ToString(); }
		}		
		#endregion

		/// <summary>
		/// Constructs a new EntityTypeFilter
		/// </summary>
		public App()
		{
			Out.Add(DataSource.DefaultStreamName, new DataStream(this, DataSource.DefaultStreamName, GetEntities));
			Configuration.Add(AppIdKey, "0");
			Configuration.Add(ZoneIdKey, "0");
		}

		private bool _initialized = false;
		private void EnsureDataSourceIsInitialized()
		{
			if (!_initialized)
			{
				bool mustChangeDS = (AppId != 0 && ZoneId != 0);

				// all not-set properties will auto-initialize
				if (ZoneId == 0)
					ZoneId = In["Default"].Source.ZoneId;
				if (AppId == 0)
					AppId = In["Default"].Source.ZoneId;

				if (mustChangeDS)
				{
					var newDS = Eav.DataSource.GetInitialDataSource(zoneId: ZoneId, appId: AppId);
					In.Remove("Default");
					In.Add("Default", newDS["Default"]);
				}

				// now provide all data streams for all data types
				var cache = (BaseCache)DataSource.GetCache(zoneId: ZoneId, appId: AppId);
				var listOfTypes = cache;

				_initialized = true;
			}
			
		}

		private IDictionary<int, IEntity> GetEntities()
		{
			EnsureConfigurationIsLoaded();
			EnsureDataSourceIsInitialized();

			var foundType = DataSource.GetCache(ZoneId, AppId).GetContentType(TypeName);

			return (from e in In[DataSource.DefaultStreamName].List
					where e.Value.Type == foundType
					select e).ToDictionary(x => x.Key, y => y.Value);
		}

	}
}