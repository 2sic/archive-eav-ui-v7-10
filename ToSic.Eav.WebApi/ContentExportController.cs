using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using ToSic.Eav.ImportExport.Refactoring;
using ToSic.Eav.ImportExport.Refactoring.Options;

namespace ToSic.Eav.WebApi
{
    public class ContentExportController : Eav3WebApiBase
    {
        public class ContentExportArgs
        {
            public int AppId;

            public string DefaultLanguage;

            public string Language;

            public RecordExport RecordExport;

            public ResourceReferenceExport ResourcesReferences;

            public LanguageReferenceExport LanguageReferences;

            public string ContentType;
        }


        public HttpResponseMessage ExportContent(ContentExportArgs args)
        {
            AppId = args.AppId;

            var fileContent = default(string);

            var contentTypeId = GetContentTypeId(args.ContentType);
            var contextLanguages = GetContextLanguages();

            var export = new XmlExport();
            if (args.RecordExport.IsBlank())
            {
                fileContent = export.CreateBlankXml(CurrentContext.ZoneId, args.AppId, contentTypeId, "");
            }
            else
            {
                fileContent = export.CreateXml(CurrentContext.ZoneId, args.AppId, contentTypeId, args.Language ?? "", args.DefaultLanguage, contextLanguages, args.LanguageReferences, args.ResourcesReferences);
            }

            HttpResponseMessage response = XXX(args, fileContent);
            return response;
        }

        private HttpResponseMessage XXX(ContentExportArgs args, string fileContent)
        {
            var fileName = string.Format
                        (
                            "2sxc {0} {1} {2} {3}.xml",
                            args.ContentType.Replace(" ", "-"),
                            args.Language.Replace(" ", "-"),
                            args.RecordExport.IsBlank() ? "Template" : "Data",
                            DateTime.Now.ToString("yyyyMMddHHmmss")
                        );

            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StreamContent(GetStreamFromString(fileContent));
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
            response.Content.Headers.ContentDisposition.FileName = fileName;
            response.Content.Headers.ContentLength = fileContent.Length;
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/xml");
            return response;
        }

        public Stream GetStreamFromString(string str)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(str);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        private int GetContentTypeId(string name)
        {
            return CurrentContext.AttribSet.GetAttributeSetId(name, null);
        }

        private string[] GetContextLanguages()
        {
            return CurrentContext.Dimensions.GetLanguages().Select(language => language.ExternalKey).ToArray();
        }
    }
}
