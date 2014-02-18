using System.Collections.Generic;
using ToSic.Eav.DataSources.Tokens;

namespace ToSic.Eav.DataSources
{
	/// <summary>
	/// Provides Configuration for a configurable DataSource
	/// </summary>
	public interface IConfigurationProvider
	{
		//string DataSourceType { get; }
		/// <summary>
		/// Property Sources this Provider can use
		/// </summary>
		Dictionary<string, IPropertyAccess> Sources { get; }

		/// <summary>
		/// Replaces all Tokens in the ConfigList with actual values provided by the Sources-Provider
		/// </summary>
		void LoadConfiguration();
	}
}
