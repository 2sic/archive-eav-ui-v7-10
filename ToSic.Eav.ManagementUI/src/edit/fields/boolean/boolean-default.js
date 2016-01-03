/* 
 * Field: Boolean - Default
 */

angular.module("eavFieldTemplates")
    .config(function(formlyConfigProvider) {
        formlyConfigProvider.setType({
            name: "boolean-default",
            templateUrl: "fields/boolean/boolean-default.html",
            wrapper: ["bootstrapHasError", "disablevisually", "eavLocalization", "collapsible"]
        });
    });