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
using ToSic.Eav.ImportExport.Refactoring;
using ToSic.Eav.ImportExport.Refactoring.Options;
using ToSic.Eav.Persistence;
using ToSic.Eav.Serializers;


namespace ToSic.Eav.WebApi
{




    public class ContentImportController : Eav3WebApiBase
    {
        public class ContentEvaluateArgs
        {
            public int AppId;
            public ResourceReferenceImport ResourcesReferences;
            public EntityClearImport ClearEntities;
            public string ContentType;
            public string ContentBase64;
        }

        public class ContentImportArgs
        {
            public int AppId;
            public ResourceReferenceImport ResourcesReferences;
            public EntityClearImport ClearEntities;
            public string ContentType;
            public string ContentBase64;
        }

        public class ContentEvaluationResult
        {
            public bool Succeeded;

            public dynamic Details;

            public ContentEvaluationResult(bool succeeded, dynamic details) { Succeeded = succeeded; Details = details; }
        }

        public class ContentImportResult
        {
            public bool Succeeded;

            public ContentImportResult(bool succeeded) { Succeeded = succeeded; }
        }


        [HttpPost]
        public ContentEvaluationResult EvaluateContent(ContentEvaluateArgs args)
        {
            AppId = args.AppId;

            using (var contentSteam = new MemoryStream(Convert.FromBase64String(args.ContentBase64)))
            {
                var contentTypeId = GetContentTypeId(args.ContentType);
                var contextLanguages = GetContextLanguages();

                var import = new XmlImport(CurrentContext.ZoneId, args.AppId, contentTypeId, contentSteam, contextLanguages, contextLanguages[0], args.ClearEntities, args.ResourcesReferences);
                if (import.ErrorProtocol.HasErrors)
                {
                    return new ContentEvaluationResult(!import.ErrorProtocol.HasErrors, import.ErrorProtocol.Errors);
                }
                else
                {
                    return new ContentEvaluationResult(import.ErrorProtocol.HasErrors, new { AmountOfEntitiesCreated = import.AmountOfEntitiesCreated, AmountOfEntitiesDeleted = import.AmountOfEntitiesDeleted, AmountOfEntitiesUpdated = import.AmountOfEntitiesUpdated, AttributeNamesInDocument = import.AttributeNamesInDocument, AttributeNamesInContentType = import.AttributeNamesInContentType, AttributeNamesNotImported = import.AttributeNamesNotImported, DocumentElementsCount = import.DocumentElements.Count(), LanguagesInDocumentCount = import.LanguagesInDocument.Count() });
                }
            }
        }

        [HttpPost]
        public ContentImportResult ImportContent(ContentImportArgs args)
        {
            AppId = args.AppId;

            using (var contentSteam = new MemoryStream(Convert.FromBase64String(args.ContentBase64)))
            {
                var contentTypeId = GetContentTypeId(args.ContentType);
                var contextLanguages = GetContextLanguages();

                var import = new XmlImport(CurrentContext.ZoneId, args.AppId, contentTypeId, contentSteam, contextLanguages, contextLanguages[0], args.ClearEntities, args.ResourcesReferences);
                if (!import.ErrorProtocol.HasErrors)
                {
                    import.PersistImportToRepository(CurrentContext.UserName);
                }
                return new ContentImportResult(!import.ErrorProtocol.HasErrors);
            }
        }


        private string[] GetContextLanguages()
        {
            return CurrentContext.Dimensions.GetLanguages().Select(language => language.ExternalKey).ToArray();
        }

        private int GetContentTypeId(string name)
        {
            return CurrentContext.AttribSet.GetAttributeSetId(name, null);
        }
    }
}
