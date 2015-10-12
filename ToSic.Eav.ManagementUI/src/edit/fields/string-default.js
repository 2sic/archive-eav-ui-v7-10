/* 
 * Field: String - Default
 */

angular.module("eavFieldTemplates")
    .config(function (formlyConfigProvider, defaultFieldWrappers) {

        formlyConfigProvider.setType({
            name: "string-default",
            template: "<input class=\"form-control\" ng-model=\"value.Value\">",
            wrapper: defaultFieldWrappers // ["eavLabel", "bootstrapHasError", "eavLocalization"]
        });

    });