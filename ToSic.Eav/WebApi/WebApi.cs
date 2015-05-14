using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Microsoft.Practices.Unity;
using ToSic.Eav.DataSources;
using ToSic.Eav.Serializers;

namespace ToSic.Eav.WebApi
{
	/// <summary>
	/// Web API Controller for the Pipeline Designer UI
	/// </summary>
	public class WebApi : ApiController
    {

        #region Helpers
        private IDataSource InitialDS(int appId)
	    {
            return DataSource.GetInitialDataSource(appId: appId);
	    }

	    private IMetaDataSource MetaDS(int appId)
	    {
	        return DataSource.GetMetaDataSource(appId: appId);
	    }

        // I must keep the serializer so it can be configured from outside if necessary
	    private Serializer _serializer;
	    public Serializer Serializer
	    {
	        get
	        {
	            if (_serializer == null)
	            {
	                _serializer = Factory.Container.Resolve<Serializer>();
	            }
	            return _serializer;
	        }
	    }

	    #endregion

	    public Dictionary<string, object> GetOne(int appId, string contentType, int id)
	    {
            var found = InitialDS(appId).List[id];
            if (found.Type.Name == contentType)
                return Serializer.Prepare(found);
	        throw new KeyNotFoundException("Can't find " + id + "of type '" + contentType + "'");
	    }

        /// <summary>
		/// Get all Entities of specified Type
		/// </summary>
		public IEnumerable<Dictionary<string, object>> GetEntities(int appId, string typeName, string cultureCode = null)
		{
			var source = InitialDS(appId);
			var typeFilter = DataSource.GetDataSource<EntityTypeFilter>(appId: appId, upstream: source);
			typeFilter.TypeName = typeName;

            return Serializer.Prepare(typeFilter.List);//  typeFilter.List.Select(t => Helpers.GetEntityValues(t.Value, cultureCode: cultureCode));
		}

		/// <summary>
		/// Get Entities with specified AssignmentObjectTypeId and Key
		/// </summary>
		public IEnumerable<Dictionary<string, object>> GetAssignedEntities(int appId, int assignmentObjectTypeId, Guid keyGuid, string contentTypeName)
		{
            var entityList = MetaDS(appId).GetAssignedEntities(assignmentObjectTypeId, keyGuid, contentTypeName);
		    return Serializer.Prepare(entityList);
		}

		/// <summary>
		/// Get Entities with specified AssignmentObjectTypeId and Key
		/// </summary>
		public IEnumerable<Dictionary<string, object>> GetAssignedEntities(int appId, int assignmentObjectTypeId, string keyString, string contentTypeName)
		{
            var entityList = MetaDS(appId).GetAssignedEntities(assignmentObjectTypeId, keyString, contentTypeName);
		    return Serializer.Prepare(entityList);
		}

		/// <summary>
		/// Get a ContentType by Name
		/// </summary>
		public IContentType GetContentType(int appId, string name)
		{
			var source = InitialDS(appId);
			var cache = DataSource.GetCache(source.ZoneId, appId);
			return cache.GetContentType(name);
		}
	}
}