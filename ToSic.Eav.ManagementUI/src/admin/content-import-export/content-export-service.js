(function () {

    angular.module("ContentExportApp")
         .factory("contentExportService", contentExportService);


    function contentExportService($http, eavConfig) {
        var srvc = {
            exportContent: exportContent,
        };
        return srvc;

        function exportContent(args) {
            var url = eavConfig.getUrlPrefix("api") + "/eav/ContentExport/ExportContent";
            window.open(url + "?appId=" + args.AppId + "&language=" + args.Language + "&defaultLanguage=" + args.DefaultLanguage + "&contentType=" + args.ContentType + "&recordExport=" + args.RecordExport + "&resourcesReferences=" + args.ResourcesReferences + "&languageReferences=" + args.LanguageReferences, "_self", "");
        }
    }
}());