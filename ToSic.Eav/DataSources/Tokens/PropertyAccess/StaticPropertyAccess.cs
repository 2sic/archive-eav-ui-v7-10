using System.Collections.Generic;
using ToSic.Eav.DataSources.Tokens;

namespace ToSic.Eav.DataSources
{
	/// <summary>
	/// Property Accessor to test a Pipeline with Static Values
	/// </summary>
	public class StaticPropertyAccess : IPropertyAccess
	{
		/// <summary>
		/// List with static properties and Test-Values
		/// </summary>
		public Dictionary<string, string> Properties { get; private set; }

		/// <summary>
		/// The class constructor
		/// </summary>
		public StaticPropertyAccess(string name)
		{
			Properties = new Dictionary<string, string>();
			Name = name;
		}

		public string Name { get; private set; }

		public string GetProperty(string propertyName, string strFormat, ref bool propertyNotFound)
		{
			try
			{
				return Properties[propertyName];
			}
			catch (KeyNotFoundException)
			{
				propertyNotFound = true;
				return null;
			}
		}
	}
}
