using System;
using System.Web;
using System.Web.UI;
using System.Linq;

namespace ToSic.Eav.ManagementUI
{
	public partial class EavManagement : UserControl
	{
		#region Events
		public event EntityDeletingEventHandler EntityDeleting;
		#endregion

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

		public int? ChangeId
		{
			get
			{
				try { return int.Parse(Request.QueryString["ChangeId"]); }
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

		public Guid? KeyGuid
		{
			get
			{
				try { return Guid.Parse(Request.QueryString["KeyGuid"]); }
				catch { return null; }
			}
		}

		private int? _assignmentObjectTypeId;
		public int? AssignmentObjectTypeId
		{
			get
			{
				if (!String.IsNullOrEmpty(Request.QueryString["AssignmentObjectTypeId"]))
					return int.Parse(Request.QueryString["AssignmentObjectTypeId"]);
				return _assignmentObjectTypeId;
			}
			set { _assignmentObjectTypeId = value; }
		}

		private string _baseUrl;
		public string BaseUrl
		{
			get { return string.IsNullOrEmpty(_baseUrl) ? Request.Url.AbsolutePath : _baseUrl; }
			set { _baseUrl = value; }
		}

		public string ReturnUrl
		{
			get
			{
				try { return Request.QueryString["ReturnUrl"]; }
				catch { return null; }
			}
		}

		private string _scope;
		public string Scope
		{
			// return Scope from URL (if specified)
			get { return !string.IsNullOrWhiteSpace(Request.QueryString["Scope"]) ? Request.QueryString["Scope"] : _scope; }
			set { _scope = value; }
		}

		public int? DefaultCultureDimension { get; set; }
		//private ICollection<int> DimensionIds
		//{
		//	get { return Forms.GetDimensionIds(DefaultCultureDimension); }
		//}

		public int? AppId { get; set; }
		public int? ZoneId { get; set; }

		private bool _addFormClientScriptAndCss = true;
		public bool AddFormClientScriptAndCss
		{
			get { return _addFormClientScriptAndCss; }
			set { _addFormClientScriptAndCss = value; }
		}

		public bool ShowPermissionsLink { get; set; }

		#endregion

		protected override void OnInit(EventArgs e)
		{
			var cultureDimensionReplaceValue = !string.IsNullOrEmpty(Request.QueryString["CultureDimension"]) ? Request.QueryString["CultureDimension"] : "[CultureDimension]";

			#region Add Eav Controls dynamically
			switch (Mode)
			{
				//case ManagementMode.ContentTypeEdit:
				//	var contentTypeEditControl = (ContentTypeEdit)Page.LoadControl(TemplateControl.TemplateSourceDirectory + "/ContentTypeEdit.ascx");
				//	contentTypeEditControl.AppId = AppId;
				//	contentTypeEditControl.AttributeSetId = AttributeSetId.Value;
				//	contentTypeEditControl.ReturnUrl = GetCurrentUrlWithParameters(true, "ManagementMode", ManagementMode.ContentTypesList.ToString());
				//	contentTypeEditControl.Scope = Scope;
				//	Controls.Add(contentTypeEditControl);
				//	break;
				//case ManagementMode.ContentTypeFields:
				//	var contentTypeFieldsControl = (ContentTypeFields)Page.LoadControl(TemplateControl.TemplateSourceDirectory + "/ContentTypeFields.ascx");
				//	contentTypeFieldsControl.AppId = AppId;
				//	contentTypeFieldsControl.DefaultCultureDimension = DefaultCultureDimension;
				//	contentTypeFieldsControl.AttributeSetId = AttributeSetId.Value;
				//	contentTypeFieldsControl.ReturnUrl = GetCurrentUrlWithParameters(true, "ManagementMode", ManagementMode.ContentTypesList.ToString());
				//	contentTypeFieldsControl.EditItemUrl = GetCurrentUrlWithParameters(true, "ManagementMode", ManagementMode.EditItem.ToString(), "EntityId", "[EntityId]", "AssignmentObjectTypeId", "[AssignmentObjectTypeId]", "ReturnUrl", "[ReturnUrl]", "CultureDimension", cultureDimensionReplaceValue);
				//	contentTypeFieldsControl.NewItemUrl = GetCurrentUrlWithParameters(true, "ManagementMode", ManagementMode.NewItem.ToString(), "AttributeSetId", "[AttributeSetId]", "keyNumber", "[KeyNumber]", "keyGuid", "[KeyGuid]", "AssignmentObjectTypeId", "[AssignmentObjectTypeId]", "ReturnUrl", "[ReturnUrl]", "CultureDimension", cultureDimensionReplaceValue);
				//	contentTypeFieldsControl.MetaDataReturnUrl = GetCurrentUrlWithParameters(true, "ManagementMode", ManagementMode.ContentTypeFields.ToString(), "AttributeSetId", "[AttributeSetId]", "CultureDimension", cultureDimensionReplaceValue);
				//	Controls.Add(contentTypeFieldsControl);
				//	break;
				case ManagementMode.NewItem:
				case ManagementMode.EditItem:
					var itemFormControl = (ItemForm)Page.LoadControl(TemplateControl.TemplateSourceDirectory + "/ItemForm.ascx");
					itemFormControl.AppId = AppId;
					itemFormControl.ZoneId = ZoneId;
					itemFormControl.DefaultCultureDimension = DefaultCultureDimension;
					itemFormControl.AttributeSetId = AttributeSetId.Value;
					itemFormControl.AssignmentObjectTypeId = AssignmentObjectTypeId;
					itemFormControl.KeyNumber = KeyNumber;
					itemFormControl.KeyGuid = KeyGuid;
					itemFormControl.AddClientScriptAndCss = AddFormClientScriptAndCss;
					itemFormControl.ReturnUrl = ReturnUrl ?? GetCurrentUrlWithParameters(true, "ManagementMode", ManagementMode.Items.ToString(), "AttributeSetId", "[AttributeSetId]", "CultureDimension", cultureDimensionReplaceValue);
					itemFormControl.ItemHistoryUrl = GetCurrentUrlWithParameters(true, "ManagementMode", ManagementMode.ItemHistory.ToString(), "EntityId", "[EntityId]", "CultureDimension", cultureDimensionReplaceValue);
                    itemFormControl.NewItemUrl = GetCurrentUrlWithParameters(true, "ManagementMode", ManagementMode.NewItem.ToString(), "AttributeSetId", "[AttributeSetId]", "CultureDimension", cultureDimensionReplaceValue);
					var formViewMode = System.Web.UI.WebControls.FormViewMode.Insert;
					if (Mode == ManagementMode.EditItem)
					{
						itemFormControl.EntityId = EntityId.Value;
						formViewMode = System.Web.UI.WebControls.FormViewMode.Edit;
					}
					itemFormControl.InitForm(formViewMode);
			        itemFormControl.PreventRedirect = Request.QueryString["PreventRedirect"] == "true";
					Controls.Add(itemFormControl);
					break;
				//case ManagementMode.Items:
				//	var itemsControl = (Items)Page.LoadControl(TemplateControl.TemplateSourceDirectory + "/Items.ascx");
				//	itemsControl.AppId = AppId;
				//	itemsControl.DefaultCultureDimension = DefaultCultureDimension;
				//	itemsControl.AttributeSetId = AttributeSetId.Value;
				//	itemsControl.ReturnUrl = GetCurrentUrlWithParameters(true, "ManagementMode", ManagementMode.ContentTypesList.ToString());
				//	itemsControl.EditItemUrl = GetCurrentUrlWithParameters(true, "ManagementMode", ManagementMode.EditItem.ToString(), "EntityId", "[EntityId]", "CultureDimension", "[CultureDimension]");
				//	itemsControl.NewItemUrl = GetCurrentUrlWithParameters(true, "ManagementMode", ManagementMode.NewItem.ToString(), "AttributeSetId", "[AttributeSetId]", "CultureDimension", cultureDimensionReplaceValue);
				//	if (EntityDeleting != null)
				//		itemsControl.EntityDeleting += EntityDeleting;
				//	Controls.Add(itemsControl);
				//	break;
				//case ManagementMode.ItemHistory:
				//	var itemHistoryControl = (ItemHistory)Page.LoadControl(TemplateControl.TemplateSourceDirectory + "/ItemHistory.ascx");
				//	itemHistoryControl.AppId = AppId;
				//	itemHistoryControl.EntityId = EntityId.Value;
				//	itemHistoryControl.DetailsUrl = GetCurrentUrlWithParameters(true, "ManagementMode", ManagementMode.ItemVersionDetails.ToString(), "EntityId", "[EntityId]", "ChangeId", "[ChangeId]", "CultureDimension", cultureDimensionReplaceValue);
				//	itemHistoryControl.ReturnUrl = GetCurrentUrlWithParameters(true, "ManagementMode", ManagementMode.EditItem.ToString(), "EntityId", "[EntityId]", "CultureDimension", cultureDimensionReplaceValue);
				//	Controls.Add(itemHistoryControl);
				//	break;
				//case ManagementMode.ItemVersionDetails:
				//	var itemVersionDetails = (ItemVersionDetails)Page.LoadControl(TemplateControl.TemplateSourceDirectory + "/ItemVersionDetails.ascx");
				//	itemVersionDetails.AppId = AppId;
				//	itemVersionDetails.DefaultCultureDimension = DefaultCultureDimension;
				//	itemVersionDetails.EntityId = EntityId.Value;
				//	itemVersionDetails.ChangeId = ChangeId.Value;
				//	itemVersionDetails.ReturnUrl = GetCurrentUrlWithParameters(true, "ManagementMode", ManagementMode.ItemHistory.ToString(), "EntityId", "[EntityId]", "CultureDimension", cultureDimensionReplaceValue);
				//	itemVersionDetails.EditItemUrl = GetCurrentUrlWithParameters(true, "ManagementMode", ManagementMode.EditItem.ToString(), "EntityId", "[EntityId]", "CultureDimension", cultureDimensionReplaceValue);
				//	Controls.Add(itemVersionDetails);
				//	break;
				//default:
				//	var contentTypesListControl = (ContentTypesList)Page.LoadControl(TemplateControl.TemplateSourceDirectory + "/ContentTypesList.ascx");
				//	contentTypesListControl.AppId = AppId;
				//	contentTypesListControl.DesignFieldsUrl = GetCurrentUrlWithParameters(true, "ManagementMode", ManagementMode.ContentTypeFields.ToString(), "AttributeSetId", "[AttributeSetId]");
				//	contentTypesListControl.ShowItemsUrl = GetCurrentUrlWithParameters(true, "ManagementMode", ManagementMode.Items.ToString(), "AttributeSetId", "[AttributeSetId]");
				//	contentTypesListControl.ConfigureContentTypeUrl = GetCurrentUrlWithParameters(true, "ManagementMode", ManagementMode.ContentTypeEdit.ToString(), "AttributeSetId", "[AttributeSetId]");
				//	contentTypesListControl.NewContentTypeUrl = GetCurrentUrlWithParameters(true, "ManagementMode", ManagementMode.ContentTypeEdit.ToString());
				//	contentTypesListControl.Scope = Scope;
				//	contentTypesListControl.ShowPermissionsLink = ShowPermissionsLink;
				//	Controls.Add(contentTypesListControl);
				//	break;
			}
			#endregion

			eavCultureSelector.ZoneId = ZoneId;

			base.OnInit(e);
		}

		protected void Page_Load(object sender, EventArgs e)
		{

		}

		public string GetCurrentUrlWithParameters(bool preserveSquareBrackets, params string[] @params)
		{
			var newQueryString = HttpUtility.ParseQueryString(string.Empty);

			for (var i = 0; i < @params.Length; i += 2)// Add Parameters
				newQueryString[@params[i]] = @params[i + 1];

			return BaseUrl + (BaseUrl.Contains('?') ? "&" : "?") + (preserveSquareBrackets ? newQueryString.ToString().Replace("%5b", "[").Replace("%5d", "]") : newQueryString.ToString());
		}
	}

	public enum ManagementMode
	{
		ContentTypeEdit,
		ContentTypeFields,
		ContentTypesList,
		NewItem,
		EditItem,
		Items,
		ItemHistory,
		ItemVersionDetails
	}
}