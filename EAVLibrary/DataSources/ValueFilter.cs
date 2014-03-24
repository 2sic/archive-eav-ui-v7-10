using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace ToSic.Eav.DataSources
{
	/// <summary>
	/// Return only entities of a specific type
	/// </summary>
	public class ValueFilter : BaseDataSource
	{
		public override string Name { get { return "ValueFilter"; } }

		#region Configuration-properties

		private const string AttrKey = "Attribute";
		private const string FilterKey = "Value";
		private const string LangKey = "Language";
		/// <summary>
		/// The attribute whoose value will be filtered
		/// </summary>
		public string Attribute
		{
			get { return Configuration[AttrKey]; }
			set { Configuration[AttrKey] = value; }
		}

		/// <summary>
		/// The filter that will be used - for example "Daniel" when looking for an entity w/the value Daniel
		/// </summary>
		public string Value
		{
			get { return Configuration[FilterKey]; }
			set { Configuration[FilterKey] = value; }
		}

		/// <summary>
		/// Language to filter for. At the moment it is not used, or it is trying to find "any"
		/// </summary>
		public string Languages
		{
			get { return Configuration[LangKey]; }
			set { Configuration[LangKey] = value; }
		}
		#endregion

		/// <summary>
		/// Constructs a new EntityTypeFilter
		/// </summary>
		public ValueFilter()
		{
			Out.Add(DataSource.DefaultStreamName, new DataStream(this, DataSource.DefaultStreamName, GetEntities));
			Configuration.Add(AttrKey, "[Settings:Attribute]");
			Configuration.Add(FilterKey, "[Settings:Value]");
			Configuration.Add(LangKey, "Default"); // "[Settings:Language|Any]"); // use setting, but by default, expect "any"
		}

		private IDictionary<int, IEntity> GetEntities()
		{
			// todo: maybe do something about languages?

			EnsureConfigurationIsLoaded();
			var attr = Attribute;
			var filter = Value;
			var lang = Languages.ToLower();
			if(lang != "default")
				throw  new Exception("Can't filter for languages other than 'default'");

			var results = (from e in In[DataSource.DefaultStreamName].List
				where e.Value.Attributes.ContainsKey(attr)
				select e);

			if (lang == "default") lang = ""; // no language is automatically the default language

			if (lang == "any")
				throw new NotImplementedException("language 'any' not implemented yet");
			else
				results = (from e in results
					where e.Value[attr][lang].ToString() == filter
					select e);

			return results.ToDictionary(x => x.Key, y => y.Value); 
		}


		
	}
}