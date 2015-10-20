(function () {

    angular.module("ContentExportApp")
         .factory("contentExportService", contentExportService);


    function contentExportService($http, sxc) {
        var srvc = {
            exportContent: exportContent,
        };
        return srvc;

        function exportContent(args) {
            var url = sxc.resolveServiceUrl("eav/ContentExport/ExportContent");
            //$http.get("eav/ContentExport/ExportContent?appId=" + args.AppId + "&language=" + args.Language + "&defaultLanguage=" + args.DefaultLanguage + "&contentType=" + args.ContentType + "&recordExport=" + args.RecordExport + "&resourcesReferences=" + args.ResourcesReferences + "&languageReferences=" + args.LanguageReferences, "_self");
            window.open(/* apiRoot + "eav/ContentExport/ExportContent */ url + "?appId=" + args.AppId + "&language=" + args.Language + "&defaultLanguage=" + args.DefaultLanguage + "&contentType=" + args.ContentType + "&recordExport=" + args.RecordExport + "&resourcesReferences=" + args.ResourcesReferences + "&languageReferences=" + args.LanguageReferences, "_self", "");
        }
    }
}());