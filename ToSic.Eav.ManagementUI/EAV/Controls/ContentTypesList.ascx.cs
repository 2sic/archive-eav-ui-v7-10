using System;
using ToSic.Eav.BLL;

namespace ToSic.Eav.ManagementUI
{
	public partial class ContentTypesList : System.Web.UI.UserControl
	{

	    public EavDataController TempController;

		public bool UseDialogs { get; set; }
		public string DesignFieldsUrl { get; set; }
		public string ShowItemsUrl { get; set; }
		public string ConfigureContentTypeUrl { get; set; }
		public string NewContentTypeUrl { get; set; }
		public string Scope { get; set; }
		public int? AppId { get; set; }
		public bool ShowPermissionsLink { get; set; }

		protected void Page_Load(object sender, EventArgs e)
		{
			hlnkNewContentType.DataBind();
		}
		protected void lbtnRefreshData_Click(object sender, EventArgs e)
		{
			grdAttributeSets.DataBind();
		}

		protected string GetDesignFieldsUrl(object attributeSetId)
		{
			return (UseDialogs ? "~/Eav/Dialogs/ContentTypeFields.aspx?AttributeSetId=[AttributeSetId]" : DesignFieldsUrl).Replace("[AttributeSetId]", attributeSetId.ToString());
		}
		protected string GetShowItemsUrl(object attributeSetId)
		{
			return (UseDialogs ? "~/Eav/Dialogs/Items.aspx?AttributeSetId=[AttributeSetId]" : ShowItemsUrl).Replace("[AttributeSetId]", attributeSetId.ToString());
		}
		protected string GetConfigureContentTypeUrl(object attributeSetId)
		{
			return (UseDialogs ? "~/Eav/Dialogs/ContentTypeEdit.aspx?AttributeSetId=[AttributeSetId]" : ConfigureContentTypeUrl).Replace("[AttributeSetId]", attributeSetId.ToString());
		}
		protected string GetNewContentTypeUrl()
		{
			return UseDialogs ? "~/Eav/Dialogs/ContentTypeEdit.aspx" : NewContentTypeUrl;
		}

		protected void dsrcAttributeSets_ContextCreating(object sender, System.Web.UI.WebControls.EntityDataSourceContextCreatingEventArgs e)
		{
		    TempController = EavDataController.Instance(appId: AppId);
			e.Context = TempController.SqlDb;//.Instance(appId: AppId);
		}

		protected void dsrcAttributeSets_Deleting(object sender, System.Web.UI.WebControls.EntityDataSourceChangingEventArgs e)
		{
			e.Cancel = true;
			var setToDelete = (AttributeSet)e.Entity;
			//var db = (EavContext)e.Context;
			setToDelete.ChangeLogIDDeleted = TempController.Versioning.GetChangeLogId(System.Web.HttpContext.Current.User.Identity.Name);
			TempController.SqlDb.SaveChanges();
            //var db = (EavContext)e.Context;
            //setToDelete.ChangeLogIDDeleted = db.Versioning.GetChangeLogId(System.Web.HttpContext.Current.User.Identity.Name);
            //db.SaveChanges();
        }

		protected void dsrcAttributeSets_Selecting(object sender, System.Web.UI.WebControls.EntityDataSourceSelectingEventArgs e)
		{
			e.DataSource.WhereParameters["Scope"].DefaultValue = Scope;
			e.DataSource.WhereParameters["AppId"].DefaultValue = AppId.ToString();
		}
	}
}