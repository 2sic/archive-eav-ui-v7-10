using System;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using ToSic.Eav.Persistence;

namespace ToSic.Eav.ManagementUI
{
	public partial class ItemVersionDetails : UserControl
	{
		private EavContext _ctx;
		private IEntity _currentEntity;
		private int[] DimensionIds
		{
			get { return Forms.GetDimensionIds(DefaultCultureDimension); }
		}

		#region Properties
		public int EntityId { get; set; }
		public int ChangeId { get; set; }
		public bool IsDialog { get; set; }
		public int? AppId { get; set; }
		public int? DefaultCultureDimension { get; set; }
		public string ReturnUrl { get; set; }
		public string EditItemUrl { get; set; }
		#endregion

		protected void Page_Load(object sender, EventArgs e)
		{
			_ctx = EavContext.Instance(null, AppId);
			_currentEntity = new DbLoadAsEav(_ctx).GetEavEntity(EntityId);

			// Set Control Heading Text
			var entityTitle = _currentEntity.Title == null ? "(no Title)" : _currentEntity.Title[DimensionIds];
			litControlHeading.Text = string.Format(litControlHeading.Text, ChangeId, entityTitle, _currentEntity.EntityId);

			hlkBack.NavigateUrl = ReturnUrl.Replace("[EntityId]", EntityId.ToString());
		}

		protected void btnRestore_Click(object sender, EventArgs e)
		{
			_ctx.Versioning.RestoreEntityVersion(EntityId, ChangeId, DefaultCultureDimension);

			Response.Redirect(EditItemUrl.Replace("[EntityId]", EntityId.ToString()));
		}

		protected void dsrcVersionDetails_Selecting(object sender, ObjectDataSourceSelectingEventArgs e)
		{
			e.InputParameters["entityId"] = EntityId;
			e.InputParameters["changeId"] = ChangeId;
			e.InputParameters["defaultCultureDimension"] = DefaultCultureDimension;
			e.InputParameters["multiValuesSeparator"] = "\n";
		}

		protected void dsrcVersionDetails_ObjectCreating(object sender, ObjectDataSourceEventArgs e)
		{
			e.ObjectInstance = _ctx;
		}

		protected void grdVersionDetails_RowDataBound(object sender, GridViewRowEventArgs e)
		{
			if (e.Row.RowType != DataControlRowType.DataRow)
				return;

			var dataItem = (DataRowView)e.Row.DataItem;
			e.Row.Cells[2].Text = HttpUtility.HtmlEncode(dataItem["Value"].ToString()).Replace("\n", "<br/>");
		}


	}
}