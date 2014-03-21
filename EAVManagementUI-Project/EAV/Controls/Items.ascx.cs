using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using System.Web.UI;

namespace ToSic.Eav.ManagementUI
{
	public partial class Items : UserControl
	{
		#region Private Fields
		private const string CultureUrlParameterName = "CultureDimension";
		private ICollection<int> DimensionIds
		{
			get { return Forms.GetDimensionIds(DefaultCultureDimension); }
		}
		#endregion

		#region Properties
		public int AttributeSetId { get; set; }
		public bool IsDialog { get; set; }
		public string ReturnUrl { get; set; }
		public string EditItemUrl { get; set; }
		public string NewItemUrl { get; set; }
		public int? DefaultCultureDimension { get; set; }
		public int? AppId { get; set; }
		#endregion

		#region Events
		public event EntityDeletingEventHandler EntityDeleting;
		#endregion

		protected void Page_Load(object sender, EventArgs e)
		{
			dsrcAttributeSet.WhereParameters["AttributeSetId"].DefaultValue = AttributeSetId.ToString();

			pnlNavigateBack.DataBind();
			hlnkNewItem.DataBind();

			// Remove/Hide previous Notifications
			lblNotifications.Visible = false;
		}

		#region Grid Event Handlers

		protected void grdItems_RowDataBound(object sender, GridViewRowEventArgs e)
		{
			if (e.Row.RowType != DataControlRowType.DataRow)
				return;

			var rowData = (System.Data.DataRowView)e.Row.DataItem;
			var entityId = rowData["EntityId"].ToString();

			#region Set Edit-Link
			var editLink = (HyperLink)e.Row.Cells[0].Controls[0];
			const string editLinkUrlSchemaForDialogs = "~/Eav/Dialogs/EditItem.aspx?EntityId=[EntityId]";
			if (DefaultCultureDimension.HasValue)
				editLink.NavigateUrl += "&" + CultureUrlParameterName + "=[CultureDimension]";
			var editLinkUrlSchema = IsDialog ? editLinkUrlSchemaForDialogs : EditItemUrl;

			editLink.NavigateUrl = editLinkUrlSchema.Replace("[EntityId]", entityId).Replace("[CultureDimension]", DefaultCultureDimension.ToString());

			#endregion

			#region Extend Delete-Link with ClientSide-Confirm
			if (EntityDeleting != null)
			{
				var deleteLink = (LinkButton)e.Row.Cells[1].Controls[0];
				deleteLink.OnClientClick = string.Format("return confirm('Delete Entity {0}?');", entityId);
			}
			#endregion
		}

		protected void grdItems_DataBound(object sender, EventArgs e)
		{
			// show/hide Delete-Column depending on EntityDeleting is set
			grdItems.Columns[1].Visible = EntityDeleting != null;
		}

		#endregion

		#region DataSources Event Handlers

		protected void dsrcAttributeSet_ContextCreating(object sender, EntityDataSourceContextCreatingEventArgs e)
		{
			e.Context = EavContext.Instance(appId: AppId);
		}

		protected void dsrcItems_ObjectCreating(object sender, ObjectDataSourceEventArgs e)
		{
			e.ObjectInstance = EavContext.Instance(appId: AppId);
		}

		protected void dsrcItems_Selecting(object sender, ObjectDataSourceSelectingEventArgs e)
		{
			e.InputParameters["AttributeSetId"] = AttributeSetId;
			e.InputParameters["DimensionIds"] = DimensionIds;
		}

		protected void dsrcItems_Deleting(object sender, ObjectDataSourceMethodEventArgs e)
		{
			// init
			var entityId = Convert.ToInt32(e.InputParameters["EntityId"]);
			var ctx = EavContext.Instance(appId: AppId);
			var deleteArgs = new EntityDeletingEventArgs { EntityId = entityId };

			// test if entity can be deleted
			var canDeleteEntity = ctx.CanDeleteEntity(entityId);
			// cancel if entity can't be deleted
			if (!canDeleteEntity.Item1)
			{
				e.Cancel = true;
				deleteArgs.Cancel = true;
				deleteArgs.CancelMessage = canDeleteEntity.Item2;
			}

			// call Deleting-EventHandler, may change Cancel-Value
			EntityDeleting(deleteArgs);

			// only accept Cancel-Value if Entity can be deleted
			if (canDeleteEntity.Item1)
				e.Cancel = deleteArgs.Cancel;

			// Handle cancel
			if (deleteArgs.Cancel)
				ShowNotification("Entity " + entityId + " not deleted. " + deleteArgs.CancelMessage);
		}

		protected void dsrcItems_Deleted(object sender, ObjectDataSourceStatusEventArgs e)
		{
			if (!(bool)e.ReturnValue)
				ShowNotification("Entity wasn't deleted");
		}

		#endregion

		private void ShowNotification(string text)
		{
			lblNotifications.Visible = true;
			lblNotifications.Text = text;
		}

		protected string GetNewItemUrl()
		{
			return (IsDialog ? "~/Eav/Dialogs/NewItem.aspx?AttributeSetId=[AttributeSetId]" : NewItemUrl).Replace("[AttributeSetId]", AttributeSetId.ToString());
		}
	}
}