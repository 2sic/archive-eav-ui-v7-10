using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Http;
using Microsoft.Practices.Unity;
using ToSic.Eav.DataSources;
using ToSic.Eav.Serializers;
using ToSic.Eav.Persistence;

namespace ToSic.Eav.WebApi
{
	/// <summary>
	/// Web API Controller for ContentTypes
	/// </summary>
	public class ContentTypeController : Eav3WebApiBase
    {

        // todo: commands I'll need

        [HttpGet]
	    public IEnumerable<dynamic> Get(int appId, string scope = null, bool withStatistics = false)
        {
            // scope can be null (eav) or alternatives would be "System", "2SexyContent-System", "2SexyContent-App", "2SexyContent"
            var ds = DataSource.GetInitialDataSource(appId: appId).Cache as DataSources.Caches.BaseCache;
            var allTypes = ds.GetContentTypes().Select(t => t.Value);

            var filteredType = allTypes.Where(t => t.Scope == scope).OrderBy(t => t.Name).Select(t => new {
                t.Name,
                t.StaticName,
                t.Scope,
                t.Description,
                DefinitionSet = t.UsesConfigurationOfAttributeSet,
                Ghost = t.UsesConfigurationOfAttributeSet == null,
                Items = ds.LightList.Count(i => i.Type == t),
                Fields = t.AttributeSetId
            });

            return filteredType;
	    }

        [HttpGet]
	    public dynamic Get(int appId, string contentTypeId, string scope = null)
	    {
            return new { Id = 17, Guid = "...", Name = "Some ContentType" };
	    }

	    [HttpDelete]
	    public bool Delete(int zoneId, int appId, int contentTypeId)
	    {
	        return false;
	    }

	    [HttpPost]
	    public bool Save(int appId, Dictionary<string, object> item)
	    {
            AppId = appId;
            CurrentContext.ContentType.AddOrUpdate(item["StaticName"].ToString(), item["Name"].ToString(), item["Description"].ToString(), null, false);
	        return true;
	    }

	    [HttpPost]
	    public bool CreateShadow(int zoneId, int appId, int sourceAppId, string sourceContentTypeId)
	    {
	        return false;
	    }


    }
}