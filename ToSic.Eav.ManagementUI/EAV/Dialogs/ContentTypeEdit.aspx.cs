using System;
using System.Web.UI;

namespace ToSic.Eav.ManagementUI.Dialogs
{
	public partial class ContentType : Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			try
			{
				ContentTypeEdit1.AttributeSetId = Convert.ToInt32(Request.QueryString["AttributeSetId"]);
			}
			catch { }
		}
	}
}