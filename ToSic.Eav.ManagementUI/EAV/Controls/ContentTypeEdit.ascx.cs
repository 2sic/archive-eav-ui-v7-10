using System;
using System.Web.UI.WebControls;
using ToSic.Eav.BLL;

namespace ToSic.Eav.ManagementUI
{
	public partial class ContentTypeEdit : System.Web.UI.UserControl
	{
	    protected EavDataController DB;// = EavContext.Instance();
		public int? AttributeSetId { get; set; }
		public bool IsDialog { get; set; }
		public string ReturnUrl { get; set; }
		public string Scope { get; set; }
		public int? AppId { get; set; }

		protected void Page_Load(object sender, EventArgs e)
		{
			#region if AttributeSetId is set, go show edit mode
			if (AttributeSetId.HasValue && AttributeSetId.Value != 0)
			{
				frvAttributeSet.DefaultMode = FormViewMode.Edit;
				dsrcAttributeSet.WhereParameters["AttributeSetId"].DefaultValue = AttributeSetId.ToString();
			}
			#endregion
		}

		#region Close Dialog or Redirect on Successful Insert/Update
		protected void frvAttributeSet_ItemInserted(object sender, FormViewInsertedEventArgs e)
		{
			HandleSuccess(e.AffectedRows);
		}

		protected void frvAttributeSet_ItemUpdated(object sender, FormViewUpdatedEventArgs e)
		{
			HandleSuccess(e.AffectedRows);
		}
		private void HandleSuccess(int AffectedRows)
		{
			if (AffectedRows == 1)
			{
				if (IsDialog)
					pnlCloseDialog.Visible = true;
				else
					Response.Redirect(ReturnUrl);
			}
		}

		#endregion

		protected void frvAttributeSet_ItemCommand(object sender, FormViewCommandEventArgs e)
		{
			switch (e.CommandName)
			{
				case "Cancel":
					Response.Redirect(ReturnUrl);
					break;
			}
		}

		protected void dsrcAttributeSet_ContextCreating(object sender, EntityDataSourceContextCreatingEventArgs e)
		{
			var context = EavDataController.Instance(appId: AppId);
			context.UserName = System.Web.HttpContext.Current.User.Identity.Name;
			e.Context = context.SqlDb;
		}

		protected void dsrcAttributeSet_Inserting(object sender, EntityDataSourceChangingEventArgs e)
		{
			var newSet = (AttributeSet)e.Entity;
			newSet.ChangeLogIDCreated = DB.Versioning.GetChangeLogId(System.Web.HttpContext.Current.User.Identity.Name);
			newSet.Scope = Scope;
            newSet.AppID = AppId ?? Constants.MetaDataAppId;
		}
	}
}