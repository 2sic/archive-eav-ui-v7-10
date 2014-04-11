using System.Collections.Generic;
using System.Linq;

namespace ToSic.Eav.DataSources
{
	/// <summary>
	/// Return only entities of a specific type
	/// </summary>
	public class PublishingFilter : BaseDataSource
	{
		public override string Name { get { return "PublishingFilter"; } }

		#region Configuration-properties
		private const string ShowDraftsKey = "ShowDrafts";

		/// <summary>
		/// Indicates whether to show drafts or only Published Entities
		/// </summary>
		public bool ShowDrafts
		{
			get { return bool.Parse(Configuration[ShowDraftsKey]); }
			set { Configuration[ShowDraftsKey] = value.ToString(); }
		}
		#endregion

		/// <summary>
		/// Constructs a new EntityTypeFilter
		/// </summary>
		public PublishingFilter()
		{
			Out.Add(DataSource.DefaultStreamName, new DataStream(this, DataSource.DefaultStreamName, GetEntities));
			Configuration.Add(ShowDraftsKey, "[Settings:ShowDrafts]");
		}

		private IDictionary<int, IEntity> GetEntities()
		{
			EnsureConfigurationIsLoaded();

			return (from e in In[DataSource.DefaultStreamName].List
					where (!ShowDrafts && e.Value.IsPublished) || (ShowDrafts && e.Value.GetDraft() == null)
					select e).ToDictionary(x => x.Key, y => y.Value);
		}

	}
}