/* 
 * Field: Entity - Default
 * Also contains much business logic and the necessary controller
 * 
 */

angular.module("eavFieldTemplates")
    .config(function(formlyConfigProvider) {

        formlyConfigProvider.setType({
            name: "entity-default",
            templateUrl: "fields/templates/entity-default.html",
            wrapper: ["eavLabel", "bootstrapHasError"],
            controller: "FieldTemplate-EntityCtrl"
        });

    })
    .controller("FieldTemplate-EntityCtrl", function($scope, $http, $filter, $modal, appId) {

        if (!$scope.to.settings.Entity)
            $scope.to.settings.Entity = {};

        $scope.availableEntities = [];

        if ($scope.model[$scope.options.key] === undefined || $scope.model[$scope.options.key].Values[0].Value === "")
            $scope.model[$scope.options.key] = { Values: [{ Value: [], Dimensions: {} }] };

        $scope.chosenEntities = $scope.model[$scope.options.key].Values[0].Value;

        $scope.addEntity = function() {
            if ($scope.selectedEntity == "new")
                $scope.openNewEntityDialog();
            else
                $scope.chosenEntities.push($scope.selectedEntity);
            $scope.selectedEntity = "";
        };

        $scope.createEntityAllowed = function() {
            return $scope.to.settings.Entity.EntityType !== null && $scope.to.settings.Entity.EntityType !== "";
        };

        $scope.openNewEntityDialog = function() {

            var modalInstance = $modal.open({
                template: "<div style=\"padding:20px;\"><edit-content-group edit=\"vm.edit\"></edit-content-group></div>",
                controller: function(entityType) {
                    var vm = this;
                    vm.edit = { contentTypeName: entityType };
                },
                controllerAs: "vm",
                resolve: {
                    entityType: function() {
                        return $scope.to.settings.Entity.EntityType;
                    }
                }
            });

            modalInstance.result.then(function() {
                $scope.getAvailableEntities();
            });

        };

        $scope.getAvailableEntities = function() {
            $http({
                method: "GET",
                url: "eav/EntityPicker/getavailableentities",
                params: {
                    contentTypeName: $scope.to.settings.Entity.EntityType,
                    appId: appId
                    // ToDo: dimensionId: $scope.configuration.DimensionId
                }
            }).then(function(data) {
                $scope.availableEntities = data.data;
            });
        };

        $scope.getEntityText = function(entityId) {
            var entities = $filter("filter")($scope.availableEntities, { Value: entityId });
            return entities.length > 0 ? entities[0].Text : "(Entity not found)";
        };

        $scope.remove = function(item) {
            var index = $scope.chosenEntities.indexOf(item);
            $scope.chosenEntities.splice(index, 1);
        };

        // Initialize entities
        $scope.getAvailableEntities();

    });
