using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Http;
using Microsoft.Practices.Unity;
using ToSic.Eav.Data;
using ToSic.Eav.DataSources;
using ToSic.Eav.DataSources.Caches;
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
            var cache = DataSource.GetCache(null, appId) as BaseCache; // DataSource.GetInitialDataSource(appId: appId).Cache as DataSources.Caches.BaseCache;
            var allTypes = cache.GetContentTypes().Select(t => t.Value);

            var filteredType = allTypes.Where(t => t.Scope == scope).OrderBy(t => t.Name).Select(t => new {
                t.Name,
                t.StaticName,
                t.Scope,
                t.Description,
                DefinitionSet = t.UsesConfigurationOfAttributeSet,
                Ghost = t.UsesConfigurationOfAttributeSet == null,
                Items = cache.LightList.Count(i => i.Type == t),
                Fields = (t as ContentType).AttributeDefinitions.Count //t.AttributeSetId
            });

            return filteredType;
	    }

        [HttpGet]
	    public dynamic Get(int appId, string contentTypeId, string scope = null)
	    {
            return new { Id = 17, Guid = "...", Name = "Some ContentType" };
	    }

	    [HttpDelete]
	    public bool Delete(int appId, string staticName)
	    {
            AppId = appId;
            CurrentContext.UserName = System.Web.HttpContext.Current.User.Identity.Name;
            CurrentContext.ContentType.Delete(staticName);
	        return true;
	    }

	    [HttpPost]
	    public bool Save(int appId, Dictionary<string, string> item)
	    {
            AppId = appId;
            CurrentContext.UserName = System.Web.HttpContext.Current.User.Identity.Name;
            CurrentContext.ContentType.AddOrUpdate(item["StaticName"], item["Scope"], item["Name"], item["Description"], null, false);
	        return true;
	    }

        /// <summary>
        /// Returns the configuration for a content type
        /// </summary>
        [HttpGet]
        public IEnumerable<dynamic> GetFields(int appId, string staticName)
        {
            AppId = appId;
            return CurrentContext.ContentType.GetContentTypeConfiguration(staticName);
        }

    }
}