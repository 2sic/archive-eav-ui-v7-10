using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using ToSic.Eav.DataSources;
using ToSic.Eav.DataSources.Caches;

namespace ToSic.Eav
{
	public partial class EavContext
	{
		/// <summary>
		/// Get a single Entity by EntityId
		/// </summary>
		/// <returns>Entity or throws InvalidOperationException</returns>
		public Entity GetEntity(int entityId)
		{
			return Entities.Single(e => e.EntityID == entityId);
		}
		/// <summary>
		/// Get a single Entity by EntityGuid. Ensure it's not deleted and has context's AppId
		/// </summary>
		/// <returns>Entity or throws InvalidOperationException</returns>
		public Entity GetEntity(Guid entityGuid)
		{
			return GetEntitiesByGuid(entityGuid).Single();
		}

		internal IQueryable<Entity> GetEntitiesByGuid(Guid entityGuid)
		{
			return Entities.Where(e => e.EntityGUID == entityGuid && !e.ChangeLogIDDeleted.HasValue && !e.Set.ChangeLogIDDeleted.HasValue && e.Set.AppID == _appId);
		}

		/// <summary>
		/// Test whehter Entity exists on current App and is not deleted
		/// </summary>
		public bool EntityExists(Guid entityGuid)
		{
			return GetEntitiesByGuid(entityGuid).Any();
		}

		/// <summary>
		/// Get a List of Entities with specified assignmentObjectTypeId and Key.
		/// </summary>
		public List<Entity> GetEntities(int assignmentObjectTypeId, int? keyNumber, Guid? keyGuid, string keyString)
		{
			return GetEntitiesInternal(assignmentObjectTypeId, keyNumber, keyGuid, keyString).ToList();
		}

		/// <summary>
		/// Get a List of Entities with specified assignmentObjectTypeId and optional Key.
		/// </summary>
		internal IQueryable<Entity> GetEntitiesInternal(int assignmentObjectTypeId, int? keyNumber = null, Guid? keyGuid = null, string keyString = null)
		{
			return from e in Entities
				   where e.AssignmentObjectTypeID == assignmentObjectTypeId
				   && (keyNumber.HasValue && e.KeyNumber == keyNumber.Value || keyGuid.HasValue && e.KeyGuid == keyGuid.Value || keyString != null && e.KeyString == keyString)
				   && e.ChangeLogIDDeleted == null
				   select e;
		}

		/// <summary>
		/// Get all EavValues of specified EntityId
		/// </summary>
		public List<EavValue> GetValues(int entityId)
		{
			return Values.Where(v => v.EntityID == entityId && !v.ChangeLogIDDeleted.HasValue).ToList();
		}

		/// <summary>
		/// Get a DataTable having all Entities of specified AttributeSetId. Used to show them in a simple GridView Control
		/// </summary>
		/// <param name="attributeSetId">AttributeSetId</param>
		/// <param name="dimensionIds">List of Dimensions/Languages to show</param>
		/// <param name="source">DataSource to get child entities</param>
		/// <param name="maxValueLength">Shorten Values longer than n Characters</param>
		/// <returns>A DataTable with all Columns defined in the AttributeSet</returns>
		public DataTable GetItemsTable(int attributeSetId, int[] dimensionIds = null, IDataSource source = null, int? maxValueLength = null)
		{
			var entityIds = Entities.Where(e => e.AttributeSetID == attributeSetId && e.ChangeLogIDDeleted == null).Select(e => e.EntityID).ToArray();
			if (!entityIds.Any())
				return null;
			var entitiesModel = GetDataForCache(entityIds, _appId, source, true).Entities;

			var columnNames = GetAttributes(attributeSetId).Select(a => a.StaticName);

			return entitiesModel.Select(v => v.Value).OrderBy(e => e.EntityId).ToDataTable(columnNames, dimensionIds, maxValueLength);
		}

		/// <summary>
		/// Get a List of all AttributeSets
		/// </summary>
		public List<AttributeSet> GetAllAttributeSets()
		{
			return AttributeSets.Where(a => a.AppID == AppId).ToList();
		}

		/// <summary>
		/// Get a single AttributeSet
		/// </summary>
		public AttributeSet GetAttributeSet(int attributeSetId)
		{
			return AttributeSets.SingleOrDefault(a => a.AttributeSetID == attributeSetId && a.AppID == AppId && !a.ChangeLogIDDeleted.HasValue);
		}
		/// <summary>
		/// Get a single AttributeSet
		/// </summary>
		public AttributeSet GetAttributeSet(string staticName)
		{
			return AttributeSets.SingleOrDefault(a => a.StaticName == staticName && a.AppID == AppId && !a.ChangeLogIDDeleted.HasValue);
		}

		/// <summary>
		/// Get AttributeSetId by StaticName and Scope
		/// </summary>
		/// <param name="staticName">StaticName of the AttributeSet</param>
		/// <param name="scope">Optional Filter by Scope</param>
		/// <returns>AttributeSetId or Exception</returns>
		public int GetAttributeSetId(string staticName, AttributeScope? scope)
		{
			var scopeFilter = scope.HasValue ? scope.ToString() : null;

			try
			{
				return AttributeSets.Single(s => s.AppID == _appId && s.StaticName == staticName && (s.Scope == scopeFilter || scopeFilter == null)).AttributeSetID;
			}
			catch (InvalidOperationException ex)
			{
				throw new Exception("Unable to get AttributeSet with StaticName \"" + staticName + "\" in Scope \"" + scopeFilter + "\".", ex);
			}
		}

		/// <summary>
		/// if AttributeSet refers another AttributeSet, get ID of the refered AttributeSet. Otherwise returns passed AttributeSetId.
		/// </summary>
		/// <param name="attributeSetId">AttributeSetId to resolve</param>
		private int ResolveAttributeSetId(int attributeSetId)
		{
			var usesConfigurationOfAttributeSet = AttributeSets.Where(a => a.AttributeSetID == attributeSetId).Select(a => a.UsesConfigurationOfAttributeSet).Single();
			return usesConfigurationOfAttributeSet.HasValue ? usesConfigurationOfAttributeSet.Value : attributeSetId;
		}

		/// <summary>
		/// Test whether AttributeSet exists on specified App and is not deleted
		/// </summary>
		public bool AttributeSetExists(string staticName, int appId)
		{
			return AttributeSets.Any(a => !a.ChangeLogIDDeleted.HasValue && a.AppID == appId && a.StaticName == staticName);
		}

		/// <summary>
		/// Get AttributeSets
		/// </summary>
		/// <param name="appId">Filter by AppId</param>
		/// <param name="scope">optional Filter by Scope</param>
		private IQueryable<AttributeSet> GetAttributeSets(int appId, AttributeScope? scope)
		{
			var result = AttributeSets.Where(a => a.AppID == appId && !a.ChangeLogIDDeleted.HasValue);

			if (scope != null)
			{
				var scopeString = scope.ToString();
				result = result.Where(a => a.Scope == scopeString);
			}

			return result;
		}

		/// <summary>
		/// Get a List of AttributeWithMetaInfo of specified AttributeSet and DimensionIds
		/// </summary>
		public List<AttributeWithMetaInfo> GetAttributesWithMetaInfo(int attributeSetId, int[] dimensionIds)
		{
			var attributesInSet = AttributesInSets.Where(a => a.AttributeSetID == attributeSetId).OrderBy(a => a.SortOrder).ToList();

			var systemScope = AttributeScope.System.ToString();

			return (from a in attributesInSet
					let metaData = GetAttributeMetaData(a.AttributeID)
					select new AttributeWithMetaInfo
					{
						AttributeID = a.AttributeID,
						IsTitle = a.IsTitle,
						StaticName = a.Attribute.StaticName,
						Name = metaData.ContainsKey("Name") && metaData["Name"].Values != null ? metaData["Name"][dimensionIds].ToString() : null,
						Notes = metaData.ContainsKey("Notes") && metaData["Notes"].Values != null ? metaData["Notes"][dimensionIds].ToString() : null,
						Type = a.Attribute.Type,
						HasTypeMetaData = AttributesInSets.Any(s => s.Set == AttributeSets.FirstOrDefault(se => se.StaticName == "@" + a.Attribute.Type && se.Scope == systemScope) && s.Attribute != null),
						MetaData = metaData
					}).ToList();
		}

		/// <summary>
		/// Get Attributes of an AttributeSet
		/// </summary>
		public IQueryable<Attribute> GetAttributes(int attributeSetId)
		{
			attributeSetId = ResolveAttributeSetId(attributeSetId);

			return from ais in AttributesInSets
				   where ais.AttributeSetID == attributeSetId
				   orderby ais.SortOrder
				   select ais.Attribute;
		}

		/// <summary>
		/// Get a List of all Attributes in specified AttributeSet
		/// </summary>
		/// <param name="attributeSet">Reference to an AttributeSet</param>
		/// <param name="includeTitleAttribute">Specify whether TitleAttribute should be included</param>
		public List<Attribute> GetAttributes(AttributeSet attributeSet, bool includeTitleAttribute = true)
		{
			var items = AttributesInSets.Where(a => a.AttributeSetID == attributeSet.AttributeSetID);
			if (!includeTitleAttribute)
				items = items.Where(a => !a.IsTitle);

			return items.Select(a => a.Attribute).ToList();
		}

		/// <summary>
		/// Get Title Attribute for specified AttributeSetId
		/// </summary>
		public Attribute GetTitleAttribute(int attributeSetId)
		{
			return AttributesInSets.Single(a => a.AttributeSetID == attributeSetId && a.IsTitle).Attribute;
		}

		/// <summary>
		/// Get Entities describing the Attribute (e.g. General and @String)
		/// </summary>
		public Dictionary<string, IAttribute> GetAttributeMetaData(int attributeId, IDataSource source = null)
		{
			return GetAttributeMetaData(attributeId, _zoneId, _appId, source);
		}
		/// <summary>
		/// Get Entities describing the Attribute (e.g. General and @String)
		/// </summary>
		public Dictionary<string, IAttribute> GetAttributeMetaData(int attributeId, int zoneId, int appId, IDataSource source = null)
		{
			// Get all EntityIds describing the Attribute (e.g. General and @String)
			var entities = DataSource.GetMetaDataSource(zoneId, appId).GetAssignedEntities(DataSource.AssignmentObjectTypeIdFieldProperties, attributeId);
			// Return all Attributes of all Entities with Value
			return entities.SelectMany(e => e.Attributes).ToDictionary(a => a.Key, a => a.Value);
		}

		/// <summary>
		/// AssignmentObjectType with specified Name 
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public AssignmentObjectType GetAssignmentObjectType(string name)
		{
			return AssignmentObjectTypes.Single(a => a.Name == name);
		}

		/// <summary>
		/// Get a list of all Attributes in Set for specified AttributeSetId
		/// </summary>
		public List<AttributeInSet> GetAttributesInSet(int attributeSetId)
		{
			return AttributesInSets.Where(a => a.AttributeSetID == attributeSetId).OrderBy(a => a.SortOrder).ToList();
		}

		/// <summary>
		/// Get all AssignmentObjectTypes with Id and Name
		/// </summary>
		public Dictionary<int, string> GetAssignmentObjectTypes()
		{
			return (from a in AssignmentObjectTypes
					select new { a.AssignmentObjectTypeID, a.Name }).ToDictionary(a => a.AssignmentObjectTypeID, a => a.Name);
		}

		#region Reading of public EAV Models

		/// <summary>
		/// Get all Entities Models for specified AppId
		/// </summary>
		internal IDictionary<int, IEntity> GetEntitiesModel(int appId, IDataSource source)
		{
			return GetDataForCache(null, appId, source, true).Entities;
		}

		/// <summary>
		/// Get all ContentTypes for specified AppId. If called multiple times it loads from a private field.
		/// </summary>
		internal IDictionary<int, IContentType> GetContentTypes(int appId)
		{
			if (!_contentTypes.ContainsKey(appId))
			{
				// Load from DB
				var contentTypes = from set in AttributeSets
								   where set.AppID == appId
								   select new
								   {
									   set.AttributeSetID,
									   set.Name,
									   set.StaticName,
									   Attributes = (from a in set.AttributesInSets
													 select new
													 {
														 a.AttributeID,
														 a.Attribute.StaticName,
														 a.Attribute.Type,
														 a.IsTitle
													 }),
									   set.UsesConfigurationOfAttributeSet,
									   SharedAttributes = (from a in AttributesInSets
														   where a.AttributeSetID == set.UsesConfigurationOfAttributeSet
														   select new
														   {
															   a.AttributeID,
															   a.Attribute.StaticName,
															   a.Attribute.Type,
															   a.IsTitle
														   })
								   };
				// Convert to ContentType-Model
				_contentTypes[appId] = contentTypes.ToDictionary(k1 => k1.AttributeSetID, set => (IContentType)new ContentType(set.Name, set.StaticName)
				{
					AttributeDefinitions = set.UsesConfigurationOfAttributeSet.HasValue
							? set.SharedAttributes.ToDictionary(k2 => k2.AttributeID, a => new AttributeDefinition(a.StaticName, a.Type, a.IsTitle, a.AttributeID))
							: set.Attributes.ToDictionary(k2 => k2.AttributeID, a => new AttributeDefinition(a.StaticName, a.Type, a.IsTitle, a.AttributeID))
				});
			}

			return _contentTypes[appId];
		}

		/// <summary>Get Data to populate ICache</summary>
		/// <param name="entityIds">null or a List of EntitiIds</param>
		/// <param name="appId">AppId (can be different than the appId on current context (e.g. if something is needed from the default appId, like MetaData)</param>
		/// <param name="source">DataSource to get child entities</param>
		/// <param name="entitiesOnly">If only the CachItem.Entities is needed, this can be set to true to imporove performance</param>
		/// <returns>Item1: EntityModels, Item2: all ContentTypes, Item3: Assignment Object Types</returns>
		internal CacheItem GetDataForCache(int[] entityIds, int appId, IDataSource source, bool entitiesOnly = false)
		{
			var contentTypes = GetContentTypes(appId);

			Dictionary<int, Dictionary<Guid, IEnumerable<IEntity>>> assignmentObjectTypesGuid = null;
			Dictionary<int, Dictionary<int, IEnumerable<IEntity>>> assignmentObjectTypesNumber = null;
			Dictionary<int, Dictionary<string, IEnumerable<IEntity>>> assignmentObjectTypesString = null;
			if (!entitiesOnly)
			{
				assignmentObjectTypesGuid = new Dictionary<int, Dictionary<Guid, IEnumerable<IEntity>>>();
				assignmentObjectTypesNumber = new Dictionary<int, Dictionary<int, IEnumerable<IEntity>>>();
				assignmentObjectTypesString = new Dictionary<int, Dictionary<string, IEnumerable<IEntity>>>();
			}
			var relationships = new List<EntityRelationshipItem>();

			#region Prepare & Extend EntityIds
			if (entityIds == null)
				entityIds = new int[0];

			var filterByEntityIds = entityIds.Any();

			// Ensure published Versions of Drafts are also loaded (if filtered by EntityId, otherwise all Entities from the app are loaded anyway)
			if (filterByEntityIds)
				entityIds = entityIds.Union(from e in Entities
											where e.PublishedEntityID.HasValue && !e.IsPublished && entityIds.Contains(e.EntityID) && !entityIds.Contains(e.PublishedEntityID.Value) && e.ChangeLogDeleted == null
											select e.PublishedEntityID.Value).ToArray();
			#endregion

			#region Get Entities with Attribute-Values from Database

			var entitiesValues = from e in Entities
								 where
									 !e.ChangeLogIDDeleted.HasValue &&
									 e.Set.AppID == appId &&
									 e.Set.ChangeLogIDDeleted == null &&
									 (	// filter by EntityIds (if set)
										 !filterByEntityIds ||
										 entityIds.Contains(e.EntityID) ||
										 (e.PublishedEntityID.HasValue && entityIds.Contains(e.PublishedEntityID.Value))	// also load Drafts
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
									 e.PublishedEntityID,
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
																				   select new DimensionModel
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
			foreach (var e in entitiesValues)
			{
				var contentType = (ContentType)contentTypes[e.AttributeSetID];
				var entityModel = new EntityModel(e.EntityGUID, e.EntityID, e.EntityID, e.AssignmentObjectTypeID, contentType, e.IsPublished, relationships);

				var entityAttributes = new Dictionary<int, IAttributeManagement>();	// temporary Dictionary to set values later more performant by Dictionary-Key (AttributeId)

				// Add all Attributes from that Content-Type
				foreach (var definition in contentType.AttributeDefinitions.Values)
				{
					var attributeModel = AttributeModel.GetAttributeManagementModel(definition);
					entityModel.Attributes.Add(((IAttributeBase)attributeModel).Name, attributeModel);
					entityAttributes.Add(definition.AttributeId, attributeModel);
				}

				// If entity is a draft, add references to Published Entity
				if (!e.IsPublished && e.PublishedEntityID.HasValue)
				{
					// Published Entity is already in the Entities-List as EntityIds is validated/extended before and Draft-EntityID is always higher as Published EntityId
					entityModel.PublishedEntity = entities[e.PublishedEntityID.Value];
					((EntityModel)entityModel.PublishedEntity).DraftEntity = entityModel;
					entityModel.EntityId = e.PublishedEntityID.Value;
				}

				#region Add assignmentObjectTypes with Key

				if (e.AssignmentObjectTypeID != 1 && !entitiesOnly)
				{
					if (e.KeyGuid.HasValue)
					{
						if (!assignmentObjectTypesGuid.ContainsKey(e.AssignmentObjectTypeID)) // ensure AssignmentObjectTypeID
							assignmentObjectTypesGuid.Add(e.AssignmentObjectTypeID, new Dictionary<Guid, IEnumerable<IEntity>>());

						if (!assignmentObjectTypesGuid[e.AssignmentObjectTypeID].ContainsKey(e.KeyGuid.Value)) // ensure Guid
							assignmentObjectTypesGuid[e.AssignmentObjectTypeID][e.KeyGuid.Value] = new List<IEntity>();

						((List<IEntity>)assignmentObjectTypesGuid[e.AssignmentObjectTypeID][e.KeyGuid.Value]).Add(entityModel);
					}
					if (e.KeyNumber.HasValue)
					{
						if (!assignmentObjectTypesNumber.ContainsKey(e.AssignmentObjectTypeID)) // ensure AssignmentObjectTypeID
							assignmentObjectTypesNumber.Add(e.AssignmentObjectTypeID, new Dictionary<int, IEnumerable<IEntity>>());

						if (!assignmentObjectTypesNumber[e.AssignmentObjectTypeID].ContainsKey(e.KeyNumber.Value)) // ensure Guid
							assignmentObjectTypesNumber[e.AssignmentObjectTypeID][e.KeyNumber.Value] = new List<IEntity>();

						((List<IEntity>)assignmentObjectTypesNumber[e.AssignmentObjectTypeID][e.KeyNumber.Value]).Add(entityModel);
					}
					if (!string.IsNullOrEmpty(e.KeyString))
					{
						if (!assignmentObjectTypesString.ContainsKey(e.AssignmentObjectTypeID)) // ensure AssignmentObjectTypeID
							assignmentObjectTypesString.Add(e.AssignmentObjectTypeID, new Dictionary<string, IEnumerable<IEntity>>());

						if (!assignmentObjectTypesString[e.AssignmentObjectTypeID].ContainsKey(e.KeyString)) // ensure Guid
							assignmentObjectTypesString[e.AssignmentObjectTypeID][e.KeyString] = new List<IEntity>();

						((List<IEntity>)assignmentObjectTypesString[e.AssignmentObjectTypeID][e.KeyString]).Add(entityModel);
					}
				}

				#endregion

				#region add Related-Entities Attributes
				foreach (var r in e.RelatedEntities)
				{
					var attributeModel = entityAttributes[r.AttributeID];
					var valueModel = ValueModel.GetValueModel(((IAttributeBase)attributeModel).Type, r.Childs, source);
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
						var valueModel = ValueModel.GetValueModel(((IAttributeBase)attributeModel).Type, v.Value, v.Languages, v.ValueID, v.ChangeLogIDCreated);
						valuesModelList.Add(valueModel);
					}
					#endregion

					attributeModel.Values = valuesModelList;
					attributeModel.DefaultValue = (IValueManagement)valuesModelList.FirstOrDefault();
				}
				#endregion

				entities.Add(e.EntityID, entityModel);
			}
			#endregion

			#region Populate Entity-Relationships (after all EntityModels are created)
			var relationshipsRaw = from r in EntityRelationships
								   where r.Attribute.AttributesInSets.Any(s => s.Set.AppID == appId && (!filterByEntityIds || entityIds.Contains(r.ChildEntityID) || entityIds.Contains(r.ParentEntityID)))
								   orderby r.ParentEntityID, r.AttributeID, r.ChildEntityID
								   select new { r.ParentEntityID, r.Attribute.StaticName, r.ChildEntityID };
			foreach (var relationship in relationshipsRaw)
			{
				try
				{
					relationships.Add(new EntityRelationshipItem(entities[relationship.ParentEntityID], entities[relationship.ChildEntityID]));
				}
				catch (KeyNotFoundException) { } // may occour if not all entities are loaded
			}
			#endregion

			return new CacheItem(entities, contentTypes, assignmentObjectTypesGuid, assignmentObjectTypesNumber, assignmentObjectTypesString, relationships);
		}

		/// <summary>
		/// Get EntityModel for specified EntityId
		/// </summary>
		/// <returns>A single IEntity or throws InvalidOperationException</returns>
		public IEntity GetEntityModel(int entityId, IDataSource source = null)
		{
			return GetDataForCache(new[] { entityId }, _appId, source, true).Entities.Single(e => e.Key == entityId).Value; // must filter by EntityId again because of Drafts
		}



		#endregion
	}
}