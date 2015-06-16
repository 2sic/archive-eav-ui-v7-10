using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography;
using ToSic.Eav.Data;

namespace ToSic.Eav.DataSources
{
	/// <summary>
	/// DataSource to only pass through configured AttributeNames
	/// </summary>
	/// <remarks>Uses Configuration "AttributeNames"</remarks>
	[PipelineDesigner]
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

            CacheRelevantConfigurations = new[] { AttributeNamesKey };
        }

	    private IDictionary<int, IEntity> cached; 
		private IDictionary<int, IEntity> GetEntities()
		{
		    if (cached != null)
		        return cached;
			EnsureConfigurationIsLoaded();

			var result = new Dictionary<int, IEntity>();

			var attributeNames = AttributeNames.Split(',');
		    attributeNames = (from a in attributeNames select a.Trim()).ToArray();

			foreach (var entity in In[DataSource.DefaultStreamName].List)
			{
				var entityModel = new Data.Entity(entity.Value, entity.Value.Attributes.Where(a => attributeNames.Contains(a.Key)).ToDictionary(k => k.Key, v => v.Value), entity.Value.Relationships.AllRelationships);

				result.Add(entity.Key, entityModel);
			}
		    cached = result;
			return result;
		}
	}
}