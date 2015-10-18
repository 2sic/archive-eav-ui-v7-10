/* 
 * Field: String - Disabled
 */

angular.module("eavFieldTemplates")
    .config(function (formlyConfigProvider, defaultFieldWrappers) {

        formlyConfigProvider.setType({
            name: "string-disabled",
            template: "<div>disb<input class=\"form-control input-lg\" ng-model=\"value.Value\" ng-disabled='true'> {{value.Value}} </div>",
            wrapper: defaultFieldWrappers
        });

    });