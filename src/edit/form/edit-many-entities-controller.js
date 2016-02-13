/* global angular */
(function () {
    /* jshint laxbreak:true*/
    "use strict";

    var app = angular.module("eavEditEntity");

    // The controller for the main form directive
    app.controller("EditEntities", function editEntityCtrl(appId, $http, $scope, entitiesSvc, saveToastr, $translate, debugState, ctrlS) {

        var vm = this;
        vm.debug = debugState;
        vm.isWorking = 0;           // isWorking is > 0 when any $http request runs
        vm.registeredControls = []; // array of input-type controls used in these forms
        vm.items = null;            // array of items to edit
        vm.willPublish = false;     // default is won't publish, but will usually be overridden

        //#region activate / deactivate + bindings
        // the activate-command, to intialize everything. Must be called later, when all methods have been attached
        function activate() {
            // bind ctrl+S
            vm.saveShortcut = ctrlS(function () { vm.save(); });

            // load all data
            vm.loadAll();
        }

        // clean-up call when the dialog is closed
        function deactivate() {
            vm.saveShortcut.unbind();
        }

        // bind the clean-up call to when the dialog is removed
        $scope.$on("$destroy", function () {
            deactivate();
        });
        //#endregion

        // add an additional input-type control for lazy-loading etc.
        vm.registerEditControl = function (control) {
            vm.registeredControls.push(control);
        };

        //#region load / save

        // load all data
        vm.loadAll = function() {
            entitiesSvc.getManyForEditing(appId, $scope.itemList)
                .then(function(result) {
                    vm.items = result.data;
                    angular.forEach(vm.items, function(v, i) {

                        // If the entity is null, it does not exist yet. Create a new one
                        if (!vm.items[i].Entity && !!vm.items[i].Header.ContentTypeName)
                            vm.items[i].Entity = entitiesSvc.newEntity(vm.items[i].Header);

                        vm.items[i].Entity = enhanceEntity(vm.items[i].Entity);

                        // set slot value - must be inverte for boolean-switch
                        var grp = vm.items[i].Header.Group;
                        vm.items[i].slotIsUsed = (grp === null
                            || grp.SlotIsEmpty !== true);
                    });
                    vm.willPublish = vm.items[0].Entity.IsPublished;
                });
        };

        // the save-call
        vm.save = function (close) {
            vm.isWorking++;
            saveToastr(entitiesSvc.saveMany(appId, vm.items)).then(function (result) {
                $scope.state.setPristine();
                if (close)
                    vm.afterSaveEvent(result);
                vm.isWorking--;
            }, function errorWhileSaving(response) {
                vm.isWorking--;
            });
        };

        // things to do after saving
        vm.afterSaveEvent = $scope.afterSaveEvent;

        //#endregion

        //#region state check/set for valid/dirty/pristine
        // check if form is valid
        vm.isValid = function () {
            var valid = true;
            angular.forEach(vm.registeredControls, function (e, i) {
                if (!e.isValid())
                    valid = false;
            });
            return valid;
        };

        // check if dirty
        $scope.state.isDirty = function() {
            var dirty = false;
            angular.forEach(vm.registeredControls, function(e, i) {
                if (e.isDirty())
                    dirty = true;
            });
            return dirty;
        };

        // set to not-dirty (pristine)
        $scope.state.setPristine = function() {
            angular.forEach(vm.registeredControls, function(e, i) {
                e.setPristine();
            });
        };
        //#endregion

        // monitor for changes in publish-state and set it for all items being edited
        $scope.$watch('vm.willPublish', function (newValue, oldValue) {
            angular.forEach(vm.items, function (v, i) {
                vm.items[i].Entity.IsPublished = vm.willPublish;
            });
        });

        /// toggle / change if a section (slot) is in use or not (like an unused presentation)
        vm.toggleSlotIsEmpty = function (item) {
            if (!item.Header.Group)
                item.Header.Group = {};
            item.Header.Group.SlotIsEmpty = !item.Header.Group.SlotIsEmpty;
            item.slotIsUsed = !item.Header.Group.SlotIsEmpty;
        };

        activate();

    });



})();