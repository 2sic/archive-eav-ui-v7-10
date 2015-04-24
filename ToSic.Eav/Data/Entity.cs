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
        #region Basic properties like EntityId, Guid, IsPublished etc.
        /// <summary>
        /// Id as an int
        /// </summary>
		public int EntityId { get; internal set; }
        /// <summary>
        /// Id of this item inside the repository. Can be different than the real Id, because it may be a temporary version of this content-item
        /// </summary>
		public int RepositoryId { get; internal set; }
        /// <summary>
        /// Id as GUID
        /// </summary>
		public Guid EntityGuid { get; internal set; }
        /// <summary>
        /// Offical title of this content-item
        /// </summary>
		public IAttribute Title { get; internal set; }
        /// <summary>
        /// List of all attributes
        /// </summary>
		public Dictionary<string, IAttribute> Attributes { get; internal set; }
        /// <summary>
        /// Type-definition of this content-item
        /// </summary>
		public IContentType Type { get; internal set; }
        /// <summary>
        /// Modified date/time
        /// </summary>
		public DateTime Modified { get; internal set; }
        /// <summary>
        /// Relationship-helper object, important to navigate to children and parents
        /// </summary>
		[ScriptIgnore]
		public RelationshipManager Relationships { get; internal set; }
        /// <summary>
        /// Published/Draft status. If not published, it may be invisble, but there may also be another item visible ATM
        /// </summary>
		public bool IsPublished { get; internal set; }
		public int AssignmentObjectTypeId { get; internal set; }
        /// <summary>
        /// If this entity is published and there is a draft of it, then it can be navigated through DraftEntity
        /// </summary>
		internal IEntity DraftEntity { get; set; }
        /// <summary>
        /// If this entity is draft and there is a published edition, then it can be navigated through PublishedEntity
        /// </summary>
        internal IEntity PublishedEntity { get; set; }

        /// <summary>
        /// Shorhand accessor to retrieve an attribute
        /// </summary>
        /// <param name="attributeName"></param>
        /// <returns></returns>
		public IAttribute this[string attributeName]
		{
			get { return (Attributes.ContainsKey(attributeName)) ? Attributes[attributeName] : null; }
		}
        #endregion

        /// <summary>
		/// Create a new Entity. Used to create InMemory Entities that are not persisted to the EAV SqlStore.
		/// </summary>
		public Entity(int entityId, string contentTypeName, IDictionary<string, object> values, string titleAttribute)
		{
			EntityId = entityId;
			Type = new ContentType(contentTypeName);
			Attributes = AttributeHelperTools.GetTypedDictionaryForSingleLanguage(values, titleAttribute);
			try
			{
				Title = Attributes[titleAttribute];
			}
			catch (KeyNotFoundException)
			{
				throw new KeyNotFoundException(string.Format("The Title AttributeHelperTools with Name \"{0}\" doesn't exist in the Entity-Attributes.", titleAttribute));
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
			Attributes = new Dictionary<string, IAttribute>(StringComparer.OrdinalIgnoreCase); // 2015-04-24 added, maybe a risk but should help with tokens
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

        /// <summary>
        /// The draft entity fitting this published entity
        /// </summary>
        /// <returns></returns>
        public IEntity GetDraft()
		{
			return DraftEntity;
		}

        /// <summary>
        /// The published entity of this draft entity
        /// </summary>
        /// <returns></returns>
        public IEntity GetPublished()
		{
			return PublishedEntity;
		}

	    public object GetBestValue(string attributeName)
	    {
	        return GetBestValue(attributeName, new string[0]);
	    }
        //public object GetBestValue(string attributeName, out bool propertyNotFound)
        //{
        //    return GetBestValue(attributeName, new string[] {""}, out propertyNotFound);
        //}

        //public object GetBestValue(string attributeName, string[] dimensions, out bool propertyNotFound)
        //{
        //    var res = GetBestValue(attributeName, dimensions);
        //    propertyNotFound = (res == null);
        //    return res;
        //}


        public object GetBestValue(string attributeName, string[] dimensions) //, out bool propertyNotFound)
        {
            // propertyNotFound = false;
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
                //    if (attribute.Type == "Entity" && result is EntityRelationship)
                //{
                //    // Convert related entities to Dynamics
                //    result = ((ToSic.Eav.EntityRelationship)result).Select(
                //        p => new DynamicEntity(p, dimensions, this.SexyContext)
                //        ).ToList();
                //}
            }
            else
            {
                switch (attributeName)
                {
                    case "EntityTitle":
                        result = Title[dimensions];
                        break;
                    case "EntityId":
                        result = EntityId;
                        break;
                    case "EntityGuid":
                        result = EntityGuid;
                        break;
                    case "EntityType":
                        result = Type.Name;
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
                        //propertyNotFound = true;
                        break;
                }
            }

            return result;
        }
	}
}
