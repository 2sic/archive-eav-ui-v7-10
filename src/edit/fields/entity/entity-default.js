/* 
 * Field: Entity - Default
 * Also contains much business logic and the necessary controller
 * 
 */

angular.module("eavFieldTemplates")
    .config(function(formlyConfigProvider) {

        formlyConfigProvider.setType({
            name: "entity-default",
            templateUrl: "fields/entity/entity-default.html",
            wrapper: ["eavLabel", "bootstrapHasError", "collapsible"],
            controller: "FieldTemplate-EntityCtrl",
            //defaultOptions: {
            //    validators: {
            //        required: function (viewValue, modelValue, scope) {
            //            var value = viewValue || modelValue;
            //            if (!Array.isArray(value))
            //                return true;
            //            return value.length > 0;
            //        }
            //    }
            //}
        });


    })
    .controller("FieldTemplate-EntityCtrl", function ($scope, $http, $filter, $translate, $modal, appId, eavAdminDialogs, eavDefaultValueService) {
        if (!$scope.to.settings.merged)
            $scope.to.settings.merged = {};

        $scope.availableEntities = [];

        if ($scope.model[$scope.options.key] === undefined || $scope.model[$scope.options.key].Values[0].Value === "")
            $scope.model[$scope.options.key] = { Values: [{ Value: eavDefaultValueService($scope.options), Dimensions: {} }] };

        $scope.chosenEntities = $scope.model[$scope.options.key].Values[0].Value;

        $scope.addEntity = function () {            
            //if ($scope.selectedEntity === "new")
            //    $scope.openNewEntityDialog();
            //else
                $scope.chosenEntities.push($scope.selectedEntity);
            $scope.selectedEntity = "";
        };

        $scope.createEntityAllowed = function() {
            var settings = $scope.to.settings.merged;
            return settings.EntityType !== null && settings.EntityType !== "" && settings.EnableCreate;
        };

        $scope.openNewEntityDialog = function() {
            function reload(result) {
                if (!result || result.data === null || result.data === undefined)
                    return;

                $scope.getAvailableEntities().then(function () {
                    $scope.chosenEntities.push(Object.keys(result.data)[0]);
                });
            }

            eavAdminDialogs.openItemNew($scope.to.settings.merged.EntityType, reload);

        };

        $scope.getAvailableEntities = function() {
            return $http({
                method: "GET",
                url: "eav/EntityPicker/getavailableentities",
                params: {
                    contentTypeName: $scope.to.settings.merged.EntityType,
                    appId: appId
                    // ToDo: dimensionId: $scope.configuration.DimensionId
                }
            }).then(function(data) {
                $scope.availableEntities = data.data;
            });
        };

        $scope.getEntityText = function (entityId) {
            if (entityId === null)
                return "empty slot"; // todo: i18n
            var entities = $filter("filter")($scope.availableEntities, { Value: entityId });
            return entities.length > 0 ? entities[0].Text : $translate.instant("FieldType.Entity.EntityNotFound"); 
        };

        // remove needs the index --> don't name "remove" - causes problems
        $scope.removeSlot = function remove(itemGuid, index) {
            $scope.chosenEntities.splice(index, 1);
        };

        // edit needs the Guid - the index isn't important
        $scope.edit = function (itemGuid, index) {
            if (itemGuid === null)
                return alert('no can do'); // todo: i18n
            var entities = $filter("filter")($scope.availableEntities, { Value: itemGuid });
            var id = entities[0].Id;

            eavAdminDialogs.openItemEditWithEntityId(id, $scope.getAvailableEntities);
        };

        // Initialize entities
        $scope.getAvailableEntities();

    }).directive('entityValidation', [function() {
        return {
            restrict: 'A',
            require: '?ngModel',
            link: function(scope, element, attrs, ngModel) {
                if (!ngModel) return;

                ngModel.$validators.required = function (modelValue, viewValue) {

                    if (!scope.$parent.$parent.to.required)
                        return true;
                    var value = modelValue || viewValue;
                    if (!value || !Array.isArray(value))
                        return true;
                    return value.length > 0;
                };

                scope.$watch(function () {
                    return ngModel.$viewValue;
                }, function (newValue) {
                    ngModel.$validate();
                }, true);
            }
        };
    }]);
