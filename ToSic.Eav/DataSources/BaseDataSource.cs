using System;
using System.Collections.Generic;
using ToSic.Eav.ValueProvider;

namespace ToSic.Eav.DataSources
{
	/// <summary>
	/// The base class, which should always be inherited. Already implements things like Get One / Get many, etc. 
	/// also maintains default User-May-Do-Edit/Sort etc. values
	/// </summary>
	public abstract class BaseDataSource : IDataSource, IDataTarget //, IDataSourceInternals
	{
		/// <summary>
		/// Constructor
		/// </summary>
		protected BaseDataSource()
		{
			In = new Dictionary<string, IDataStream>();
			Out = new Dictionary<string, IDataStream>();
			Configuration = new Dictionary<string, string>();
		}

		/// <summary>
		/// Name of this data source - mainly to aid debugging
		/// </summary>
		public string Name { get { return GetType().Name; } }

		/// <summary>
		/// The app this data-source is attached to
		/// </summary>
		public virtual int AppId { get; set; }

		/// <summary>
		/// The Zone this data-source is attached to
		/// </summary>
		public virtual int ZoneId { get; set; }

		public IDictionary<string, IDataStream> In { get; internal set; }
		public virtual IDictionary<string, IDataStream> Out { get; internal set; }

		public IDataStream this[string outName]
		{
			get { return Out[outName]; }
		}

		public IDictionary<int, IEntity> List
		{
			get { return Out[DataSource.DefaultStreamName].List; }
		}
		public IValueCollectionProvider ConfigurationProvider { get; internal set; }
		public IDictionary<string, string> Configuration { get; internal set; }
		internal bool _configurationIsLoaded;

        /// <summary>
        /// Make sure that configuration-parameters have been parsed (tokens resolved)
        /// but do it only once (for performance reasons)
        /// </summary>
		protected internal virtual void EnsureConfigurationIsLoaded()
		{
			if (_configurationIsLoaded)
				return;

            // Ensure that we have a configuration-provider (not always the case, but required)
            if(ConfigurationProvider == null)
                throw new Exception("No ConfigurationProvider configured on this data-source. Cannot EnsureConfigurationIsLoadedr");

            // construct a property access for in, use it in the config provider
		    var instancePAs = new Dictionary<string, IValueProvider>() {{"In".ToLower(), new DataTargetValueProvider(this)}};
			ConfigurationProvider.LoadConfiguration(Configuration, instancePAs);
			_configurationIsLoaded = true;
		}

		/// <summary>
		/// Attach specified DataSource to In
		/// </summary>
		/// <param name="dataSource">DataSource to attach</param>
		public void Attach(IDataSource dataSource)
		{
			// ensure list is blank, otherwise we'll have name conflicts when replacing a source
			if (In.Count > 0)
				In.Clear();
			foreach (var dataStream in dataSource.Out)
				In.Add(dataStream.Key, dataStream.Value);
		}


		public void Attach(string streamName, IDataSource dataSource)
		{
			Attach(streamName, dataSource[DataSource.DefaultStreamName]);
		}

		public void Attach(string streamName, IDataStream dataStream)
		{
			if (In.ContainsKey(streamName))
				In.Remove(streamName);

			In.Add(streamName, dataStream);
		}

		#region User Interface - not implemented yet
		//public virtual bool AllowUserEdit
		//{
		//    get { return true; }
		//}

		//public virtual bool AllowUserSort
		//{
		//    get { return true; }
		//}

		//public virtual bool AllowVersioningUI
		//{
		//    get { return false; }
		//}
		#endregion

		#region Configuration - not implemented yet
		//public virtual bool IsConfigurable
		//{
		//    get { return false; }
		//}
		#endregion

		#region Internals (Ready)

		/// <summary>
		/// Indicates whether the DataSource is ready for use (initialized/configured)
		/// </summary>
		public virtual bool Ready
		{
			get { return (In[DataSource.DefaultStreamName].Source != null && In[DataSource.DefaultStreamName].Source.Ready); }
		}

		#endregion


	}
}