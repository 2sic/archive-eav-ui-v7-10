/* 
 * Field: String - Dropdown
 */

angular.module("eavFieldTemplates")
    .config(function(formlyConfigProvider, defaultFieldWrappers) {

        formlyConfigProvider.setType({
            name: "string-contenttype",
            templateUrl: "fields/string/string-contenttype.html",
            wrapper: defaultFieldWrappers,
            controller: "FieldTemplate-String-ContentType"
        });

    })
    .controller("FieldTemplate-String-ContentType", function($scope, contentTypeSvc, appId) { //, $http, $filter, $translate, $modal, eavAdminDialogs, eavDefaultValueService) {
        // ensure settings are merged
        if (!$scope.to.settings.merged)
            $scope.to.settings.merged = {};

        // create initial list for binding
        $scope.contentTypes = [];

        var svc = contentTypeSvc(appId);
        svc.retrieveContentTypes().then(function(result) {
            $scope.contentTypes = result.data;
        });

    });