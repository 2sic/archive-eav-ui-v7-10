using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using ToSic.Eav.Data;
using ToSic.Eav.DataSources.Caches;

namespace ToSic.Eav.WebApi
{
	/// <summary>
	/// Web API Controller for ContentTypes
	/// </summary>
	public class ContentTypeController : Eav3WebApiBase
    {
        #region Content-Type Get, Delete, Save
        [HttpGet]
	    public IEnumerable<dynamic> Get(int appId, string scope = null, bool withStatistics = false)
        {
            // scope can be null (eav) or alternatives would be "System", "2SexyContent-System", "2SexyContent-App", "2SexyContent"
            var cache = DataSource.GetCache(null, appId) as BaseCache;
            var allTypes = cache.GetContentTypes().Select(t => t.Value);

            var filteredType = allTypes.Where(t => t.Scope == scope).OrderBy(t => t.Name).Select(t => new {
                Id = t.AttributeSetId,
                t.Name,
                t.StaticName,
                t.Scope,
                t.Description,
                DefinitionSet = t.UsesConfigurationOfAttributeSet,
                Ghost = t.UsesConfigurationOfAttributeSet == null,
                Items = cache.LightList.Count(i => i.Type == t),
                Fields = (t as ContentType).AttributeDefinitions.Count 
            });

            return filteredType;
	    }

        [HttpGet]
	    public IContentType Get(int appId, string contentTypeId, string scope = null)
	    {
            SetAppIdAndUser(appId);
            // var source = InitialDS;
            var cache = DataSource.GetCache(null, appId);
            return cache.GetContentType(contentTypeId);
        }

	    [HttpDelete]
	    public bool Delete(int appId, string staticName)
	    {
            SetAppIdAndUser(appId);
            CurrentContext.ContentType.Delete(staticName);
	        return true;
	    }

	    [HttpPost]
	    public bool Save(int appId, Dictionary<string, string> item)
	    {
            SetAppIdAndUser(appId);
            CurrentContext.ContentType.AddOrUpdate(item["StaticName"], item["Scope"], item["Name"], item["Description"], null, false);
	        return true;
	    }
        #endregion

        #region Fields - Get, Reorder, Data-Types (for dropdown), etc.
        /// <summary>
        /// Returns the configuration for a content type
        /// </summary>
        [HttpGet]
        public IEnumerable<dynamic> GetFields(int appId, string staticName)
        {
            SetAppIdAndUser(appId);
            return CurrentContext.ContentType.GetContentTypeConfiguration(staticName);
        }

        [HttpGet]
        public bool Reorder(int appId, int contentTypeId, int attributeId, string direction)
        {
            SetAppIdAndUser(appId);
            CurrentContext.ContentType.Reorder(contentTypeId, attributeId, direction);
            return true;
        }

	    [HttpGet]
	    public string[] DataTypes(int appId)
	    {
            SetAppIdAndUser(appId);
	        return CurrentContext.SqlDb.AttributeTypes.OrderBy(a => a.Type).Select(a => a.Type).ToArray();
	    }

        [HttpGet]
	    public int AddField(int appId, int contentTypeId, string staticName, string type, int sortOrder)
	    {
            SetAppIdAndUser(appId);
	        return CurrentContext.Attributes.AddAttribute(contentTypeId, staticName, type, sortOrder, 1, false, true).AttributeID;
	        throw new HttpUnhandledException();
	    }

        [HttpDelete]
	    public bool DeleteField(int appId, int contentTypeId, int attributeId)
	    {
            SetAppIdAndUser(appId);
            // todo: add security check if it really is in this app and content-type
            return CurrentContext.Attributes.RemoveAttribute(attributeId);
	    }

        [HttpGet]
	    public void SetTitle(int appId, int contentTypeId, int attributeId)
	    {
            SetAppIdAndUser(appId);
            CurrentContext.Attributes.SetTitleAttribute(attributeId, contentTypeId);
	    }

        #endregion
    }
}