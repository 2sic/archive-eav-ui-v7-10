using System.Collections.Generic;
using System.Linq;

namespace ToSic.Eav.DataSources
{
	/// <summary>
	/// Return only entities of a specific type
	/// </summary>
	public class EntityTypeFilter : BaseDataSource
	{
		public override string Name { get { return "EntityTypeFilter"; } }

		/// <summary>
		/// Constructs a new EntityTypeFilter
		/// </summary>
		public EntityTypeFilter()
		{
			Out.Add(DataSource.DefaultStreamName, new DataStream(this, DataSource.DefaultStreamName, GetEntities));
			Configuration.Add("TypeName", "[Settings:TypeName]");
		}

		private IDictionary<int, IEntity> GetEntities()
		{
			EnsureConfigurationIsLoaded();

			var foundType = DataSource.GetCache(ZoneId, AppId).GetContentType(Configuration["TypeName"]);

			return (from e in In[DataSource.DefaultStreamName].List
					where e.Value.Type == foundType
					select e).ToDictionary(x => x.Key, y => y.Value);
		}
	}
}