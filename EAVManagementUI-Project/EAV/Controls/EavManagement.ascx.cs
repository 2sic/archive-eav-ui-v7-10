using System;
using System.Web;
using System.Web.UI;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Linq;

namespace ToSic.Eav.ManagementUI
{
	public partial class EavManagement : UserControl
	{
		#region Properties
		public ManagementMode? Mode
		{
			get
			{
				try { return (ManagementMode)Enum.Parse(typeof(ManagementMode), Request.QueryString["ManagementMode"]); }
				catch { return null; }
			}
		}
		public int? AttributeSetId
		{
			get
			{
				try { return Convert.ToInt32(Request.QueryString["AttributeSetId"]); }
				catch { return null; }
			}
		}

		public int? EntityId
		{
			get
			{
				try { return int.Parse(Request.QueryString["EntityId"]); }
				catch { return null; }
			}
		}

		public int? KeyNumber
		{
			get
			{
				try { return int.Parse(Request.QueryString["KeyNumber"]); }
				catch { return null; }
			}
		}

	    private int? _AssignmentObjectTypeId;
		public int? AssignmentObjectTypeId
		{
			get
			{
			    if (!String.IsNullOrEmpty(Request.QueryString["AssignmentObjectTypeId"]))
			        return int.Parse(Request.QueryString["AssignmentObjectTypeId"]);
			    return _AssignmentObjectTypeId;
			}
            set { _AssignmentObjectTypeId = value; }
		}

		private string _BaseUrl;
		public string BaseUrl
		{
			get
			{
				return string.IsNullOrEmpty(_BaseUrl) ? Request.Url.AbsolutePath : _BaseUrl;
			}
			set { _BaseUrl = value; }
		}

		public string ReturnUrl
		{
			get
			{
				try { return Request.QueryString["ReturnUrl"]; }
				catch { return null; }
			}
		}

		private string _Scope;
		public string Scope
		{
			get
			{
				// return Scope from URL (if specified)
				if (!string.IsNullOrWhiteSpace(Request.QueryString["Scope"]))
					return Request.QueryString["Scope"];

				return _Scope;
			}
			set
			{
				_Scope = value;
			}
		}

		public int? DefaultCultureDimension { get; set; }
		private ICollection<int> DimensionIds
		{
			get { return Forms.GetDimensionIds(DefaultCultureDimension); }
		}

		public int? AppId { get; set; }
		public int? ZoneId { get; set; }

        private bool _addFormClientScriptAndCss = true;
        public bool AddFormClientScriptAndCss
        {
            get { return _addFormClientScriptAndCss; }
            set { _addFormClientScriptAndCss = value; }
        }

		#endregion

		protected override void OnInit(EventArgs e)
		{
			var CultureDimensionReplaceValue = !string.IsNullOrEmpty(Request.QueryString["CultureDimension"]) ? Request.QueryString["CultureDimension"] : "[CultureDimension]";
			#region Add Eav Controls dynamically
			switch (Mode)
			{
				case ManagementMode.ContentTypeEdit:
					var ContentTypeEditControl = (ContentTypeEdit)Page.LoadControl(TemplateControl.TemplateSourceDirectory + "/ContentTypeEdit.ascx");
					ContentTypeEditControl.AppId = AppId;
					ContentTypeEditControl.AttributeSetId = AttributeSetId.Value;
					ContentTypeEditControl.ReturnUrl = GetCurrentUrlWithParameters(true, "ManagementMode", ManagementMode.ContentTypesList.ToString());
					ContentTypeEditControl.Scope = Scope;
					Controls.Add(ContentTypeEditControl);
					break;
				case ManagementMode.ContentTypeFields:
					var ContentTypeFieldsControl = (ContentTypeFields)Page.LoadControl(TemplateControl.TemplateSourceDirectory + "/ContentTypeFields.ascx");
					ContentTypeFieldsControl.AppId = AppId;
					ContentTypeFieldsControl.DefaultCultureDimension = DefaultCultureDimension;
					ContentTypeFieldsControl.AttributeSetId = AttributeSetId.Value;
					ContentTypeFieldsControl.ReturnUrl = GetCurrentUrlWithParameters(true, "ManagementMode", ManagementMode.ContentTypesList.ToString());
					ContentTypeFieldsControl.EditItemUrl = GetCurrentUrlWithParameters(true, "ManagementMode", ManagementMode.EditItem.ToString(), "EntityId", "[EntityId]", "AssignmentObjectTypeId", "[AssignmentObjectTypeId]", "ReturnUrl", "[ReturnUrl]", "CultureDimension", CultureDimensionReplaceValue);
					ContentTypeFieldsControl.NewItemUrl = GetCurrentUrlWithParameters(true, "ManagementMode", ManagementMode.NewItem.ToString(), "AttributeSetId", "[AttributeSetId]", "keyNumber", "[KeyNumber]", "AssignmentObjectTypeId", "[AssignmentObjectTypeId]", "ReturnUrl", "[ReturnUrl]", "CultureDimension", CultureDimensionReplaceValue);
					ContentTypeFieldsControl.MetaDataReturnUrl = GetCurrentUrlWithParameters(true, "ManagementMode", ManagementMode.ContentTypeFields.ToString(), "AttributeSetId", "[AttributeSetId]", "CultureDimension", CultureDimensionReplaceValue);
					Controls.Add(ContentTypeFieldsControl);
					break;
				case ManagementMode.NewItem:
				case ManagementMode.EditItem:
					var itemFormControl = (ItemForm)Page.LoadControl(TemplateControl.TemplateSourceDirectory + "/ItemForm.ascx");
					itemFormControl.AppId = AppId;
					itemFormControl.ZoneId = ZoneId;
					itemFormControl.DefaultCultureDimension = DefaultCultureDimension;
					itemFormControl.AttributeSetId = AttributeSetId.Value;
					itemFormControl.AssignmentObjectTypeId = AssignmentObjectTypeId;
					itemFormControl.KeyNumber = KeyNumber;
			        itemFormControl.AddClientScriptAndCss = AddFormClientScriptAndCss;
					itemFormControl.ReturnUrl = ReturnUrl ?? GetCurrentUrlWithParameters(true, "ManagementMode", ManagementMode.Items.ToString(), "AttributeSetId", "[AttributeSetId]", "CultureDimension", CultureDimensionReplaceValue);
					var formViewMode = System.Web.UI.WebControls.FormViewMode.Insert;
					if (Mode == ManagementMode.EditItem)
					{
						itemFormControl.EntityId = EntityId.Value;
						formViewMode = System.Web.UI.WebControls.FormViewMode.Edit;
					}
					itemFormControl.InitForm(formViewMode);
					Controls.Add(itemFormControl);
					break;
				case ManagementMode.Items:
					var itemsControl = (Items)Page.LoadControl(TemplateControl.TemplateSourceDirectory + "/Items.ascx");
					itemsControl.AppId = AppId;
					itemsControl.DefaultCultureDimension = DefaultCultureDimension;
					itemsControl.AttributeSetId = AttributeSetId.Value;
					itemsControl.ReturnUrl = GetCurrentUrlWithParameters(true, "ManagementMode", ManagementMode.ContentTypesList.ToString());
					itemsControl.EditItemUrl = GetCurrentUrlWithParameters(true, "ManagementMode", ManagementMode.EditItem.ToString(), "EntityId", "[EntityId]", "CultureDimension", "[CultureDimension]");
					itemsControl.NewItemUrl = GetCurrentUrlWithParameters(true, "ManagementMode", ManagementMode.NewItem.ToString(), "AttributeSetId", "[AttributeSetId]", "CultureDimension", CultureDimensionReplaceValue);
					Controls.Add(itemsControl);
					break;
				case ManagementMode.ContentTypesList:
				default:
					var contentTypesListControl = (ContentTypesList)Page.LoadControl(TemplateControl.TemplateSourceDirectory + "/ContentTypesList.ascx");
					contentTypesListControl.AppId = AppId;
					contentTypesListControl.DesignFieldsUrl = GetCurrentUrlWithParameters(true, "ManagementMode", ManagementMode.ContentTypeFields.ToString(), "AttributeSetId", "[AttributeSetId]");
					contentTypesListControl.ShowItemsUrl = GetCurrentUrlWithParameters(true, "ManagementMode", ManagementMode.Items.ToString(), "AttributeSetId", "[AttributeSetId]");
					contentTypesListControl.ConfigureContentTypeUrl = GetCurrentUrlWithParameters(true, "ManagementMode", ManagementMode.ContentTypeEdit.ToString(), "AttributeSetId", "[AttributeSetId]");
					contentTypesListControl.NewContentTypeUrl = GetCurrentUrlWithParameters(true, "ManagementMode", ManagementMode.ContentTypeEdit.ToString());
					contentTypesListControl.Scope = Scope;
					Controls.Add(contentTypesListControl);
					break;
			}
			#endregion

			eavCultureSelector.ZoneId = ZoneId;

			base.OnInit(e);
		}

		protected void Page_Load(object sender, EventArgs e)
		{

		}

		public string GetCurrentUrlWithParameters(bool PreserveSquareBrackets, params string[] Params)
		{
			NameValueCollection NewQueryString = HttpUtility.ParseQueryString(string.Empty);

			for (int i = 0; i < Params.Length; i += 2)// Add Parameters
				NewQueryString[Params[i]] = Params[i + 1];

			return BaseUrl + (BaseUrl.Contains('?') ? "&" : "?") + (PreserveSquareBrackets ? NewQueryString.ToString().Replace("%5b", "[").Replace("%5d", "]") : NewQueryString.ToString());
		}
	}

	public enum ManagementMode
	{
		ContentTypeEdit,
		ContentTypeFields,
		ContentTypesList,
		NewItem,
		EditItem,
		Items
	}
}