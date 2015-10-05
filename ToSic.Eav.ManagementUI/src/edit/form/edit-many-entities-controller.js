/* global angular */
(function () {
    "use strict";

    var app = angular.module("eavEditEntity");

    // The controller for the main form directive
    app.controller("EditEntities", function editEntityCtrl(appId, $http, $scope, entitiesSvc, uiNotification) {

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
            entitiesSvc.saveMany(appId, vm.items).then(vm.afterSaveEvent);
        };

        // todo: translate
        vm.saveAndKeepOpen = function () {
                uiNotification.note("Saving", "", true);
            entitiesSvc.saveMany(appId, vm.items).then(function () {
                uiNotification.note("Saved", "", true);
            });
        };
        vm.items = null;

        entitiesSvc.getManyForEditing(appId, $scope.itemList)
            .then(function (result) {
                vm.items = result.data;
                angular.forEach(vm.items, function (v, i) {

                    // If the entity is null, it does not exist yet. Create a new one
                    if (!vm.items[i].Entity && !!vm.items[i].Header.ContentTypeName)
                        vm.items[i].Entity = entitiesSvc.newEntity(vm.items[i].Header.ContentTypeName);

                    vm.items[i].Entity = enhanceEntity(vm.items[i].Entity);
                });
                vm.willPublish = vm.items[0].Entity.IsPublished;
            });

        vm.willPublish = false;

        vm.togglePublish = function() {
            vm.willPublish = !vm.willPublish;
            angular.forEach(vm.items, function(v, i) {
                vm.items[i].Entity.IsPublished = vm.willPublish;
            });
        };


    });



})();