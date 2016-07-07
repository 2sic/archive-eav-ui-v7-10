/* 
 * Field: Boolean - Default
 */

angular.module("eavFieldTemplates")
    .config(function (formlyConfigProvider, fieldWrappersNoLabel) {
        formlyConfigProvider.setType({
            name: "boolean-default",
            templateUrl: "fields/boolean/boolean-default.html",
            wrapper: fieldWrappersNoLabel // ["bootstrapHasError", "disablevisually", "eavLocalization", "responsive", "collapsible"]
        });
    });