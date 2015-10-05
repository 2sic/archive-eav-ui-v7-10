(function () {

    angular.module("ContentImportApp")
        .controller("ContentImport", contentImportController);

    function contentImportController(appId, contentType, contentImportService, eavAdminDialogs, eavConfig, languages, debugState, $modalInstance, $filter) {
        var translate = $filter("translate");

        var vm = this;
        vm.debug = debugState;

        vm.formValues = {};

        vm.formFields = [{
            // Content type
            key: "AppId",
            type: "hidden",
            defaultValue: appId
        }, {
            // Default / fallback language
            key: "DefaultLanguage",
            type: "hidden",
            defaultValue: $filter("isoLangCode")(languages.defaultLanguage)
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

        vm.viewStates = {
            "Waiting":   0,
            "Default":   1,
            "Evaluated": 2,
            "Imported":  3
        };

        vm.viewStateSelected = vm.viewStates.Default;


        vm.evaluationResult = { };

        vm.importResult = { };


        vm.evaluateContent = function evaluateContent() {
            vm.viewStateSelected = vm.viewStates.Waiting;
            return contentImportService.evaluateContent(vm.formValues).then(function (result) {
                vm.evaluationResult = result.data;
                vm.viewStateSelected = vm.viewStates.Evaluated;
            });
        };

        vm.importContent = function importContent() {
            vm.viewStateSelected = vm.viewStates.Waiting;
            return contentImportService.importContent(vm.formValues).then(function (result) {
                vm.importResult = result.data;
                vm.viewStateSelected = vm.viewStates.Imported;
            });
        };

        vm.reset = function reset() {
            vm.formValues = { };
            vm.evaluationResult = { };
            vm.importResult = { };
        };

        vm.back = function back() {
            vm.viewStateSelected = vm.viewStates.Default;
        };

        vm.close = function close() {
            vm.viewStateSelected = vm.viewStates.Default;
            $modalInstance.dismiss("cancel");
        };
    }
}());