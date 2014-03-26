using System;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ToSic.Eav.ManagementUI
{
	public partial class ItemHistory : UserControl
	{
		#region Properties
		public int EntityId { get; set; }
		public bool IsDialog { get; set; }
		public int? AppId { get; set; }
		public string DetailsUrl { get; set; }
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

		protected void grdItemHistory_RowDataBound(object sender, GridViewRowEventArgs e)
		{
			// Init Row and RowData
			if (e.Row.RowType != DataControlRowType.DataRow)
				return;
			var rowData = (EavContext.EntityVersionInfo)e.Row.DataItem;

			// Set Changes-Link
			var hlkChanges = (HyperLink)e.Row.FindControl("hlkChanges");
			hlkChanges.NavigateUrl = DetailsUrl.Replace("[ChangeId]", rowData.ChangeId.ToString()).Replace("[EntityId]", EntityId.ToString());
		}
	}
}