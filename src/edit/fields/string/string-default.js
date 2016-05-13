/* 
 * Field: String - Default
 */

angular.module("eavFieldTemplates")
    .config(function (formlyConfigProvider, defaultFieldWrappers) {

        formlyConfigProvider.setType({
            name: "string-default",
            template: "<div><input class=\"form-control input-lg test material\" ng-if=\"!(options.templateOptions.settings.merged.RowCount > 1)\" ng-pattern=\"vm.regexPattern\" ng-model=\"value.Value\">" +
                "<textarea ng-if=\"options.templateOptions.settings.merged.RowCount > 1\" rows=\"{{options.templateOptions.settings.merged.RowCount}}\" class=\"form-control input-lg\" ng-model=\"value.Value\"></textarea></div>",
            wrapper: defaultFieldWrappers, 
            controller: "FieldTemplate-StringCtrl as vm"
        });

    }).controller("FieldTemplate-StringCtrl", function ($scope) {
        var vm = this;
        var validationRegexString = ".*";
        var stringSettings = $scope.options.templateOptions.settings.merged;
        if (stringSettings && stringSettings.ValidationRegExJavaScript)
            validationRegexString = stringSettings.ValidationRegExJavaScript;
        vm.regexPattern = new RegExp(validationRegexString, 'i');

        console.log($scope.options.templateOptions);
    });