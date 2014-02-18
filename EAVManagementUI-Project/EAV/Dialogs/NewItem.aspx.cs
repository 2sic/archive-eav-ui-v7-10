using System;
using System.Linq;
using System.Web.UI;

namespace ToSic.Eav.ManagementUI.Dialogs
{
	public partial class NewItem : Page
	{
		protected override void OnInit(EventArgs e)
		{
			if (!Request.QueryString.AllKeys.Contains("AttributeSetId"))
				throw new ArgumentException("AttributeSetId not specified", "AttributeSetId");

			try
			{
				NewItemForm.AttributeSetId = Convert.ToInt32(Request.QueryString["AttributeSetId"]);
			}
			catch
			{
				throw new ArgumentException("invalid AttributeSetId specified", "AttributeSetId");
			}

			NewItemForm.InitForm(System.Web.UI.WebControls.FormViewMode.Insert);

			base.OnInit(e);
		}
		protected void Page_Load(object sender, EventArgs e)
		{
		}
	}
}