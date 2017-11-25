(function () {

    angular.module('ContentExportApp')
         .factory('contentExportService', contentExportService);


    function contentExportService($http, eavConfig) {
        return {
            exportContent: exportContent,
            exportJson: exportJson
        };

        function exportContent(args, selectedIds) {
            var url = eavConfig.getUrlPrefix('api') + '/eav/ContentExport/ExportContent',
                addids = selectedIds ? '&selectedids=' + selectedIds.join() : '',
                fullUrl = url
                    + '?appId=' + args.AppId
                    + '&language=' + args.Language
                    + '&defaultLanguage=' + args.DefaultLanguage
                    + '&contentType=' + args.ContentType
                    + '&recordExport=' + args.RecordExport
                    + '&resourcesReferences=' + args.ResourcesReferences
                    + '&languageReferences=' + args.LanguageReferences
                    + addids;

            window.open(fullUrl, '_blank', '');
        }

        function exportJson(appId, typeName) {
            var url = eavConfig.getUrlPrefix('api')
                + '/eav/ContentExport/DownloadTypeAsJson'
                + '?appId=' + appId
                + '&name=' + typeName;

            window.open(url, '_blank', '');
        }
    }
}());