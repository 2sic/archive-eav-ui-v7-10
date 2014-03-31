using System;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ToSic.Eav.ManagementUI
{
	public partial class ItemVersionDetails : UserControl
	{
		private EavContext _ctx;
		private IEntity _currentEntity;
		private int[] DimensionIds
		{
			get { return Forms.GetDimensionIds(DefaultCultureDimension); }
		}

		#region Properties
		public int EntityId { get; set; }
		public int ChangeId { get; set; }
		public bool IsDialog { get; set; }
		public int? AppId { get; set; }
		public int? DefaultCultureDimension { get; set; }
		public string ReturnUrl { get; set; }
		#endregion

		protected void Page_Load(object sender, EventArgs e)
		{
			_ctx = EavContext.Instance(AppId);
			_currentEntity = _ctx.GetEntityModel(EntityId);

			// Set Control Heading Text
			litControlHeading.Text = string.Format(litControlHeading.Text, ChangeId, _currentEntity.Title[DimensionIds], _currentEntity.EntityId);

			hlkBack.NavigateUrl = ReturnUrl.Replace("[EntityId]", EntityId.ToString());
		}

		protected void btnRestore_Click(object sender, EventArgs e)
		{
			_ctx.RestoreEntityVersion(EntityId, ChangeId, DefaultCultureDimension);
		}

		protected void dsrcVersionDetails_Selecting(object sender, ObjectDataSourceSelectingEventArgs e)
		{
			e.InputParameters["entityId"] = EntityId;
			e.InputParameters["changeId"] = ChangeId;
			e.InputParameters["defaultCultureDimension"] = DefaultCultureDimension;
		}

		protected void dsrcVersionDetails_ObjectCreating(object sender, ObjectDataSourceEventArgs e)
		{
			e.ObjectInstance = _ctx;
		}


	}
}