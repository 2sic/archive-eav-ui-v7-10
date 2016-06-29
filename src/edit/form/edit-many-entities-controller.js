/* global angular */
(function () {
    /* jshint laxbreak:true*/
    "use strict";

    var app = angular.module("eavEditEntity");

    // The controller for the main form directive
    app.controller("EditEntities", function editEntityCtrl(appId, $http, $scope, entitiesSvc, toastr, saveToastr, $translate, debugState, ctrlS) {
        //#region detailled logging if necessary
        var detailedLogging = false;
        var clog = detailedLogging
            ? function () { for (var i = 0; i < arguments.length; i++) console.log(arguments[i]); }
            : function () { };
        //#endregion

        var vm = this;
        vm.debug = debugState;
        vm.isWorking = 0;           // isWorking is > 0 when any $http request runs
        vm.registeredControls = []; // array of input-type controls used in these forms
        vm.items = null;            // array of items to edit
        vm.willPublish = false;     // default is won't publish, but will usually be overridden
        vm.publishMode = "hide";    // has 3 modes: show, hide, branch (where branch is a hidden, linked clone)
        vm.enableDraft = false;

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
                        vm.items[i].slotIsUsed = (grp === null || grp === undefined || grp.SlotIsEmpty !== true);
                    });
                    vm.willPublish = vm.items[0].Entity.IsPublished;
                    vm.enableDraft = vm.items[0].Header.EntityId !== 0; // it already exists, so enable draft
                    vm.publishMode = vm.items[0].Entity.IsBranch
                        ? "branch" // it's a branch, so it must have been saved as a draft-branch
                        : vm.items[0].Entity.IsPublished ? "show" : "hide";
                });
        };

        vm.showFormErrors = function() {
                var errors = vm.formErrors();
                var msgs = [], msgTemplate = $translate.instant("Message.FieldErrorList");
                for (var set = 0; set < errors.length; set++) {
                    if (errors[set].required) {
                        var req = errors[set].required.map(function (itm) { return { field: itm.$name, error: "required" }; });
                        msgs = msgs.concat(req);
                    }
                }
                var nice = msgs.map(function (err) {
                    var specs = err.field.split("_");

                    return msgTemplate.replace("{form}", specs[1])
                        .replace("{field}", specs[3])
                        .replace("{error}", err.error);
                });
            var msg = nice.join("<br/>");
            return toastr.error($translate.instant("Message.CantSaveInvalid").replace("{0}", msg),
                $translate.instant("Message.Error"), { allowHtml: true }); 
        };

        // the save-call
        vm.save = function (close) {
            // check if saving is allowed
            if (!vm.isValid()) 
                return vm.showFormErrors();

            if (vm.isWorking > 0)
                return toastr.error($translate.instant("Message.CantSaveProcessing")); // todo: i18n

            // save
            vm.isWorking++;
            saveToastr(entitiesSvc.saveMany(appId, vm.items)).then(function (result) {
                $scope.state.setPristine();
                if (close) {
                    vm.allowCloseWithoutAsking = true;
                    vm.afterSaveEvent(result);
                }
                vm.enableDraft = true;  // after saving, we can re-save as draft
                vm.isWorking--;
            }, function errorWhileSaving() {
                vm.isWorking--;
            });
            return null;
        };

        // things to do after saving
        vm.afterSaveEvent = $scope.afterSaveEvent;

        //#endregion

        //#region state check/set for valid/dirty/pristine
        // check if form is valid
        vm.isValid = function () {
            var valid = true;
            angular.forEach(vm.registeredControls, function (e) {
                if (!e.isValid())
                    valid = false;
            });
            return valid;
        };

        vm.formErrors = function () {
            var list = [];
            angular.forEach(vm.registeredControls, function (e) {
                if (!e.isValid())
                    list.push(e.error());
            });
            return list;
        };

        // check if dirty
        $scope.state.isDirty = function() {
            var dirty = false;
            angular.forEach(vm.registeredControls, function(e) {
                if (e.isDirty())
                    dirty = true;
            });
            return dirty;
        };

        // set to not-dirty (pristine)
        $scope.state.setPristine = function() {
            angular.forEach(vm.registeredControls, function(e) {
                e.setPristine();
            });
        };
        //#endregion

        // monitor for changes in publish-state and set it for all items being edited
        $scope.$watch("vm.willPublish", function () {   // ToDO Todo
            angular.forEach(vm.items, function (v, i) {
                vm.items[i].Entity.IsPublished = vm.willPublish;
            });
        });

        $scope.$watch("vm.publishMode", function () {   // ToDO Todo
            var publish = vm.publishMode === "show"; // all other cases are hide
            var branch = vm.publishMode === "branch"; // all other cases are no-branch
            angular.forEach(vm.items, function (v, i) {
                vm.items[i].Entity.IsPublished = publish;
                vm.items[i].Entity.IsBranch = branch;
            });
        });

        // handle maybe-leave
        vm.maybeLeave = {
            save: function() { vm.save(true); },
            quit: $scope.close,
            handleClick: function (event) {
                clog('handleClick', event);
                var target = event.target || event.srcElement;
                if (target.nodeName === "I") target = target.parentNode;
                if (target.id === "save" || target.id === "quit") {
                    clog('for ' + target.id);
                    vm.allowCloseWithoutAsking = true;
                    vm.maybeLeave[target.id]();
                }
            },
            ask: function(e) {
                if (vm.allowCloseWithoutAsking)
                    return;
                var template = "<div>"  // note: this variable must be inside this method, to ensure that translate is pre-loaded before we call it
                    + $translate.instant("Errors.UnsavedChanges") + "<br>"
                    + "<button type='button' id='save' class='btn btn-primary' ><i class='icon-ok'></i>" +  $translate.instant("General.Buttons.Save") + "</button> &nbsp;"
                    + "<button type='button' id='quit' class='btn btn-default' ><i class= 'icon-cancel'></i>" + $translate.instant("General.Buttons.NotSave") + "</button>"
                    + "</div>";
                if (vm.dialog && vm.dialog.isOpened)
                    toastr.clear(vm.dialog);
                vm.dialog = toastr.warning(template, {
                    allowHtml: true,
                    timeOut: 3000,
                    onShown: function (toast) {
                        toast.el[0].onclick = vm.maybeLeave.handleClick;
                    }
                });
                e.preventDefault();
            }
        };
        
        $scope.$on("modal.closing", vm.maybeLeave.ask);



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