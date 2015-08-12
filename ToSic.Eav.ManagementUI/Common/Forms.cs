using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using ToSic.Eav.BLL;

namespace ToSic.Eav.ManagementUI
{
	public class Forms
	{
		private const string CultureUrlParameterName = "CultureDimension";

		public static FieldTemplateUserControl GetFieldTemplate(Control formControl, string fieldTemplatesPath, string fieldType)
		{
			var editControl = string.Format(fieldTemplatesPath, fieldType);
			if (!System.IO.File.Exists(HttpContext.Current.Server.MapPath(editControl)))	// use String Control if appropriate not found
				editControl = string.Format(fieldTemplatesPath, "String");

			return formControl.Page.LoadControl(editControl) as FieldTemplateUserControl;
		}

		/// <summary>
		/// Create a URL to the New or Edit Item Form, depending whether the item already exists
		/// </summary>
		/// <param name="key">KeyNumber or KeyGuid</param>
		/// <param name="attributeSetId"></param>
		/// <param name="assignmentObjectTypeId"></param>
		/// <param name="newItemUrl">URL Schema, like NewItem.aspx?AttributeSetId=[AttributeSetId]</param>
		/// <param name="editItemUrl">URL Schema, like EditItem.aspx?EntityId=[EntityId]</param>
		/// <param name="returnUrl"></param>
		/// <param name="isDialog"></param>
		/// <param name="cultureDimension"></param>
		public static string GetItemFormUrl(object key, int attributeSetId, int assignmentObjectTypeId, string newItemUrl, string editItemUrl, string returnUrl = null, bool isDialog = false, int? cultureDimension = null)
		{
			var keyNumber = key as int?;
			var keyGuid = key as Guid?;

			var db = EavDataController.Instance();

			var existingEntity = db.SqlDb.Entities.FirstOrDefault(e => e.AssignmentObjectTypeID == assignmentObjectTypeId && ((keyNumber.HasValue && e.KeyNumber == keyNumber) || (keyGuid.HasValue && e.KeyGuid == keyGuid)) && e.AttributeSetID == attributeSetId);

			string result;
			// NewItem URL
			if (existingEntity == null)
			{
				var dialogUrl = "~/Eav/Dialogs/NewItem.aspx?AttributeSetId=[AttributeSetId]&AssignmentObjectTypeId=[AssignmentObjectTypeId]";
				if (keyNumber.HasValue)
					dialogUrl += "&KeyNumber=[KeyNumber]";
				else if (keyGuid.HasValue)
					dialogUrl += "&KeyGuid=[KeyGuid]";

				if (cultureDimension.HasValue)
					dialogUrl += "&CultureDimension=[CultureDimension]";
				if (!string.IsNullOrEmpty(returnUrl))
					dialogUrl += "&ReturnUrl=[ReturnUrl]";


				result = (isDialog ? dialogUrl : newItemUrl).Replace("[AttributeSetId]", attributeSetId.ToString()).Replace("[AssignmentObjectTypeId]", assignmentObjectTypeId.ToString()).Replace("[CultureDimension]", cultureDimension.ToString()).Replace("[KeyNumber]", keyNumber.ToString()).Replace("[KeyGuid]", keyGuid.ToString());
			}
			// EditItem URL
			else
			{
				var dialogUrl = "~/Eav/Dialogs/EditItem.aspx?EntityId=[EntityId]";
				if (cultureDimension.HasValue)
					dialogUrl += "&CultureDimension=[CultureDimension]";
				if (!string.IsNullOrEmpty(returnUrl))
					dialogUrl += "&ReturnUrl=[ReturnUrl]";

				result = (isDialog ? dialogUrl : editItemUrl).Replace("[AssignmentObjectTypeId]", assignmentObjectTypeId.ToString()).Replace("[EntityId]", existingEntity.EntityID.ToString()).Replace("[CultureDimension]", cultureDimension.ToString());
			}

			if (!string.IsNullOrEmpty(returnUrl))
				result = result.Replace("[ReturnUrl]", HttpUtility.UrlEncode(returnUrl));

			return result;
		}

		internal static void AddClientScriptAndCss(Control parent)
		{
			parent.Page.Header.Controls.Add(new LiteralControl
			{
				// ToDo: Convert to array of JavaScript-Paths and CSS-Paths (relative)
				Text = "<script src='" + parent.ResolveClientUrl("ItemForm.js") + "' type='text/javascript'></script>" +
					"<script src='" + parent.ResolveClientUrl("ItemFormEntityModelCreator.js") + "' type='text/javascript'></script>" +
					"<link rel='stylesheet' href='" + parent.ResolveClientUrl("ItemForm.css") + "'/>" +
					"<script type='text/javascript' src='/EAV/AngularServices/EavGlobalConfigurationProvider.js'></script>" +
					"<script type='text/javascript' src='/EAV/AngularServices/EavApiService.js'></script>" +
					"<script type='text/javascript' src='/EAV/AngularServices/EavDialogService.js'></script>"
			});
		}


		/// <summary>
		/// Get current DimensionIds
		/// </summary>
		/// <remarks>Used this Property, because Property-Connections/Inheritance in OnInit Events happens too late</remarks>
		public static int[] GetDimensionIds(int? defaultCultureDimension)
		{
			var dimensionIds = new List<int>();

			int cultureDimension;
			if (int.TryParse(HttpContext.Current.Request.QueryString[CultureUrlParameterName], out cultureDimension) && cultureDimension != 0)
				dimensionIds.Add(cultureDimension);
			else if (defaultCultureDimension.HasValue)
				dimensionIds.Add(defaultCultureDimension.Value);

			return dimensionIds.ToArray();
		}
	}
}