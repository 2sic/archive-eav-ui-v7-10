/* 
 * Field: Entity - Query
 * 
 */

angular.module("eavFieldTemplates")
    .config(function (formlyConfigProvider, defaultFieldWrappers) {

        var wrappers = defaultFieldWrappers.slice(0); // copy the array
        wrappers.splice(defaultFieldWrappers.indexOf("eavLocalization"), 1); // remove the localization...

        formlyConfigProvider.setType({
            name: "entity-query",
            templateUrl: "fields/entity/entity-default.html",
            wrapper: wrappers,
            controller: "FieldTemplate-EntityQueryCtrl"
        });
    })
    .controller("FieldTemplate-EntityQueryCtrl", function ($controller, $scope, fieldMask, $q, query, $timeout) {

        // use "inherited" controller just like described in http://stackoverflow.com/questions/18461263/can-an-angularjs-controller-inherit-from-another-controller-in-the-same-module
        $controller("FieldTemplate-EntityCtrl", { $scope: $scope });

        var paramsMask, lastParamsMask;
        $scope.inicateReload = false;
        $scope.showReloadButton = true;

        if (!$scope.to.settings.merged.StreamName || $scope.to.settings.merged.StreamName === "") {
            $scope.to.settings.merged.StreamName = "Default";
        }

        function activate() {
            // Initialize url parameters mask
            paramsMask = fieldMask($scope.to.settings.merged.UrlParameters || null, $scope, $scope.maybeReload, null); // this will contain the auto-resolve url parameters     
            $timeout($scope.getAvailableEntities, 0);
        }
        
        // ajax call to get the entities
        $scope.getAvailableEntities = function () {
            if (!$scope.to.settings.merged.Query)
                alert("No query defined for " + $scope.options.key + " - can't load entities");
            var params = paramsMask.resolve(); // always get the latest definition
            var queryUrl = $scope.to.settings.merged.Query;
            if (queryUrl.indexOf('/') == -1) // append stream name if not defined
                queryUrl = queryUrl + "/" + $scope.to.settings.merged.StreamName;
            return query(queryUrl + "?includeGuid=true" + (params ? '&' + params : '')).get().then(function (data) {
                $scope.availableEntities = $scope.selectEntities = data.data[$scope.to.settings.merged.StreamName].map($scope.queryEntityMapping);
                $scope.indicateReload = false;
            });
        };

        $scope.queryEntityMapping = function (entity) {
            return { Value: entity.Guid, Text: entity.Title, Id: entity.Id };
        };

        $scope.selectHighlighted = function () {
            if ($scope.indicateReload)
                return $scope.getAvailableEntities();
        };

        $scope.maybeReload = function (force) {
            var newMask = paramsMask.resolve();
            if (lastParamsMask !== newMask || force) {
                lastParamsMask = newMask;
                $scope.indicateReload = true;
                $scope.selectEntities = [];
            }
            return $q.when();
        };
        

        activate();
    });
