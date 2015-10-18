/* shared debugState = advancedMode
 * 
 * vm.debug -> shows if in debug mode - bind ng-ifs to this
 * vm.maybeEnableDebug - a method which checks for ctrl+shift-click and if yes, changes debug state
 *
 * How to use
 * 1. add uiDebug to your controller dependencies like:    contentImportController(appId, ..., debugState, $modalInstance, $filter)
 * 2. add a line after creating your vm-object like:       vm.debug = debugState;
 * 3. add a click event as far out as possible on html:    <div ng-click="vm.debug.autoEnableAsNeeded($event)">
 * 4. wrap your hidden stuff in an ng-if:                  <div ng-if="vm.debug.on">
 *
 * Note that if you're using it in a directive you'll use $scope instead of vm, so the binding is different.
 * For example, instead of <div ng-if="vm.debug.on"> you would write <div ng-if="debug.on">
 */

angular.module("EavServices")
    .factory("debugState", function ($filter, toastr) {
        var translate = $filter("translate");
        var svc = {
            on: false
        };

        svc.toggle = function toggle() {
            svc.on = !svc.on;
            var toast = toastr.info(translate("AdvancedMode.Info.Turn" + (svc.on ? "On" : "Off")), { timeOut: 3000 });
        };

        svc.autoEnableAsNeeded = function (evt) {
            evt = window.event || evt;
            var ctrlAndShiftPressed = evt.ctrlKey;
            if (ctrlAndShiftPressed)
                svc.toggle();
        };

        return svc;
    });