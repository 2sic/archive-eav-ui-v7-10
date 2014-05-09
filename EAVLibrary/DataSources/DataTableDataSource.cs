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
		/// Name of the EntityId Colum
		/// </summary>
		public static readonly string EntityIdColumnName = "EntityId";

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

			var result = ConvertToEntityDictionary(Source, ContentTypeName, TitleAttributeName);

			return result;
		}

		/// <summary>
		/// Convert a DataTable to a Dictionary of EntityModels
		/// </summary>
		private static Dictionary<int, IEntity> ConvertToEntityDictionary(DataTable source, string contentTypeName, string titleAttributeName)
		{
			var result = new Dictionary<int, IEntity>();
			foreach (DataRow row in source.Rows)
			{
				try
				{
					var entityId = Convert.ToInt32(row[EntityIdColumnName]);
					var values = row.Table.Columns.Cast<DataColumn>().Where(c => c.ColumnName != EntityIdColumnName).ToDictionary(c => c.ColumnName, c => row.Field<object>(c.ColumnName));
					var entity = new EntityModel(entityId, contentTypeName, values, titleAttributeName);
					result.Add(entity.EntityId, entity);
				}
				catch { }
			}
			return result;
		}
	}
}