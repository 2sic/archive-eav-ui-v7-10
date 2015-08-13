using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using ToSic.Eav.BLL;
using ToSic.Eav.BLL.Parts;
using ToSic.Eav.Persistence;

namespace ToSic.Eav.AscxHelpers
{
    [DataObject(true)]
	public class ListForSomeAscx : BllCommandBase 
	{
        public ListForSomeAscx(EavDataController cntx, string username = null) : base(cntx)
        {
            if(username != null)
                cntx.UserName = username;
        }

        /// <summary>
        /// Get a List of Dimensions having specified SystemKey and current ZoneId and AppId
        /// </summary>
        public List<Dimension> GetDimensionChildren(string systemKey)
        {
            return Context.Dimensions.GetDimensionChildren(systemKey);
        }

        /// <summary>
        /// Get a List of AttributeWithMetaInfo of specified AttributeSet and DimensionIds
        /// </summary>
        public List<AttributeWithMetaInfo> GetAttributesWithMetaInfo(int attributeSetId, int[] dimensionIds)
        {
            var attributesInSet = Context.SqlDb.AttributesInSets.Where(a => a.AttributeSetID == attributeSetId).OrderBy(a => a.SortOrder).ToList();

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
                        HasTypeMetaData = Context.SqlDb.AttributesInSets.Any(s => s.Set == Context.SqlDb.AttributeSets.FirstOrDefault(se => se.StaticName == "@" + a.Attribute.Type && se.Scope == systemScope) && s.Attribute != null),
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
			var entityIds = Context.SqlDb.Entities.Where(e => e.AttributeSetID == attributeSetId && e.ChangeLogIDDeleted == null).Select(e => e.EntityID).ToArray();
			if (!entityIds.Any())
				return null;
			var publishedEntities = DataSource.GetInitialDataSource(Context.ZoneId, Context.AppId).List.Where(e => entityIds.Contains(e.Key));
			var draftEntities = DataSource.GetInitialDataSource(Context.ZoneId, Context.AppId, true).List.Where(e => entityIds.Contains(e.Key));
			var entitiesModel = publishedEntities.Union(draftEntities);

			var columnNamesArray = Context.Attributes.GetAttributes(attributeSetId).Select(a => a.StaticName);
			if (columnNames != null)
			{
				var columNamesFilter = columnNames.Split(',');
				columnNamesArray = columnNamesArray.Where(n => columNamesFilter.Contains(n));
			}

			var eList = entitiesModel.Select(v => v.Value).OrderBy(e => e.EntityId);
            return ToDataTable(eList, columnNamesArray, dimensionIds, maxValueLength);
		}


        /// <summary>
        /// Convert Entities to a DataTable. All Entities must have same AttributeSet.
        /// </summary>
        /// <param name="items">Entities</param>
        /// <param name="columnNames">ColumnNames of the Entities</param>
        /// <param name="dimensionIds">DimensionIds/LanguageIds to show</param>
        /// <param name="maxValueLength">Shorten Values longer than n Characters</param>
        /// <returns>A flat DataTable of the Entities</returns>
        public DataTable ToDataTable(IEnumerable<IEntity> items, IEnumerable<string> columnNames, int[] dimensionIds, int? maxValueLength = null)
        {
            var dt = new DataTable();

            var systemColumns = new[] { "EntityId", "EntityTitle", "IsPublished", "PublishedRepositoryId", "DraftRepositoryId" };
            dt.Columns.Add("EntityId", typeof(int));
            dt.Columns.Add("RepositoryId", typeof(int));
            dt.Columns.Add("EntityTitle");
            dt.Columns.Add("IsPublished", typeof(bool));
            dt.Columns.Add("PublishedRepositoryId");
            dt.Columns.Add("DraftRepositoryId");

            // Add all columns
            foreach (var columnName in columnNames)
                dt.Columns.Add(columnName);

            foreach (var item in items)
            {
                var row = dt.NewRow();

                #region Set System-Columns (EntityId, IsPublished, PublishedRepositoryId, DraftRepositoryId, Title
                row["EntityId"] = item.EntityId;
                row["RepositoryId"] = item.RepositoryId;
                row["IsPublished"] = item.IsPublished;
                var publishedEntity = item.GetPublished();
                row["PublishedRepositoryId"] = publishedEntity != null ? (int?)publishedEntity.RepositoryId : null;
                var draftEntity = item.GetDraft();
                row["DraftRepositoryId"] = draftEntity != null ? (int?)draftEntity.RepositoryId : null;
                try
                {
                    row["EntityTitle"] = item.Title[dimensionIds];
                }
                catch (NullReferenceException) { }
                #endregion

                foreach (var col in dt.Columns.Cast<DataColumn>().Where(col => !systemColumns.Contains(col.ColumnName)))
                {
                    try
                    {
                        var value = item[col.ColumnName][dimensionIds];
                        var stringValue = value as string;
                        if (stringValue != null && maxValueLength.HasValue && stringValue.Length > maxValueLength)
                            value = stringValue.Substring(0, maxValueLength.Value) + "…";

                        row[col.ColumnName] = value;
                    }
                    catch (NullReferenceException) { } // if attribute has no value
                }

                dt.Rows.Add(row);
            }

            return dt.Rows.Count != 0 ? dt : null;
        }

	}
}