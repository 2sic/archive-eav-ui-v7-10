(function () {

    angular.module("ContentExportApp")
        .controller("ContentExport", contentExportController);

    function contentExportController(appId, contentType, contentExportService, eavAdminDialogs, eavConfig, languages, $modalInstance, $filter) {
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
            defaultValue: $filter("caseSensitiveLanguageKey")(languages.defaultLanguage)
        }, {
            // Content type
            key: "ContentType",
            type: "hidden",
            templateOptions: { label: "" },
            defaultValue: contentType
        }, {
            key: "Language",
            type: "select",
            templateOptions: {

            },
            expressionProperties: {
                "templateOptions.label": "'Content.Export.Fields.Language.Label' | translate",
                "templateOptions.options": function () {
                    return [{
                        "name": translate("Content.Export.Fields.Language.Options.All"),
                        "value": ""
                    }];
                }
            },
            defaultValue: ""
        }, {
            key: "RecordExport",
            type: "radio",
            expressionProperties: {
                "templateOptions.label": "'Content.Export.Fields.RecordExport.Label' | translate",
                "templateOptions.options": function () {
                    return [{
                        "name": translate("Content.Export.Fields.RecordExport.Options.Blank"),
                        "value": "Blank"
                    }, {
                        "name": translate("Content.Export.Fields.RecordExport.Options.All"),
                        "value": "All"
                    }];
                }
            },
            defaultValue: "All"
        }, {
            // Language references
            key: "LanguageReferences",
            type: "radio",
            expressionProperties: {
                "templateOptions.label": "'Content.Export.Fields.LanguageReferences.Label' | translate",
                "templateOptions.options": function () {
                    return [{
                        "name": translate("Content.Export.Fields.LanguageReferences.Options.Link"),
                        "value": "Link"
                    }, {
                        "name": translate("Content.Export.Fields.LanguageReferences.Options.Resolve"),
                        "value": "Resolve"
                    }];
                }
            },
            defaultValue: "Link"
        }, {
            // File / page references
            key: "ResourcesReferences",
            type: "radio",
            expressionProperties: {
                "templateOptions.label": "'Content.Export.Fields.ResourcesReferences.Label' | translate",
                "templateOptions.options": function () {
                    return [{
                        "name": translate("Content.Export.Fields.ResourcesReferences.Options.Link"),
                        "value": "Link"
                    }, {
                        "name": translate("Content.Export.Fields.ResourcesReferences.Options.Resolve"),
                        "value": "Resolve"
                    }];
                }
            },
            defaultValue: "Link"
        }];

        vm.exportContent = function exportContent() {
            contentExportService.exportContent(formFields).then(function () { });
        };


        vm.close = function close() {
            $modalInstance.dismiss("cancel");
        };
    }
}());