(function() {

    angular.module("ContentTypesApp")
        .controller("Edit", contentTypeEditController);

    /// Edit or add a content-type
    /// Note that the svc can also be null if you don't already have it, the system will then create its own
    function contentTypeEditController(appId, item, contentTypeSvc, debugState, $translate, $uibModalInstance) {
        var vm = this;
        var svc = contentTypeSvc(appId);

        vm.debug = debugState;

        vm.item = item;
        vm.item.ChangeStaticName = false;
        vm.item.NewStaticName = vm.item.StaticName; // in case you really, really want to change it

        vm.ok = function () {
            svc.save(item).then(function() {
                $uibModalInstance.close(vm.item);              
            });
        };

        vm.close = function () {
            $uibModalInstance.dismiss("cancel");
        };
    }

}());