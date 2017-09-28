(function () { 

    angular.module("PermissionsApp", [
        "EavServices",
        "EavConfiguration",
        "EavAdminUi"])
        .controller("PermissionList", permissionListController)
        ;

    function permissionListController(permissionsSvc, eavAdminDialogs, eavConfig, appId, targetGuid, $uibModalInstance /* $location */) {
        var vm = this;
        var svc = permissionsSvc(appId, targetGuid);

        vm.edit = function edit(item) {
            eavAdminDialogs.openItemEditWithEntityId(item.Id, svc.liveListReload);
        };

        vm.add = function add() {
            eavAdminDialogs.openMetadataNew(appId, "entity", svc.PermissionTargetGuid, svc.ctName, svc.liveListReload);
        };

        vm.items = svc.liveList();
        vm.refresh = svc.liveListReload;
        
        vm.tryToDelete = function tryToDelete(item) {
            if (confirm("Delete '" + item.Title + "' (" + item.Id + ") ?")) // todo: probably change .Title to ._Title
                svc.delete(item.Id);
        };

        vm.close = function () {
            $uibModalInstance.dismiss("cancel");
        };
    }

} ());