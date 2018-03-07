/* 
 * Field: String - Default
 */

angular.module("eavFieldTemplates")
    .config(function (formlyConfigProvider, defaultFieldWrappers) {

        formlyConfigProvider.setType({
            name: "string-default",
            template: "<div><input class=\"form-control material\" ng-if=\"!(options.templateOptions.settings.merged.RowCount > 1)\" ng-pattern=\"vm.regexPattern\" ng-model=\"value.Value\">" +
                "<textarea ng-if=\"options.templateOptions.settings.merged.RowCount > 1\" rows=\"{{options.templateOptions.settings.merged.RowCount}}\" class=\"form-control material\" ng-model=\"value.Value\"></textarea></div>",
            wrapper: defaultFieldWrappers, 
            controller: "FieldTemplate-StringCtrl as vm"
        });

    }).controller("FieldTemplate-StringCtrl", function ($scope) {
        var vm = this;
        var validationRegexString = ".*";

        var settings = $scope.options.templateOptions.settings;
        
        // Do not use settings.merged here because there is an old (hidden field) that causes
        // merged.ValidationRegExJavaScript to be always empty
        if (settings.All && settings.All.ValidationRegExJavaScript)
            validationRegexString = settings.All.ValidationRegExJavaScript;
        
        vm.regexPattern = new RegExp(validationRegexString, 'i');

        console.log($scope.options.templateOptions);
    });