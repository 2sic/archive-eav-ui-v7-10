using System;
using System.Data;
using System.Linq;
using System.Web.UI;

namespace ToSic.Eav.ManagementUI
{
	public partial class ItemVersionDetails : UserControl
	{
		private EavContext _ctx;
		private IEntity _currentEntity;
		private Import.Entity _entityVersion;
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
		#endregion

		protected void Page_Load(object sender, EventArgs e)
		{
			_ctx = EavContext.Instance(AppId);
			_currentEntity = _ctx.GetEntityModel(EntityId);
			_entityVersion = _ctx.GetEntityVersion(EntityId, ChangeId);

			// Set Control Heading Text
			litControlHeading.Text = string.Format(litControlHeading.Text, ChangeId, _currentEntity.Title[DimensionIds], _currentEntity.EntityId);

			grdVersionDetails.DataSource = GetValuesTable(_entityVersion);
			grdVersionDetails.DataBind();
		}

		private DataTable GetValuesTable(Import.Entity entityVersion)
		{
			var result = new DataTable();
			result.Columns.Add("Field");
			result.Columns.Add("Language");
			result.Columns.Add("Value");

			foreach (var attribute in entityVersion.Values)
			{
				foreach (var valueModel in attribute.Value)
				{
					foreach (var valueDimension in valueModel.ValueDimensions)
					{
						result.Rows.Add(attribute.Key, valueDimension.DimensionExternalKey, EavContext.GetTypedValue(valueModel));
					}
					
				}
			}

			return result;
		}
	}
}