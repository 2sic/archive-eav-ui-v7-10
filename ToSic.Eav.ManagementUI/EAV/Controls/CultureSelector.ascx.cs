using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using ToSic.Eav.AscxHelpers;

namespace ToSic.Eav.ManagementUI
{
	public partial class CultureSelector : UserControl
	{
		private const string _CultureUrlParameterName = "CultureDimension";
		public int? ZoneId { get; set; }

		protected void Page_Load(object sender, EventArgs e)
		{

		}

		protected void dsrcCultureDimension_ObjectCreating(object sender, ObjectDataSourceEventArgs e)
		{
            e.ObjectInstance = new ListForSomeAscx(EavContext.Instance(zoneId: ZoneId));//  EavContext.Instance(zoneId: ZoneId);
		}

		protected void ddlCultureDimension_DataBound(object sender, EventArgs e)
		{
			// select current culture
			try
			{
				ddlCultureDimension.ClearSelection();
				ddlCultureDimension.Items.FindByValue(Request.QueryString[_CultureUrlParameterName]).Selected = true;
			}
			catch (NullReferenceException) { }

			if (ddlCultureDimension.Items.Count == 0)	// hide this control if no Cultures are specified
				Visible = false;
		}

		/// <summary>
		/// Redirect to current Page with changed CultureDimension-URL-Parameter
		/// </summary>
		protected void ddlCultureDimension_SelectedIndexChanged(object sender, EventArgs e)
		{
			var NewQueryString = HttpUtility.ParseQueryString(Request.Url.Query);

			if (ddlCultureDimension.SelectedValue != "-1")
				NewQueryString[_CultureUrlParameterName] = ddlCultureDimension.SelectedValue;
			else
				NewQueryString.Remove(_CultureUrlParameterName);

			var TargetUrl = Request.Url.AbsolutePath + "?" + NewQueryString;
			Response.Redirect(TargetUrl);
		}
	}
}