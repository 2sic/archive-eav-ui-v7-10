using System;
using System.Data;
using System.Collections.Generic;
using System.Data.Objects.DataClasses;
using System.Linq;
using System.Reflection;

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
		/// <param name="maxValueLength">Shorten Values longer than n Characters</param>
		/// <returns>A flat DataTable of the Entities</returns>
		public static DataTable ToDataTable(this IEnumerable<IEntity> items, IEnumerable<string> columnNames, int[] dimensionIds, int? maxValueLength = null)
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


        // moved, only used once...
        ///// <summary>
        ///// Get Loadable Types from an assembly
        ///// </summary>
        ///// <remarks>Source: http://stackoverflow.com/questions/7889228/how-to-prevent-reflectiontypeloadexception-when-calling-assembly-gettypes </remarks>
        //public static IEnumerable<Type> GetLoadableTypes(this Assembly assembly)
        //{
        //    try
        //    {
        //        return assembly.GetTypes();
        //    }
        //    catch (ReflectionTypeLoadException e)
        //    {
        //        return e.Types.Where(t => t != null);
        //    }
        //}
	}
}