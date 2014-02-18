using System.Collections.Generic;

namespace ToSic.Eav.DataSources
{
	/// <summary>
	/// Public interface for an Eav Data Source
	/// </summary>
	public interface IDataSource
	{
		#region Data Interfaces

		/// <summary>
		/// Gets the ZoneId of this DataSource
		/// </summary>
		int ZoneId { get; }
		/// <summary>
		/// Gets the AppId of this DataSource
		/// </summary>
		int AppId { get; }

		/// <summary>
		/// Gets the Dictionary of Out-Streams
		/// </summary>
		IDictionary<string, IDataStream> Out { get; }

		/// <summary>
		/// Gets the Out-Stream with specified Name
		/// </summary>
		IDataStream this[string outName] { get; }

		/// <summary>
		/// Gets the ConfigurationProvider for this DataSource
		/// </summary>
		IConfigurationProvider ConfigurationProvider { get; }

		/// <summary>
		/// Gets a Dictionary of Configurations for this DataSource, e.g. Key: EntityId, Value: [QueryString:EntityId]
		/// </summary>
		IDictionary<string, string> Configuration { get; }

		#endregion

		#region UI Interfaces -- not implemented yet

		///// <summary>
		///// if the UI should show editing features for the user
		///// </summary>
		//bool AllowUserEdit { get; }
		///// <summary>
		///// if the UI should show sorting features for the user
		///// </summary>
		//bool AllowUserSort { get; }

		///// <summary>
		///// if the UI should show versioning features for the user
		///// </summary>
		//bool AllowVersioningUI { get; }

		#endregion

		#region Internals (Ready, DistanceFromSource)
		/// <summary>
		/// If this data source is ready to supply data
		/// </summary>
		bool Ready { get; }

		/// <summary>
		/// Gets the Name of this DataSource
		/// </summary>
		string Name { get; }
		#endregion
	}

	/// <summary>
	/// Internal interface for building the object
	/// </summary>
	public interface IDataSourceInternals
	{
		#region Configuration -- not implemented yet
		// IEntity ConfigEntity { set; }
		// Dictionary<string, string> ConfigValues { set; }
		#endregion
	}
}
