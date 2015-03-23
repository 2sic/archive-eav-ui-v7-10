using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using ToSic.Eav.DataSources.Exceptions;

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

	    private IDictionary<int, IEntity> _results;
		private IDictionary<int, IEntity> GetEntities()
		{
		    if (_results != null)
		        return _results;
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


			if (lang == "default") lang = ""; // no language is automatically the default language

			if (lang == "any")
				throw new NotImplementedException("language 'any' not implemented yet");

            _results = GetFilteredWithLinq(originals, attr, lang, filter);
            //_results = GetFilteredWithLoop(originals, attr, lang, filter);

		    return _results;
		}

	    private Dictionary<int, IEntity> GetFilteredWithLinq(IDictionary<int, IEntity> originals, string attr, string lang, string filter)
	    {
	        var langArr = new string[] {lang};
	        string nullError = "{error: not found}";
            try
	        {
	            var results = (from e in originals
	                where (e.Value.GetBestValue(attr, langArr) ?? nullError).ToString() == filter
	                select e);
	            return results.ToDictionary(x => x.Key, y => y.Value);
	        }
	        catch (Exception ex)
	        {
	            throw new DataSourceException(
	                "Experienced error in ValueFilter while executing the filter LINQ. Probably something with type-missmatch or the same field using different types or null.",
	                ex);
	        }
	    }

	    /// <summary>
        /// A helper function to apply the filter without LINQ - ideal when trying to debug exactly what value crashed
        /// </summary>
        /// <param name="inList"></param>
        /// <param name="attr"></param>
        /// <param name="lang"></param>
        /// <returns></returns>
	    private IDictionary<int, IEntity> GetFilteredWithLoop(IDictionary<int, IEntity> inList, string attr, string lang, string filter)
	    {
            var result = new Dictionary<int, IEntity>();
            var langArr = new string[] { lang };
            foreach (var res in inList)
                //try
                //{
                    //if (res.Value[attr][lang].ToString() == filter)
                    if ((res.Value.GetBestValue(attr, langArr) ?? "").ToString() == filter)
                        result.Add(res.Key, res.Value);
                //}
                //catch { }
	        return result;
	    }
		
	}
}