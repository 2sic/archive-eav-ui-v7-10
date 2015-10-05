(function () {

    angular.module("ContentExportApp")
         .factory("contentExportService", contentExportService);


    function contentExportService() {
        var srvc = {
            exportContent: exportContent,
        };
        return srvc;

        function exportContent(args) {
            window.open("/api/eav/ContentExport/ExportContent?appId=" + args.AppId + "&language=" + args.Language + "&defaultLanguage=" + args.DefaultLanguage + "&contentType=" + args.ContentType + "&recordExport=" + args.RecordExport + "&resourcesReferences=" + args.ResourcesReferences + "&languageReferences=" + args.LanguageReferences, "_self", "");
        }
    }
}());