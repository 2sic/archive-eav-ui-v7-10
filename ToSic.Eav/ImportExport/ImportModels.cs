using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace ToSic.Eav.Import
{
	public class AttributeSet
	{
		public string Name { get; set; }
		public string StaticName { get; set; }
		public string Description { get; set; }
		public string Scope { get; set; }
		public List<Attribute> Attributes { get; set; }	// The List<> class does guarantee ordering
		public Attribute TitleAttribute { get; set; }
	}

	public class Attribute
	{
		public string StaticName { get; set; }
		public string Type { get; set; }
		public List<Entity> AttributeMetaData { get; set; }
	}

	public class Entity
	{
		public string AttributeSetStaticName { get; set; }
		public int? KeyNumber { get; set; }
		public int AssignmentObjectTypeId { get; set; }
		public Guid? EntityGuid { get; set; }
        public bool IsPublished { get; set; }
		public Dictionary<string, List<IValueImportModel>> Values { get; set; }

        public Entity()
        {
            IsPublished = true;
        }
	}

	public class ValueImportModel<T> : IValueImportModel
	{
		public T Value { get; set; }
		public IEnumerable<ValueDimension> ValueDimensions { get; set; }
		public Entity Entity { get; private set; }

		public ValueImportModel(Entity entity)
		{
			Entity = entity;
		}
	}

	internal static class ValueImportModel
	{
		internal static IValueImportModel GetModel(string value, string type, IEnumerable<ValueDimension> dimensions, Entity entity)
		{
			IValueImportModel valueModel;

			switch (type)
			{
				case "String":
				case "Hyperlink":
					valueModel = new ValueImportModel<string>(entity) { Value = value };
					break;
				case "Number":
					decimal typedDecimal;
					var isDecimal = Decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out typedDecimal);
					decimal? typedDecimalNullable = null;
					if (isDecimal)
						typedDecimalNullable = typedDecimal;
					valueModel = new ValueImportModel<decimal?>(entity)
					{
						Value = typedDecimalNullable
					};
					break;
				case "Entity":
					var entityGuids = !String.IsNullOrEmpty(value) ? value.Split(',').Select(Guid.Parse).ToList() : new List<Guid>(0);
					valueModel = new ValueImportModel<List<Guid>>(entity) { Value = entityGuids };
					break;
				case "DateTime":
					DateTime typedDateTime;
					valueModel = new ValueImportModel<DateTime?>(entity)
					{
						Value = DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out typedDateTime) ? typedDateTime : new DateTime?()
					};
					break;
				case "Boolean":
					bool typedBoolean;
					valueModel = new ValueImportModel<bool?>(entity)
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
		Entity Entity { get; }
	}

	public class ValueDimension
	{
		public string DimensionExternalKey { get; set; }
		public bool ReadOnly { get; set; }
	}

	public class LogItem
	{
		public EventLogEntryType EntryType { get; private set; }
		public Entity Entity { get; set; }
		public AttributeSet AttributeSet { get; set; }
		public Attribute Attribute { get; set; }
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