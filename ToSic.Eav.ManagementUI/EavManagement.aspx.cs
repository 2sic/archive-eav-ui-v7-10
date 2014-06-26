using System;

namespace ToSic.Eav.ManagementUI.Pages
{
	public partial class EavManagement : System.Web.UI.Page
	{
		protected void Page_Init(object sender, EventArgs e)
		{
			// Optional: Set a custom Connection String
			//Eav.Configuration.SetConnectionString("SiteSqlServer");
		}

		protected void Page_Load(object sender, EventArgs e)
		{

		}

		protected void btnClearCache_Click(object sender, EventArgs e)
		{
			DataSource.GetCache(EAVManagement1.ZoneId.Value, EAVManagement1.AppId.Value).PurgeCache(EAVManagement1.ZoneId.Value, EAVManagement1.AppId.Value);
		}
	}
}