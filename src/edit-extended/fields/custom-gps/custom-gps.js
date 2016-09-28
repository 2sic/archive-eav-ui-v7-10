/* 
 * Field: Custom - GPS (basically something you should never see)
 */
(function () {
    "use strict";

    var app = angular.module("ToSicEavCustomGps", ["uiGmapgoogle-maps"]);
    app.config(function (formlyConfigProvider, defaultFieldWrappers) {

        formlyConfigProvider.setType({
            name: "custom-gps",
            templateUrl: "fields/custom-gps/custom-gps.html",
            wrapper: defaultFieldWrappers,
            controller: "FieldTemplate-CustomGpsController"
        });
    });
    app.config(function (uiGmapGoogleMapApiProvider) {
        uiGmapGoogleMapApiProvider.configure({
            key: 'AIzaSyDPhnNKpEg8FmY8nooE7Zwnue6SusxEnHE',
            //v: '3.20', //defaults to latest 3.X anyhow
            //libraries: 'weather,geometry,visualization'
        });
    });
    app.controller("FieldTemplate-CustomGpsController", function ($scope, $filter, $uibModal, appId, debugState, eavAdminDialogs, addtemplatestocache, uiGmapGoogleMapApi) {

        var latField, lngField;
        
        var controlSettings = $scope.to.settings["custom-gps"];
        if (controlSettings) {
            latField = controlSettings.LatField || null;
            lngField = controlSettings.LongField || null;
        }

        var hasAddressMask = $scope.hasAddressMask = controlSettings && controlSettings.AddressMask && controlSettings.AddressMask !== "";
        console.log(controlSettings);
        var defaultCoordinates = { latitude: 47.17465989999999, longitude: 9.469142499999975 };

        $scope.position = angular.extend({}, defaultCoordinates);
        $scope.showMap = false;

        $scope.map = { center: angular.extend({}, $scope.position), zoom: 15 };

        // Initialize value that was saved as string
        $scope.$watch('value.Value', function () {
            if ($scope.value && typeof $scope.value.Value === 'string' && $scope.value.Value !== '') {
                //alert("got position from existing value: " + $scope.value.Value);
                var position = JSON.parse($scope.value.Value);
                angular.extend($scope.position, position);
                angular.extend($scope.map.center, $scope.position);
            }
        });

        $scope.$watch('position', function () {
            updatePosition($scope.position);
        }, true);

        // Update the position where needed (map center, marker)
        var updatePosition = function (position) {
            // Update field values if lat/lng fields are defined
            if (latField && $scope.model.hasOwnProperty(latField) && $scope.model[latField] && $scope.model[latField]._currentValue)
                $scope.model[latField]._currentValue.Value = position.latitude;
            if (lngField && $scope.model.hasOwnProperty(lngField) && $scope.model[lngField] && $scope.model[lngField]._currentValue)
                $scope.model[lngField]._currentValue.Value = position.longitude;

            // Center the map and update string value
            angular.extend($scope.map.center, position);
            if (position !== $scope.position)
                angular.extend($scope.position, position);
            if($scope.value)
                $scope.value.Value = JSON.stringify(position);
        };

        $scope.marker = {
            position: $scope.position,
            options: { draggable: true },
            events: {
                "dragend": function (e) {
                    updatePosition($scope.position);
                }
            }
        };

        // todo: should replace this block with the new fieldMask service
        $scope.formattedAddress = function () {
            if (!controlSettings)
                return "";
            var address = controlSettings.AddressMask;
            if (address === undefined) return "";
            var tokenRe = /\[.*?\]/ig;
            var matches = address.match(tokenRe);
            angular.forEach(matches, function (e, i) {
                var staticName = e.replace(/[\[\]]/ig, '');
                var replaceValue = ($scope.model.hasOwnProperty(staticName) && $scope.model[staticName] !== null) ? $scope.model[staticName]._currentValue.Value : '';
                address = address.replace(e, replaceValue);
            });
            return address;
        };

        $scope.autoSelect = function () {
            var address = $scope.formattedAddress();
            (new google.maps.Geocoder()).geocode({
                address: address
            }, function (results, status) {
                if (status == google.maps.GeocoderStatus.OK) {
                    var result = results[0].geometry.location;
                    updatePosition({ latitude: result.lat(), longitude: result.lng() });
                    $scope.showMap = true;
                    $scope.$apply();
                }
                else {
                    alert("Could not locate address: " + address);
                }
            });
        };

        $scope.debug = debugState;
    });

    // tests w/template cache
    app.service("addtemplatestocache", //)
                function ($templateCache) {
                    $templateCache.put('fields/custom-gps/custom-gps.html',
                      "<div>" +
                      "Lat: <input type='number' ng-model='marker.position.latitude'/>, Lng: <input type='number' ng-model='marker.position.longitude'/><br>" +
                      "<a ng-click='showMap = !showMap' class='btn btn-default' ng-click='autoSelect'><span icon='map-marker'></span></a><a class='btn btn-default' ng-click='autoSelect()' ng-show='hasAddressMask'><span icon='search'></span></a> {{formattedAddress()}}<br>" +
                      "<ui-gmap-google-map center='map.center' zoom='map.zoom' ng-if='showMap'>" +
                      "<ui-gmap-marker idkey='\"mapMarker1\"' coords='marker.position' options='marker.options' events='marker.events'></ui-gmap-marker>" +
                      "</ui-gmap-google-map>" +
                      "<div>" +
                      "<div ng-if=\"debug.on\">" +
                      "<h4>debug info</h4>" +
                      "<div>lat field name: '{{latField}}' lng field name: '{{longField}}'<br><pre>{{value | json}}</pre></div></div></div></div>"
                    );

                })

    ;
})();