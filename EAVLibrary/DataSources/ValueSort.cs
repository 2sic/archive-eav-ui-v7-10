using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

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
			// todo: enable title & id
			// todo: ensure that attribute exists...
			// todo: sort

			// todo: maybe do something about languages?

			EnsureConfigurationIsLoaded();
			var attr = Attributes.Split(',');
			var directions = Directions.Split(',');
			var lang = Languages.ToLower();
			if(lang != "default")
				throw  new Exception("Can't filter for languages other than 'default'");

			if (lang == "default") lang = ""; // no language is automatically the default language

			if (lang == "any")
				throw new NotImplementedException("language 'any' not implemented yet");

			var list = In[DataSource.DefaultStreamName].List;
			var attribstest = list.FirstOrDefault().Value.Attributes.Keys;
			var x1 = attribstest.All(attr.Contains);
			var x2 = attribstest.Where(attr.Contains);
			var results = (from e in list 
				where e.Value.Attributes.Keys.Where(attr.Contains).Count() == attr.Length
				select e);
			IOrderedEnumerable<KeyValuePair<int, IEntity>> ordered = null;// = (IOrderedEnumerable<KeyValuePair<int, IEntity>>)results;

			for(var i = 0; i < attr.Count(); i++)
			{
				var a = attr[i];
				bool ascending = false;
				if (directions.Count() - 1 >= i)	// if it has a specification, use that...
					ascending = directions[i].ToLower().Trim() == "asc" || directions[i].Trim() == "1";
				else
					ascending = true;

				if (ordered == null)
				{
					ordered = @ascending 
						? results.OrderBy(e => e.Value[a][lang].ToString()) 
						: results.OrderByDescending(e => e.Value[a][lang].ToString());
				}
				else
				{
					if (ascending)
						ordered = ordered.ThenBy(e => e.Value[a][lang].ToString());
					else
						ordered = ordered.ThenByDescending(e => e.Value[a][lang].ToString());
				}
			}

			return ordered.ToDictionary(x => x.Key, y => y.Value); 
		}


		
	}
}