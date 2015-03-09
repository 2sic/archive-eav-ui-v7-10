using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Script.Serialization;
using ToSic.Eav.DataSources;

namespace ToSic.Eav.Data
{
	/// <summary>
	/// Represents an Entity
	/// </summary>
	public class Entity : IEntity
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
		/// Create a new Entity. Used to create InMemory Entities that are not persisted to the EAV SqlStore.
		/// </summary>
		public Entity(int entityId, string contentTypeName, IDictionary<string, object> values, string titleAttribute)
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
		/// Create a new Entity
		/// </summary>
		internal Entity(Guid entityGuid, int entityId, int repositoryId, int assignmentObjectTypeId, IContentType type, bool isPublished, IEnumerable<EntityRelationshipItem> allRelationships, DateTime modified)
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
		/// Create a new Entity based on an Entity and Attributes
		/// </summary>
		internal Entity(IEntity entity, Dictionary<string, IAttribute> attributes, IEnumerable<EntityRelationshipItem> allRelationships)
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

	    public object GetBestValue(string attributeName, out bool propertyNotFound)
	    {
	        return GetBestValue(attributeName, new string[] {""}, out propertyNotFound);
	    }
        public object GetBestValue(string attributeName, string[] dimensions, out bool propertyNotFound)
        {
            propertyNotFound = false;
            object result = null;


            if (Attributes.ContainsKey(attributeName))
            {
                var attribute = Attributes[attributeName];
                result = attribute[dimensions];

                // todo in 2sxc
                //if (attribute.Type == "Hyperlink" && result is string)
                //{
                //    result = SexyContent.ResolveHyperlinkValues((string)result, SexyContext == null ? PortalSettings.Current : SexyContext.OwnerPS);
                //}
                //else


                // todo 2sxc or just return the entity-list, so 2sxc can dynamic it if desired...
                //    if (attribute.Type == "Entity" && result is EntityRelationshipModel)
                //{
                //    // Convert related entities to Dynamics
                //    result = ((ToSic.Eav.EntityRelationshipModel)result).Select(
                //        p => new DynamicEntity(p, dimensions, this.SexyContext)
                //        ).ToList();
                //}
            }
            else
            {
                switch (attributeName)
                {
                    case "EntityTitle":
                        result = Title;
                        break;
                    case "EntityId":
                        result = EntityId;
                        break;
                        // todo in 2sxc
                    //case "Toolbar":
                    //    result = Toolbar.ToString();
                    //    break;
                    case "IsPublished":
                        result = IsPublished;
                        break;
                    case "Modified":
                        result = Modified;
                        break;
                        // todo in 2sxc
                    //case "Presentation":
                    //    var inContentGroup = Entity as EntityInContentGroup;
                    //    if (inContentGroup != null)
                    //        result = inContentGroup.Presentation;
                    //    break;
                    default:
                        result = null;
                        propertyNotFound = true;
                        break;
                }
            }

            return result;
        }
	}
}
