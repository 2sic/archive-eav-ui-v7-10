(function () {

    angular.module("ContentImportApp")
        .controller("ContentImport", contentImportController);


    function contentImportController(appId, eavAdminDialogs, eavConfig, $modalInstance, $filter, $translate) {
        var vm = this;

        var translate = $filter("translate");


        vm.formFields = [{
            // File
            key: "File",
            type: "file",
            templateOptions: {
                required: true
            },
            expressionProperties: {
                "templateOptions.label": "'Content.Import.Form.File.Label' | translate"
            }
        }, {
            // File references
            key: "FileReferences",
            type: "radio",
            expressionProperties: {
                "templateOptions.label": "'Content.Import.Form.FileReferences.Label' | translate",
                "templateOptions.options": function () {
                    return [{
                        "name": translate("Content.Import.Form.FileReferences.Option.Keep"),
                        "value": "Keep"
                    }, {
                        "name": translate("Content.Import.Form.FileReferences.Option.Resolve"),
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
                "templateOptions.label": "'Content.Import.Form.ClearEntities.Label' | translate",
                "templateOptions.options": function () {
                    return [{
                        "name": translate("Content.Import.Form.ClearEntities.Option.None"),
                        "value": "None"
                    }, {
                        "name": translate("Content.Import.Form.ClearEntities.Option.All"),
                        "value": "All"
                    }];
                }
            },
            defaultValue: "None"
        }];


        vm.formValues = {};


        vm.close = function () {
            $modalInstance.dismiss("cancel");
        };

        vm.submit = function () {

        };
    }
}());