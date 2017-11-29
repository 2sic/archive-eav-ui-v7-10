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
            name: "entity-query",
            templateUrl: "fields/entity/entity-query.html",
            wrapper: wrappers,
            controller: "FieldTemplate-EntityQueryCtrl"
        });
    })
    .controller("FieldTemplate-EntityQueryCtrl", function ($scope, $http, $filter, $translate, $uibModal, appId, eavAdminDialogs, eavDefaultValueService, fieldMask, $q, $timeout, entitiesSvc, debugState, query) {
        
        var contentType, paramsMask, lastParamsMask;

        function activate() {
            // ensure settings are merged
            if (!$scope.to.settings.merged)
                $scope.to.settings.merged = {};

            // of no real data-model exists yet for this value (list of chosen entities), then create a blank
            if ($scope.model[$scope.options.key] === undefined || $scope.model[$scope.options.key].Values[0].Value === "") {
                var initVal = eavDefaultValueService($scope.options);   // note: works for simple entries as well as multiple, then it has to be an array though
                $scope.model[$scope.options.key] = { Values: [{ Value: initVal, Dimensions: {} }]};
            }

            // create short names for template
            var valList = $scope.model[$scope.options.key].Values[0].Value;

            $scope.chosenEntities = valList;
            $scope.selectedEntity = null;

            // Initialize url parameters mask
            paramsMask = fieldMask($scope.to.settings.merged.UrlParameters || null, $scope, $scope.maybeReload, null);// this will contain the auto-resolve url parameters

            // Initialize content type mask
            contentType = fieldMask($scope.to.settings.merged.EntityType || null, $scope, $scope.maybeReload, null);// this will contain the auto-resolve type (based on other contentType-field)

            $scope.availableEntities = [];
        }

        $scope.debug = debugState;

        // add an just-picked entity to the selected list
        $scope.addEntity = function(item) {
            if (item === null) return false;
            $scope.chosenEntities.push(item);
            $scope.selectedEntity = null;
            return true;
        };

        // open the dialog for a new item
        $scope.openNewEntityDialog = function () {
            function reloadAfterAdd(result) {
                if (!result || result.data === null || result.data === undefined)
                    return;

                $scope.maybeReload(true).then(function () {
                    $scope.chosenEntities.push(Object.keys(result.data)[0]);
                    setDirty();
                });
            }
            eavAdminDialogs.openItemNew(contentType.resolve(), reloadAfterAdd);
        };

        // ajax call to get all entities
        // todo: move to a service some time + enhance to provide more fields if needed
        $scope.getAvailableEntities = function () {
            var params = paramsMask.resolve(); // always get the latest definition
            return query("Test?includeGuid=true" + (params ? '&' + params : '')).get().then(function (data) {
                $scope.availableEntities = data.data[$scope.to.settings.merged.StreamName].map(function (e) {
                    return { Value: e.Guid, Text: e.Title, Id: e.Id };
                });
            });
        };

        $scope.maybeReload = function (force) {
            var newMask = paramsMask.resolve();
            if (lastParamsMask !== newMask || force) {
                lastParamsMask = newMask;
                return $scope.getAvailableEntities();
            }
            return $q.when();
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
            setDirty();
        };

        $scope.deleteItemInSlot = function (itemGuid, index) {
            if ($scope.to.settings.merged.EntityType === '') {
                alert('delete not possible - no type specified in entity field configuration');
                return;
            }

            var entities = $filter("filter")($scope.availableEntities, { Value: itemGuid });
            var id = entities[0].Id;

            entitiesSvc.tryDeleteAndAskForce(contentType.resolve(), id, entities[0].Text).then(function () {
                $scope.chosenEntities.splice(index, 1);
                $scope.maybeReload(true);
            });
        };

        // edit needs the Guid - the index isn't important
        $scope.edit = function (itemGuid, index) {
            if (itemGuid === null)
                return alert("no can do"); // todo: i18n
            var entities = $filter("filter")($scope.availableEntities, { Value: itemGuid });
            var id = entities[0].Id;

            return eavAdminDialogs.openItemEditWithEntityId(id, $scope.getAvailableEntities);
        };

        $scope.insertNull = function() {
            $scope.chosenEntities.push(null);
        };

        function setDirty() {
            $scope.form.$setDirty();
        }

        activate();
    })

    .directive("entityValidation", [function () {
        return {
            restrict: "A",
            require: "?ngModel",
            link: function(scope, element, attrs, ngModel) {
                if (!ngModel) return;

                ngModel.$validators.required = function (modelValue, viewValue) {
                    var value;

                    if (!scope.$parent.$parent.to.required) return true;

                    value = modelValue || viewValue;
                    if (!value || !Array.isArray(value)) return true;
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
