using System;
using System.Data;
using System.Collections.Generic;
using System.Data.Objects.DataClasses;
using System.Linq;

namespace ToSic.Eav
{
	/// <summary>
	/// Global Tools/Helpers
	/// </summary>
	public static class Tools
	{
		/// <summary>
		/// Convert Entities to a DataTable. All Entities must have same AttributeSet.
		/// </summary>
		/// <param name="items">Entities</param>
		/// <param name="columnNames">ColumnNames of the Entities</param>
		/// <param name="dimensionIds">DimensionIds/LanguageIds to show</param>
		/// <returns>A flat DataTable of the Entities</returns>
		public static DataTable ToDataTable(this IEnumerable<IEntity> items, IEnumerable<string> columnNames, int[] dimensionIds)
		{
			var dt = new DataTable();

			var systemColumns = new[] { "EntityId", "EntityTitle", "IsPublished", "PublishedEntityId", "DraftEntityId" };
			dt.Columns.Add("EntityId", typeof(int));
			dt.Columns.Add("EntityTitle");
			dt.Columns.Add("IsPublished", typeof(bool));
			dt.Columns.Add("PublishedEntityId");
			dt.Columns.Add("DraftEntityId");

			// Add all columns
			foreach (var columnName in columnNames)
				dt.Columns.Add(columnName);

			foreach (var item in items)
			{
				var row = dt.NewRow();

				#region Set System-Columns (EntityId, IsPublished, PublishedEntityID, DraftEntityId, Title
				row["EntityId"] = item.EntityId;
				row["IsPublished"] = item.IsPublished;
				var publishedEntity = item.GetPublished();
				row["PublishedEntityId"] = publishedEntity != null ? (int?)publishedEntity.EntityId : null;
				var draftEntity = item.GetDraft();
				row["DraftEntityId"] = draftEntity != null ? (int?)draftEntity.EntityId : null;
				try
				{
					row["EntityTitle"] = item.Title[dimensionIds];
				}
				catch (NullReferenceException) { }
				#endregion

				foreach (DataColumn col in dt.Columns.Cast<DataColumn>().Where(col => !systemColumns.Contains(col.ColumnName)))
				{
					try
					{
						row[col.ColumnName] = item[col.ColumnName][dimensionIds];
					}
					catch (NullReferenceException) { } // if attribute has no value
				}

				dt.Rows.Add(row);
			}

			return dt.Rows.Count != 0 ? dt : null;
		}

		/// <summary>
		/// Clone an Entity in Entity Framework 4
		/// </summary>
		/// <remarks>Source: http://www.codeproject.com/Tips/474296/Clone-an-Entity-in-Entity-Framework </remarks>
		public static T CopyEntity<T>(this T entity, EavContext ctx, bool copyKeys = false) where T : EntityObject
		{
			var clone = ctx.CreateObject<T>();
			var pis = entity.GetType().GetProperties();

			foreach (var pi in from pi in pis let attrs = (EdmScalarPropertyAttribute[])pi.GetCustomAttributes(typeof(EdmScalarPropertyAttribute), false) from attr in attrs where copyKeys || !attr.EntityKeyProperty select pi)
				pi.SetValue(clone, pi.GetValue(entity, null), null);

			return clone;
		}

	}
}