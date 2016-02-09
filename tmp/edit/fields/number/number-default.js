/* 
 * Field: Number - Default
 */

angular.module("eavFieldTemplates")
    .config(function (formlyConfigProvider, defaultFieldWrappers) {
        formlyConfigProvider.setType({
            name: "number-default",
            template: "<input type=\"number\" class=\"form-control input-lg\" ng-model=\"value.Value\">{{vm.isGoogleMap}}",
            wrapper: defaultFieldWrappers,
            defaultOptions: {
                ngModelAttrs: {
                    '{{to.settings.merged.Min}}': { value: "min" },
                    '{{to.settings.merged.Max}}': { value: "max" },
                    '{{to.settings.merged.Decimals ? "^[0-9]+(\.[0-9]{1," + to.settings.merged.Decimals + "})?$" : null}}': { value: "pattern" }
                }
            },
            controller: "FieldTemplate-NumberCtrl as vm"
        });
    }).controller("FieldTemplate-NumberCtrl", function () {
        var vm = this;
    });