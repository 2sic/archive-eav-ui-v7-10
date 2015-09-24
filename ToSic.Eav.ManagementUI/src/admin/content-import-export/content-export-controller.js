(function () {

    angular.module("ContentExportApp")
        .controller("ContentExport", contentExportController);

    function contentExportController(appId, defaultLanguage, contentType, contentExportService, eavAdminDialogs, eavConfig, $modalInstance, $filter) {
        var translate = $filter("translate");

        var vm = this;

        vm.formValues = { };

        vm.formFields = [{
            // Content type
            key: "AppId",
            type: "hidden",
            templateOptions: { label: "" },
            defaultValue: appId
        }, {
            // Default / fallback language
            key: "DefaultLanguage",
            type: "hidden",
            templateOptions: { label: "" },
            defaultValue: defaultLanguage.substring(0, 2).toLowerCase() + "-" + defaultLanguage.substring(3, 5).toUpperCase()
        }, {
            // Content type
            key: "ContentType",
            type: "hidden",
            templateOptions: { label: "" },
            defaultValue: contentType
        }];

        vm.exportContent = function exportContent() {

        };


        vm.close = function close() {
            $modalInstance.dismiss("cancel");
        };
    }
}());