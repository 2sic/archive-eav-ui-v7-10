using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
using System.Web.UI;
using ToSic.Eav.AscxHelpers;
using ToSic.Eav.BLL;

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
		private const int ColIndexEdit = 0;
		private const int ColIndexDelete = 1;
		private const int ColIndexEntityId = 2;
		private const int ColIndexRepositoryId = 3;
		private const int ColIndexEntityTitle = 4;
		private const int ColIndexIsPublished = 5;
		private const int ColIndexPublishedRepositoryId = 6;
		private const int ColIndexDraftRepositoryId = 7;
		private string[] _columnNamesArray;
		#endregion

		#region Properties
		public int AttributeSetId { get; set; }
		public bool IsDialog { get; set; }
		public string ReturnUrl { get; set; }
		public string EditItemUrl { get; set; }
		public string NewItemUrl { get; set; }
		public int? DefaultCultureDimension { get; set; }
		public int? AppId { get; set; }
		public string ColumnNames { get; set; }
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

		protected void grdItems_DataBinding(object sender, EventArgs e)
		{
			// init columnNamesArray if ColumnNames is set
			if (!string.IsNullOrEmpty(ColumnNames))
				_columnNamesArray = ColumnNames.Split(',');
		}

		protected void grdItems_RowDataBound(object sender, GridViewRowEventArgs e)
		{
			if (e.Row.RowType == DataControlRowType.EmptyDataRow || e.Row.RowType == DataControlRowType.Pager)
				return;

			// Hide some Auto-Generated Columns
			e.Row.Cells[ColIndexPublishedRepositoryId].Visible = false;
			e.Row.Cells[ColIndexDraftRepositoryId].Visible = false;

			// hide some columns if columnNamesArray is set
			if (_columnNamesArray != null)
			{
				if (!_columnNamesArray.Contains("Edit"))
					e.Row.Cells[ColIndexEdit].Visible = false;
				if (!_columnNamesArray.Contains("Delete"))
					e.Row.Cells[ColIndexDelete].Visible = false;
				if (!_columnNamesArray.Contains("EntityId"))
					e.Row.Cells[ColIndexEntityId].Visible = false;
				if (!_columnNamesArray.Contains("RepositoryId"))
					e.Row.Cells[ColIndexRepositoryId].Visible = false;
				if (!_columnNamesArray.Contains("EntityTitle"))
					e.Row.Cells[ColIndexEntityTitle].Visible = false;
				if (!_columnNamesArray.Contains("IsPublished"))
					e.Row.Cells[ColIndexIsPublished].Visible = false;
			}

			if (e.Row.RowType != DataControlRowType.DataRow)
				return;

			var rowData = (System.Data.DataRowView)e.Row.DataItem;
			var entityId = rowData["EntityId"].ToString();

			// Show Draft Info next to IsPublished
			var isPublishedCell = e.Row.Cells[ColIndexIsPublished];
			if (rowData["PublishedRepositoryId"] != DBNull.Value)
				isPublishedCell.Controls.Add(new Label { Text = " " + rowData["PublishedRepositoryId"], ToolTip = "Draft of RepositoryId " + rowData["PublishedRepositoryId"] });
			else if (rowData["DraftRepositoryId"] != DBNull.Value)
				isPublishedCell.Controls.Add(new Label { Text = " has Draft", ToolTip = "Draft RepositoryId " + rowData["DraftRepositoryId"] });

			#region Set Edit-Link
			var editLink = (HyperLink)e.Row.Cells[ColIndexEdit].Controls[0];
			const string editLinkUrlSchemaForDialogs = "~/Eav/Dialogs/EditItem.aspx?EntityId=[EntityId]";
			if (DefaultCultureDimension.HasValue)
				editLink.NavigateUrl += "&" + CultureUrlParameterName + "=[CultureDimension]";
			var editLinkUrlSchema = IsDialog ? editLinkUrlSchemaForDialogs : EditItemUrl;
			editLink.NavigateUrl = editLinkUrlSchema.Replace("[EntityId]", entityId).Replace("[CultureDimension]", DefaultCultureDimension.ToString());
			#endregion

			#region Extend Delete-Link with ClientSide-Confirm
			if (EntityDeleting != null)
			{
				var deleteLink = (LinkButton)e.Row.Cells[ColIndexDelete].Controls[0];
				deleteLink.OnClientClick = string.Format("return confirm('Delete Entity {0}?');", rowData["RepositoryId"]);
			}
			#endregion
		}

		protected void grdItems_DataBound(object sender, EventArgs e)
		{
			// show/hide Delete-Column depending on EntityDeleting is set
			grdItems.Columns[ColIndexDelete].Visible = EntityDeleting != null;
		}

		#endregion

		#region DataSources Event Handlers

        protected void dsrcAttributeSet_ContextCreating(object sender, EntityDataSourceContextCreatingEventArgs e)
        {
            e.Context = EavDataController.Instance(appId: AppId).SqlDb;
        }

		protected void dsrcItems_ObjectCreating(object sender, ObjectDataSourceEventArgs e)
		{
            e.ObjectInstance = new ListForSomeAscx(EavDataController.Instance(appId: AppId));// EavContext.Instance(appId: AppId);
		}

		protected void dsrcItems_Selecting(object sender, ObjectDataSourceSelectingEventArgs e)
		{
			e.InputParameters["AttributeSetId"] = AttributeSetId;
			e.InputParameters["DimensionIds"] = DimensionIds;
			e.InputParameters["ColumnNames"] = ColumnNames;
		}

		protected void dsrcItems_Deleting(object sender, ObjectDataSourceMethodEventArgs e)
		{
			// init
			var repositoryId = Convert.ToInt32(e.InputParameters["RepositoryId"]);
			var ctx = EavDataController.Instance(appId: AppId);
			var deleteArgs = new EntityDeletingEventArgs { EntityId = repositoryId };

			// test if entity can be deleted
		    var canDeleteEntity = ctx.EntCommands.CanDeleteEntity(repositoryId); 
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
				ShowNotification("Entity " + repositoryId + " not deleted. " + deleteArgs.CancelMessage);
		}

		protected void dsrcItems_Deleted(object sender, ObjectDataSourceStatusEventArgs e)
		{
			if (e.ReturnValue == null || !(bool)e.ReturnValue)
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