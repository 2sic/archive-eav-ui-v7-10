using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;

namespace ToSic.Eav
{
	/// <summary>
	/// Global Tools/Helpers
	/// </summary>
	public static class Tools
	{
		/// <summary>
		/// Convert dynamic data to a DataTable. Useful for databinding to a GridView.
		/// </summary>
		/// <param name="items"></param>
		/// <returns>A DataTable with the copied dynamic data.</returns>
		public static DataTable ToDataTable(this IEnumerable<dynamic> items)
		{
			var data = items.ToArray();
			if (!data.Any())
				return null;

			var dt = new DataTable();
			foreach (var d in data)
			{
				var row = dt.NewRow();
				var record = d;

				foreach (var key in record.Keys)
				{
					if (!dt.Columns.Contains(key))
						dt.Columns.Add(key);

					row[key] = record[key];
				}

				dt.Rows.Add(row);
			}
			return dt;
		}

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

			dt.Columns.Add("EntityId");
			dt.Columns.Add("EntityTitle");

			// Add all columns
			foreach (var columnName in columnNames)
				dt.Columns.Add(columnName);

			foreach (var item in items)
			{
				var row = dt.NewRow();
				row["EntityId"] = item.EntityId;
				try
				{
					row["EntityTitle"] = item.Title[dimensionIds];
				}
				catch (NullReferenceException) { }

				foreach (DataColumn col in dt.Columns)
				{
					if (col.ColumnName == "EntityId" || col.ColumnName == "EntityTitle")
						continue;

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
	}
}