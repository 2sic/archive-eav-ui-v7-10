/* 
 * Field: Custom - Default (basically something you should never see)
 */

angular.module("eavFieldTemplates")
    .config(function (formlyConfigProvider, defaultFieldWrappers) {

        formlyConfigProvider.setType({
            name: "custom-gps",
            templateUrl: "fields/custom/custom-gps.html",
            wrapper: defaultFieldWrappers
        });

    });