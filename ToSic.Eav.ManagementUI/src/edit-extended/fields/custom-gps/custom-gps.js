/* 
 * Field: Custom - Default (basically something you should never see)
 */

angular.module("eavFieldTemplates")
    .config(function(formlyConfigProvider, defaultFieldWrappers) {

        formlyConfigProvider.setType({
            name: "custom-gps",
            templateUrl: "fields/custom-gps/custom-gps.html",
            wrapper: defaultFieldWrappers,
            controller: "FieldTemplate-CustomGpsController"
        });
    })
    .controller("FieldTemplate-CustomGpsController", function ($scope, $filter, $modal, appId, debugState, eavAdminDialogs) {
        $scope.debug = debugState;
        // try to find the settings, where to copy the field to...
        $scope.latField = "";
        $scope.LongField = "";

        var controlSettings = $scope.to.settings["custom-gps"];
        if (controlSettings) {
            $scope.latField = controlSettings.LatField || null;
        }
        // alert('gps' + $scope.latField);

    });