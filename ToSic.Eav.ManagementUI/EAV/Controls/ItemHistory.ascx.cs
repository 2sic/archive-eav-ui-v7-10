using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using ToSic.Eav.Persistence;

namespace ToSic.Eav.ManagementUI
{
	public partial class ItemHistory : UserControl
	{
		private EavContext _eavContext;
		protected int? DraftRepositoryId;

		#region Properties
		public int EntityId { get; set; }
		public bool IsDialog { get; set; }
		public int? AppId { get; set; }
		public string DetailsUrl { get; set; }
		public string ReturnUrl { get; set; }
		#endregion

		protected void Page_Load(object sender, EventArgs e)
		{
			_eavContext = EavContext.Instance(appId: AppId);
			var entityDraft = new DbLoadForCaching(_eavContext).GetEntityModel(EntityId).GetDraft();
			if (entityDraft != null)
				DraftRepositoryId = entityDraft.RepositoryId;

			// Set Back-Link and Header-Info
			hlkBack.NavigateUrl = ReturnUrl.Replace("[EntityId]", EntityId.ToString());
			litEntityId.DataBind();
			lblHasDraft.DataBind();
		}

		#region DataSources Events
		protected void dsrcEntityVersions_ObjectCreating(object sender, ObjectDataSourceEventArgs e)
		{
			e.ObjectInstance = _eavContext;
		}

		protected void dsrcEntityVersions_Selecting(object sender, ObjectDataSourceSelectingEventArgs e)
		{
			e.InputParameters["entityId"] = EntityId;
		}
		#endregion

		protected void grdItemHistory_RowDataBound(object sender, GridViewRowEventArgs e)
		{
			// Init Row and RowData
			if (e.Row.RowType != DataControlRowType.DataRow)
				return;
			var rowData = (DataRowView)e.Row.DataItem;

			// Set Changes-Link
			var hlkChanges = (HyperLink)e.Row.FindControl("hlkChanges");
			hlkChanges.NavigateUrl = DetailsUrl.Replace("[ChangeId]", rowData["ChangeId"].ToString()).Replace("[EntityId]", EntityId.ToString());
		}
	}
}