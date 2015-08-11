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
	public class ContentTypeController : ApiController
    {

        // todo: commands I'll need

        [HttpGet]
	    public IEnumerable<dynamic> Get(int zoneId, int appId, string scope = "")
	    {
            return new[] { 
                new { Id = 17, Name = "Some ContentType" }
            };
	    }

        [HttpGet]
	    public dynamic Get(int zoneId, int appId, int contentTypeId, string scope = "")
	    {
            return new { Id = 17, Guid = "...", Name = "Some ContentType" };
	    }

	    [HttpDelete]
	    public bool Delete(int zoneId, int appId, int contentTypeId)
	    {
	        return false;
	    }

	    [HttpPost]
	    public bool Create(int zoneId, int appId, string contentTypeName, string scope = "")
	    {
	        return false;
	    }

	    [HttpPost]
	    public bool CreateShadow(int zoneId, int appId, int sourceAppId, string sourceContentTypeId)
	    {
	        return false;
	    }


    }
}