// uiDebug
// will add
// vm.debug -> shows if in debug mode - bind ng-ifs to this
// vm.maybeEnableDebug - a method which checks for ctrl+shift-click and if yes, changes debug state
//
// How to use
// 1. add uiDebug to your controller dependencies like:    contentImportController(appId, ..., uiDebug, $modalInstance, $filter)
// 2. add a line after creating your vm-object like:       uiDebug.extendViewModel(vm);
// 3. add a click event as far out as possible on html:    <div ng-click="vm.maybeEnableDebug($event)">
// 4. wrap your hidden stuff in an ng-if:                  <div ng-if="vm.debug">

angular.module("EavServices")
    .factory("uiDebug", function () {
        var svc = {};

        svc.extendViewModel = function extendViewModel(viewModel) {
            viewModel.debug = false;
            viewModel.maybeEnableDebug = function maybeEnableDebug(evt) {
                evt = window.event || evt;
                var ctrlAndShiftPressed = evt.ctrlKey && evt.shiftKey;
                if (ctrlAndShiftPressed)
                    viewModel.debug = !viewModel.debug;
            };
        };

        return svc;
    });