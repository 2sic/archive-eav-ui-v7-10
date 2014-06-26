using System;
using System.Linq;
using System.Web.UI;

namespace ToSic.Eav.ManagementUI.Dialogs
{
	public partial class EditItem : Page
	{
		protected override void OnInit(EventArgs e)
		{
			if (!Request.QueryString.AllKeys.Contains("EntityId"))
				throw new ArgumentException("EntityId not specified", "EntityId");

			try
			{
				EditItemForm.EntityId = Convert.ToInt32(Request.QueryString["EntityId"]);
			}
			catch
			{
				throw new ArgumentException("invalid EntityId specified", "EntityId");
			}

			EditItemForm.InitForm(System.Web.UI.WebControls.FormViewMode.Edit);

			base.OnInit(e);
		}
		protected void Page_Load(object sender, EventArgs e)
		{
		}

		//public void btnUpdate_Click(object sender, EventArgs e)
		//{
		//    EditItem1.Update();
		//    RedirectToListItems();
		//}

		//private void RedirectToListItems()
		//{
		//    Response.Redirect("Items.aspx?AttributeSetId=" + EditItem1.AttributeSetId.ToString());
		//}

		//protected void btnCancel_Click(object sender, EventArgs e)
		//{
		//    EditItem1.Cancel();
		//    RedirectToListItems();
		//}
	}
}