/* 
 * Field: Boolean - Default
 */

angular.module("eavFieldTemplates")
    .config(function(formlyConfigProvider) {
        formlyConfigProvider.setType({
            name: "boolean-default",
            templateUrl: "fields/templates/boolean-default.html",
            wrapper: ["eavLabel", "bootstrapHasError", "eavLocalization"]
        });
    });