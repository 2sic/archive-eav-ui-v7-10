/* 
 * Field: Empty - Default: this is usually a title/group section
 */

angular.module("eavFieldTemplates")
    .config(function(formlyConfigProvider) {
        formlyConfigProvider.setType({
            name: "empty-default",
            templateUrl: "fields/empty/empty-default.html",
            wrapper: ["fieldGroup"],
            controller: "FieldTemplate-TitleController"
        });
    })
    .controller("FieldTemplate-TitleController", function($scope, debugState) {
        if (!$scope.to.settings.merged)
            $scope.to.settings.merged = {};

        //$scope.to.settings.merged.DefaultCollapsed = true;// = "show";


        $scope.set = function(newState) {
            $scope.to.collapseGroup = newState;
        };

        $scope.toggle = function() {
            $scope.to.collapseGroup = !$scope.to.collapseGroup;
        };

        if ($scope.to.settings.merged.DefaultCollapsed === true) 
            $scope.set(true);

    });