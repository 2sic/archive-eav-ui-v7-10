/* This file has 2 things
1. An eavAPIService, which should be removed since it solves a problem which the bootstrapper solves
2. The EntityEditCtrl - which I don't know what it does, but it uses this eavAPIService

*/

// The EavApiService wraps $http while making sure the base url is
// correct and adding some default parameters to every web service call

(function () {
    angular.module('2sic-EAV')
        .factory('eavApiService', function (eavConfig, $http) {
            return function (settings) {
                settings.url = eavConfig.api.baseUrl + settings.url;
                settings.params = $.extend({}, eavConfig.defaultApiParams, settings.params);
                return $http(settings);
            }
        });
})();

(function () {
    angular.module('2sic-EAV')
        .controller('EntityEditCtrl', function ($scope, eavDialogService, eavApiService, $rootElement, $http, $element, $filter) {

            // Prepare configuration
            $scope.configuration = $.parseJSON($($element).find("input[id$='hfConfiguration']").val());
            $scope.configuration.Entities = [];
            $scope.selectedEntity = "";
            $scope.entityIds = function () {
                return $scope.configuration.SelectedEntities.join(',');
            };

            // Controller functions
            $scope.AddEntity = function () {
                if ($scope.selectedEntity == "new")
                    $scope.OpenNewEntityDialog();
                else
                    $scope.configuration.SelectedEntities.push(parseInt($scope.selectedEntity));
                $scope.selectedEntity = "";
            };

            $scope.CreateEntityAllowed = function () { return $scope.configuration.AttributeSetId != null && $scope.configuration.AttributeSetId != 0; };

            $scope.OpenNewEntityDialog = function () {
                var url = $($rootElement).attr("data-newdialogurl") + "&PreventRedirect=true";
                url = url.replace("[AttributeSetId]", $scope.configuration.AttributeSetId);
                eavDialogService.open({
                    url: url,
                    onClose: function () { $scope.getAvailableEntities(); }
                });
            };

            $scope.getAvailableEntities = function () {
                eavApiService({
                    method: 'GET',
                    url: '/EAV/EntityPicker/getavailableentities',
                    params: {
                        zoneId: $scope.configuration.ZoneId,
                        appId: $scope.configuration.AppId,
                        attributeSetId: $scope.configuration.AttributeSetId,
                        dimensionId: $scope.configuration.DimensionId
                    }
                }).then(function (data) {
                    $scope.configuration.Entities = data.data;
                });
            };

            $scope.getEntityText = function (entityId) {
                var entities = $filter('filter')($scope.configuration.Entities, { Value: entityId });
                return entities.length > 0 ? entities[0].Text : "(Entity not found)";
            }

            // Initialize entities
            $scope.getAvailableEntities();

        });

})();