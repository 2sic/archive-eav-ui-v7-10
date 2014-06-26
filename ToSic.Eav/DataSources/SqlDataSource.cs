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
		private IDictionary<int, IEntity> _entities;

		#region Configuration-properties
		private const string TitleFieldKey = "TitleField";
		private const string EntityIdFieldKey = "EntityIdField";
		private const string ContentTypeKey = "ContentType";
		private const string SelectCommandKey = "SelectCommand";
		private const string ConnectionStringKey = "ConnectionString";
		private const string ConnectionStringNameKey = "ConnectionStringName";
		private const string ConnectionStringDefault = "[Settings:ConnectionString]";

		/// <summary>
		/// Default Name of the EntityId Column
		/// </summary>
		public static readonly string EntityIdDefaultColumnName = "EntityId";

		/// <summary>
		/// Default Name of the EntityTitle Column
		/// </summary>
		public static readonly string EntityTitleDefaultColumnName = "EntityTitle";

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
		public string ContentType
		{
			get { return Configuration[ContentTypeKey]; }
			set { Configuration[ContentTypeKey] = value; }
		}

		/// <summary>
		/// Gets or sets the Name of the Title Attribute of the Source DataTable
		/// </summary>
		public string TitleField
		{
			get { return Configuration[TitleFieldKey]; }
			set { Configuration[TitleFieldKey] = value; }
		}

		/// <summary>
		/// Gets or sets the Name of the Column used as EntityId
		/// </summary>
		public string EntityIdField
		{
			get { return Configuration[EntityIdFieldKey]; }
			set { Configuration[EntityIdFieldKey] = value; }
		}

		#endregion

		/// <summary>
		/// Initializes a new instance of the SqlDataSource class
		/// </summary>
		public SqlDataSource()
		{
			Out.Add(DataSource.DefaultStreamName, new DataStream(this, DataSource.DefaultStreamName, GetEntities));
			Configuration.Add(TitleFieldKey, EntityTitleDefaultColumnName);
			Configuration.Add(EntityIdFieldKey, EntityIdDefaultColumnName);
			Configuration.Add(ContentTypeKey, "[Settings:ContentType]");
			Configuration.Add(SelectCommandKey, "[Settings:SelectCommand]");
			Configuration.Add(ConnectionStringKey, ConnectionStringDefault);
			Configuration.Add(ConnectionStringNameKey, "[Settings:ConnectionStringName]");
		}

		/// <summary>
		/// Initializes a new instance of the SqlDataSource class
		/// </summary>
		public SqlDataSource(string connectionString, string selectCommand, string contentType, string entityIdField = null, string titleField = null)
			: this()
		{
			ConnectionString = connectionString;
			SelectCommand = selectCommand;
			ContentType = contentType;
			EntityIdField = entityIdField ?? EntityIdDefaultColumnName;
			TitleField = titleField ?? EntityTitleDefaultColumnName;
		}

		private IDictionary<int, IEntity> GetEntities()
		{
			if (_entities != null)
				return _entities;

			EnsureConfigurationIsLoaded();

			_entities = new Dictionary<int, IEntity>();

			// Load ConnectionString by Name (if specified)
			if (!string.IsNullOrEmpty(ConnectionStringName) && (string.IsNullOrEmpty(ConnectionString) || ConnectionString == ConnectionStringDefault))
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
					#region Get the SQL Column List and validate it
					var columNames = new string[reader.FieldCount];
					for (var i = 0; i < reader.FieldCount; i++)
						columNames[i] = reader.GetName(i);

					if (!columNames.Contains(EntityIdField))
						throw new Exception(string.Format("SQL Result doesn't contain an EntityId Column with Name \"{0}\"", EntityIdField));
					if (!columNames.Contains(TitleField))
						throw new Exception(string.Format("SQL Result doesn't contain an EntityTitle Column with Name \"{0}\"", TitleField));
					#endregion

					#region Read all Rows from SQL Server
					while (reader.Read())
					{
						var entityId = Convert.ToInt32(reader[EntityIdField]);
						var values = columNames.Where(c => c != EntityIdField).ToDictionary(c => c, c => reader[c]);
						var entity = new EntityModel(entityId, ContentType, values, TitleField);
						_entities.Add(entityId, entity);
					}
					#endregion
				}
				finally
				{
					reader.Close();
				}
			}

			return _entities;
		}
	}
}