// uiDebug
// will add
// vm.debug -> shows if in debug mode - bind ng-ifs to this
// vm.maybeEnableDebug - a method which checks for ctrl+shift-click and if yes, changes debug state

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