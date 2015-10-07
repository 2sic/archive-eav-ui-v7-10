/* 
 * Field: String - Default
 */

angular.module("eavFieldTemplates")
    .config(function(formlyConfigProvider) {

        formlyConfigProvider.setType({
            name: "string-default",
            template: "<input class=\"form-control\" ng-model=\"value.Value\">",
            wrapper: ["eavLabel", "bootstrapHasError", "eavLocalization"]
        });

    });