(function () {

    angular.module("ContentExportApp")
        .controller("ContentExport", contentExportController);

    function contentExportController(appId, contentType, itemIds, contentExportService, eavAdminDialogs, eavConfig, languages, $uibModalInstance, $filter, $translate) {

        var vm = this;

        vm.formValues = {};

        // check if we were given some IDs to export only that
        var hasIdList = (Array.isArray(itemIds) && itemIds.length > 0);
        var cSelection = "Selection";

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
            key: "Language",
            type: "select",
            expressionProperties: {
                "templateOptions.label": "'Content.Export.Fields.Language.Label' | translate",
                "templateOptions.options": function () {
                    var options = [{
                        "name": $translate.instant("Content.Export.Fields.Language.Options.All"),
                        "value": ""
                    }];
                    angular.forEach(languages.languages, function (lang) {
                        var langCode = $filter("isoLangCode")(lang.key);
                        options.push({ "name": langCode, "value": langCode });
                    });
                    return options;
                }
            },
            defaultValue: ""
        }, {
            key: "RecordExport",
            type: "radio",
            expressionProperties: {
                "templateOptions.label": "'Content.Export.Fields.RecordExport.Label' | translate",
                "templateOptions.options": function () {
                    var opts = [{
                        "name": $translate.instant("Content.Export.Fields.RecordExport.Options.Blank"),
                        "value": "Blank"
                    }, {
                        "name": $translate.instant("Content.Export.Fields.RecordExport.Options.All"),
                        "value": "All"
                    }];
                    if (hasIdList)
                        opts.push({
                            "name": $translate.instant("Content.Export.Fields.RecordExport.Options.Selection", { count: itemIds.length }), // "todo: selected " + itemIds.length + " items",
                            "value": cSelection
                        });
                    return opts;
                }
            },
            defaultValue: hasIdList ? cSelection : "All"
        }, {
            // Language references
            key: "LanguageReferences",
            type: "radio",
            expressionProperties: {
                "templateOptions.label": "'Content.Export.Fields.LanguageReferences.Label' | translate",
                "templateOptions.disabled": function () {
                    return vm.formValues.RecordExport === "Blank";
                },
                "templateOptions.options": function () {
                    return [{
                        "name": $translate.instant("Content.Export.Fields.LanguageReferences.Options.Link"),
                        "value": "Link"
                    }, {
                        "name": $translate.instant("Content.Export.Fields.LanguageReferences.Options.Resolve"),
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
                "templateOptions.disabled": function () {
                    return vm.formValues.RecordExport === "Blank";
                },
                "templateOptions.options": function () {
                    return [{
                        "name": $translate.instant("Content.Export.Fields.ResourcesReferences.Options.Link"),
                        "value": "Link"
                    }, {
                        "name": $translate.instant("Content.Export.Fields.ResourcesReferences.Options.Resolve"),
                        "value": "Resolve"
                    }];
                }
            },
            defaultValue: "Link"
        }];


        vm.exportContent = function exportContent() {
            contentExportService.exportContent(vm.formValues, hasIdList && vm.formValues.RecordExport === cSelection ? itemIds : null);
        };

        vm.close = function close() {
            $uibModalInstance.dismiss("cancel");
        };
    }
}());