using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using ToSic.Eav.Persistence;

namespace ToSic.Eav.AscxHelpers
{
    [DataObject(true)]
	public class ListForSomeAscx : DbExtensionCommandsBase // : EavContext
	{
        public ListForSomeAscx(EavContext cntx, string username = null) : base(cntx)
        {
            if(username != null)
                cntx.UserName = username;
        }

        /// <summary>
        /// Get a List of Dimensions having specified SystemKey and current ZoneId and AppId
        /// </summary>
        public List<Dimension> GetDimensionChildren(string systemKey)
        {
            return new DbDimensions(Context).GetDimensionChildren(systemKey);
        }

        /// <summary>
        /// Get a List of AttributeWithMetaInfo of specified AttributeSet and DimensionIds
        /// </summary>
        public List<AttributeWithMetaInfo> GetAttributesWithMetaInfo(int attributeSetId, int[] dimensionIds)
        {
            var attributesInSet = Context.AttributesInSets.Where(a => a.AttributeSetID == attributeSetId).OrderBy(a => a.SortOrder).ToList();

            var systemScope = AttributeScope.System.ToString();

            return (from a in attributesInSet
                    let metaData = new Metadata().GetAttributeMetaData(a.AttributeID, Context.ZoneId, Context.AppId)
                    select new AttributeWithMetaInfo
                    {
                        AttributeID = a.AttributeID,
                        IsTitle = a.IsTitle,
                        StaticName = a.Attribute.StaticName,
                        Name = metaData.ContainsKey("Name") && metaData["Name"].Values != null ? metaData["Name"][dimensionIds].ToString() : null,
                        Notes = metaData.ContainsKey("Notes") && metaData["Notes"].Values != null ? metaData["Notes"][dimensionIds].ToString() : null,
                        Type = a.Attribute.Type,
                        HasTypeMetaData = Context.AttributesInSets.Any(s => s.Set == Context.AttributeSets.FirstOrDefault(se => se.StaticName == "@" + a.Attribute.Type && se.Scope == systemScope) && s.Attribute != null),
                        MetaData = metaData
                    }).ToList();
        }

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
			var entityIds = Context.Entities.Where(e => e.AttributeSetID == attributeSetId && e.ChangeLogIDDeleted == null).Select(e => e.EntityID).ToArray();
			if (!entityIds.Any())
				return null;
			var publishedEntities = DataSource.GetInitialDataSource(Context.ZoneId, Context.AppId).List.Where(e => entityIds.Contains(e.Key));
			var draftEntities = DataSource.GetInitialDataSource(Context.ZoneId, Context.AppId, true).List.Where(e => entityIds.Contains(e.Key));
			var entitiesModel = publishedEntities.Union(draftEntities);

			var columnNamesArray = new DbAttributeCommands(Context).GetAttributes(attributeSetId).Select(a => a.StaticName);
			if (columnNames != null)
			{
				var columNamesFilter = columnNames.Split(',');
				columnNamesArray = columnNamesArray.Where(n => columNamesFilter.Contains(n));
			}

			return entitiesModel.Select(v => v.Value).OrderBy(e => e.EntityId).ToDataTable(columnNamesArray, dimensionIds, maxValueLength);
		}


	}
}