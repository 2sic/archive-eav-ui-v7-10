using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.DataSources.Caches;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav.BLL
{
    public class DbLoadIntoEavDataStructure: BllCommandBase
    {
        public DbLoadIntoEavDataStructure(EavDataController cntx) : base(cntx)
        {
        }

        /// <summary>
        /// Get all Entities Models for specified AppId
        /// </summary>
        internal IDictionary<int, IEntity> GetEavEntities(int appId, BaseCache source)
        {
            return GetAppDataPackage(null, appId, source, true).Entities;
        }

        /// <summary>
        /// Get all ContentTypes for specified AppId. If called multiple times it loads from a private field.
        /// </summary>
        internal IDictionary<int, IContentType> GetEavContentTypes(int appId)
        {
            if (!Context.AttribSet.ContentTypes.ContainsKey(appId))
            {
                // Load from DB
                var contentTypes = from set in Context.SqlDb.AttributeSets
                                   where set.AppID == appId && !set.ChangeLogIDDeleted.HasValue
                                   select new
                                   {
                                       set.AttributeSetID,
                                       set.Name,
                                       set.StaticName,
                                       set.Scope,
                                       set.Description,
                                       Attributes = (from a in set.AttributesInSets
                                                     select new
                                                     {
                                                         a.AttributeID,
                                                         a.Attribute.StaticName,
                                                         a.Attribute.Type,
                                                         a.IsTitle, 
                                                         a.SortOrder
                                                     }),
                                       set.UsesConfigurationOfAttributeSet,
                                       SharedAttributes = (from a in Context.SqlDb.AttributesInSets
                                                           where a.AttributeSetID == set.UsesConfigurationOfAttributeSet
                                                           select new
                                                           {
                                                               a.AttributeID,
                                                               a.Attribute.StaticName,
                                                               a.Attribute.Type,
                                                               a.IsTitle,
                                                               a.SortOrder
                                                           })
                                   };
                // Convert to ContentType-Model
                Context.AttribSet.ContentTypes[appId] = contentTypes.ToDictionary(k1 => k1.AttributeSetID, set => (IContentType)new ContentType(set.Name, set.StaticName, set.AttributeSetID, set.Scope, set.Description, set.UsesConfigurationOfAttributeSet)
                {
                    AttributeDefinitions = set.UsesConfigurationOfAttributeSet.HasValue
                            ? set.SharedAttributes.ToDictionary(k2 => k2.AttributeID, a => new AttributeBase(a.StaticName, a.Type, a.IsTitle, a.AttributeID, a.SortOrder) as IAttributeBase)
                            : set.Attributes.ToDictionary(k2 => k2.AttributeID, a => new AttributeBase(a.StaticName, a.Type, a.IsTitle, a.AttributeID, a.SortOrder) as IAttributeBase)
                });
            }

            return Context.AttribSet.ContentTypes[appId];
        }

        /// <summary>Get Data to populate ICache</summary>
        /// <param name="entityIds">null or a List of EntitiIds</param>
        /// <param name="appId">AppId (can be different than the appId on current context (e.g. if something is needed from the default appId, like MetaData)</param>
        /// <param name="source">DataSource to get child entities</param>
        /// <param name="entitiesOnly">If only the CachItem.Entities is needed, this can be set to true to imporove performance</param>
        /// <returns>Item1: EntityModels, Item2: all ContentTypes, Item3: Assignment Object Types</returns>
        internal AppDataPackage GetAppDataPackage(int[] entityIds, int appId, IDeferredEntitiesList source, bool entitiesOnly = false)
        {
            var contentTypes = GetEavContentTypes(appId);

            var metadataForGuid = new Dictionary<int, Dictionary<Guid, IEnumerable<IEntity>>>();
            var metadataForNumber = new Dictionary<int, Dictionary<int, IEnumerable<IEntity>>>();
            var metadataForString = new Dictionary<int, Dictionary<string, IEnumerable<IEntity>>>();

            var relationships = new List<EntityRelationshipItem>();

            #region Prepare & Extend EntityIds
            if (entityIds == null)
                entityIds = new int[0];

            var filterByEntityIds = entityIds.Any();

            // Ensure published Versions of Drafts are also loaded (if filtered by EntityId, otherwise all Entities from the app are loaded anyway)
            if (filterByEntityIds)
                entityIds = entityIds.Union(from e in Context.SqlDb.Entities
                                            where e.PublishedEntityId.HasValue && !e.IsPublished && entityIds.Contains(e.EntityID) && !entityIds.Contains(e.PublishedEntityId.Value) && e.ChangeLogDeleted == null
                                            select e.PublishedEntityId.Value).ToArray();
            #endregion

            #region Get Entities with Attribute-Values from Database

            var entitiesValues = from e in Context.SqlDb.Entities
                                 where
                                     !e.ChangeLogIDDeleted.HasValue &&
                                     e.Set.AppID == appId &&
                                     e.Set.ChangeLogIDDeleted == null &&
                                     (	// filter by EntityIds (if set)
                                         !filterByEntityIds ||
                                         entityIds.Contains(e.EntityID) ||
                                         (e.PublishedEntityId.HasValue && entityIds.Contains(e.PublishedEntityId.Value))	// also load Drafts
                                         )
                                 orderby
                                     e.EntityID	// guarantees Published appear before draft
                                 select new
                                 {
                                     e.EntityID,
                                     e.EntityGUID,
                                     e.AttributeSetID,
                                     e.KeyGuid,
                                     e.KeyNumber,
                                     e.KeyString,
                                     e.AssignmentObjectTypeID,
                                     e.IsPublished,
                                     e.PublishedEntityId,
                                     Modified = e.ChangeLogModified.Timestamp,
                                     RelatedEntities = from r in e.EntityParentRelationships
                                                       group r by r.AttributeID
                                                           into rg
                                                           select new
                                                           {
                                                               AttributeID = rg.Key,
                                                               Childs = rg.OrderBy(c => c.SortOrder).Select(c => c.ChildEntityID)
                                                           },
                                     Attributes = from v in e.Values
                                                  where !v.ChangeLogIDDeleted.HasValue
                                                  group v by v.AttributeID
                                                      into vg
                                                      select new
                                                      {
                                                          AttributeID = vg.Key,
                                                          Values = from v2 in vg
                                                                   orderby v2.ChangeLogIDCreated
                                                                   select new
                                                                   {
                                                                       v2.ValueID,
                                                                       v2.Value,
                                                                       Languages = from l in v2.ValuesDimensions
                                                                                   select new Data.Dimension
                                                                                   {
                                                                                       DimensionId = l.DimensionID,
                                                                                       ReadOnly = l.ReadOnly,
                                                                                       Key = l.Dimension.ExternalKey.ToLower()
                                                                                   },
                                                                       v2.ChangeLogIDCreated
                                                                   }
                                                      }
                                 };
            #endregion

            #region Build EntityModels
            var entities = new Dictionary<int, IEntity>();
            var entList = new List<IEntity>();

            foreach (var e in entitiesValues)
            {
                var contentType = (ContentType)contentTypes[e.AttributeSetID];
                var entityModel = new Data.Entity(e.EntityGUID, e.EntityID, e.EntityID, e.AssignmentObjectTypeID, contentType, e.IsPublished, relationships, e.Modified);

                var entityAttributes = new Dictionary<int, IAttributeManagement>();	// temporary Dictionary to set values later more performant by Dictionary-Key (AttributeId)

                // Add all Attributes from that Content-Type
                foreach (var definition in contentType.AttributeDefinitions.Values)
                {
                    var attributeModel = Data.AttributeHelperTools.GetAttributeManagementModel(definition);
                    entityModel.Attributes.Add(((IAttributeBase)attributeModel).Name, attributeModel);
                    entityAttributes.Add(definition.AttributeId, attributeModel);
                }

                // If entity is a draft, add references to Published Entity
                if (!e.IsPublished && e.PublishedEntityId.HasValue)
                {
                    // Published Entity is already in the Entities-List as EntityIds is validated/extended before and Draft-EntityID is always higher as Published EntityId
                    entityModel.PublishedEntity = entities[e.PublishedEntityId.Value];
                    ((Data.Entity)entityModel.PublishedEntity).DraftEntity = entityModel;
                    entityModel.EntityId = e.PublishedEntityId.Value;
                }

                #region Add metadata-lists based on AssignmentObjectTypes

                // unclear why #1 is handled in a special way - why should this not be cached? I believe 1 means no specific assignment
                if (e.AssignmentObjectTypeID != 1 && !entitiesOnly)
                {
                    // Try guid first. Note that an item can be assigned to both a guid, string and an int if necessary, though not commonly used
                    if (e.KeyGuid.HasValue)
                    {
                        // Ensure that this assignment-Type (like 4 = entity-assignment) already has a dictionary for storage
                        if (!metadataForGuid.ContainsKey(e.AssignmentObjectTypeID)) // ensure AssignmentObjectTypeID
                            metadataForGuid.Add(e.AssignmentObjectTypeID, new Dictionary<Guid, IEnumerable<IEntity>>());

                        // Ensure that the assignment type (like 4) the target guid (like a350320-3502-afg0-...) has an empty list of items
                        if (!metadataForGuid[e.AssignmentObjectTypeID].ContainsKey(e.KeyGuid.Value)) // ensure Guid
                            metadataForGuid[e.AssignmentObjectTypeID][e.KeyGuid.Value] = new List<IEntity>();

                        // Now all containers must exist, add this item
                        ((List<IEntity>)metadataForGuid[e.AssignmentObjectTypeID][e.KeyGuid.Value]).Add(entityModel);
                    }
                    if (e.KeyNumber.HasValue)
                    {
                        if (!metadataForNumber.ContainsKey(e.AssignmentObjectTypeID)) // ensure AssignmentObjectTypeID
                            metadataForNumber.Add(e.AssignmentObjectTypeID, new Dictionary<int, IEnumerable<IEntity>>());

                        if (!metadataForNumber[e.AssignmentObjectTypeID].ContainsKey(e.KeyNumber.Value)) // ensure Guid
                            metadataForNumber[e.AssignmentObjectTypeID][e.KeyNumber.Value] = new List<IEntity>();

                        ((List<IEntity>)metadataForNumber[e.AssignmentObjectTypeID][e.KeyNumber.Value]).Add(entityModel);
                    }
                    if (!string.IsNullOrEmpty(e.KeyString))
                    {
                        if (!metadataForString.ContainsKey(e.AssignmentObjectTypeID)) // ensure AssignmentObjectTypeID
                            metadataForString.Add(e.AssignmentObjectTypeID, new Dictionary<string, IEnumerable<IEntity>>());

                        if (!metadataForString[e.AssignmentObjectTypeID].ContainsKey(e.KeyString)) // ensure Guid
                            metadataForString[e.AssignmentObjectTypeID][e.KeyString] = new List<IEntity>();

                        ((List<IEntity>)metadataForString[e.AssignmentObjectTypeID][e.KeyString]).Add(entityModel);
                    }
                }

                #endregion

                #region add Related-Entities Attributes
                foreach (var r in e.RelatedEntities)
                {
                    var attributeModel = entityAttributes[r.AttributeID];
                    var valueModel = Value.GetValueModel(((IAttributeBase)attributeModel).Type, r.Childs, source);
                    var valuesModelList = new List<IValue> { valueModel };
                    attributeModel.Values = valuesModelList;
                    attributeModel.DefaultValue = (IValueManagement)valuesModelList.FirstOrDefault();
                }
                #endregion

                #region Add "normal" Attributes (that are not Entity-Relations)
                foreach (var a in e.Attributes)
                {
                    IAttributeManagement attributeModel;
                    try
                    {
                        attributeModel = entityAttributes[a.AttributeID];
                    }
                    catch (KeyNotFoundException)
                    {
                        continue;
                    }
                    if (attributeModel.IsTitle)
                        entityModel.Title = attributeModel;
                    var valuesModelList = new List<IValue>();

                    #region Add all Values
                    foreach (var v in a.Values)
                    {
                        var valueModel = Value.GetValueModel(((IAttributeBase)attributeModel).Type, v.Value, v.Languages, v.ValueID, v.ChangeLogIDCreated);
                        valuesModelList.Add(valueModel);
                    }
                    #endregion

                    attributeModel.Values = valuesModelList;
                    attributeModel.DefaultValue = (IValueManagement)valuesModelList.FirstOrDefault();
                }
                #endregion

                entities.Add(e.EntityID, entityModel);
                entList.Add(entityModel);
            }
            #endregion

            #region Populate Entity-Relationships (after all EntityModels are created)
            var relationshipsRaw = from r in Context.SqlDb.EntityRelationships
                                   where r.Attribute.AttributesInSets.Any(s => s.Set.AppID == appId && (!filterByEntityIds || (!r.ChildEntityID.HasValue || entityIds.Contains(r.ChildEntityID.Value)) || entityIds.Contains(r.ParentEntityID)))
                                   orderby r.ParentEntityID, r.AttributeID, r.ChildEntityID
                                   select new { r.ParentEntityID, r.Attribute.StaticName, r.ChildEntityID };
            foreach (var relationship in relationshipsRaw)
            {
                try
                {
                    if(entities.ContainsKey(relationship.ParentEntityID) && (!relationship.ChildEntityID.HasValue || entities.ContainsKey(relationship.ChildEntityID.Value)))
                        relationships.Add(new EntityRelationshipItem(entities[relationship.ParentEntityID], relationship.ChildEntityID.HasValue ? entities[relationship.ChildEntityID.Value] : null));
                }
                catch (KeyNotFoundException) { } // may occour if not all entities are loaded - edited 2rm 2015-09-29: Should not occur anymore
            }
            #endregion

            return new AppDataPackage(entities, entList, contentTypes, metadataForGuid, metadataForNumber, metadataForString, relationships);
        }

        /// <summary>
        /// Get EntityModel for specified EntityId
        /// </summary>
        /// <returns>A single IEntity or throws InvalidOperationException</returns>
        public IEntity GetEavEntity(int entityId, BaseCache source = null)
        {
            return GetAppDataPackage(new[] { entityId }, Context.AppId /*_appId*/, source, true).Entities.Single(e => e.Key == entityId).Value; // must filter by EntityId again because of Drafts
        }
    }
}
