/* 
 * Field: String - Disabled
 */

angular.module("eavFieldTemplates")
    .config(function (formlyConfigProvider, defaultFieldWrappers) {

        formlyConfigProvider.setType({
            name: "string-disabled",
            template: "<input class=\"form-control input-lg\" ng-model=\"value.Value\" ng-disabled='true'>",
            wrapper: defaultFieldWrappers
        });

    });