using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Web.Http;
using ToSic.Eav.DataSources;
using ToSic.Eav.Persistence;
using ToSic.Eav.WebApi.Formats;
using ToSic.Eav.Import;


namespace ToSic.Eav.WebApi
{
	/// <summary>
	/// Web API Controller for various actions
	/// </summary>
	public class EntitiesController : Eav3WebApiBase
    {
        public EntitiesController(int appId) : base(appId) { }
        public EntitiesController() : base() { }

        #region GetOne GetAll calls
        internal IEntity GetEntityOrThrowError(string contentType, int id, int? appId = null)
        {
            if (appId.HasValue)
                AppId = appId.Value;

            var found = InitialDS.List[id];
            if (contentType != null && found.Type.Name != contentType)
                throw new KeyNotFoundException("Can't find " + id + "of type '" + contentType + "'");
            return found;
        }

        public Dictionary<string, object> GetOne(string contentType, int id, int? appId = null, string cultureCode = null)
	    {
            var found = GetEntityOrThrowError(contentType, id, appId);
            return Serializer.Prepare(found);
	    }


        /// <summary>
		/// Get all Entities of specified Type
		/// </summary>
		public IEnumerable<Dictionary<string, object>> GetEntities(string contentType, string cultureCode = null, int? appId = null)
		{
            if (appId.HasValue)
                AppId = appId.Value;

			var typeFilter = DataSource.GetDataSource<EntityTypeFilter>(appId: appId, upstream: InitialDS);
			typeFilter.TypeName = contentType;

            return Serializer.Prepare(typeFilter.LightList);//  typeFilter.List.Select(t => Helpers.GetEntityValues(t.Value, cultureCode: cultureCode));
		}

	    public IEnumerable<Dictionary<string, object>> GetAllOfTypeForAdmin(int appId, string contentType)
	    {
	        SetAppIdAndUser(appId);
	        var ds = InitialDS;

            var typeFilter = DataSource.GetDataSource<EntityTypeFilter>(appId: appId, upstream: ds.Cache, valueCollectionProvider: ds.ConfigurationProvider); // need to go to cache, to include published & unpublished
            typeFilter.TypeName = contentType;

            Serializer.IncludeGuid = true;
	        Serializer.IncludePublishingInfo = true;

            return Serializer.Prepare(typeFilter.LightList);
        }

        //2015-08-29 2dm: these commands are kind of ready, but not in use yet
        /// <summary>
        /// Get all Entities of specified Type
        /// </summary>
     //   public IEnumerable<Dictionary<string, object>> GetEntitiesByType(int appId, string contentType, int pageSize = 1000, int pageNumber = 1)
     //   {
     //       AppId = appId;

     //       var typeFilter = DataSource.GetDataSource<EntityTypeFilter>(appId: appId, upstream: InitialDS);
     //       typeFilter.TypeName = contentType;
     //       var paging = DataSource.GetDataSource<Paging>(upstream: typeFilter);
     //       paging.PageNumber = pageNumber;
     //       paging.PageSize = pageSize;

     //       return Serializer.Prepare(paging.LightList);//  typeFilter.List.Select(t => Helpers.GetEntityValues(t.Value, cultureCode: cultureCode));
     //   }

	    //public int CountEntitiesOfType(int appId, string contentType)
	    //{
     //       AppId = appId;

     //       var typeFilter = DataSource.GetDataSource<EntityTypeFilter>(appId: appId, upstream: InitialDS);
     //       typeFilter.TypeName = contentType;
	    //    return typeFilter.LightList.Count();
	    //}

        public dynamic /* Formats.EntityWithLanguages */ GetOne(int appId, string contentType, int id, string format = "multi-language")
        {
            switch (format)
            {
                case "multi-language":
                    Serializer.IncludeAllEditingInfos = true;

                    var found = GetEntityOrThrowError(contentType, id, appId);

                    //return Serializer.Prepare(found);

                    var ce = new Formats.EntityWithLanguages()
                    {
                        AppId = appId,
                        Id = found.EntityId,
                        Guid = found.EntityGuid,
                        Type = new Formats.Type() { Name = found.Type.Name, StaticName = found.Type.StaticName },
                        TitleAttributeName = found.Title == null ? null : found.Title.Name,
                        Attributes = found.Attributes.ToDictionary(a => a.Key, a => new Formats.Attribute()
                        {
							Values = a.Value.Values == null ? new ValueSet[0] : a.Value.Values.Select(v => new Formats.ValueSet()
                            {
                                Value = v.Serialized,
                                Dimensions = v.Languages.ToDictionary(l => l.Key, y => y.ReadOnly)
                            }).ToArray()
                        })
                    };
                    return ce;
                default:
                    throw new Exception("format: " + format + " unknown");
            }
        }


        // trying to develop a save 2dm 2015-09-20
	    [HttpPost]
	    public bool Save([FromUri]int appId, Formats.EntityWithLanguages newData)
	    {
            // var zoneId = 0; // TODO2tk: Define
            // TODO2tk: Multi or single language
            // TODO2tk: Create or update
            // TODO2tk: Convert structures
            //var entity = new ToSic.Eav.Import.ImportEntity();

            //// GUID
            //if (newData.Id == 0)
            //{
            //    entity.EntityGuid = Guid.NewGuid();
            //}
            //else
            //{
            //    entity.EntityGuid = newData.Guid;
            //}

            //// Content type
            //entity.AttributeSetStaticName = newData.Type.StaticName;
            //entity.KeyNumber = newData.Metadata.KeyNumber;
            //entity.IsPublished =
            //// ...

            //if(newData.Metadata.HasMetadata)
            //{
            //    entity.AssignmentObjectTypeId = newData.Metadata.TargetType;
            //}
            //// entity.AssignmentObjectTypeId // Type of metadata metadata.TargetType

            //foreach(var attribute in newData.Attributes)
            //{
            //    foreach(var value in attribute.Value.Values)
            //    {
            //        foreach(var dimension in value.Dimensions)
            //        {

            //        }
            //    }
            //}


            //var import = new ToSic.Eav.Import.Import(null, appId,  User.Identity.Name, leaveExistingValuesUntouched: false, preserveUndefinedValues: false);
            //import.RunImport(null, new ToSic.Eav.Import.ImportEntity[] { entity }, true, true);

            return false;
	    }
        private void OldInsertWebForm()
        {
            //// Cancel insert if current language is not default language
            //if (DefaultCultureDimension.HasValue && !DimensionIds.Contains(DefaultCultureDimension.Value))
            //    return;

            //var values = new Dictionary<string, ValueViewModel>();

            //// Extract Values
            //foreach (var fieldTemplate in phFields.Controls.OfType<FieldTemplateUserControl>())
            //    fieldTemplate.ExtractValues(values);

            //// Prepare DimensionIds
            //var dimensionIds = new List<int>();
            //if (DefaultCultureDimension.HasValue)
            //    dimensionIds.Add(DefaultCultureDimension.Value);

            //Entity result;
            //var assignmentObjectTypeId = AssignmentObjectTypeId.HasValue ? AssignmentObjectTypeId.Value : EavContext.DefaultAssignmentObjectTypeId;
            //if (!KeyGuid.HasValue)
            //    result = Db.AddEntity(AttributeSetId, values, null, KeyNumber, assignmentObjectTypeId, dimensionIds: dimensionIds, isPublished: IsPublished);
            //else
            //    result = Db.AddEntity(AttributeSetId, values, null, KeyGuid.Value, assignmentObjectTypeId, dimensionIds: dimensionIds, isPublished: IsPublished);

            //RedirectToListItems();

            //if (Inserted != null)
            //    Inserted(result);

            //if (Saved != null)
            //    Saved(result);
        }

        private void OldUpdateWebForm()
        {
            //var values = new Dictionary<string, ValueViewModel>();

            //#region Extract Values (only of enabled fields)
            //foreach (var fieldTemplate in phFields.Controls.OfType<FieldTemplateUserControl>().Where(f => f.Enabled))
            //{
            //    // if not master and not translated, don't pass/extract this value
            //    if (!MasterRecord && fieldTemplate.ValueId == null && fieldTemplate.ReadOnly)
            //        continue;

            //    fieldTemplate.ExtractValues(values);
            //}
            //#endregion

            //var result = Db.UpdateEntity(_repositoryId, values, dimensionIds: DimensionIds, masterRecord: MasterRecord, isPublished: IsPublished);

            //RedirectToListItems();

            //if (Updated != null)
            //    Updated(result);

            //if (Saved != null)
            //    Saved(result);
        }
        #endregion


        #region Delete calls

        /// <summary>
        /// Delete the entity specified by ID.
        /// </summary>
        /// <param name="contentType"></param>
        /// <param name="id">Entity ID</param>
        /// <param name="appId"></param>
        /// <exception cref="InvalidOperationException">Entity cannot be deleted for example when it is referenced by another object</exception>
        public void Delete(string contentType, int id, int? appId = null)
	    {
	        if (appId.HasValue)
	            AppId = appId.Value;
	        var finalAppId = appId ?? AppId;
            var found = InitialDS.List[id];
            if (found.Type.Name != contentType && found.Type.StaticName != contentType)
                throw new KeyNotFoundException("Can't find " + id + "of type '" + contentType + "'");
            if (!(CurrentContext.Entities.CanDeleteEntity(id).Item1)) // (!CurrentContext.EntCommands.CanDeleteEntity(id).Item1)
                throw new InvalidOperationException("The entity " + id  + " cannot be deleted because of it is referenced by another object.");
            CurrentContext.Entities.DeleteEntity(id);
        }

	    /// <summary>
	    /// Delete the entity specified by GUID.
	    /// </summary>
	    /// <param name="contentType"></param>
	    /// <param name="entityGuid">Entity GUID</param>
	    /// <param name="appId"></param>
	    /// <exception cref="ArgumentNullException">Entity does not exist</exception>
	    /// <exception cref="InvalidOperationException">Entity cannot be deleted for example when it is referenced by another object</exception>
	    public void Delete(string contentType, Guid entityGuid, int? appId = null)
        {
            if (appId.HasValue)
                AppId = appId.Value;
            var entity = CurrentContext.Entities.GetEntity(entityGuid);
            Delete(contentType, entity.EntityID);
        }


        #endregion


        #region History

	    [HttpGet]
	    public List<DbVersioning.EntityHistoryItem> History(int appId, int entityId)
	    {
            SetAppIdAndUser(appId);
            var versions = CurrentContext.Versioning.GetEntityHistory(entityId);
	        return versions;
	    }

	    [HttpGet]
	    public dynamic HistoryDetails(int appId, int entityId, int changeId)
	    {
            SetAppIdAndUser(appId);
            var result = CurrentContext.Versioning.GetEntityVersionValues(entityId, changeId, null, null);
	        return result;
	    }

	    [HttpGet]
	    public bool HistoryRestore(int appId, int entityId, int changeId)
	    {
	        var DefaultCultureDimension = 0;
            throw  new Exception("this is not tested yet!");
            SetAppIdAndUser(appId);
            CurrentContext.Versioning.RestoreEntityVersion(entityId, changeId, DefaultCultureDimension);
	        return true;
        }
        #endregion
    }
}