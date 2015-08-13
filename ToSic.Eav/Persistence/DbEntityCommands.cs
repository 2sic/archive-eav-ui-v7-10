using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ToSic.Eav.BLL;
using ToSic.Eav.Data;
using ToSic.Eav.Import;
using ToSic.Eav.ImportExport;

namespace ToSic.Eav.Persistence
{
    public class DbEntityCommands: BllCommandBase
    {
        public DbEntityCommands(EavDataController cntx) : base(cntx)
        {
        }

        /// <summary>
        /// Import a new Entity
        /// </summary>
        internal Entity AddEntity(int attributeSetId, Import.ImportEntity importEntity, List<LogItem> importLog, bool isPublished = true)
        {
            return AddEntity(null, attributeSetId, importEntity.Values, null, importEntity.KeyNumber, importEntity.KeyGuid, importEntity.KeyString, importEntity.AssignmentObjectTypeId, 0, importEntity.EntityGuid, null, updateLog: importLog, isPublished: isPublished);
        }
        /// <summary>
        /// Add a new Entity
        /// </summary>
        public Entity AddEntity(AttributeSet attributeSet, IDictionary values, int? configurationSet, int? key, int assignmentObjectTypeId = Constants.DefaultAssignmentObjectTypeId, int sortOrder = 0, Guid? entityGuid = null, ICollection<int> dimensionIds = null, bool isPublished = true)
        {
            return AddEntity(attributeSet, 0, values, configurationSet, key, null, null, assignmentObjectTypeId, sortOrder, entityGuid, dimensionIds, isPublished: isPublished);
        }
        /// <summary>
        /// Add a new Entity
        /// </summary>
        public Entity AddEntity(int attributeSetId, IDictionary values, int? configurationSet, int? key, int assignmentObjectTypeId = Constants.DefaultAssignmentObjectTypeId, int sortOrder = 0, Guid? entityGuid = null, ICollection<int> dimensionIds = null, bool isPublished = true)
        {
            return AddEntity(null, attributeSetId, values, configurationSet, key, null, null, assignmentObjectTypeId, sortOrder, entityGuid, dimensionIds, isPublished: isPublished);
        }
        /// <summary>
        /// Add a new Entity
        /// </summary>
        public Entity AddEntity(int attributeSetId, IDictionary values, int? configurationSet, Guid key, int assignmentObjectTypeId = Constants.DefaultAssignmentObjectTypeId, int sortOrder = 0, Guid? entityGuid = null, ICollection<int> dimensionIds = null, bool isPublished = true)
        {
            return AddEntity(null, attributeSetId, values, configurationSet, null, key, null, assignmentObjectTypeId, sortOrder, entityGuid, dimensionIds, isPublished: isPublished);
        }
        /// <summary>
        /// Add a new Entity
        /// </summary>
        private Entity AddEntity(AttributeSet attributeSet, int attributeSetId, IDictionary values, int? configurationSet, int? keyNumber, Guid? keyGuid, string keyString, int assignmentObjectTypeId, int sortOrder, Guid? entityGuid, ICollection<int> dimensionIds, List<LogItem> updateLog = null, bool isPublished = true)
        {
            // Prevent duplicate add of FieldProperties
            if (assignmentObjectTypeId == Constants.AssignmentObjectTypeIdFieldProperties && keyNumber.HasValue)
            {
                if (Context.DbS.GetEntities(Constants.AssignmentObjectTypeIdFieldProperties, keyNumber.Value).Any(e => e.AttributeSetID == attributeSetId))
                    throw new Exception(string.Format("An Entity already exists with AssignmentObjectTypeId {0} and KeyNumber {1}", Constants.AssignmentObjectTypeIdFieldProperties, keyNumber));
            }

            var changeId = Context.Versioning.GetChangeLogId();

            var newEntity = new Entity
            {
                ConfigurationSet = configurationSet,
                AssignmentObjectTypeID = assignmentObjectTypeId,
                KeyNumber = keyNumber,
                KeyGuid = keyGuid,
                KeyString = keyString,
                SortOrder = sortOrder,
                ChangeLogIDCreated = changeId,
                ChangeLogIDModified = changeId,
                EntityGUID = (entityGuid.HasValue && entityGuid.Value != new Guid()) ? entityGuid.Value : Guid.NewGuid(),
                IsPublished = isPublished
            };

            if (attributeSet != null)
                newEntity.Set = attributeSet;
            else
                newEntity.AttributeSetID = attributeSetId;

            Context.SqlDb.AddToEntities(newEntity);

            Context.SqlDb.SaveChanges();

            UpdateEntity(newEntity.EntityID, values, masterRecord: true, dimensionIds: dimensionIds, autoSave: false, updateLog: updateLog, isPublished: isPublished);

            Context.SqlDb.SaveChanges();

            return newEntity;
        }

        /// <summary>
        /// Clone an Entity with all Values
        /// </summary>
        internal Entity CloneEntity(Entity sourceEntity, bool assignNewEntityGuid = false)
        {
            var clone = sourceEntity.CopyEntity(Context.SqlDb);

            Context.SqlDb.AddToEntities(clone);

            Context.ValCommands.CloneEntityValues(sourceEntity, clone);

            if (assignNewEntityGuid)
                clone.EntityGUID = Guid.NewGuid();

            return clone;
        }


        /// <summary>
        /// Update an Entity
        /// </summary>
        /// <param name="entityGuid">EntityGUID</param>
        /// <param name="newValues">new Values of this Entity</param>
        /// <param name="autoSave">auto save Changes to DB</param>
        /// <param name="dimensionIds">DimensionIds for all Values</param>
        /// <param name="masterRecord">Is this the Master Record/Language</param>
        /// <param name="updateLog">Update/Import Log List</param>
        /// <param name="preserveUndefinedValues">Preserve Values if Attribute is not specifeied in NewValues</param>
        /// <returns>the updated Entity</returns>
        public Entity UpdateEntity(Guid entityGuid, IDictionary newValues, bool autoSave = true, ICollection<int> dimensionIds = null, bool masterRecord = true, List<LogItem> updateLog = null, bool preserveUndefinedValues = true)
        {
            var entity = Context.DbS.GetEntity(entityGuid);
            return UpdateEntity(entity.EntityID, newValues, autoSave, dimensionIds, masterRecord, updateLog, preserveUndefinedValues);
        }


        /// <summary>
        /// Update an Entity
        /// </summary>
        /// <param name="entityId">EntityID</param>
        /// <param name="newValues">new Values of this Entity</param>
        /// <param name="autoSave">auto save Changes to DB</param>
        /// <param name="dimensionIds">DimensionIds for all Values</param>
        /// <param name="masterRecord">Is this the Master Record/Language</param>
        /// <param name="updateLog">Update/Import Log List</param>
        /// <param name="preserveUndefinedValues">Preserve Values if Attribute is not specifeied in NewValues</param>
        /// <param name="isPublished">Is this Entity Published or a draft</param>
        /// <returns>the updated Entity</returns>
        public Entity UpdateEntity(int entityId, IDictionary newValues, bool autoSave = true, ICollection<int> dimensionIds = null, bool masterRecord = true, List<LogItem> updateLog = null, bool preserveUndefinedValues = true, bool isPublished = true)
        {
            var entity = Context.SqlDb.Entities.Single(e => e.EntityID == entityId);
            var draftEntityId = Context.EntCommands.GetDraftEntityId(entityId);

            #region Unpublished Save (Draft-Saves)
            // Current Entity is published but Update as a draft
            if (entity.IsPublished && !isPublished)
            {
                // Prevent duplicate Draft
                if (draftEntityId.HasValue)
                    throw new InvalidOperationException(string.Format("Published EntityId {0} has already a draft with EntityId {1}", entityId, draftEntityId));

                // create a new Draft-Entity
                entity = Context.EntCommands.CloneEntity(entity);
                entity.IsPublished = false;
                entity.PublishedEntityId = entityId;
            }
            // Prevent editing of Published if there's a draft
            else if (entity.IsPublished && draftEntityId.HasValue)
            {
                throw new Exception(string.Format("Update Entity not allowed because a draft exists with EntityId {0}", draftEntityId));
            }
            #endregion

            if (dimensionIds == null)
                dimensionIds = new List<int>(0);

            // Load all Attributes and current Values - .ToList() to prevent (slow) lazy loading
            var attributes = new DbAttributeCommands(Context).GetAttributes(entity.AttributeSetID).ToList();
            var currentValues = entity.EntityID != 0 ? Context.SqlDb.Values.Include("Attribute").Include("ValuesDimensions").Where(v => v.EntityID == entity.EntityID).ToList() : entity.Values.ToList();

            // Update Values from Import Model
            var newValuesImport = newValues as Dictionary<string, List<IValueImportModel>>;
            if (newValuesImport != null)
                UpdateEntityFromImportModel(entity, newValuesImport, updateLog, attributes, currentValues, preserveUndefinedValues);
            // Update Values from ValueViewModel
            else
                Context.EntCommands.UpdateEntityDefault(entity, newValues, dimensionIds, masterRecord, attributes, currentValues);

            // Update as Published but Current Entity is a Draft-Entity
            if (!entity.IsPublished && isPublished)
            {
                if (entity.PublishedEntityId.HasValue)	// if Entity has a published Version, add an additional DateTimeline Item for the Update of this Draft-Entity
                    Context.Versioning.SaveEntityToDataTimeline(entity);
                entity = Context.PubCommands.PublishEntity(entityId, false);
            }

            entity.ChangeLogIDModified = Context.Versioning.GetChangeLogId();

            Context.SqlDb.SaveChanges();	// must save now to generate EntityModel afterward for DataTimeline

            Context.Versioning.SaveEntityToDataTimeline(entity);

            return entity;
        }

        /// <summary>
        /// Update an Entity when using the Import
        /// </summary>
        internal void UpdateEntityFromImportModel(Entity currentEntity, Dictionary<string, List<IValueImportModel>> newValuesImport, List<LogItem> updateLog, List<Attribute> attributeList, List<EavValue> currentValues, bool preserveUndefinedValues)
        {
            if (updateLog == null)
                throw new ArgumentNullException("updateLog", "When Calling UpdateEntity() with newValues of Type IValueImportModel updateLog must be set.");

            // track updated values to remove values that were not updated automatically
            var updatedValueIds = new List<int>();
            var updatedAttributeIds = new List<int>();
            foreach (var newValue in newValuesImport)
            {
                #region Get Attribute Definition from List (or skip this field if not found)
                var attribute = attributeList.SingleOrDefault(a => a.StaticName == newValue.Key);
                if (attribute == null) // Attribute not found
                {
                    // Log Warning for all Values
                    updateLog.AddRange(newValue.Value.Select(v => new LogItem(EventLogEntryType.Warning, "Attribute not found for Value")
                    {
                        ImportAttribute = new Import.ImportAttribute { StaticName = newValue.Key },
                        Value = v,
                        ImportEntity = v.ParentEntity
                    }));
                    continue;
                }
                #endregion

                updatedAttributeIds.Add(attribute.AttributeID);

                // Go through each value / dimensions combination
                foreach (var newSingleValue in newValue.Value)
                {
                    try
                    {
                        var updatedValue = Context.ValCommands.UpdateValueByImport(currentEntity, attribute, currentValues, newSingleValue);

                        var updatedEavValue = updatedValue as EavValue;
                        if (updatedEavValue != null)
                            updatedValueIds.Add(updatedEavValue.ValueID);
                    }
                    catch (Exception ex)
                    {
                        updateLog.Add(new LogItem(EventLogEntryType.Error, "Update Entity-Value failed")
                        {
                            ImportAttribute = new ImportAttribute { StaticName = newValue.Key },
                            Value = newSingleValue,
                            ImportEntity = newSingleValue.ParentEntity,
                            Exception = ex
                        });
                    }
                }
            }

            // remove all existing values that were not updated
            // Logic should be:
            // Of all values - skip the ones we just modified and those which are deleted
            var valuesToDeleteNew = currentEntity.Values.Where(
                v => !updatedValueIds.Contains(v.ValueID) && v.ChangeLogIDDeleted == null);

            // Clean up - sometimes the default language doesn't clean properly - so even if it's good now...
            // ...there is old data which sometimes still is duplicate and causes issues, so this clean-up is important
            // So goal: every same-attribute-ID as the updated, with the same non-language-settings, is a left-over
            var reallyDelete = valuesToDeleteNew.Where(e => updatedAttributeIds.Contains(e.AttributeID));

            if (preserveUndefinedValues)
            {
                var valuesToKeep = valuesToDeleteNew.Where(v => updatedAttributeIds.Contains(v.AttributeID));
                if (valuesToKeep.Count() > updatedAttributeIds.Count) // in this case something is bad
                    throw new Exception("have too many to keep, don't know what to do, abort...");
                valuesToDeleteNew = valuesToDeleteNew.Where(v => !updatedAttributeIds.Contains(v.AttributeID));
            }

            // && (preserveUndefinedValues == false || updatedAttributeIds.Contains(v.AttributeID))).ToList();
            var valuesToDelete = valuesToDeleteNew.ToList();
            //var valuesToDelete = currentEntity.Values.Where(
            //    v => !updatedValueIds.Contains(v.ValueID) && v.ChangeLogIDDeleted == null 
            //        && (preserveUndefinedValues == false || updatedAttributeIds.Contains(v.AttributeID))).ToList();
            valuesToDelete.ForEach(v => v.ChangeLogIDDeleted = Context.Versioning.GetChangeLogId());
        }



        /// <summary>
        /// Get Draft EntityId of a Published EntityId. Returns null if there's none.
        /// </summary>
        /// <param name="entityId">EntityId of the Published Entity</param>
        internal int? GetDraftEntityId(int entityId)
        {
            return Context.SqlDb.Entities.Where(e => e.PublishedEntityId == entityId && !e.ChangeLogIDDeleted.HasValue).Select(e => (int?)e.EntityID).SingleOrDefault();
        }

        /// <summary>
        /// Update an Entity when not using the Import
        /// </summary>
        internal void UpdateEntityDefault(Entity entity, IDictionary newValues, ICollection<int> dimensionIds, bool masterRecord, List<Attribute> attributes, List<EavValue> currentValues)
        {
            var entityModel = entity.EntityID != 0 ? new DbLoadIntoEavDataStructure(Context).GetEavEntity(entity.EntityID) : null;
            var newValuesTyped = DictionaryToValuesViewModel(newValues);
            foreach (var newValue in newValuesTyped)
            {
                var attribute = attributes.Single(a => a.StaticName == newValue.Key);
                Context.ValCommands.UpdateValue(entity, attribute, masterRecord, currentValues, entityModel, newValue.Value, dimensionIds);
            }

            #region if Dimensions are specified, purge/remove specified dimensions for Values that are not in newValues
            if (dimensionIds.Count > 0)
            {
                var attributeMetadataSource = DataSource.GetMetaDataSource(Context.ZoneId, Context.AppId);

                var keys = newValuesTyped.Keys.ToArray();
                // Get all Values that are not present in newValues
                var valuesToPurge = entity.Values.Where(v => !v.ChangeLogIDDeleted.HasValue && !keys.Contains(v.Attribute.StaticName) && v.ValuesDimensions.Any(d => dimensionIds.Contains(d.DimensionID)));
                foreach (var valueToPurge in valuesToPurge)
                {
                    // Don't touch Value if attribute is not visible in UI
                    var attributeMetadata = attributeMetadataSource.GetAssignedEntities(Constants.AssignmentObjectTypeIdFieldProperties, valueToPurge.AttributeID, "@All").FirstOrDefault();
                    if (attributeMetadata != null)
                    {
                        var visibleInEditUi = ((Attribute<bool?>)attributeMetadata["VisibleInEditUI"]).TypedContents;
                        if (visibleInEditUi == false)
                            continue;
                    }

                    // Check if the Value is only used in this supplied dimension (carefull, dont' know what to do if we have multiple dimensions!, must define later)
                    // if yes, delete/invalidate the value
                    if (valueToPurge.ValuesDimensions.Count == 1)
                        valueToPurge.ChangeLogIDDeleted = Context.Versioning.GetChangeLogId();
                    // if now, remove dimension from Value
                    else
                    {
                        foreach (var valueDimension in valueToPurge.ValuesDimensions.Where(d => dimensionIds.Contains(d.DimensionID)).ToList())
                            valueToPurge.ValuesDimensions.Remove(valueDimension);
                    }
                }
            }
            #endregion
        }

        /// <summary>
        /// Convert IOrderedDictionary to <see cref="Dictionary{String, ValueViewModel}" /> (for backward capability)
        /// </summary>
        private Dictionary<string, ValueToImport> DictionaryToValuesViewModel(IDictionary newValues)
        {
            if (newValues is Dictionary<string, ValueToImport>)
                return (Dictionary<string, ValueToImport>)newValues;

            return newValues.Keys.Cast<object>().ToDictionary(key => key.ToString(), key => new ValueToImport { ReadOnly = false, Value = newValues[key] });
        }

        /// <summary>
        /// Delete an Entity
        /// </summary>
        public bool DeleteEntity(int repositoryId)
        {
            return DeleteEntity(Context.DbS.GetEntity(repositoryId));
        }

        /// <summary>
        /// Delete an Entity
        /// </summary>
        public bool DeleteEntity(Guid entityGuid)
        {
            return DeleteEntity(Context.DbS.GetEntity(entityGuid));
        }

        /// <summary>
        /// Delete an Entity
        /// </summary>
        internal bool DeleteEntity(Entity entity, bool autoSave = true)
        {
            if (entity == null)
                return false;

            #region Delete Related Records (Values, Value-Dimensions, Relationships)
            // Delete all Value-Dimensions
            var valueDimensions = entity.Values.SelectMany(v => v.ValuesDimensions).ToList();
            valueDimensions.ForEach(Context.SqlDb.DeleteObject);
            // Delete all Values
            entity.Values.ToList().ForEach(Context.SqlDb.DeleteObject);
            // Delete all Parent-Relationships
            entity.EntityParentRelationships.ToList().ForEach(Context.SqlDb.DeleteObject);
            #endregion

            // If entity was Published, set Deleted-Flag
            if (entity.IsPublished)
            {
                entity.ChangeLogIDDeleted = Context.Versioning.GetChangeLogId();
                // Also delete the Draft (if any)
                var draftEntityId = GetDraftEntityId(entity.EntityID);
                if (draftEntityId.HasValue)
                    DeleteEntity(draftEntityId.Value);
            }
            // If entity was a Draft, really delete that Entity
            else
            {
                // Delete all Child-Relationships
                entity.EntityChildRelationships.ToList().ForEach(Context.SqlDb.DeleteObject);
                Context.SqlDb.DeleteObject(entity);
            }

            if (autoSave)
                Context.SqlDb.SaveChanges();

            return true;
        }



    }
}
