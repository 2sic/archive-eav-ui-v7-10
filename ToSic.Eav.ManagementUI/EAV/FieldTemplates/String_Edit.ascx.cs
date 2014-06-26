using System;
using System.Web.UI;

namespace ToSic.Eav.ManagementUI
{
	public partial class Text_Edit : FieldTemplateUserControl
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			var rows = GetMetaDataValue<decimal?>("RowCount");
			if (rows.HasValue && rows > 1)
			{
				TextBox1.TextMode = System.Web.UI.WebControls.TextBoxMode.MultiLine;
				TextBox1.Rows = Convert.ToInt32(rows);
			}

			TextBox1.ToolTip = GetMetaDataValue<string>("Notes");
			FieldLabel.Text = GetMetaDataValue("Name", Attribute.StaticName);

			if (ShowDataControlOnly)
				FieldLabel.Visible = false;
		}

		protected override void OnPreRender(EventArgs e)
		{
			if (!IsPostBack)
			{
				DataBind();
			}
			base.OnPreRender(e);
		}

		public override object Value
		{
			get { return TextBox1.Text; }
		}

		public override Control DataControl
		{
			get { return TextBox1; }
		}
	}
}