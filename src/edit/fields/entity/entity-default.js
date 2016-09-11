/* 
 * Field: Entity - Default
 * Also contains much business logic and the necessary controller
 * 
 */

angular.module("eavFieldTemplates")
    .config(function (formlyConfigProvider, defaultFieldWrappers) {

        var wrappers = defaultFieldWrappers.slice(0); // copy the array
        wrappers.splice(defaultFieldWrappers.indexOf("eavLocalization"), 1); // remove the localization...

        formlyConfigProvider.setType({
            name: "entity-default",
            templateUrl: "fields/entity/entity-default.html",
            wrapper: wrappers,
            controller: "FieldTemplate-EntityCtrl"
        });


    })
    .controller("FieldTemplate-EntityCtrl", function ($scope, $http, $filter, $translate, $modal, appId, eavAdminDialogs, eavDefaultValueService) {
        // ensure settings are merged
        if (!$scope.to.settings.merged)
            $scope.to.settings.merged = {};

        $scope.availableEntities = [];

        // of no real data-model exists yet for this value (list of chosen entities), then create a blank
        if ($scope.model[$scope.options.key] === undefined || $scope.model[$scope.options.key].Values[0].Value === "")
            $scope.model[$scope.options.key] = { Values: [{ Value: eavDefaultValueService($scope.options), Dimensions: {} }] };

        // create short names for template
        $scope.chosenEntities = $scope.model[$scope.options.key].Values[0].Value;
        $scope.selectedEntity = null;

        // add an just-picked entity to the selected list
        $scope.addEntity = function () {
            if ($scope.selectedEntity !== null) {
                $scope.chosenEntities.push($scope.selectedEntity);
                $scope.selectedEntity = null;
            }
        };

        // check if new-entity is an allowed operation
        $scope.createEntityAllowed = function() {
            var settings = $scope.to.settings.merged;
            return settings.EntityType !== null && settings.EntityType !== "" && settings.EnableCreate;
        };

        // open the dialog for a new item
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

        // ajax call to get all entities
        // todo: move to a service some time
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

        // get a nice label for any entity, including non-existing ones
        $scope.getEntityText = function (entityId) {
            if (entityId === null)
                return "empty slot"; // todo: i18n
            var entities = $filter("filter")($scope.availableEntities, { Value: entityId });
            return entities.length > 0 ? entities[0].Text : $translate.instant("FieldType.Entity.EntityNotFound"); 
        };

        // remove needs the index --> don't name "remove" - causes problems
        $scope.removeSlot = function(itemGuid, index) {
            $scope.chosenEntities.splice(index, 1);
        };

        $scope.deleteItemInSlot = function(itemGuid, index) {
            alert("this feature is not implemented yet, sorry. it will be added some day...");
        };

        // edit needs the Guid - the index isn't important
        $scope.edit = function (itemGuid, index) {
            if (itemGuid === null)
                return alert("no can do"); // todo: i18n
            var entities = $filter("filter")($scope.availableEntities, { Value: itemGuid });
            var id = entities[0].Id;

            return eavAdminDialogs.openItemEditWithEntityId(id, $scope.getAvailableEntities);
        };

        // Initialize entities
        $scope.getAvailableEntities();

    })

    .directive("entityValidation", [function () {
        return {
            restrict: "A",
            require: "?ngModel",
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
