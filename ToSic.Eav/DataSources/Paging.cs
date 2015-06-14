using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Configuration;

namespace ToSic.Eav.DataSources
{
	/// <summary>
	/// A DataSource that filters Entities by Ids
	/// </summary>
	[PipelineDesigner]
	public class Paging: BaseDataSource
	{
		#region Configuration-properties (no config)
        private const string PageSizeKey = "PageSize";
        private const string PageNumberKey = "PageNumber";


        /// <summary>
        /// The attribute whoose value will be filtered
        /// </summary>
        public int PageSize
        {
            get { return int.Parse(Configuration[PageSizeKey]); }
            set { Configuration[PageSizeKey] = value.ToString(); }
        }

        /// <summary>
        /// The attribute whoose value will be filtered
        /// </summary>
        public int PageNumber
        {
            get { return int.Parse(Configuration[PageNumberKey]); }
            set { Configuration[PageNumberKey] = value.ToString(); }
        }

		#endregion

        #region Debug-Properties

	    public string ReturnedStreamName { get; private set; }
        #endregion


        /// <summary>
		/// Constructs a new EntityIdFilter
		/// </summary>
		public Paging()
		{
			Out.Add(DataSource.DefaultStreamName, new DataStream(this, DataSource.DefaultStreamName, GetEntities));
            Out.Add("Paging", new DataStream(this, DataSource.DefaultStreamName, GetPaging));
            Configuration.Add(PageSizeKey, "[Settings:PageSize||10]");
            Configuration.Add(PageNumberKey, "[Settings:PageNumber||1]");
		}

		private IDictionary<int, IEntity> GetEntities()
		{
			EnsureConfigurationIsLoaded();

		    var itemsToSkip = (PageNumber - 1)*PageSize;

		    var result = In["Default"].List.Skip(itemsToSkip).Take(PageSize).ToList().ToDictionary(x => x.Key, y => y.Value);
		    return result;

		    // nothing found so far, return blank
            return new Dictionary<int, IEntity>();
		}

        private IDictionary<int, IEntity> GetPaging()
        {
            EnsureConfigurationIsLoaded();

            // Calculate any additional stuff
            var itemCount = In["Default"].List.Count;
            var pageCount = Math.Ceiling((decimal) itemCount / PageSize);

            // Assemble the entity
            var paging = new Dictionary<string, object>();
            paging.Add("Title", "Paging Information");
            paging.Add("PageSize", PageSize);
            paging.Add("PageNumber", PageNumber);
            paging.Add("ItemCount", itemCount);
            paging.Add("PageCount", pageCount);

            var entity = new Data.Entity(0, "Paging", paging, "Title");

            // Assemble list of this for the stream
            var result = new Dictionary<int, IEntity>();
            result.Add(entity.EntityId, entity);
            return result;
        }

	}
}