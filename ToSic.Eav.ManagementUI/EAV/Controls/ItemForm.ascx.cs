using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using ToSic.Eav.Data;
using ToSic.Eav.Persistence;

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
		private int _repositoryId;
		private int[] DimensionIds
		{
			get { return Forms.GetDimensionIds(DefaultCultureDimension); }
		}
		private bool MasterRecord
		{
			get { return DefaultCultureDimension == null || DimensionIds.Contains(DefaultCultureDimension.Value); }
		}
		protected EavContext Db;
	    protected DbShortcuts DbS;
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
		public Guid? KeyGuid { get; set; }
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
		public string NewItemUrl { get; set; }
		public bool IsPublished
		{
			get { return rblPublished.SelectedValue == "Published"; }
			set { rblPublished.SelectedIndex = value ? 0 : 1; }
		}

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
		    DbS = new DbShortcuts(Db);
			Db.UserName = System.Web.HttpContext.Current.User.Identity.Name;

			SetJsonGeneralData();

			_viewMode = mode;
			switch (mode)
			{
				case FormViewMode.Insert:
					AddFormControls(AttributeSetId);

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
					var entity = DbS.GetEntity(EntityId);
					var entityModel = new DbLoadForCaching(Db).GetEntityModel(EntityId);
					AttributeSetId = entity.AttributeSetID;
					AddFormControls(entity, entityModel, Db.ZoneId, Db.AppId);
					break;
				case FormViewMode.ReadOnly:
					throw new NotImplementedException();
			}

			hlkShowHistory.Visible = mode == FormViewMode.Edit;
			_initFormCompleted = true;
		}

		/// <summary>
		/// Ad Form Controls for Edit-Mode
		/// </summary>
		/// <param name="entity">Entity containing the Attributes/Fields</param>
		/// <param name="entityModel">Entity Model</param>
		/// <param name="zoneId">ZoneId of the Entity</param>
		/// <param name="appId">AppId of the Entity</param>
		private void AddFormControls(Entity entity, IEntity entityModel, int zoneId, int appId)
		{
			// Load draft instead (if any)
			if (entityModel.GetDraft() != null)
			{
				entityModel = entityModel.GetDraft();
				entity = DbS.GetEntity(entityModel.RepositoryId);
			}
			_repositoryId = entityModel.RepositoryId;
			#region Set JSON Data/Models
			litJsonEntityModel.Text = GetJsonString(new Serialization.EntityJavaScriptModel(entityModel));
			litJsonEntityId.Text = entity.EntityID.ToString(CultureInfo.InvariantCulture);
			pnlEditForm.Attributes["data-entityid"] = entity.EntityID.ToString(CultureInfo.InvariantCulture);
			#endregion

			// Resolve ZoneId & AppId of the MetaData. If this AttributeSet uses configuration of another AttributeSet, use MetaData-ZoneId & -AppId
			var metaDataAppId = entity.Set.UsesConfigurationOfAttributeSet.HasValue ? DataSource.MetaDataAppId : appId;
			var metaDataZoneId = entity.Set.UsesConfigurationOfAttributeSet.HasValue ? DataSource.DefaultZoneId : zoneId;
			var entityReadOnly = entityModel.GetDraft() != null;

			foreach (var attribute in DbS.GetAttributes(entity.AttributeSetID))
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
				fieldTemplate.MetaData = new Metadata().GetAttributeMetaData(attribute.AttributeID, metaDataZoneId, metaDataAppId);
				fieldTemplate.MasterRecord = MasterRecord;
				fieldTemplate.AppId = AppId;
				fieldTemplate.ZoneId = ZoneId;
				fieldTemplate.DimensionIds = DimensionIds;
				if (entityReadOnly)
					fieldTemplate.Enabled = false;
				#endregion

				// Only add if VisibleInEditUI != false or AssignmentObjectType == Field Properties
				if (!(fieldTemplate.MetaData.ContainsKey("VisibleInEditUI") && ((IAttribute<bool?>)fieldTemplate.MetaData["VisibleInEditUI"]).Typed[DimensionIds] == false) || entity.AssignmentObjectTypeID == DataSource.AssignmentObjectTypeIdFieldProperties)
					phFields.Controls.Add(fieldTemplate);
			}

			IsPublished = entityModel.IsPublished;
		}

		private void SetJsonGeneralData()
		{
			litJsonDimensionsModel.Text = GetJsonString(Db.GetLanguages().Select(l => new { DimensionId = l.DimensionID, l.ExternalKey, l.Name }));
			pnlEditForm.Attributes["data-defaultculturedimension"] = DefaultCultureDimension.ToString();
			pnlEditForm.Attributes["data-activeculturedimension"] = DimensionIds.SingleOrDefault().ToString();
			pnlEditForm.Attributes["data-applicationpath"] = Request.ApplicationPath;
			pnlEditForm.Attributes["data-newdialogurl"] = (IsDialog ? "~/Eav/Dialogs/NewItem.aspx?AttributeSetId=[AttributeSetId]" : NewItemUrl);
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
			var attributeSet = DbS.GetAttributeSet(attributeSetId);

			// Resolve ZoneId & AppId of the MetaData. If this AttributeSet uses configuration of another AttributeSet, use MetaData-ZoneId & -AppId
			var metaDataAppId = attributeSet.UsesConfigurationOfAttributeSet.HasValue ? DataSource.MetaDataAppId : Db.AppId;
			var metaDataZoneId = attributeSet.UsesConfigurationOfAttributeSet.HasValue ? DataSource.DefaultZoneId : Db.ZoneId;

			foreach (var attribute in DbS.GetAttributes(attributeSetId))
			{
				var fieldTemplate = Forms.GetFieldTemplate(this, FieldTemplatesPath, attribute.Type);

				#region Set Values and other Meta data
				fieldTemplate.Attribute = attribute;
				fieldTemplate.MetaData = new Metadata().GetAttributeMetaData(attribute.AttributeID, metaDataZoneId, metaDataAppId);
				fieldTemplate.EnableViewState = true;
				fieldTemplate.Enabled = true;
				fieldTemplate.MasterRecord = true;
				fieldTemplate.AppId = AppId;
				fieldTemplate.ZoneId = ZoneId;
				#endregion

				if (!(fieldTemplate.MetaData.ContainsKey("VisibleInEditUI") && ((IAttribute<bool?>)fieldTemplate.MetaData["VisibleInEditUI"]).Typed[DimensionIds] == false))
					phFields.Controls.Add(fieldTemplate);
			}

			IsPublished = true;
		}

		#endregion

		#region Event Handlers for Button-Clicks

		protected void btnSave_Click(object sender, EventArgs e)
		{
			Save();
		}

		protected void btnCancel_Click(object sender, EventArgs e)
		{
			Cancel();
		}

		#endregion

		private void RedirectToListItems()
		{
			if (!PreventRedirect)
				Response.Redirect((IsDialog ? "Items.aspx?AttributeSetId=[AttributeSetId]" : ReturnUrl).Replace("[AttributeSetId]", AttributeSetId.ToString()), false);
		}

		protected string GetHistoryUrl()
		{
			return (IsDialog ? "ItemHistory.aspx?EntityId=[EntityId]" : ItemHistoryUrl).Replace("[EntityId]", EntityId.ToString());
		}

		public void Cancel()
		{
			RedirectToListItems();

			if (Canceled != null)
				Canceled(this, null);

			hfLastAction.Value = "Cancelled";
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

			hfLastAction.Value = "Saved";
		}

		private void Insert()
		{
			// Cancel insert if current language is not default language
			if (DefaultCultureDimension.HasValue && !DimensionIds.Contains(DefaultCultureDimension.Value))
				return;

			var values = new Dictionary<string, ValueViewModel>();

			// Extract Values
			foreach (var fieldTemplate in phFields.Controls.OfType<FieldTemplateUserControl>())
				fieldTemplate.ExtractValues(values);

			// Prepare DimensionIds
			var dimensionIds = new List<int>();
			if (DefaultCultureDimension.HasValue)
				dimensionIds.Add(DefaultCultureDimension.Value);

			Entity result;
			var assignmentObjectTypeId = AssignmentObjectTypeId.HasValue ? AssignmentObjectTypeId.Value : EavContext.DefaultAssignmentObjectTypeId;
			if (!KeyGuid.HasValue)
				result = Db.AddEntity(AttributeSetId, values, null, KeyNumber, assignmentObjectTypeId, dimensionIds: dimensionIds, isPublished: IsPublished);
			else
				result = Db.AddEntity(AttributeSetId, values, null, KeyGuid.Value, assignmentObjectTypeId, dimensionIds: dimensionIds, isPublished: IsPublished);

			RedirectToListItems();

			if (Inserted != null)
				Inserted(result);

			if (Saved != null)
				Saved(result);
		}

		private void Update()
		{
			var values = new Dictionary<string, ValueViewModel>();

			#region Extract Values (only of enabled fields)
			foreach (var fieldTemplate in phFields.Controls.OfType<FieldTemplateUserControl>().Where(f => f.Enabled))
			{
				// if not master and not translated, don't pass/extract this value
				if (!MasterRecord && fieldTemplate.ValueId == null && fieldTemplate.ReadOnly)
					continue;

				fieldTemplate.ExtractValues(values);
			}
			#endregion

			var result = Db.UpdateEntity(_repositoryId, values, dimensionIds: DimensionIds, masterRecord: MasterRecord, isPublished: IsPublished);

			RedirectToListItems();

			if (Updated != null)
				Updated(result);

			if (Saved != null)
				Saved(result);
		}
	}
}