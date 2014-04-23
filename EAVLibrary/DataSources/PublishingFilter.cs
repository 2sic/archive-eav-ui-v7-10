using System.Collections.Generic;
using ToSic.Eav.DataSources.Caches;

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

			var outStreamName = ShowDrafts ? BaseCache.DraftsStreamName : BaseCache.PublishedStreamName;
			return In[outStreamName].List;
		}
	}
}