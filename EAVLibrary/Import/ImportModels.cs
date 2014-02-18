using System;
using System.Collections.Generic;
using System.Diagnostics;

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
		public Dictionary<string, List<IValueImportModel>> Values { get; set; }
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