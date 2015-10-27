/* 
 * Field: Custom - GPS (basically something you should never see)
 */
(function () {
    "use strict";

    var app = angular.module("ToSicEavCustomGps", ["uiGmapgoogle-maps"]);
    app.config(["formlyConfigProvider", "defaultFieldWrappers", function (formlyConfigProvider, defaultFieldWrappers) {

        formlyConfigProvider.setType({
            name: "custom-gps",
            templateUrl: "fields/custom-gps/custom-gps.html",
            wrapper: defaultFieldWrappers,
            controller: "FieldTemplate-CustomGpsController"
        });
    }]);
    app.config(["uiGmapGoogleMapApiProvider", function (uiGmapGoogleMapApiProvider) {
        uiGmapGoogleMapApiProvider.configure({
            //    key: 'your api key',
            //v: '3.20', //defaults to latest 3.X anyhow
            //libraries: 'weather,geometry,visualization'
        });
    }]);
    app.controller("FieldTemplate-CustomGpsController", ["$scope", "$filter", "$modal", "appId", "debugState", "eavAdminDialogs", "addtemplatestocache", function ($scope, $filter, $modal, appId, debugState, eavAdminDialogs, addtemplatestocache) {

        var latField, lngField;

        var controlSettings = $scope.to.settings["custom-gps"];
        if (controlSettings) {
            latField = controlSettings.LatField || null;
            lngField = controlSettings.LongField || null;
        }

        var hasAddressMask = !controlSettings || !controlSettings.AddressMask || controlSettings.AdressMask === "";
        var defaultCoordinates = { latitude: 47.1747363, longitude: 9.4671813 };

        $scope.position = angular.extend({}, defaultCoordinates);

        $scope.$watch('position', function() {
            if($scope.value)
                $scope.value.Value = JSON.stringify($scope.position);
        });

        $scope.map = { center: angular.extend({}, defaultCoordinates), zoom: 15 };

        $scope.marker = {
            position: $scope.position,// position.Value,
            options: { draggable: true },
            events: {
                "dragend": function (e) {
                    // Update field values if lat/lng fields are defined
                    if (latField && $scope.model.hasOwnProperty(latField) && $scope.model[latField] !== null)
                        $scope.model[latField]._currentValue.Value = $scope.marker.position.latitude;
                    if (lngField && $scope.model.hasOwnProperty(lngField) && $scope.model[lngField] !== null)
                        $scope.model[lngField]._currentValue.Value = $scope.marker.position.longitude;
                }
            }
        };

        // Watch lat/lng fields if defined
        if (latField)
            $scope.$watch('model["' + latField + '"]._currentValue.Value', function(newValue, oldValue) {
                $scope.marker.position.latitude = newValue;
                $scope.map.center.latitude = newValue;
            });

        if (lngField)
            $scope.$watch('model["' + lngField + '"]._currentValue.Value', function (newValue, oldValue) {
                $scope.marker.position.longitude = newValue;
                $scope.map.center.longitude = newValue;
            });

        $scope.formattedAddress = function() {
            if (!hasAddressMask)
                return "";
            return controlSettings.AddressMask;
        };

        $scope.autoSelect = function() {
            if (hasAddressMask) {
                alert(controlSettings.AddressMask);
            }
        };

        $scope.debug = debugState;
    }]);

    // tests w/template cache
    app.service("addtemplatestocache", //)
                ["$templateCache", function ($templateCache) {
                    $templateCache.put('fields/custom-gps/custom-gps.html',
                      "<div>" +
                      "<a class='btn btn-default' ng-click='autoSelect'>Auto-select from address</a>" +
                      "<ui-gmap-google-map center='map.center' zoom='map.zoom'>" +
                      "<ui-gmap-marker idkey='\"mapMarker1\"' coords='marker.position' options='marker.options' events='marker.events'></ui-gmap-marker>" +
                      "</ui-gmap-google-map>" +
                      "<div>" +
                      "<div ng-if=\"debug.on\">" +
                      "<h4>debug info</h4>" +
                      "<div>lat field name: '{{latField}}' lng field name: '{{longField}}'<br><pre>{{value | json}}</pre></div></div></div></div>"
                    );

                }])

    ;
})();