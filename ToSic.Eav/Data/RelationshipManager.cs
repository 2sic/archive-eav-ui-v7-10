using System.Collections.Generic;
using System.Linq;

namespace ToSic.Eav.Data
{
	/// <summary>
	/// Used to get relationships between entities.
	/// </summary>
	public class RelationshipManager
	{
		private readonly IEntity _entity;
		internal readonly IEnumerable<EntityRelationshipItem> AllRelationships;

		/// <summary>
		/// Initializes a new instance of the RelationshipManager class.
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="allAllRelationships"></param>
		public RelationshipManager(IEntity entity, IEnumerable<EntityRelationshipItem> allAllRelationships)
		{
			_entity = entity;
			AllRelationships = allAllRelationships;
		}

		/// <summary>
		/// Get all Child Entities
		/// </summary>
		public IEnumerable<IEntity> AllChildren
		{
			get { return AllRelationships.Where(r => r.Parent == _entity).Select(r => r.Child); }
		}

		/// <summary>
		/// Get all Parent Entities
		/// </summary>
		public IEnumerable<IEntity> AllParents
		{
			get { return AllRelationships.Where(r => r.Child == _entity).Select(r => r.Parent); }
		}

		/// <summary>
		/// Get Children of a specified AttributeHelperTools Name
		/// </summary>
		public ChildEntities Children
		{
			get { return new ChildEntities(_entity.Attributes); }
		}

		/// <summary>
		/// Represents Child Entities by attribute name
		/// </summary>
		public class ChildEntities
		{
			private readonly Dictionary<string, IAttribute> _attributes;

			/// <summary>
			/// Initializes a new instance of the ChildEntities class.
			/// </summary>
			/// <param name="attributes"></param>
			public ChildEntities(Dictionary<string, IAttribute> attributes)
			{
				_attributes = attributes;
			}

			/// <summary>
			/// Get Children of a specified AttributeHelperTools Name
			/// </summary>
			/// <param name="attributeName">AttributeHelperTools Name</param>
			public IEnumerable<IEntity> this[string attributeName]
			{
				get
				{
					Attribute<Data.EntityRelationship> relationship;
					try
					{
						relationship = _attributes[attributeName] as Attribute<Data.EntityRelationship>;
					}
					catch (KeyNotFoundException)
					{
						return new List<IEntity>();
					}

					return relationship != null ? relationship.TypedContents : null;
				}
			}
		}
	}
}