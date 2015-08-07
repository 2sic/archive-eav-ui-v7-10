using System.Data;
using System.Linq;

namespace ToSic.Eav
{
	public partial class EavContext
	{
        // 2015-08-07 2dm: the only code left here is for an ASCX-view in the management-UI which will be replaced by angularjs soon
        // so I'm not refactoring this as it's not relevant

		/// <summary>
		/// Get a DataTable having all Entities of specified AttributeSetId. Used to show them in a simple GridView Control
		/// </summary>
		/// <param name="attributeSetId">AttributeSetId</param>
		/// <param name="dimensionIds">List of Dimensions/Languages to show</param>
		/// <param name="maxValueLength">Shorten Values longer than n Characters</param>
		/// <param name="columnNames">Comma separated List of Column Names to show. If not set, all columns are shown</param>
		/// <returns>A DataTable with all Columns defined in the AttributeSet</returns>
		public DataTable GetItemsTable(int attributeSetId, int[] dimensionIds = null, int? maxValueLength = null, string columnNames = null)
		{
			var entityIds = Entities.Where(e => e.AttributeSetID == attributeSetId && e.ChangeLogIDDeleted == null).Select(e => e.EntityID).ToArray();
			if (!entityIds.Any())
				return null;
			var publishedEntities = DataSource.GetInitialDataSource(_zoneId, _appId).List.Where(e => entityIds.Contains(e.Key));
			var draftEntities = DataSource.GetInitialDataSource(_zoneId, _appId, true).List.Where(e => entityIds.Contains(e.Key));
			var entitiesModel = publishedEntities.Union(draftEntities);

			var columnNamesArray = DbS.GetAttributes(attributeSetId).Select(a => a.StaticName);
			if (columnNames != null)
			{
				var columNamesFilter = columnNames.Split(',');
				columnNamesArray = columnNamesArray.Where(n => columNamesFilter.Contains(n));
			}

			return entitiesModel.Select(v => v.Value).OrderBy(e => e.EntityId).ToDataTable(columnNamesArray, dimensionIds, maxValueLength);
		}


	}
}