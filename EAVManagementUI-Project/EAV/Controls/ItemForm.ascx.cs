using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ToSic.Eav.ManagementUI
{
	public partial class ItemForm : UserControl
	{
		#region Events
		public event EntityUpdatedEventHandler Inserted;
		public event EntityUpdatedEventHandler Updated;
		public event EntityUpdatedEventHandler Saved;
		public event EventHandler Canceled;
		#endregion

		#region Private and Protected Fields
		private int[] DimensionIds
		{
			get { return Forms.GetDimensionIds(DefaultCultureDimension); }
		}
		private bool MasterRecord
		{
			get { return DefaultCultureDimension == null || DimensionIds.Contains(DefaultCultureDimension.Value); }
		}
		protected EavContext Db;
		private string _fieldTemplatesPath;
		protected string FieldTemplatesPath
		{
			get
			{
				return _fieldTemplatesPath ??
					   (_fieldTemplatesPath = TemplateControl.TemplateSourceDirectory + "/../FieldTemplates/{0}_Edit.ascx");
			}
		}
		private bool _initFormCompleted;
		private FormViewMode _viewMode;
		private bool _addClientScriptAndCss = true;
		#endregion

		#region Properties
		public int AttributeSetId { get; set; }
		public int EntityId { get; set; }
		public bool IsDialog { get; set; }
		public bool HideNavigationButtons { get; set; }
		public bool PreventRedirect { get; set; }
		public string ReturnUrl { get; set; }
		public int? KeyNumber { get; set; }
		public int? AssignmentObjectTypeId { get; set; }
		public int? DefaultCultureDimension { get; set; }
		public int? AppId { get; set; }
		public int? ZoneId { get; set; }
		public bool AddClientScriptAndCss
		{
			get { return _addClientScriptAndCss; }
			set { _addClientScriptAndCss = value; }
		}
		public string ItemHistoryUrl { get; set; }
		#endregion

		#region Init the Form

		protected void Page_Load(object sender, EventArgs e)
		{
			pnlNavigateBack.DataBind();
			pnlActions.DataBind();

			if (AddClientScriptAndCss)
				Forms.AddClientScriptAndCss(this);
		}

		public void InitForm(FormViewMode mode)
		{
			if (_initFormCompleted)
				throw new Exception("Form can be initialized only once!");

			Db = EavContext.Instance(ZoneId, AppId);
			Db.UserName = System.Web.HttpContext.Current.User.Identity.Name;

			SetJsonGeneralData();

			_viewMode = mode;
			switch (mode)
			{
				case FormViewMode.Insert:
					AddFormControls(AttributeSetId);
					btnUpdate.Visible = false;

					// If edited culture is not the default culture, show message that content must be edited in default culture first
					if (DefaultCultureDimension.HasValue && !DimensionIds.Contains(DefaultCultureDimension.Value))
					{
						phFields.Visible = false;

						// ToDo: Localization is not solved yet (doen't work the same in ASP.NET and DotNetNuke)
						//if (System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.ToUpper() == "DE")
						//    pnlEditDefaultFirstDE.Visible = true;
						//else
						pnlEditDefaultFirstEN.Visible = true;
					}


					break;
				case FormViewMode.Edit:
					var item = Db.GetEntity(EntityId);
					AttributeSetId = item.AttributeSetID;
					AddFormControls(item, Db.ZoneId, Db.AppId);
					btnInsert.Visible = false;
					break;
				case FormViewMode.ReadOnly:
					throw new NotImplementedException();
			}

			btnShowHistory.Visible = mode == FormViewMode.Edit;
			_initFormCompleted = true;
		}

		/// <summary>
		/// Ad Form Controls for Edit-Mode
		/// </summary>
		/// <param name="entity">Entity containing the Attributes/Fields</param>
		/// <param name="zoneId">ZoneId of the Entity</param>
		/// <param name="appId">AppId of the Entity</param>
		private void AddFormControls(Entity entity, int zoneId, int appId)
		{
			var dsrc = DataSource.GetInitialDataSource(zoneId, appId);
			var entityModel = dsrc[DataSource.DefaultStreamName].List[entity.EntityID];
			#region Set JSON Data/Models
			litJsonEntityModel.Text = GetJsonString(new Serialization.EntityJavaScriptModel(entityModel));
			litJsonEntityId.Text = entity.EntityID.ToString(CultureInfo.InvariantCulture);
			pnlEditForm.Attributes["data-entityid"] = entity.EntityID.ToString(CultureInfo.InvariantCulture);
			#endregion

			// Resolve ZoneId & AppId of the MetaData. If this AttributeSet uses configuration of another AttributeSet, use MetaData-ZoneId & -AppId
			var metaDataAppId = entity.Set.UsesConfigurationOfAttributeSet.HasValue ? DataSource.MetaDataAppId : appId;
			var metaDataZoneId = entity.Set.UsesConfigurationOfAttributeSet.HasValue ? DataSource.DefaultZoneId : zoneId;

			foreach (var attribute in Db.GetAttributes(entity.AttributeSetID))
			{
				var fieldTemplate = Forms.GetFieldTemplate(this, FieldTemplatesPath, attribute.Type);
				var attributeModel = entityModel[attribute.StaticName];

				#region Set Values and other Meta data
				fieldTemplate.Enabled = true;

				// Set Value (current-Value or Fallback-Value)
				if (attributeModel != null && attributeModel[DimensionIds] != null)
				{
					var currentValue = attributeModel[DimensionIds];
					fieldTemplate.FieldValue = currentValue;
					fieldTemplate.FieldValueEditString = currentValue.ToString();

					var valueHavingDimensions = attributeModel.Values.FirstOrDefault(a => DimensionIds.All(di => a.Languages.Select(d => d.DimensionId).Contains(di)));
					if (valueHavingDimensions != null)
					{
						fieldTemplate.ValueId = ((IValueManagement)valueHavingDimensions).ValueId;
						if (valueHavingDimensions.Languages.Any())
							fieldTemplate.ReadOnly = valueHavingDimensions.Languages.First(d => d.DimensionId == DimensionIds.First()).ReadOnly;
					}
					else if (!MasterRecord)
						fieldTemplate.ReadOnly = true;
				}
				// Set Disabled, if this is not the Master Record and Master Record has no DefaultValue
				else if (!MasterRecord)
				{
					fieldTemplate.Enabled = false;
				}

				fieldTemplate.EnableViewState = true;
				fieldTemplate.Attribute = attribute;
				fieldTemplate.MetaData = Db.GetAttributeMetaData(attribute.AttributeID, metaDataZoneId, metaDataAppId);
				fieldTemplate.MasterRecord = MasterRecord;
				fieldTemplate.AppId = AppId;
				fieldTemplate.ZoneId = ZoneId;
				fieldTemplate.DimensionIds = DimensionIds;

				#endregion

				// Only add if VisibleInEditUI != false or AssignmentObjectType == Field Properties
				if (!(fieldTemplate.MetaData.ContainsKey("VisibleInEditUI") && ((IAttribute<bool?>)fieldTemplate.MetaData["VisibleInEditUI"]).Typed[DimensionIds] == false) || entity.AssignmentObjectTypeID == DataSource.AssignmentObjectTypeIdFieldProperties)
					phFields.Controls.Add(fieldTemplate);
			}
		}

		private void SetJsonGeneralData()
		{
			litJsonDimensionsModel.Text = GetJsonString(Db.GetLanguages().Select(l => new { DimensionId = l.DimensionID, l.ExternalKey, l.Name }));
			pnlEditForm.Attributes["data-defaultculturedimension"] = DefaultCultureDimension.ToString();
			pnlEditForm.Attributes["data-activeculturedimension"] = DimensionIds.SingleOrDefault().ToString();
			pnlEditForm.Attributes["data-applicationpath"] = Request.ApplicationPath;
			// Set default values to ensure valid JSON/JavaScript
			litJsonEntityId.Text = "0";
			litJsonEntityModel.Text = "null";
		}

		private static string GetJsonString(object objectToSerialize)
		{
			var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
			return serializer.Serialize(objectToSerialize);
		}


		/// <summary>
		/// Add Form Controls for Insert-Mode
		/// </summary>
		private void AddFormControls(int attributeSetId)
		{
			var attributeSet = Db.GetAttributeSet(attributeSetId);

			// Resolve ZoneId & AppId of the MetaData. If this AttributeSet uses configuration of another AttributeSet, use MetaData-ZoneId & -AppId
			var metaDataAppId = attributeSet.UsesConfigurationOfAttributeSet.HasValue ? DataSource.MetaDataAppId : Db.AppId;
			var metaDataZoneId = attributeSet.UsesConfigurationOfAttributeSet.HasValue ? DataSource.DefaultZoneId : Db.ZoneId;

			foreach (var attribute in Db.GetAttributes(attributeSetId))
			{
				var fieldTemplate = Forms.GetFieldTemplate(this, FieldTemplatesPath, attribute.Type);

				#region Set Values and other Meta data
				fieldTemplate.Attribute = attribute;
				fieldTemplate.MetaData = Db.GetAttributeMetaData(attribute.AttributeID, metaDataZoneId, metaDataAppId);
				fieldTemplate.EnableViewState = true;
				fieldTemplate.Enabled = true;
				fieldTemplate.MasterRecord = true;
				fieldTemplate.AppId = AppId;
				fieldTemplate.ZoneId = ZoneId;
				#endregion

				if (!(fieldTemplate.MetaData.ContainsKey("VisibleInEditUI") && ((IAttribute<bool?>)fieldTemplate.MetaData["VisibleInEditUI"]).Typed[DimensionIds] == false))
					phFields.Controls.Add(fieldTemplate);
			}
		}
		#endregion

		#region Event Handlers for Button-Clicks

		protected void btnUpdate_Click(object sender, EventArgs e)
		{
			Update();
		}

		protected void btnInsert_Click(object sender, EventArgs e)
		{
			Insert();
		}

		protected void btnCancel_Click(object sender, EventArgs e)
		{
			Cancel();
		}

		protected void btnShowHistory_Click(object sender, EventArgs e)
		{
			ShowHistory();
		}

		#endregion

		private void RedirectToListItems()
		{
			if (!PreventRedirect)
				Response.Redirect((IsDialog ? "Items.aspx?AttributeSetId=[AttributeSetId]" : ReturnUrl).Replace("[AttributeSetId]", AttributeSetId.ToString()), false);
		}

		private void RedirectToHistory()
		{
			if (!PreventRedirect)
				Response.Redirect((IsDialog ? "ItemHistory.aspx?EntityId=[EntityId]" : ItemHistoryUrl).Replace("[EntityId]", EntityId.ToString()), false);
		}

		public void Cancel()
		{
			RedirectToListItems();

			if (Canceled != null)
				Canceled(this, null);
		}

		public void Save()
		{
			switch (_viewMode)
			{
				case FormViewMode.Edit:
					Update();
					break;
				case FormViewMode.Insert:
					Insert();
					break;
				default:
					throw new NotSupportedException();
			}
		}

		public void Insert()
		{
			// Cancel insert if current language is not default language
			if (DefaultCultureDimension.HasValue && !DimensionIds.Contains(DefaultCultureDimension.Value))
				return;

			var values = new Dictionary<string, ValueViewModel>();

			#region Extract Values
			foreach (var ChildControl in phFields.Controls)
			{
				if (ChildControl is FieldTemplateUserControl)
				{
					var FieldTemplate = ChildControl as FieldTemplateUserControl;
					FieldTemplate.ExtractValues(values);
				}
			}
			#endregion

			// Prepare DimensionIds
			var dimensionIds = new List<int>();
			if (DefaultCultureDimension.HasValue)
				dimensionIds.Add(DefaultCultureDimension.Value);

			Entity result;
			if (AssignmentObjectTypeId.HasValue)
				result = Db.AddEntity(AttributeSetId, values, null, KeyNumber, AssignmentObjectTypeId.Value, dimensionIds: dimensionIds);
			else
				result = Db.AddEntity(AttributeSetId, values, null, KeyNumber, dimensionIds: dimensionIds);

			RedirectToListItems();

			if (Inserted != null)
				Inserted(result);

			if (Saved != null)
				Saved(result);
		}

		public void Update()
		{
			var values = new Dictionary<string, ValueViewModel>();

			#region Extract Values (only of enabled fields)
			foreach (var FieldTemplate in phFields.Controls.OfType<FieldTemplateUserControl>().Where(f => f.Enabled))
			{
				// if not master and not translated, don't pass/extract this value
				if (!MasterRecord && FieldTemplate.ValueId == null && FieldTemplate.ReadOnly)
					continue;

				FieldTemplate.ExtractValues(values);
			}
			#endregion

			var result = Db.UpdateEntity(EntityId, values, dimensionIds: DimensionIds, masterRecord: MasterRecord);

			RedirectToListItems();

			if (Updated != null)
				Updated(result);

			if (Saved != null)
				Saved(result);
		}

		public void ShowHistory()
		{
			RedirectToHistory();
		}
	}
}