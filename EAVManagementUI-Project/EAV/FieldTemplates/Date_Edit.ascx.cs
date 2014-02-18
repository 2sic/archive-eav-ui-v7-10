using System;
using System.Web.UI;

namespace ToSic.Eav.ManagementUI
{
	public partial class Date_Edit : FieldTemplateUserControl
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			Calendar1.ToolTip = MetaData.ContainsKey("Notes") ? MetaData["Notes"][DimensionIds].ToString() : null;
			FieldLabel.Text = MetaData.ContainsKey("Name") ? MetaData["Name"][DimensionIds].ToString() : Attribute.StaticName;

			if (ShowDataControlOnly)
				FieldLabel.Visible = false;
		}

		protected override void OnPreRender(EventArgs e)
		{
			if (!IsPostBack)
			{
				DateTime SelectedDate;
				DateTime.TryParse(FieldValueEditString, out SelectedDate);

				Calendar1.SelectedDate = SelectedDate;
				Calendar1.DataBind();
			}
			base.OnPreRender(e);
		}

		public override object Value
		{
			get
			{
				return Calendar1.SelectedDate;
			}
		}

		public override Control DataControl
		{
			get
			{
				return Calendar1;
			}
		}
	}
}