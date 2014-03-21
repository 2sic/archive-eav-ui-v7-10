using System;

namespace ToSic.Eav.ManagementUI.Pages
{
	public partial class EavManagementApp2 : System.Web.UI.Page
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
			DataSource.GetCache(EAVManagement2.ZoneId.Value, EAVManagement2.AppId.Value).PurgeCache(EAVManagement2.ZoneId.Value, EAVManagement2.AppId.Value);
		}

		protected void EAVManagement2_EntityDeleting(EntityDeletingEventArgs e)
		{
			//e.Cancel = true;
			//e.CancelMessage += " Prevented deleting by EavManagementApp2";
		}
	}
}