/* 
 * Field: Custom - Default (basically something you should never see)
 */
(function() {
    "use strict";

    var mod = angular.module("eavCustomFields");

    // registration is a bit special - because we're lazy-loading
    mod.formlyConfig.setType({
        name: "custom-gps",
        template: "<span>hello</span>",
            //templateUrl: "fields/custom-gps/custom-gps.html",
            wrapper: mod.defaultFieldWrappers,
            controller: "FieldTemplate-CustomGpsController"
        }); 

    mod.controller("FieldTemplate-CustomGpsController", function ($scope, $filter, $modal, appId, debugState, eavAdminDialogs) {
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
})();