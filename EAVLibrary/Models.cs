using System;
using System.Collections;
using System.Collections.Generic;
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
		/// Create a new EntityModel
		/// </summary>
		internal EntityModel(Guid entityGuid, int entityId, int repositoryId, int assignmentObjectTypeId, IContentType type, bool isPublished, IEnumerable<EntityRelationshipItem> allRelationships = null)
		{
			EntityId = entityId;
			EntityGuid = entityGuid;
			AssignmentObjectTypeId = assignmentObjectTypeId;
			Attributes = new Dictionary<string, IAttribute>();
			Type = type;
			IsPublished = isPublished;
			RepositoryId = repositoryId;

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
	}

	/// <summary>
	/// Extended Attribute Model for internal Cache
	/// </summary>
	internal class AttributeDefinition : AttributeBaseModel
	{
		public int AttributeId { get; set; }
		public bool IsTitle { get; set; }
	}

	/// <summary>
	/// Represents an Attribute with Values of a Generic Type
	/// </summary>
	/// <typeparam name="ValueType">Type of the Value</typeparam>
	public class AttributeModel<ValueType> : AttributeBaseModel, IAttribute<ValueType>, IAttributeManagement
	{
		public bool IsTitle { get; set; }
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
				if (languageKeys != null && languageKeys.Length > 0)
				{
					// try match all specified Dimensions
					var valueHavingSpecifiedLanguages = Values.FirstOrDefault(va => languageKeys.All(vk => va.Languages.Select(d => d.Key).Contains(vk.ToLower())));
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
	}

	/// <summary>
	/// Represents Relationships to Child Entities
	/// </summary>
	public class EntityRelationshipModel : IEnumerable<IEntity>
	{
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
		}

		public override string ToString()
		{
			return string.Join(", ", EntityIds.Select(e => e));
		}

		public IEnumerator<IEntity> GetEnumerator()
		{
			if (_entities == null)
				_entities = _source == null ? new List<IEntity>() : _source.Out[DataSource.DefaultStreamName].List.Where(l => EntityIds.Contains(l.Key)).Select(l => l.Value).ToList();

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
	internal class ContentType : IContentType
	{
		public string Name { get; set; }
		public string StaticName { get; set; }
		/// <summary>
		/// Dictionary with all Attribute Definitions
		/// </summary>
		internal IDictionary<int, AttributeDefinition> AttributeDefinitions { get; set; }
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
				if (languageKeys != null && languageKeys.Length > 0)
				{
					// try match all specified Dimensions
					var valueHavingSpecifiedLanguages = _values.FirstOrDefault(va => languageKeys.All(lk => va.Languages.Select(d => d.Key).Contains(lk.ToLower())));
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
	}

	/// <summary>
	/// Represents a Value
	/// </summary>
	/// <typeparam name="T">Type of the actual Value</typeparam>
	public class ValueModel<T> : IValue<T>, IValueManagement
	{
		public int ValueId { get; set; }
		public IEnumerable<ILanguage> Languages { get; set; }
		public int ChangeLogIdCreated { get; set; }
		public T TypedContents { get; internal set; }
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
