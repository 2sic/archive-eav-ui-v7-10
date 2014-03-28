using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using ToSic.Eav;

public partial class Pipelines_ValueFilter : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
		Configuration.SetConnectionString("SiteSqlServer");

    }
}