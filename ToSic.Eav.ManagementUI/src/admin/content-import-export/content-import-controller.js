(function () {

    angular.module("ContentImportApp")
        .controller("ContentImport", contentImportController);


    function contentImportController(appId, contentType, contentImportService, eavAdminDialogs, eavConfig, $modalInstance, $filter) {
        var translate = $filter("translate");

        var vm = this;

        vm.debug = {};

        vm.formValues = { };

        vm.formFields = [{
            // Content type
            key: "AppId",
            type: "hidden",
            defaultValue: appId
        }, {
            // Content type
            key: "ContentType",
            type: "hidden",
            defaultValue: contentType
        }, {
            // File
            key: "File",
            type: "file",
            templateOptions: {
                required: true
            },
            expressionProperties: {
                "templateOptions.label": "'Content.Import.Fields.File.Label' | translate"
            }
        }, {
            // File references
            key: "FileReferences",
            type: "radio",
            expressionProperties: {
                "templateOptions.label": "'Content.Import.Fields.FileReferences.Label' | translate",
                "templateOptions.options": function () {
                    return [{
                        "name": translate("Content.Import.Fields.FileReferences.Options.Keep"),
                        "value": "Keep"
                    }, {
                        "name": translate("Content.Import.Fields.FileReferences.Options.Resolve"),
                        "value": "Resolve"
                    }];
                }
            },
            defaultValue: "Keep"
        }, {
            // Clear entities
            key: "ClearEntities",
            type: "radio",
            expressionProperties: {
                "templateOptions.label": "'Content.Import.Fields.ClearEntities.Label' | translate",
                "templateOptions.options": function () {
                    return [{
                        "name": translate("Content.Import.Fields.ClearEntities.Options.None"),
                        "value": "None"
                    }, {
                        "name": translate("Content.Import.Fields.ClearEntities.Options.All"),
                        "value": "All"
                    }];
                }
            },
            defaultValue: "None"
        }];

        vm.currentStep = "1"; // 1, 2, 3...

        vm.evaluateContent = function previewContent() {
            contentImportService.evaluateContent(vm.formValues).then(function (result) { vm.debug = result; });
        };

        vm.importContent = function importContent() {

        };

        vm.close = function close() {
            $modalInstance.dismiss("cancel");
        };
    }
}());