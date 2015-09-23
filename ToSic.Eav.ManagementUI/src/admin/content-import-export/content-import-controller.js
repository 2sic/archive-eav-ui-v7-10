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
            // File / page references
            key: "ResourcesReferences",
            type: "radio",
            expressionProperties: {
                "templateOptions.label": "'Content.Import.Fields.ResourcesReferences.Label' | translate",
                "templateOptions.options": function () {
                    return [{
                        "name": translate("Content.Import.Fields.ResourcesReferences.Options.Keep"),
                        "value": "Keep"
                    }, {
                        "name": translate("Content.Import.Fields.ResourcesReferences.Options.Resolve"),
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


        vm.evaluation = {};
        vm.evaluationInProgress = false;

        vm.import = { };
        vm.importInProgress = false;

        vm.evaluateContent = function previewContent() {
            vm.evaluation = { };
            vm.evaluationInProgress = true;
            return contentImportService.evaluateContent(vm.formValues).then(function (result) {
                vm.evaluation = result.data;
                vm.evaluationInProgress = true;
            });
        };

        vm.importContent = function importContent() {
            vm.import = { };
            vm.importInProgress = true;
            return contentImportService.importContent(vm.formValues).then(function (result) {
                vm.import = result.data;
                vm.importInProgress = false;
            });
        };

        vm.close = function close() {
            $modalInstance.dismiss("cancel");
        };
    }
}());