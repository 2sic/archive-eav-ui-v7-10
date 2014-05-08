using System;
using System.Web.UI;

namespace ToSic.Eav.ManagementUI
{
	public partial class DateTime_Edit : FieldTemplateUserControl
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			Calendar1.ToolTip = GetMetaDataValue<string>("Notes");
			FieldLabel.Text = GetMetaDataValue("Name", Attribute.StaticName);

			if (ShowDataControlOnly)
				FieldLabel.Visible = false;
		}

		protected override void OnPreRender(EventArgs e)
		{
			if (!IsPostBack)
			{
				DateTime selectedDate;
				DateTime.TryParse(FieldValueEditString, out selectedDate);

				Calendar1.SelectedDate = selectedDate;
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