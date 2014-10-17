using System.Collections.Generic;
using System.Linq;

namespace ToSic.Eav.DataSources
{
	/// <summary>
	/// Return only entities of a specific type
	/// </summary>
	[PipelineDesigner]
	public class EntityTypeFilter : BaseDataSource
	{
		#region Configuration-properties
		private const string TypeNameKey = "TypeName";

		/// <summary>
		/// The name of the type to filter for. 
		/// </summary>
		public string TypeName
		{
			get { return Configuration[TypeNameKey]; }
			set { Configuration[TypeNameKey] = value; }
		}
		#endregion

		/// <summary>
		/// Constructs a new EntityTypeFilter
		/// </summary>
		public EntityTypeFilter()
		{
			Out.Add(DataSource.DefaultStreamName, new DataStream(this, DataSource.DefaultStreamName, GetEntities));
			Configuration.Add(TypeNameKey, "[Settings:TypeName]");
		}

		private IDictionary<int, IEntity> GetEntities()
		{
			EnsureConfigurationIsLoaded();

			var foundType = DataSource.GetCache(ZoneId, AppId).GetContentType(TypeName);

			return (from e in In[DataSource.DefaultStreamName].List
					where e.Value.Type == foundType
					select e).ToDictionary(x => x.Key, y => y.Value);
		}

	}
}