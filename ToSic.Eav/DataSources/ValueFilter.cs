using System;
using System.Collections.Generic;
using System.Linq;

namespace ToSic.Eav.DataSources
{
	/// <summary>
	/// Return only Entities having a specific value in an AttributeHelperTools
	/// </summary>
	[PipelineDesigner]
	public class ValueFilter : BaseDataSource
	{
		#region Configuration-properties

		private const string AttrKey = "AttributeHelperTools";
		private const string FilterKey = "Value";
		private const string LangKey = "Language";
		//private const string PassThroughOnEmptyValueKey = "PassThroughOnEmptyValue";


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

		///// <summary>
		///// Pass throught all Entities if Value is empty
		///// </summary>
		//public bool PassThroughOnEmptyValue
		//{
		//	get { return bool.Parse(Configuration[PassThroughOnEmptyValueKey]); }
		//	set { Configuration[PassThroughOnEmptyValueKey] = value.ToString(); }
		//}


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
		/// Constructs a new ValueFilter
		/// </summary>
		public ValueFilter()
		{
			Out.Add(DataSource.DefaultStreamName, new DataStream(this, DataSource.DefaultStreamName, GetEntities));
			Configuration.Add(AttrKey, "[Settings:AttributeHelperTools]");
			Configuration.Add(FilterKey, "[Settings:Value]");
			//Configuration.Add(PassThroughOnEmptyValueKey, "[Settings:PassThroughOnEmptyValue]");
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

			var originals = In[DataSource.DefaultStreamName].List;

			//if (string.IsNullOrEmpty(Value) && PassThroughOnEmptyValue)
			//	return originals;

			var results = (from e in originals
				where e.Value.Attributes.ContainsKey(attr)
				select e);

			if (lang == "default") lang = ""; // no language is automatically the default language

			if (lang == "any")
				throw new NotImplementedException("language 'any' not implemented yet");

			results = (from e in results
				where e.Value[attr][lang].ToString() == filter
				select e);

			return results.ToDictionary(x => x.Key, y => y.Value); 
		}


		
	}
}