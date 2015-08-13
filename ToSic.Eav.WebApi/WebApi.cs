using System;
using System.Collections.Generic;
using ToSic.Eav.DataSources;

namespace ToSic.Eav.WebApi
{
	/// <summary>
	/// Web API Controller for the Pipeline Designer UI
	/// </summary>
	public class WebApi : Eav3WebApiBase
    {
        public WebApi(int appId) : base(appId) { }
        public WebApi() : base() { }

        #region GetOne GetAll calls
        public Dictionary<string, object> GetOne(string contentType, int id, int? appId = null, string cultureCode = null)
	    {
            if (appId.HasValue)
                AppId = appId.Value;

            var found = InitialDS.List[id];
            if (found.Type.Name == contentType)
                return Serializer.Prepare(found);
	        throw new KeyNotFoundException("Can't find " + id + "of type '" + contentType + "'");
	    }

        /// <summary>
		/// Get all Entities of specified Type
		/// </summary>
		public IEnumerable<Dictionary<string, object>> GetEntities(string contentType, string cultureCode = null, int? appId = null)
		{
            if (appId.HasValue)
                AppId = appId.Value;

			var source = InitialDS;
			var typeFilter = DataSource.GetDataSource<EntityTypeFilter>(appId: appId, upstream: source);
			typeFilter.TypeName = contentType;

            return Serializer.Prepare(typeFilter.LightList);//  typeFilter.List.Select(t => Helpers.GetEntityValues(t.Value, cultureCode: cultureCode));
		}
        #endregion

        #region Todo: Create / Edit Calls

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
            if (found.Type.Name != contentType)
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

        #region AssignedEntities - these are entities which have been assigned to something else
        /// <summary>
		/// Get Entities with specified AssignmentObjectTypeId and Key
		/// </summary>
		public IEnumerable<Dictionary<string, object>> GetAssignedEntities(int assignmentObjectTypeId, Guid keyGuid, string contentType, int? appId = null)
		{
            if (appId.HasValue)
                AppId = appId.Value;
            var entityList = MetaDS.GetAssignedEntities(assignmentObjectTypeId, keyGuid, contentType);
		    return Serializer.Prepare(entityList);
		}

		/// <summary>
		/// Get Entities with specified AssignmentObjectTypeId and Key
		/// </summary>
		public IEnumerable<Dictionary<string, object>> GetAssignedEntities(int assignmentObjectTypeId, string keyString, string contentType, int? appId = null)
		{
            if (appId.HasValue)
                AppId = appId.Value; 
            var entityList = MetaDS.GetAssignedEntities(assignmentObjectTypeId, keyString, contentType);
		    return Serializer.Prepare(entityList);
		}
        #endregion


        #region Content Type Information
        /// <summary>
		/// Get a ContentType by Name
		/// </summary>
		public IContentType GetContentType(string contentType, int? appId = null, bool detailed = false)
        {
            if (appId.HasValue)
                AppId = appId.Value;

            var source = InitialDS;
			var cache = DataSource.GetCache(source.ZoneId, appId);
			var result = cache.GetContentType(contentType);

            // todo: implement abilty to deliver detailled infos

            return result;
		}

        // Todo
	    public List<IContentType> GetAllContentTypes(int? appId = null)
	    {
            if (appId.HasValue)
                AppId = appId.Value;

	        var source = InitialDS;
	        var cache = DataSource.GetCache(source.ZoneId, appId);
            throw new NotImplementedException("getcontentypes not implemented yet");
	    }

        #endregion
    }
}