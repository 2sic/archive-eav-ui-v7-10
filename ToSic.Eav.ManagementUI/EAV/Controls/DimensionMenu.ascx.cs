using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Globalization;

namespace ToSic.Eav.ManagementUI
{
	public partial class DimensionMenu : System.Web.UI.UserControl
	{
		public int? ZoneId
		{
			get
			{
				// ToDo: Review, pass ZoneId from each FieldTemplate?
				return ((FieldTemplateUserControl)Parent).ZoneId;
			}
		}

		protected void Page_Load(object sender, EventArgs e)
		{
            //// ToDo: Localization is not solved yet (doen't work the same in ASP.NET and DotNetNuke)
            //if (CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.ToUpper() == "DE")
            //{
            //    phEnglish.Visible = false;
            //    phGerman.Visible = true;
            //}
		}
	}
}