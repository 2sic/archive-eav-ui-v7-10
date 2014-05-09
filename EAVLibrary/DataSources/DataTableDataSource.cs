using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace ToSic.Eav.DataSources
{
	/// <summary>
	/// Provide Entities from a System.Data.DataTable
	/// </summary>
	public class DataTableDataSource : BaseDataSource
	{
		#region Configuration-properties
		private const string TitleAttributeNameKey = "TitleAttributeName";
		private const string ContentTypeNameKey = "ContentTypeName";

		/// <summary>
		/// Source DataTable
		/// </summary>
		public DataTable Source { get; set; }

		/// <summary>
		/// Gets or sets the Name of the ContentType
		/// </summary>
		public string ContentTypeName
		{
			get { return Configuration[ContentTypeNameKey]; }
			set { Configuration[ContentTypeNameKey] = value; }
		}

		/// <summary>
		/// Gets or sets the Name of the Title Attribute of the Source DataTable
		/// </summary>
		public string TitleAttributeName
		{
			get { return Configuration[TitleAttributeNameKey]; }
			set { Configuration[TitleAttributeNameKey] = value; }
		}
		#endregion

		/// <summary>
		/// Initializes a new instance of the DataTableDataSource class
		/// </summary>
		public DataTableDataSource()
		{
			Out.Add(DataSource.DefaultStreamName, new DataStream(this, DataSource.DefaultStreamName, GetEntities));
			Configuration.Add(TitleAttributeNameKey, "[Settings:TitleAttributeName]");
			Configuration.Add(ContentTypeNameKey, "[Settings:ContentTypeName]");
		}

		/// <summary>
		/// Initializes a new instance of the DataTableDataSource class
		/// </summary>
		public DataTableDataSource(DataTable source, string contentTypeName, string titleAttributeName)
			: this()
		{
			Source = source;
			ContentTypeName = contentTypeName;
			TitleAttributeName = titleAttributeName;
		}

		private IDictionary<int, IEntity> GetEntities()
		{
			EnsureConfigurationIsLoaded();

			// Convert DataTable to a Dictionary of EntityModels
			var result = new Dictionary<int, IEntity>();
			foreach (DataRow row in Source.Rows)
			{
				try
				{
					var entityId = Convert.ToInt32(row["EntityId"]);
					var values = row.Table.Columns.Cast<DataColumn>().Where(c => c.ColumnName.ToLower() != "entityid").ToDictionary(col => col.ColumnName, col => row.Field<object>(col.ColumnName));
					var entity = new EntityModel(entityId, ContentTypeName, values, TitleAttributeName);
					result.Add(entity.EntityId, entity);
				}
				catch { }
			}

			return result;
		}
	}
}