// uiDebug
// will add
// vm.debug -> shows if in debug mode - bind ng-ifs to this
// vm.maybeEnableDebug - a method which checks for ctrl+shift-click and if yes, changes debug state
//
// How to use
// 1. add uiDebug to your controller dependencies like:    contentImportController(appId, ..., debugState, $modalInstance, $filter)
// 2. add a line after creating your vm-object like:       vm.debug = debugState;
// 3. add a click event as far out as possible on html:    <div ng-click="vm.debug.autoEnableAsNeeded($event)">
// 4. wrap your hidden stuff in an ng-if:                  <div ng-if="vm.debug.on">

angular.module("EavServices")
    .factory("debugState", function () {
        var svc = {
            on: false
        };

        svc.autoEnableAsNeeded = function (evt) {
            evt = window.event || evt;
            var ctrlAndShiftPressed = evt.ctrlKey;// && evt.shiftKey;
            if (ctrlAndShiftPressed)
                svc.on = !svc.on;
        };

        return svc;
    });