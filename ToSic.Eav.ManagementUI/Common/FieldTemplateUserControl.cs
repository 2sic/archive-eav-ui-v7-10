using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;
using ToSic.Eav.Data;
using ToSic.Eav.ImportExport;

namespace ToSic.Eav.ManagementUI
{
	public abstract class FieldTemplateUserControl : UserControl
	{
		#region Private Fields
		private readonly HiddenField _hfValueId = new HiddenField { ID = "hfValueId" };
		private readonly HiddenField _hfReadOnly = new HiddenField { ID = "hfReadOnly", Value = "false" };
		private readonly Literal _wrapperStart = new Literal();
		private readonly Literal _wrapperEnd = new Literal { Text = "</div>" };
		#endregion

		#region Properties
		public object FieldValue { get; set; }
		public string FieldValueEditString { get; set; }
		public Attribute Attribute { get; set; }
		public bool ShowDataControlOnly { get; set; }
		public Dictionary<string, IAttribute> MetaData { get; set; }
		public bool MasterRecord { get; set; }
		public bool Enabled { get; set; }
		public bool ReadOnly
		{
			get { return bool.Parse(_hfReadOnly.Value); }
			set { _hfReadOnly.Value = value.ToString().ToLower(); }
		}
		public int? ValueId
		{
			get
			{
				int i;
				return int.TryParse(_hfValueId.Value, out i) ? i : (int?)null;
			}
			set { _hfValueId.Value = value.ToString(); }
		}
		public virtual Control DataControl { get; set; }
		public int[] DimensionIds { get; set; }
		public int? AppId { get; set; }
		public int? ZoneId { get; set; }
		#endregion

		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);

			// Append EAVField wrapper div and hidden field for Enabled / Disabled state
			Controls.AddAt(0, _wrapperStart);
			Controls.AddAt(1, _hfValueId);
			Controls.AddAt(2, _hfReadOnly);
			Controls.Add(_wrapperEnd);
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			// Set EAVField wrapper div and hidden field for Enabled / Disabled state
			//var allowMultiValue = GetMetaDataValue<bool?>("AllowMultiValue");
			var addressMask = GetMetaDataValue<string>("AddressMask");
			var wrapperStartText = "<div class='eav-field dnnFormItem' data-staticname='" + Attribute.StaticName + "' data-enabled='" + Enabled.ToString().ToLower() +
							   "' data-ismasterrecord='" + MasterRecord.ToString().ToLower() + "' data-fieldtype='" + Attribute.Type + "' data-fieldsubtype='" + GetMetaDataValue<string>("InputType") + "'" +
							   //(allowMultiValue != null ? " data-allowmultivalue='" + allowMultiValue.Value.ToString().ToLower() + "' " : "") +
							   (addressMask != null ? " data-addressmask='" + addressMask + "' " : "") + ">";
			_wrapperStart.Text = wrapperStartText;

			_hfReadOnly.Value = ReadOnly.ToString().ToLower();
		}

		public void ExtractValues(IDictionary<string, ValueToImport> dictionary)
		{
			dictionary[Attribute.StaticName] = new ValueToImport { Value = Value, ValueId = ValueId, ReadOnly = ReadOnly };
		}

		public abstract object Value { get; }

		protected T GetMetaDataValue<T>(string keyName)
		{
			return GetMetaDataValue(keyName, default(T));
		}

		protected T GetMetaDataValue<T>(string keyName, T defaultValue)
		{
			if (MetaData.ContainsKey(keyName) && MetaData[keyName].Values != null)
			{
				var value = MetaData[keyName][DimensionIds];

				if (value != null)
				{
					if (typeof(T).IsEnum) // handle/parse Enum
					{
						// Source: http://stackoverflow.com/questions/1082532/how-to-tryparse-for-enum-value
						if (!Enum.IsDefined(typeof(T), value.ToString()))
							return defaultValue;

						return (T)Enum.Parse(typeof(T), value.ToString());
					}

					return (T)value;
				}
			}

			return defaultValue;
		}
	}
}