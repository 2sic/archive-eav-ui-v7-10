using System;

namespace ToSic.Eav
{
	/// <summary>
	/// Global Eav Configuration
	/// </summary>
	public class Configuration
	{
		private const string DefaultConnectionStringName = "EavContext";
		private static string _connectionStringName;

		/// <summary>
		/// Set ConnectionStringName for current Web Application. When set it's keeped while the IIS Application is running.
		/// </summary>
		/// <param name="connectionStringName">Name of the ConnectionString in web.config</param>
		public static void SetConnectionString(string connectionStringName)
		{
			//HttpContext.Current.Application[AppSettingPrefix + "." + ConnectionStringSetting] = ConnectionStringName;
			if (_connectionStringName != connectionStringName)
				_connectionStringName = connectionStringName;
		}

		/// <summary>
		/// Builds a Connection String that's usable by EntityFramework.
		/// SetConnectionString must be called before (if not, DefaultConnectionStringName is used).
		/// ConnectionString must be a simple SQL Connection string (not a Entity Framework one).
		/// </summary>
		/// <returns>ConnectionString for the EntityFramework</returns>
		internal static string GetConnectionString()
		{
			if (!string.IsNullOrEmpty(_connectionStringName))
			{
				//string ConnectionStringName = HttpContext.Current.Application[AppSettingPrefix + "." + ConnectionStringSetting].ToString();
				var connectionStringName = _connectionStringName;

				var builder = new System.Data.EntityClient.EntityConnectionStringBuilder();
				try
				{
					builder.ProviderConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings[connectionStringName].ConnectionString;
				}
				catch (NullReferenceException)
				{
					throw new Exception("Couldn't load Connection String with name \"" + connectionStringName + "\"");
				}
				if (!builder.ProviderConnectionString.Contains("MultipleActiveResultSets"))
					builder.ProviderConnectionString += ";MultipleActiveResultSets=True";

				builder.Metadata = "res://ToSic.Eav/Persistence.EavContext.csdl|res://ToSic.Eav/Persistence.EavContext.ssdl|res://ToSic.Eav/Persistence.EavContext.msl";
				builder.Provider = "System.Data.SqlClient";

				return builder.ToString();
			}
			try
			{
				return System.Configuration.ConfigurationManager.ConnectionStrings[DefaultConnectionStringName].ConnectionString;
			}
			catch (NullReferenceException)
			{
				throw new Exception("Couldn't load Connection String with name \"" + DefaultConnectionStringName + "\"");
			}
        }

        #region Common Assignment Object Types

	    private static int _assignmentObjectTypeIdDefault = -1;
        public static int AssignmentObjectTypeIdDefault
        {
            get
            {
                if(_assignmentObjectTypeIdDefault == -1)
                    _assignmentObjectTypeIdDefault = DataSource.GetCache(DataSource.DefaultZoneId, DataSource.MetaDataAppId).GetAssignmentObjectTypeId("Default");
                return _assignmentObjectTypeIdDefault;
            }
        }
        #endregion
    }
}
