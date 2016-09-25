(function () {
    /*jshint laxbreak:true */
    angular.module("ContentTypesApp")
        .controller("FieldEdit", contentTypeFieldEditController)
    ;

    /// This is the main controller for adding a field
    /// Add is a standalone dialog, showing 10 lines for new field names / types
    function contentTypeFieldEditController(appId, svc, item, $filter, $uibModalInstance) {
        var vm = this;

        vm.items = [item];

        vm.types = svc.types.liveList();

        vm.allInputTypes = svc.getInputTypesList();

        vm.resetSubTypes = function resetSubTypes(item) {
            item.InputType = item.Type.toLowerCase() + "-default";
        };

        vm.ok = function () {
            svc.updateInputType(vm.items[0]);
            $uibModalInstance.close();
        };

        vm.close = function() { $uibModalInstance.dismiss("cancel"); };
    }
}());