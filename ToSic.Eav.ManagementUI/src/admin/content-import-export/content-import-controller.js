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
                label: translate("Content.Import.Form.File.Label"),
                required: true
            }     
        }, {
            // File references
            key: "FileReferences",
            type: "radio",
            templateOptions: {
                label: translate("Content.Import.Form.FileReferences.Label"),
                options: [{
                    "name":  translate("Content.Import.Form.FileReferences.Option.Keep"),
                    "value": "Keep"
                }, {
                    "name":  translate("Content.Import.Form.FileReferences.Option.Resolve"),
                    "value": "Resolve"
                }],
            },
            defaultValue: "Keep"
        }, {
            key: "ClearEntities",
            type: "radio",
            templateOptions: {
                label: translate("Content.Import.Form.ClearEntities.Label"),
                options: [{
                    "name":  translate("Content.Import.Form.ClearEntities.Option.None"),
                    "value": "None"
                }, {
                    "name":  translate("Content.Import.Form.ClearEntities.Option.All"),
                    "value": "All"
                }],
            }
        }];


        vm.formValues = { };


        vm.close = function () {
            $modalInstance.dismiss("cancel");
        };

        vm.submit = function () {
            
        };
    }
} ());