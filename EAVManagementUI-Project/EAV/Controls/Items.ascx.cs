using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using System.Web.UI;

namespace ToSic.Eav.ManagementUI
{
	public partial class Items : UserControl
	{
		public int AttributeSetId { get; set; }
		public bool IsDialog { get; set; }
		public string ReturnUrl { get; set; }
		public string EditItemUrl { get; set; }
		public string NewItemUrl { get; set; }
		private const string CultureUrlParameterName = "CultureDimension";
		public int? DefaultCultureDimension { get; set; }
		private ICollection<int> DimensionIds
		{
			get { return Forms.GetDimensionIds(DefaultCultureDimension); }
		}
		public int? AppId { get; set; }

		protected void Page_Load(object sender, EventArgs e)
		{
			dsrcAttributeSet.WhereParameters["AttributeSetId"].DefaultValue = AttributeSetId.ToString();

			pnlNavigateBack.DataBind();
			hlnkNewItem.DataBind();
		}
		protected void grdItems_RowDataBound(object sender, GridViewRowEventArgs e)
		{
			#region Add Edit-Column
			var editCell = new TableCell();
			e.Row.Cells.Add(editCell);

			if (e.Row.RowType != DataControlRowType.DataRow)
				return;

			var editLink = new HyperLink { Text = "Edit" };
			const string editLinkUrlSchemaForDialogs = "~/Eav/Dialogs/EditItem.aspx?EntityId=[EntityId]";
			if (DefaultCultureDimension.HasValue)
				editLink.NavigateUrl += "&" + CultureUrlParameterName + "=[CultureDimension]";
			var editLinkUrlSchema = IsDialog ? editLinkUrlSchemaForDialogs : EditItemUrl;

			editLink.NavigateUrl = editLinkUrlSchema.Replace("[EntityId]", DataBinder.Eval(e.Row.DataItem, "EntityId").ToString()).Replace("[CultureDimension]", DefaultCultureDimension.ToString());
			editCell.Controls.Add(editLink);

			#endregion
		}

		protected string GetNewItemUrl()
		{
			return (IsDialog ? "~/Eav/Dialogs/NewItem.aspx?AttributeSetId=[AttributeSetId]" : NewItemUrl).Replace("[AttributeSetId]", AttributeSetId.ToString());
		}

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
	}
}