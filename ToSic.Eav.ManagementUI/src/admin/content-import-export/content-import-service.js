(function () {

    angular.module("ContentImportApp")
         .factory("contentImportService", contentImportService);


    function contentImportService($http) {
        var srvc = {
            evaluateContent: evaluateContent,
            importContent: importContent
        };

        return srvc;

        function evaluateContent(args) {

            return $http.post("eav/ContentImport/EvaluateContent", { AppId: args.AppId, ContentType: args.ContentType, ContentBase64: args.File.base64, ResourcesReferences: args.ResourcesReferences, ClearEntities: args.ClearEntities });
        }

        function importContent(args) {

        }
    }
}());