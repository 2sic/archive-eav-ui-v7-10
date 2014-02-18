using System;
using System.Web.UI;

namespace ToSic.Eav.ManagementUI.Dialogs
{
	public partial class ContentTypeFields : Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			try
			{
				ContentTypeFields1.AttributeSetId = Convert.ToInt32(Request.QueryString["AttributeSetId"]);
			}
			catch { }
		}
	}
}