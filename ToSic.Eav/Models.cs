using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Script.Serialization;
using ToSic.Eav.DataSources;

namespace ToSic.Eav
{
	/// <summary>
	/// Represents an Entity
	/// </summary>
	public class EntityModel : IEntity
	{
		public int EntityId { get; internal set; }
		public int RepositoryId { get; internal set; }
		public Guid EntityGuid { get; internal set; }
		public IAttribute Title { get; internal set; }
		public Dictionary<string, IAttribute> Attributes { get; internal set; }
		public IContentType Type { get; internal set; }
		public DateTime Modified { get; internal set; }
		[ScriptIgnore]
		public RelationshipManager Relationships { get; internal set; }
		public bool IsPublished { get; internal set; }
		public int AssignmentObjectTypeId { get; internal set; }
		internal IEntity DraftEntity { get; set; }
		internal IEntity PublishedEntity { get; set; }

		public IAttribute this[string attributeName]
		{
			get { return (Attributes.ContainsKey(attributeName)) ? Attributes[attributeName] : null; }
		}

		/// <summary>
		/// Create a new EntityModel. Used to create InMemory Entities that are not persisted to the EAV SqlStore.
		/// </summary>
		public EntityModel(int entityId, string contentTypeName, IDictionary<string, object> values, string titleAttribute)
		{
			EntityId = entityId;
			Type = new ContentType(contentTypeName);
			Attributes = AttributeModel.GetTypedDictionaryForSingleLanguage(values, titleAttribute);
			try
			{
				Title = Attributes[titleAttribute];
			}
			catch (KeyNotFoundException)
			{
				throw new KeyNotFoundException(string.Format("The Title Attribute with Name \"{0}\" doesn't exist in the Entity-Attributes.", titleAttribute));
			}
			AssignmentObjectTypeId = EavContext.DefaultAssignmentObjectTypeId;
			IsPublished = true;
			Relationships = new RelationshipManager(this, new EntityRelationshipItem[0]);
		}

		/// <summary>
		/// Create a new EntityModel
		/// </summary>
		internal EntityModel(Guid entityGuid, int entityId, int repositoryId, int assignmentObjectTypeId, IContentType type, bool isPublished, IEnumerable<EntityRelationshipItem> allRelationships, DateTime modified)
		{
			EntityId = entityId;
			EntityGuid = entityGuid;
			AssignmentObjectTypeId = assignmentObjectTypeId;
			Attributes = new Dictionary<string, IAttribute>();
			Type = type;
			IsPublished = isPublished;
			RepositoryId = repositoryId;
			Modified = modified;

			if (allRelationships == null)
				allRelationships = new List<EntityRelationshipItem>();
			Relationships = new RelationshipManager(this, allRelationships);
		}

		/// <summary>
		/// Create a new EntityModel based on an Entity and Attributes
		/// </summary>
		internal EntityModel(IEntity entity, Dictionary<string, IAttribute> attributes, IEnumerable<EntityRelationshipItem> allRelationships)
		{
			EntityId = entity.EntityId;
			EntityGuid = entity.EntityGuid;
			AssignmentObjectTypeId = entity.AssignmentObjectTypeId;
			Type = entity.Type;
			Title = entity.Title;
			IsPublished = entity.IsPublished;
			Attributes = attributes;
			RepositoryId = entity.RepositoryId;
			Relationships = new RelationshipManager(this, allRelationships);
		}

		public IEntity GetDraft()
		{
			return DraftEntity;
		}

		public IEntity GetPublished()
		{
			return PublishedEntity;
		}
	}

	/// <summary>
	/// Represents an Attribute with Values of a Generic Type
	/// </summary>
	/// <typeparam name="ValueType">Type of the Value</typeparam>
	public class AttributeBaseModel : IAttributeBase
	{
		public string Name { get; set; }
		public string Type { get; set; }
		public bool IsTitle { get; set; }

		internal AttributeBaseModel(string name, string type, bool isTitle)
		{
			Name = name;
			Type = type;
			IsTitle = isTitle;
		}
	}

	/// <summary>
	/// Extended Attribute Model for internal Cache
	/// </summary>
	internal class AttributeDefinition : AttributeBaseModel
	{
		public int AttributeId { get; set; }

		public AttributeDefinition(string name, string type, bool isTitle, int attributeId)
			: base(name, type, isTitle)
		{
			AttributeId = attributeId;
		}
	}


	internal class AttributeModel
	{
		private static readonly ValueModel<EntityRelationshipModel> EntityRelationshipModelDefaultValue = new ValueModel<EntityRelationshipModel>(new EntityRelationshipModel(null)) { Languages = new DimensionModel[0] };

		/// <summary>
		/// Convert a NameValueCollection-Like List to a Dictionary of IAttributes
		/// </summary>
		internal static Dictionary<string, IAttribute> GetTypedDictionaryForSingleLanguage(IDictionary<string, object> attributes, string titleAttributeName)
		{
			var result = new Dictionary<string, IAttribute>();

			foreach (var attribute in attributes)
			{
				var attributeType = GetAttributeTypeName(attribute.Value);
				var baseModel = new AttributeBaseModel(attribute.Key, attributeType, attribute.Key == titleAttributeName);
				var attributeModel = GetAttributeManagementModel(baseModel);
				var valuesModelList = new List<IValue>();
				if (attribute.Value != null)
				{
					var valueModel = ValueModel.GetValueModel(baseModel.Type, attribute.Value.ToString());
					valuesModelList.Add(valueModel);
				}

				attributeModel.Values = valuesModelList;

				result[attribute.Key] = attributeModel;
			}

			return result;
		}

		/// <summary>
		/// Get EAV AttributeType for a value, like String, Number, DateTime or Boolean
		/// </summary>
		static string GetAttributeTypeName(object value)
		{
			if (value is DateTime)
				return "DateTime";
			if (value is decimal || value is int || value is double)
				return "Number";
			if (value is bool)
				return "Boolean";
			return "String";
		}

		/// <summary>
		/// Get AttributeModel for specified Typ
		/// </summary>
		/// <returns><see cref="AttributeModel{T}"/></returns>
		internal static IAttributeManagement GetAttributeManagementModel(AttributeBaseModel definition)
		{
			switch (definition.Type)
			{
				case "Boolean":
					return new AttributeModel<bool?>(definition.Name, definition.Type, definition.IsTitle);
				case "DateTime":
					return new AttributeModel<DateTime?>(definition.Name, definition.Type, definition.IsTitle);
				case "Number":
					return new AttributeModel<decimal?>(definition.Name, definition.Type, definition.IsTitle);
				case "Entity":
					return new AttributeModel<EntityRelationshipModel>(definition.Name, definition.Type, definition.IsTitle) { Values = new IValue[] { EntityRelationshipModelDefaultValue } };
				default:
					return new AttributeModel<string>(definition.Name, definition.Type, definition.IsTitle);
			}
		}
	}

	/// <summary>
	/// Represents an Attribute with Values of a Generic Type
	/// </summary>
	/// <typeparam name="ValueType">Type of the Value</typeparam>
	public class AttributeModel<ValueType> : AttributeBaseModel, IAttribute<ValueType>, IAttributeManagement
	{
		public AttributeModel(string name, string type, bool isTitle) : base(name, type, isTitle) { }

		public IEnumerable<IValue> Values { get; set; }
		public IValueManagement DefaultValue { get; set; }

		public ValueType TypedContents
		{
			get
			{
				try
				{
					var value = (IValue<ValueType>)Values.FirstOrDefault();
					return value != null ? value.TypedContents : default(ValueType);
				}
				catch
				{
					return default(ValueType);
				}
			}
		}

		public ITypedValue<ValueType> Typed
		{
			get { return new TypedValue<ValueType>(Values, TypedContents); }
		}

		public object this[int languageId]
		{
			get { return this[new[] { languageId }]; }
		}

		public object this[int[] languageIds]
		{
			get
			{
				// Value with Dimensions specified
				if (languageIds != null && languageIds.Length > 0 && Values != null)
				{
					// try match all specified Dimensions
					var valueHavingSpecifiedLanguages = Values.FirstOrDefault(va => languageIds.All(di => va.Languages.Select(d => d.DimensionId).Contains(di)));
					if (valueHavingSpecifiedLanguages != null)
					{
						try
						{
							return ((IValue<ValueType>)valueHavingSpecifiedLanguages).TypedContents;
						}
						catch (InvalidCastException) { } // may occour for nullable types
					}
				}
				// use Default
				return TypedContents == null ? default(ValueType) : TypedContents;
			}
		}

		public object this[string languageKey]
		{
			get { return this[new[] { languageKey }]; }
		}

		public object this[string[] languageKeys]
		{
			get
			{
				// Value with Dimensions specified
				if (languageKeys != null && languageKeys.Length > 0 && Values != null)
				{
					// try match all specified Dimensions
					var valueHavingSpecifiedLanguages = Values.FirstOrDefault(va => languageKeys.All(vk => va.Languages.Select(d => d.Key).Contains(vk.ToLower())));
					if (valueHavingSpecifiedLanguages != null)
					{
						try
						{
							return ((IValue<ValueType>)valueHavingSpecifiedLanguages).TypedContents;
						}
						catch (InvalidCastException) { }	// may occour for nullable types
					}
				}
				// use Default
				return TypedContents == null ? default(ValueType) : TypedContents;
			}
		}
	}

	/// <summary>
	/// Represents Relationships to Child Entities
	/// </summary>
	public class EntityRelationshipModel : IEnumerable<IEntity>
	{
		private static readonly int[] EntityIdsEmpty = new int[0];
		/// <summary>
		/// List of Child EntityIds
		/// </summary>
		public IEnumerable<int> EntityIds { get; internal set; }

		private readonly IDataSource _source;
		//private EntityEnum _entityEnum;
		private List<IEntity> _entities;

		/// <summary>
		/// Initializes a new instance of the EntityRelationshipModel class.
		/// </summary>
		/// <param name="source">DataSource to retrieve child entities</param>
		public EntityRelationshipModel(IDataSource source)
		{
			_source = source;
			EntityIds = EntityIdsEmpty;
		}

		public override string ToString()
		{
			return EntityIds == null ? string.Empty : string.Join(", ", EntityIds.Select(e => e));
		}

		public IEnumerator<IEntity> GetEnumerator()
		{
		    if (_entities == null)
		        //_entities = _source == null ? new List<IEntity>() : _source.Out[DataSource.DefaultStreamName].List.Where(l => EntityIds.Contains(l.Key)).Select(l => l.Value).ToList();
		        _entities = _source == null ? new List<IEntity>() : EntityIds.Select(l => _source.Out[DataSource.DefaultStreamName].List[l]).ToList();

			return new EntityEnum(_entities);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		/// <remarks>Source: http://msdn.microsoft.com/en-us/library/system.collections.ienumerable.getenumerator.aspx </remarks>
		class EntityEnum : IEnumerator<IEntity>
		{
			private readonly List<IEntity> _entities;
			private int _position = -1;

			public EntityEnum(List<IEntity> entities)
			{
				_entities = entities;
			}

			public void Dispose() { }

			public bool MoveNext()
			{
				_position++;
				return (_position < _entities.Count);
			}

			public void Reset()
			{
				_position = -1;
			}

			public IEntity Current
			{
				get
				{
					try
					{
						return _entities[_position];
					}
					catch (IndexOutOfRangeException)
					{
						throw new InvalidOperationException();
					}
				}
			}

			object IEnumerator.Current
			{
				get { return Current; }
			}
		}
	}

	/// <summary>
	/// Represents a Content Type
	/// </summary>
	public class ContentType : IContentType
	{
		public string Name { get; set; }
		public string StaticName { get; set; }
		/// <summary>
		/// Dictionary with all Attribute Definitions
		/// </summary>
		internal IDictionary<int, AttributeDefinition> AttributeDefinitions { get; set; }

		/// <summary>
		/// Initializes a new instance of the ContentType class.
		/// </summary>
		public ContentType(string name, string staticName = null)
		{
			Name = name;
			StaticName = staticName;
		}
	}

	/// <summary>
	/// MetaData of an Attribute
	/// </summary>
	public class AttributeWithMetaInfo : Attribute
	{
		/// <summary>
		/// Indicates whehter Attributes is Title of the AttributeSet
		/// </summary>
		public bool IsTitle { get; internal set; }
		/// <summary>
		/// Attribute name
		/// </summary>
		public string Name { get; internal set; }
		/// <summary>
		/// Attribute Notes
		/// </summary>
		public string Notes { get; internal set; }
		/// <summary>
		/// Indicates whether MetaData has items
		/// </summary>
		public bool HasTypeMetaData { get; internal set; }
		/// <summary>
		/// Dictionary with all MetaData belonging to the Attribute
		/// </summary>
		public IDictionary<string, IAttribute> MetaData { get; internal set; }
	}

	/// <summary>
	/// Typed Value
	/// </summary>
	public class TypedValue<T> : ITypedValue<T>
	{
		private readonly IEnumerable<IValue> _values;
		private readonly T _typedContents;

		/// <summary>
		/// Constructs a new TypedValue
		/// </summary>
		public TypedValue(IEnumerable<IValue> values, T typedContents)
		{
			_values = values;
			_typedContents = typedContents;
		}

		public T this[int languageId]
		{
			get { return this[new[] { languageId }]; }
		}

		public T this[int[] languageIds]
		{
			get
			{
				// Value with Dimensions specified
				if (languageIds != null && languageIds.Length > 0 && _values != null)
				{
					// try match all specified Dimensions
					var valueHavingSpecifiedLanguages = _values.FirstOrDefault(va => languageIds.All(di => va.Languages.Select(d => d.DimensionId).Contains(di)));
					if (valueHavingSpecifiedLanguages != null)
					{
						try
						{
							return ((IValue<T>)valueHavingSpecifiedLanguages).TypedContents;
						}
						catch (InvalidCastException) { }// may occour for nullable types
					}
				}

				// use Default
				return _typedContents == null ? default(T) : _typedContents;
			}
		}

		public T this[string languageKey]
		{
			get { return this[new[] { languageKey }]; }
		}

		public T this[string[] languageKeys]
		{
			get
			{
				// Value with Dimensions specified
				if (languageKeys != null && languageKeys.Length > 0 && _values != null)
				{
					// try match all specified Dimensions
					var valueHavingSpecifiedLanguages = _values.FirstOrDefault(va => languageKeys.All(lk => va.Languages.Select(d => d.Key).Contains(lk.ToLower())));
					if (valueHavingSpecifiedLanguages != null)
					{
						try
						{
							return ((IValue<T>)valueHavingSpecifiedLanguages).TypedContents;
						}
						catch (InvalidCastException) { }	// may occour for nullable types
					}
				}

				// use Default
				return _typedContents == null ? default(T) : _typedContents;
			}
		}
	}

	/// <summary>
	/// Represents a Value
	/// </summary>
	public class ValueModel
	{
		public int ValueId { get; set; }
		public IEnumerable<ILanguage> Languages { get; set; }
		public int ChangeLogIdCreated { get; set; }


		/// <summary>
		/// Creates a Typed Value Model
		/// </summary>
		internal static IValue GetValueModel(string attributeType, string value)
		{
			return GetValueModel(attributeType, (object)value, new DimensionModel[0], -1, -1);
		}
		/// <summary>
		/// Creates a Typed Value Model
		/// </summary>
		internal static IValue GetValueModel(string attributeType, string value, IEnumerable<ILanguage> languages, int valueID, int changeLogIDCreated)
		{
			return GetValueModel(attributeType, (object)value, languages, valueID, changeLogIDCreated);
		}

		/// <summary>
		/// Creates a Typed Value Model for an Entity-Attribute
		/// </summary>
		internal static IValue GetValueModel(string attributeType, IEnumerable<int> entityIds, IDataSource source)
		{
			return GetValueModel(attributeType, entityIds, new DimensionModel[0], -1, -1, source);
		}

		/// <summary>
		/// Creates a Typed Value Model
		/// </summary>
		private static IValue GetValueModel(string attributeType, object value, IEnumerable<ILanguage> languages, int valueID, int changeLogIDCreated, IDataSource source = null)
		{
			IValueManagement typedModel;
			var stringValue = value as string;
			try
			{
				switch (attributeType)
				{
					case "Boolean":
						typedModel = new ValueModel<bool?>(string.IsNullOrEmpty(stringValue) ? (bool?)null : bool.Parse(stringValue));
						break;
					case "DateTime":
						typedModel = new ValueModel<DateTime?>(string.IsNullOrEmpty(stringValue) ? (DateTime?)null : DateTime.Parse(stringValue));
						break;
					case "Number":
						typedModel = new ValueModel<decimal?>(string.IsNullOrEmpty(stringValue) ? (decimal?)null : decimal.Parse(stringValue, CultureInfo.InvariantCulture));
						break;
					case "Entity":
						var entityIds = value as IEnumerable<int>;
						typedModel = new ValueModel<EntityRelationshipModel>(new EntityRelationshipModel(source) { EntityIds = entityIds });
						break;
					default:
						typedModel = new ValueModel<string>(stringValue);
						break;
				}
			}
			catch
			{
				return new ValueModel<string>(stringValue);
			}

			typedModel.Languages = languages;
			typedModel.ValueId = valueID;
			typedModel.ChangeLogIdCreated = changeLogIDCreated;

			return (IValue)typedModel;
		}
	}

	/// <summary>
	/// Represents a Value
	/// </summary>
	/// <typeparam name="T">Type of the actual Value</typeparam>
	public class ValueModel<T> : ValueModel, IValue<T>, IValueManagement
	{
		public T TypedContents { get; internal set; }

		internal ValueModel(T typedContents)
		{
			TypedContents = typedContents;
		}
	}

	/// <summary>
	/// Represents a Value
	/// </summary>
	public class ValueViewModel
	{
		/// <summary>
		/// Gets or sets the Value
		/// </summary>
		public object Value { get; set; }
		/// <summary>
		/// Gets or sets the internal ValueId
		/// </summary>
		public int? ValueId { get; set; }
		/// <summary>
		/// Gets or sets whether the Value is read only (means shared from another Language)
		/// </summary>
		public bool ReadOnly { get; set; }
	}

	/// <summary>
	/// Represents a Dimension Assignment
	/// </summary>
	internal class DimensionModel : IDimension, ILanguage
	{
		public int DimensionId { get; set; }
		public bool ReadOnly { get; set; }
		public string Key { get; set; }
	}

	/// <summary>
	/// Represents a Zone
	/// </summary>
	public class ZoneModel
	{
		/// <summary>
		/// ZoneId
		/// </summary>
		public int ZoneId { get; internal set; }
		/// <summary>
		/// AppId of the default App in this Zone
		/// </summary>
		public int DefaultAppId { get; internal set; }
		/// <summary>
		/// All Apps in this Zone with Id and Name
		/// </summary>
		public Dictionary<int, string> Apps { get; internal set; }
	}

	#region ENUMs

	/// <summary>
	/// Attribute move direction (Up/Down)
	/// </summary>
	public enum AttributeMoveDirection
	{
		/// <summary>Move attribute Up</summary>
		Up,
		/// <summary>Move attribute down</summary>
		Down
	}

	/// <summary>
	/// Scope of an AttributeSet
	/// </summary>
	public enum AttributeScope
	{
		/// <summary>
		/// System Attribute
		/// </summary>
		System
	}

	#endregion
}
