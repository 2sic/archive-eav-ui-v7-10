using System;
using System.Web.UI.WebControls;


namespace ToSic.Eav.ManagementUI
{
	public partial class ItemHistory : System.Web.UI.UserControl
	{
		#region Properties
		public int EntityId { get; set; }
		public bool IsDialog { get; set; }
		public int? AppId { get; set; }
		public int? ZoneId { get; set; }
		#endregion

		protected void Page_Load(object sender, EventArgs e)
		{

		}

		protected void dsrcEntityVersions_ObjectCreating(object sender, ObjectDataSourceEventArgs e)
		{
			e.ObjectInstance = EavContext.Instance(appId: AppId);
		}

		protected void dsrcEntityVersions_Selecting(object sender, ObjectDataSourceSelectingEventArgs e)
		{
			e.InputParameters["entityId"] = EntityId;
		}
	}
}