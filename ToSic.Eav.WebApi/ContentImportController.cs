using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Http;
using ToSic.Eav.BLL;
using ToSic.Eav.Data;
using ToSic.Eav.DataSources.Caches;
using ToSic.Eav.Persistence;
using ToSic.Eav.Serializers;


namespace ToSic.Eav.WebApi
{




    public class ContentImportController : ApiController
    {
        public struct ContentEvaluateArgs {
            public string AppId;
            public string ContentType;
            public string ContentBase64;
        }

        public struct ContentImportArgs {
            public string AppId;
            public string ContentType;
            public string ContentBase64;
        }


        [HttpPost]
        public dynamic EvaluateContent(ContentEvaluateArgs args)
        {
            var contentBytes = Convert.FromBase64String(args.ContentBase64);
            var contentString = Encoding.UTF8.GetString(contentBytes);
            return new { };
        }


        [HttpPost]
        public dynamic ImportContent(ContentImportArgs args)
        {
            return new { };
        }
    }
}
