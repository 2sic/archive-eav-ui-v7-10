/* 
 * Field: Number - Default
 */

angular.module("eavFieldTemplates")
    .config(function(formlyConfigProvider) {
        formlyConfigProvider.setType({
            name: "number-default",
            template: "<input type=\"number\" class=\"form-control\" ng-model=\"value.Value\">{{vm.isGoogleMap}}",
            wrapper: ["eavLabel", "bootstrapHasError", "eavLocalization"],
            defaultOptions: {
                ngModelAttrs: {
                    '{{to.settings.Number.Min}}': { value: "min" },
                    '{{to.settings.Number.Max}}': { value: "max" },
                    '{{to.settings.Number.Decimals ? "^[0-9]+(\.[0-9]{1," + to.settings.Number.Decimals + "})?$" : null}}': { value: "pattern" }
                }
            },
            controller: "FieldTemplate-NumberCtrl as vm"
        });
    }).controller("FieldTemplate-NumberCtrl", function () {
        var vm = this;
        // ToDo: Implement Google Map
    });