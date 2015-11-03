/* 
 * Field: Custom - Default (basically something you should never see)
 */

angular.module("eavFieldTemplates")
    .config(function (formlyConfigProvider, defaultFieldWrappers) {

        formlyConfigProvider.setType({
            name: "custom-default",
            templateUrl: "fields/custom/custom-default.html",
            wrapper: defaultFieldWrappers
        });

    });