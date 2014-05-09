using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace ToSic.Eav.DataSources
{
	/// <summary>
	/// Provide Entities from a SQL Server
	/// </summary>
	public class SqlDataSource : BaseDataSource
	{
		#region Configuration-properties
		private const string TitleAttributeNameKey = "TitleAttributeName";
		private const string ContentTypeNameKey = "ContentTypeName";
		private const string SelectCommandKey = "SelectCommand";
		private const string ConnectionStringKey = "ConnectionString";
		private const string ConnectionStringNameKey = "ConnectionStringName";
		/// <summary>
		/// Name of the EntityId Colum
		/// </summary>
		public static readonly string EntityIdColumnName = "EntityId";

		/// <summary>
		/// Gets or sets the name of the ConnectionString in the Application.Config to use
		/// </summary>
		public string ConnectionStringName
		{
			get { return Configuration[ConnectionStringNameKey]; }
			set { Configuration[ConnectionStringNameKey] = value; }
		}

		/// <summary>
		/// Gets or sets the ConnectionString
		/// </summary>
		public string ConnectionString
		{
			get { return Configuration[ConnectionStringKey]; }
			set { Configuration[ConnectionStringKey] = value; }
		}

		/// <summary>
		/// Gets or sets the SQL Command
		/// </summary>
		public string SelectCommand
		{
			get { return Configuration[SelectCommandKey]; }
			set { Configuration[SelectCommandKey] = value; }
		}

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
		/// Initializes a new instance of the SqlDataSource class
		/// </summary>
		public SqlDataSource()
		{
			Out.Add(DataSource.DefaultStreamName, new DataStream(this, DataSource.DefaultStreamName, GetEntities));
			Configuration.Add(TitleAttributeNameKey, "[Settings:TitleAttributeName]");
			Configuration.Add(ContentTypeNameKey, "[Settings:ContentTypeName]");
			Configuration.Add(SelectCommandKey, "[Settings:SelectCommand]");
			Configuration.Add(ConnectionStringKey, "[Settings:ConnectionString]");
			Configuration.Add(ConnectionStringNameKey, "[Settings:ConnectionStringName]");
		}

		/// <summary>
		/// Initializes a new instance of the SqlDataSource class
		/// </summary>
		public SqlDataSource(string connectionString, string selectCommand, string contentTypeName, string titleAttributeName)
			: this()
		{
			ConnectionString = connectionString;
			SelectCommand = selectCommand;
			ContentTypeName = contentTypeName;
			TitleAttributeName = titleAttributeName;
		}

		private IDictionary<int, IEntity> GetEntities()
		{
			EnsureConfigurationIsLoaded();

			var result = new Dictionary<int, IEntity>();

			// Load ConnectionString by Name (if specified)
			if (string.IsNullOrEmpty(ConnectionString) && !string.IsNullOrEmpty(ConnectionStringName))
				ConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings[ConnectionStringName].ConnectionString;

			using (var connection = new SqlConnection(ConnectionString))
			{
				var command = new SqlCommand(SelectCommand, connection);
				foreach (var sqlParameter in Configuration.Where(k => k.Key.StartsWith("@")))
					command.Parameters.AddWithValue(sqlParameter.Key, sqlParameter.Value);

				connection.Open();
				var reader = command.ExecuteReader();

				try
				{
					// Get the SQL Column List
					var columNames = new string[reader.FieldCount];
					for (var i = 0; i < reader.FieldCount; i++)
						columNames[i] = reader.GetName(i);

					// Read all Rows from SQL Server
					while (reader.Read())
					{
						var entityId = Convert.ToInt32(reader[EntityIdColumnName]);
						var values = columNames.Where(c => c != EntityIdColumnName).ToDictionary(c => c, c => reader[c]);
						var entity = new EntityModel(entityId, ContentTypeName, values, TitleAttributeName);
						result.Add(entityId, entity);
					}
				}
				finally
				{
					reader.Close();
				}
			}

			return result;
		}
	}
}