using System;
using System.Web.UI;

namespace ToSic.Eav.ManagementUI
{
	public partial class Boolean_Edit : FieldTemplateUserControl
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			CheckBox1.ToolTip = MetaData.ContainsKey("Notes") ? MetaData["Notes"][DimensionIds].ToString() : null;
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
			get { return CheckBox1.Checked; }
		}

		public override Control DataControl
		{
			get { return CheckBox1; }
		}
	}
}