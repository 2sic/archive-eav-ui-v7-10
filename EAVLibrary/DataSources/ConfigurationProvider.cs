using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.DataSources.Tokens;

namespace ToSic.Eav.DataSources
{
	/// <summary>
	/// Provides Configuration for a configurable DataSource
	/// </summary>
	public class ConfigurationProvider : IConfigurationProvider
	{
		//public string DataSourceType { get; internal set; }
		public Dictionary<string, IPropertyAccess> Sources { get; private set; }
		/// <summary>
		/// List of all Configurations for this DataSource
		/// </summary>
		public IDictionary<string, string> ConfigList { get; internal set; }
		private readonly TokenReplace _tokenReplace;

		/// <summary>
		/// Constructs a new Configuration Provider
		/// </summary>
		public ConfigurationProvider()
		{
			Sources = new Dictionary<string, IPropertyAccess>();
			_tokenReplace = new TokenReplace(Sources);
		}

		public void LoadConfiguration()
		{
			foreach (var o in ConfigList.ToList())
			{
				if (!_tokenReplace.ContainsTokens(o.Value))
					continue;

				var newValue = _tokenReplace.ReplaceTokens(o.Value);

				// do recursion 3 times
				for (var i = 0; i < 3; i++)
				{
					if (_tokenReplace.ContainsTokens(newValue))
						newValue = _tokenReplace.ReplaceTokens(newValue);
					else
						break;
				}

				ConfigList[o.Key] = newValue;
			}
		}
	}
}