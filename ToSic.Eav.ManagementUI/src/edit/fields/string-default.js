/* 
 * Field: String - Default
 */

angular.module("eavFieldTemplates")
    .config(function (formlyConfigProvider, defaultFieldWrappers) {

        formlyConfigProvider.setType({
            name: "string-default",
            template: "<input class=\"form-control\" ng-pattern=\"vm.regexPattern\" ng-model=\"value.Value\">",
            wrapper: defaultFieldWrappers, // ["eavLabel", "bootstrapHasError", "eavLocalization"]
            controller: "FieldTemplate-StringCtrl as vm"
        });

    }).controller("FieldTemplate-StringCtrl", function ($scope) {
        var vm = this;
        var validationRegexString = ".*";
        var stringSettings = $scope.options.templateOptions.settings.String;
        if (stringSettings && stringSettings.ValidationRegExJavaScript)
            validationRegexString = stringSettings.ValidationRegExJavaScript;
        vm.regexPattern = new RegExp(validationRegexString, 'i');
    });