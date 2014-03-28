using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Practices.ObjectBuilder2;

namespace ToSic.Eav.DataSources
{
	/// <summary>
	/// Return only entities of a specific type
	/// </summary>
	public class ValueSort : BaseDataSource
	{
		public override string Name { get { return "ValueSort"; } }

		#region Configuration-properties

		private const string AttrKey = "Attributes";
		private const string DirectionKey = "Value";
		private const string LangKey = "Language";
		/// <summary>
		/// The attribute whoose value will be filtered
		/// </summary>
		public string Attributes
		{
			get { return Configuration[AttrKey]; }
			set { Configuration[AttrKey] = value; }
		}

		/// <summary>
		/// The filter that will be used - for example "Daniel" when looking for an entity w/the value Daniel
		/// </summary>
		public string Directions
		{
			get { return Configuration[DirectionKey]; }
			set { Configuration[DirectionKey] = value; }
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
		public ValueSort()
		{
			Out.Add(DataSource.DefaultStreamName, new DataStream(this, DataSource.DefaultStreamName, GetEntities));
			Configuration.Add(AttrKey, "[Settings:Attributes]");
			Configuration.Add(DirectionKey, "[Settings:Directions]");
			Configuration.Add(LangKey, "Default"); // "[Settings:Language|Default]"); // use setting, but by default, expect "any"
		}

		private IDictionary<int, IEntity> GetEntities()
		{
			// todo: maybe do something about languages?
			// todo: test datetime & decimal types

			EnsureConfigurationIsLoaded();
			var attr = Attributes.Split(',').Select(s => s.Trim()).ToArray();
			var directions = Directions.Split(',').Select(s => s.Trim()).ToArray();
			var descendingCodes = "desc,d,0,>".Split(',');

			#region Languages check - not fully implemented yet, only supports "default"
			var lang = Languages.ToLower();
			if (lang != "default")
				throw  new Exception("Can't filter for languages other than 'default'");

			if (lang == "default") lang = ""; // no language is automatically the default language

			if (lang == "any")
				throw new NotImplementedException("language 'any' not implemented yet");
			#endregion

			// only get the entities, that have these attributes (but don't test for id/title, as all have these)
			var attrWithoutIdAndTitle = attr.Where(v => v.ToLower() != "entityid" && v.ToLower() != "entitytitle").ToArray(); 
			var results = (from e in In[DataSource.DefaultStreamName].List
				where e.Value.Attributes.Keys.Where(attrWithoutIdAndTitle.Contains).Count() == attrWithoutIdAndTitle.Length
				select e);

			// if list is blank, stop here and return blank list
			if (!results.Any())
				return results.ToDictionary(x => x.Key, y => y.Value);

			IOrderedEnumerable<KeyValuePair<int, IEntity>> ordered = null;

			for(var i = 0; i < attr.Count(); i++)
			{
				// get attribute-name and type; set type=id|title for special cases
				var a = attr[i];
				var specAttr = a.ToLower() == "entityid" ? 'i' : a.ToLower() == "entitytitle" ? 't' : 'x';
				bool isAscending = true;			// default
				if (directions.Count() - 1 >= i)	// if this value has a direction specified, use that...
					isAscending = !descendingCodes.Any(directions[i].ToLower().Trim().Contains);

				if (ordered == null)
				{
					// First sort...
					ordered = isAscending
						? results.OrderBy(e => getObjToSort(e.Value, a, specAttr))
						: results.OrderByDescending(e => getObjToSort(e.Value, a, specAttr));
				}
				else
				{
					// following sorts...
					ordered = isAscending
						? ordered.ThenBy(e => getObjToSort(e.Value, a, specAttr))
						: ordered.ThenByDescending(e => getObjToSort(e.Value, a, specAttr));
				}
			}

			return ordered.ToDictionary(x => x.Key, y => y.Value); 
		}

		private object getObjToSort(IEntity e, string a, char special)
		{
			// get either the special id or title, if title or normal field, then use language [0] = default
			return special == 'i' ? e.EntityId : (special == 't' ? e.Title : e[a])[0];
		}
	}
}