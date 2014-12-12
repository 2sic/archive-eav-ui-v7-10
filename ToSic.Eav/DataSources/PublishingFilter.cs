using System.Collections.Generic;
using ToSic.Eav.DataSources.Caches;

namespace ToSic.Eav.DataSources
{
	/// <summary>
	/// Filter entities to show Drafts or only Published Entities
	/// </summary>
	[PipelineDesigner]
	public class PublishingFilter : BaseDataSource
	{
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
		/// Constructs a new PublishingFilter
		/// </summary>
		public PublishingFilter()
		{
			Out.Add(DataSource.DefaultStreamName, new DataStream(this, DataSource.DefaultStreamName, GetEntities));
			Configuration.Add(ShowDraftsKey, "[Settings:ShowDrafts||false]");
		}

		private IDictionary<int, IEntity> GetEntities()
		{
			EnsureConfigurationIsLoaded();

			var outStreamName = ShowDrafts ? BaseCache.DraftsStreamName : BaseCache.PublishedStreamName;
			return In[outStreamName].List;
		}
	}
}