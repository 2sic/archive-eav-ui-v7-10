using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace ToSic.Eav.Import
{
	public class ImportAttributeSet
	{
		public string Name { get; set; }
		public string StaticName { get; set; }
		public string Description { get; set; }
		public string Scope { get; set; }
		public List<ImportAttribute> Attributes { get; set; }	// The List<> class does guarantee ordering
		public ImportAttribute TitleAttribute { get; set; }
		public bool AlwaysShareConfiguration { get; set; }

		public ImportAttributeSet() { }

		public ImportAttributeSet(string name, string staticName, string description, string scope, List<ImportAttribute> attributes, bool alwaysShareConfiguration = false)
		{
			Name = name;
			StaticName = staticName;
			Description = description;
			Scope = scope;
			Attributes = attributes;
			AlwaysShareConfiguration = alwaysShareConfiguration;
		}

		/// <summary>
		/// Shortcut go get a new AttributeSet with Scope=System and Name=StaticName
		/// </summary>
		public static ImportAttributeSet SystemAttributeSet(string staticName, string description, List<ImportAttribute> attributes, bool alwaysShareConfiguration = false)
		{
			return new ImportAttributeSet(staticName, staticName, description, "System", attributes, alwaysShareConfiguration);
		}
	}

	public class ImportAttribute
	{
		public string StaticName { get; set; }
		public string Type { get; set; }
		public List<ImportEntity> AttributeMetaData { get; set; }

		/// <summary>
		/// Default Constructor
		/// </summary>
		public ImportAttribute() { }

		/// <summary>
		/// Get an Import-Attribute
		/// </summary>
		private ImportAttribute(string staticName, string name, AttributeTypeEnum type, string notes, bool? visibleInEditUi, object defaultValue)
		{
			StaticName = staticName;
			Type = type.ToString();
			AttributeMetaData = new List<ImportEntity> { GetAttributeMetaData(name, notes, visibleInEditUi, EavContext.SerializeValue(defaultValue)) };
		}

		/// <summary>
		/// Get an Import-Attribute
		/// </summary>
		public static ImportAttribute StringAttribute(string staticName, string name, string notes, bool? visibleInEditUi, string inputType = null, int? rowCount = null, string defaultValue = null)
		{
			var attribute = new ImportAttribute(staticName, name, AttributeTypeEnum.String, notes, visibleInEditUi, defaultValue);
			attribute.AttributeMetaData.Add(GetStringAttributeMetaData(inputType, rowCount));
			return attribute;
		}

		/// <summary>
		/// Get an Import-Attribute
		/// </summary>
		public static ImportAttribute BooleanAttribute(string staticName, string name, string notes, bool? visibleInEditUi, bool? defaultValue = null)
		{
			var attribute = new ImportAttribute(staticName, name, AttributeTypeEnum.Boolean, notes, visibleInEditUi, defaultValue);
			return attribute;
		}

		/// <summary>
		/// Shortcut to get an @All Entity Describing an Attribute
		/// </summary>
		private static ImportEntity GetAttributeMetaData(string name, string notes, bool? visibleInEditUi, string defaultValue = null)
		{
			var allEntity = new ImportEntity
			{
				AttributeSetStaticName = "@All",
				Values = new Dictionary<string, List<IValueImportModel>>()
			};
			if (!string.IsNullOrEmpty(name))
				allEntity.Values.Add("Name", new List<IValueImportModel> { new ValueImportModel<string>(allEntity) { Value = name } });
			if (!string.IsNullOrEmpty(notes))
				allEntity.Values.Add("Notes", new List<IValueImportModel> { new ValueImportModel<string>(allEntity) { Value = notes } });
			if (visibleInEditUi.HasValue)
				allEntity.Values.Add("VisibleInEditUI", new List<IValueImportModel> { new ValueImportModel<bool?>(allEntity) { Value = visibleInEditUi } });
			if (defaultValue != null)
				allEntity.Values.Add("DefaultValue", new List<IValueImportModel> { new ValueImportModel<string>(allEntity) { Value = defaultValue } });

			return allEntity;
		}

		private static ImportEntity GetStringAttributeMetaData(string inputType, int? rowCount)
		{
			var stringEntity = new ImportEntity
			{
				AttributeSetStaticName = "@String",
				Values = new Dictionary<string, List<IValueImportModel>>()
			};
			if (!string.IsNullOrEmpty(inputType))
				stringEntity.Values.Add("InputType", new List<IValueImportModel> { new ValueImportModel<string>(stringEntity) { Value = inputType } });
			if (rowCount.HasValue)
				stringEntity.Values.Add("RowCount", new List<IValueImportModel> { new ValueImportModel<decimal?>(stringEntity) { Value = rowCount } });

			return stringEntity;
		}
	}

	public class ImportEntity
	{
		public string AttributeSetStaticName { get; set; }
		public int? KeyNumber { get; set; }
		public Guid? KeyGuid { get; set; }
		public string KeyString { get; set; }
		public int AssignmentObjectTypeId { get; set; }
		public Guid? EntityGuid { get; set; }
		public bool IsPublished { get; set; }
		public Dictionary<string, List<IValueImportModel>> Values { get; set; }

		public ImportEntity()
		{
			IsPublished = true;
		}
	}

	public class ValueImportModel<T> : IValueImportModel
	{
		public T Value { get; set; }
		public IEnumerable<ValueDimension> ValueDimensions { get; set; }
		public ImportEntity ParentEntity { get; private set; }

		public ValueImportModel(ImportEntity parentEntity)
		{
			ParentEntity = parentEntity;
		}

	    public string StringValueForTesting 
	    {
	        get { return Value.ToString(); }
	    }
	}

	internal static class ValueImportModel
	{   
		internal static IValueImportModel GetModel(string value, string type, IEnumerable<ValueDimension> dimensions, ImportEntity importEntity)
		{
			IValueImportModel valueModel;

			switch (type)
			{
				case "String":
				case "Hyperlink":
					valueModel = new ValueImportModel<string>(importEntity) { Value = value };
					break;
				case "Number":
					decimal typedDecimal;
					var isDecimal = Decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out typedDecimal);
					decimal? typedDecimalNullable = null;
					if (isDecimal)
						typedDecimalNullable = typedDecimal;
					valueModel = new ValueImportModel<decimal?>(importEntity)
					{
						Value = typedDecimalNullable
					};
					break;
				case "Entity":
					var entityGuids = !String.IsNullOrEmpty(value) ? value.Split(',').Select(v =>
					{
						var guid = Guid.Parse(v);
						return guid == Guid.Empty ? new Guid?() : guid;
					}).ToList() : new List<Guid?>(0);
					valueModel = new ValueImportModel<List<Guid?>>(importEntity) { Value = entityGuids };
					break;
				case "DateTime":
					DateTime typedDateTime;
					valueModel = new ValueImportModel<DateTime?>(importEntity)
					{
						Value = DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out typedDateTime) ? typedDateTime : new DateTime?()
					};
					break;
				case "Boolean":
					bool typedBoolean;
					valueModel = new ValueImportModel<bool?>(importEntity)
					{
						Value = Boolean.TryParse(value, out typedBoolean) ? typedBoolean : new bool?()
					};
					break;
				default:
					throw new ArgumentOutOfRangeException(type, value, "Unknown type argument found in import XML.");
			}

			valueModel.ValueDimensions = dimensions;

			return valueModel;
		}
	}

	public interface IValueImportModel
	{
		IEnumerable<ValueDimension> ValueDimensions { get; set; }
		ImportEntity ParentEntity { get; }

        String StringValueForTesting { get; }
	}

	public class ValueDimension
	{
		public string DimensionExternalKey { get; set; }
		public bool ReadOnly { get; set; }
	}

	public class LogItem
	{
		public EventLogEntryType EntryType { get; private set; }
		public ImportEntity ImportEntity { get; set; }
		public ImportAttributeSet ImportAttributeSet { get; set; }
		public ImportAttribute ImportAttribute { get; set; }
		public IValueImportModel Value { get; set; }
		public Exception Exception { get; set; }
		public string Message { get; private set; }

		public LogItem(EventLogEntryType entryType, string message)
		{
			EntryType = entryType;
			Message = message;
		}
	}
}