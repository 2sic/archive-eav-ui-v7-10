using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web;
using ToSic.Eav.Data;
using ToSic.Eav.Persistence;

namespace ToSic.Eav.ManagementUI
{
	public partial class ContentTypeFields : UserControl
	{
		protected EavContext Db;
	    protected DbShortcuts DbS;

		#region Properties
		public int AttributeSetId { get; set; }
		public bool IsDialog { get; set; }
		public string ReturnUrl { get; set; }
		public string MetaDataReturnUrl { get; set; }
		public string EditItemUrl { get; set; }
		public string NewItemUrl { get; set; }
		public int? AppId { get; set; }

		public int? DefaultCultureDimension { get; set; }

		private ICollection<int> DimensionIds
		{
			get { return Forms.GetDimensionIds(DefaultCultureDimension); }
		}

		#endregion

		protected void Page_Init(object sender, EventArgs e)
		{
			Db = EavContext.Instance(appId: AppId);
            DbS = new DbShortcuts(Db);
			Db.UserName = HttpContext.Current.User.Identity.Name;
		}

		protected void Page_Load(object sender, EventArgs e)
		{
			// Set Filters in Datasources
			dsrcAttributeSet.WhereParameters["AttributeSetId"].DefaultValue = AttributeSetId.ToString();

			pnlNavigateBack.DataBind();
		}

		#region Handle Edit-Field Cancel Event
		protected void frvAddEditField_ItemCommand(object sender, FormViewCommandEventArgs e)
		{
			switch (e.CommandName)
			{
				case "Cancel":
					pnlAddEditField.Visible = false;
					lbtnInsertField.Visible = true;
					grdAttributesInSets.SelectedIndex = -1;
					frvAddEditField.PageIndex = -1;
					break;
			}
		}
		#endregion

		#region Insert Field
		protected void lbtnInsertField_Click(object sender, EventArgs e)
		{
			pnlAddEditField.Visible = true;
			lbtnInsertField.Visible = false;
			frvAddEditField.ChangeMode(FormViewMode.Insert);
			grdAttributesInSets.SelectedIndex = -1;
			frvAddEditField.PageIndex = -1;
			SetDefaultValuesInFormView();
		}
		protected void dsrcAttributes_Inserting(object sender, ObjectDataSourceMethodEventArgs e)
		{
			e.InputParameters["attributeSetId"] = AttributeSetId;
		}

		protected void frvAddEditField_ItemInserted(object sender, FormViewInsertedEventArgs e)
		{
			if (e.Exception == null)
				Response.Redirect(Request.Url.AbsoluteUri);
		}
		#endregion

		#region Edit/Move Field stuff
		#region Init Field Configuration UI (show/hide stuff)

		#endregion

		/// <summary>
		/// Handle Field Selected/editing, Move Up, Move Down
		/// </summary>
		protected void grdAttributesInSets_RowCommand(object sender, GridViewCommandEventArgs e)
		{
			var attributeId = Convert.ToInt32(e.CommandArgument);

			switch (e.CommandName)
			{
				case "MoveUp":
					Db.AttrCommands.ChangeAttributeOrder(attributeId, AttributeSetId, AttributeMoveDirection.Up);
					grdAttributesInSets.DataBind();
					break;
				case "MoveDown":
					Db.AttrCommands.ChangeAttributeOrder(attributeId, AttributeSetId, AttributeMoveDirection.Down);
					grdAttributesInSets.DataBind();
					break;
				case "MakeTitle":
					Db.AttrCommands.SetTitleAttribute(attributeId, AttributeSetId);
					grdAttributesInSets.DataBind();
					break;
				case "EditAllTypeMetaData":
				case "EditTypeMetaData":
					var metaDataAttributeSetStaticName = e.CommandName == "EditAllTypeMetaData" ? "@All" : "@" + Db.Attributes.Single(a => a.AttributeID == attributeId).Type;
					var metaDataAttributeSetId = DbS.GetAttributeSetId(metaDataAttributeSetStaticName, AttributeScope.System);

					var editMetaDataUrl = Forms.GetItemFormUrl(attributeId, metaDataAttributeSetId, Constants.AssignmentObjectTypeIdFieldProperties, NewItemUrl, EditItemUrl, MetaDataReturnUrl.Replace("[AttributeSetId]", AttributeSetId.ToString(CultureInfo.InvariantCulture)), IsDialog, DimensionIds.FirstOrDefault());
					Response.Redirect(editMetaDataUrl);
					break;
			}
		}
		#endregion

		/// <summary>
		/// Set some default values, like Type=String, Visible=true
		/// </summary>
		private void SetDefaultValuesInFormView()
		{
			var ddlType = frvAddEditField.FindControl("TypeDropDownList") as DropDownList;
			switch (frvAddEditField.CurrentMode)
			{
				case FormViewMode.Insert:
					ddlType.ClearSelection();
					try
					{
						ddlType.Items.FindByValue("String").Selected = true;
					}
					catch { }
					break;
			}
		}

		/// <summary>
		/// Handle last "move field down" link
		/// </summary>
		protected void grdAttributesInSets_DataBound(object sender, EventArgs e)
		{
			if (grdAttributesInSets.Rows.Count > 0)
			{
				// hide last colum if only one row
				if (grdAttributesInSets.Rows.Count == 1)
					grdAttributesInSets.Columns[grdAttributesInSets.Columns.Count - 1].Visible = false;
				// hide last "Move Down" link
				else
					grdAttributesInSets.Rows[grdAttributesInSets.Rows.Count - 1].FindControl("lbtnMoveDown").Visible = false;
			}
		}

		#region ContextCreating

		protected void dsrcAttributeTypes_ContextCreating(object sender, EntityDataSourceContextCreatingEventArgs e)
		{
			var context = EavContext.Instance(appId: AppId);
			context.UserName = HttpContext.Current.User.Identity.Name;
			e.Context = context;
		}

		protected void dsrcAttributeSet_ContextCreating(object sender, EntityDataSourceContextCreatingEventArgs e)
		{
			var context = EavContext.Instance(appId: AppId);
			context.UserName = HttpContext.Current.User.Identity.Name;
			e.Context = context;
		}

		protected void dsrcAttributes_ObjectCreating(object sender, ObjectDataSourceEventArgs e)
		{
			var context = EavContext.Instance(appId: AppId);
			context.UserName = HttpContext.Current.User.Identity.Name;
			e.ObjectInstance = context;
		}
		#endregion

		protected void dsrcAttributes_Deleting(object sender, ObjectDataSourceMethodEventArgs e)
		{
			e.InputParameters["AttributeSetId"] = AttributeSetId;
		}

		protected void dsrcAttributes_Inserted(object sender, ObjectDataSourceStatusEventArgs e)
		{
			var insertedAttribute = e.ReturnValue as Attribute;

			#region Add MetaData for @All
			var values = new Dictionary<string, ValueViewModel> { { "Name", new ValueViewModel { Value = insertedAttribute.StaticName } } };

			var dbMetaData = EavContext.Instance(Db.ZoneId, Db.AppId);
			dbMetaData.Versioning.SetChangeLogId(insertedAttribute.ChangeLogIDCreated);
			dbMetaData.UserName = HttpContext.Current.User.Identity.Name;
			dbMetaData.AttrCommands.UpdateAttributeAdditionalProperties(insertedAttribute.AttributeID, true, values);
			#endregion
		}

		protected void dsrcAttributes_Selecting(object sender, ObjectDataSourceSelectingEventArgs e)
		{
			e.InputParameters["AttributeSetId"] = AttributeSetId;
			e.InputParameters["DimensionIds"] = DimensionIds;
		}

		protected void dsrcAttributeSet_Selected(object sender, EntityDataSourceSelectedEventArgs e)
		{
			// prevent Editing of Fiels if AttributeSet UsesConfigurationOfAttributeSet
			var attributeSet = e.Results.Cast<AttributeSet>().Single();
			if (attributeSet.UsesConfigurationOfAttributeSet.HasValue)
				throw new Exception("Edit Fields not allowed because this AttributeSet uses Fiels of another AttributeSet.");
		}
	}
}