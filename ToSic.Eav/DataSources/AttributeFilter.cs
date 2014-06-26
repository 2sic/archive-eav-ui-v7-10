using System.Collections.Generic;
using System.Linq;

namespace ToSic.Eav.DataSources
{
	/// <summary>
	/// DataSource to only pass through configured AttributeNames
	/// </summary>
	/// <remarks>Uses Configuration "AttributeNames"</remarks>
	public class AttributeFilter : BaseDataSource
	{
		#region Configuration-properties
		private const string AttributeNamesKey = "AttributeNames";

		/// <summary>
		/// A string containing one or more entity-ids. like "27" or "27,40,3063,30306"
		/// </summary>
		public string AttributeNames
		{
			get { return Configuration[AttributeNamesKey]; }
			set { Configuration[AttributeNamesKey] = value; }

		}

		#endregion

		/// <summary>
		/// Constructs a new AttributeFilter DataSource
		/// </summary>
		public AttributeFilter()
		{
			Out.Add(DataSource.DefaultStreamName, new DataStream(this, DataSource.DefaultStreamName, GetEntities));
			Configuration.Add(AttributeNamesKey, "[Settings:AttributeNames]");
		}

		private IDictionary<int, IEntity> GetEntities()
		{
			EnsureConfigurationIsLoaded();

			var result = new Dictionary<int, IEntity>();

			var attributeNames = AttributeNames.Split(',');

			foreach (var entity in In[DataSource.DefaultStreamName].List)
			{
				var entityModel = new EntityModel(entity.Value, entity.Value.Attributes.Where(a => attributeNames.Contains(a.Key)).ToDictionary(k => k.Key, v => v.Value), entity.Value.Relationships.AllRelationships);

				result.Add(entity.Key, entityModel);
			}

			return result;
		}
	}
}