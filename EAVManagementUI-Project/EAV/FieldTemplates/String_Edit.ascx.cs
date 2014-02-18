using System;
using System.Web.UI;
using System.Linq;

namespace ToSic.Eav.ManagementUI
{
	public partial class Text_Edit : FieldTemplateUserControl
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			decimal? Rows = 1;
			if (MetaData.ContainsKey("RowCount"))
				Rows = ((IAttribute<decimal?>)MetaData["RowCount"]).Typed[DimensionIds];
			if (Rows.HasValue && Rows > 1)
			{
				TextBox1.TextMode = System.Web.UI.WebControls.TextBoxMode.MultiLine;
				TextBox1.Rows = Convert.ToInt32(Rows);
			}

			//if (Attribute.Length.HasValue)
			//    TextBox1.MaxLength = Attribute.Length.Value;

			TextBox1.ToolTip = MetaData.ContainsKey("Notes") ? MetaData["Notes"][DimensionIds].ToString() : null;
			FieldLabel.Text = MetaData.ContainsKey("Name") ? MetaData["Name"][DimensionIds].ToString() : Attribute.StaticName;

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