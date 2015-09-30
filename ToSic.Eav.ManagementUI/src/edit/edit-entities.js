/* global angular */
(function () {
    'use strict';

    var app = angular.module('eavEditEntities', ['formly', 'ui.bootstrap', 'eavFieldTemplates', 'eavNgSvcs', 'EavServices', 'eavEditTemplates', 'eavEditEntity']);

    app.directive('eavEditEntities', function () {
        return {
            templateUrl: 'edit-entities.html',
            restrict: 'E',
            scope: {
                editPackageRequest: '=editPackageRequest',
                afterSaveEvent: '=afterSaveEvent'
            },
            controller: 'EditEntities',
            controllerAs: 'vm'
        };
    });

    // The controller for the main form directive
    app.controller('EditEntities', function editEntityCtrl(appId, $http, $scope, entitiesSvc, $modalInstance) {

        var vm = this;

        vm.registeredControls = [];
        vm.registerEditControl = function (control) {
            vm.registeredControls.push(control);
        };

        vm.afterSaveEvent = $scope.afterSaveEvent;

        vm.isValid = function () {
            var valid = true;
            angular.forEach(vm.registeredControls, function (e, i) {
                if (!e.isValid())
                    valid = false;
            });
            return valid;
        };

        vm.save = function () {
            entitiesSvc.saveMany(appId, vm.editPackage).then(vm.afterSaveEvent);
        };

        vm.editPackage = null;

        entitiesSvc.getManyForEditing(appId, $scope.editPackageRequest)
            .then(function (result) {
                vm.editPackage = result.data;
                angular.forEach(vm.editPackage.entities, function (v, i) {

                    // If the entity is null, it does not exist yet. Create a new one
                    if (!vm.editPackage.entities[i].entity && !!vm.editPackage.entities[i].packageInfo.contentTypeName)
                        vm.editPackage.entities[i].entity = entitiesSvc.newEntity(vm.editPackage.entities[i].packageInfo.contentTypeName);

                    vm.editPackage.entities[i].entity = enhanceEntity(vm.editPackage.entities[i].entity);
                });
            });

    });



})();