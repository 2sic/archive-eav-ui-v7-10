using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ToSic.Eav.Data;
using ToSic.Eav.Tokens;

namespace ToSic.Eav.DataSources
{
	/// <summary>
	/// Provide Entities from a SQL Server
	/// </summary>
	public class SqlDataSource : BaseDataSource
	{
		private IDictionary<int, IEntity> _entities;

		#region Configuration-properties
		protected const string TitleFieldKey = "TitleField";
		protected const string EntityIdFieldKey = "EntityIdField";
		protected const string ContentTypeKey = "ContentType";
		protected const string SelectCommandKey = "SelectCommand";
		protected const string ConnectionStringKey = "ConnectionString";
		protected const string ConnectionStringNameKey = "ConnectionStringName";
		protected const string ConnectionStringDefault = "[Settings:ConnectionString]";

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
		/// Gets or sets the Name of the Title AttributeHelperTools of the Source DataTable
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

        #region Special SQL specific properties to prevent SQL Injection

	    private string originalUnsafeSql;
        //private Dictionary<string, string> sqlParams = new Dictionary<string, string>();

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


	    protected override void EnsureConfigurationIsLoaded()
	    {
	        if (_configurationIsLoaded)
	            return;

            throw new Exception("this code is not tested yet! must test before we publish 2dm 2015-03-09");

            // todo: ensure that we can protect ourselves against SQL injection
	        var tokenizer = Tokens.BaseTokenReplace.Tokenizer;
	        originalUnsafeSql = SelectCommand;
	        var matches = tokenizer.Matches(SelectCommand);
            var cleanedSql = new StringBuilder();
	        int ParamNumber = 0;

            // Try to extract all tokens, and replace them with Param-syntax
            foreach (Match currentMatch in matches)
            {
                string strObjectName = currentMatch.Result("${object}");
                if (!String.IsNullOrEmpty(strObjectName))
                {
                    var paramName = "@AutoExtractedParam" + (ParamNumber++).ToString();
                    cleanedSql.Append(paramName);
                    Configuration.Add(paramName, currentMatch.ToString());
                }
                else
                {
                    cleanedSql.Append(currentMatch.Result("${text}"));
                }
            }
            SelectCommand = cleanedSql.ToString();

            // Process the additional parameters - not necessary, because it's automatically in Configuration
            // var instancePAs = new Dictionary<string, IValueProvider>() { { "In", new DataTargetValueProvider(this) } };
            // ConfigurationProvider.LoadConfiguration(sqlParams, instancePAs);


	        base.EnsureConfigurationIsLoaded();
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

                // Add all items in Configuration starting with an @, as this should be an SQL parameter
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
						throw new Exception(string.Format("SQL Result doesn't contain an EntityId Column with Name \"{0}\". Ideally use something like Select ID As EntityId...", EntityIdField));
					if (!columNames.Contains(TitleField))
                        throw new Exception(string.Format("SQL Result doesn't contain an EntityTitle Column with Name \"{0}\". Ideally use something like Select FullName As EntityTitle...", TitleField));
					#endregion

					#region Read all Rows from SQL Server
					while (reader.Read())
					{
						var entityId = Convert.ToInt32(reader[EntityIdField]);
						var values = columNames.Where(c => c != EntityIdField).ToDictionary(c => c, c => reader[c]);
						var entity = new Data.Entity(entityId, ContentType, values, TitleField);
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