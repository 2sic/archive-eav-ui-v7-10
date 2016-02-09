(function () { 

    angular.module("ContentEditApp", [
        "EavServices",
        "EavAdminUi"
    ])
        .controller("EditContentItem", EditContentItemController)
        ;

    function EditContentItemController(mode, entityId, contentType, eavAdminDialogs, $modalInstance) { //}, contentTypeId, eavAdminDialogs) {
        var vm = this;
        vm.mode = mode;
        vm.entityId = entityId;
        vm.contentType = contentType;
        vm.TestMessage = "Test message the controller is binding correctly...";

        vm.history = function history() {
            return eavAdminDialogs.openItemHistory(vm.entityId);
        };

        vm.close = function () { $modalInstance.dismiss("cancel"); };
    }

} ());