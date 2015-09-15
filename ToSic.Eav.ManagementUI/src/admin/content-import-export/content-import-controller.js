(function () { 

    angular.module("ContentImportExportApp")
        .controller("ContentImport", contentImportController)
        ;

    function contentImportController(eavAdminDialogs, eavConfig, appId, $modalInstance /* $location */) {
        var vm = this;
        //var svc = permissionsSvc(appId, targetGuid);

        //vm.edit = function edit(item) {
        //    eavAdminDialogs.openItemEditWithEntityId(item.Id, svc.liveListReload);
        //};

        //vm.add = function add() {
        //    eavAdminDialogs.openMetadataNew(appId, "entity", svc.PermissionTargetGuid, svc.ctName, svc.liveListReload);
        //};

        //vm.items = svc.liveList();
        //vm.refresh = svc.liveListReload;
        
        //vm.tryToDelete = function tryToDelete(item) {
        //    if (confirm("Delete '" + item.Title + "' (" + item.Id + ") ?"))
        //        svc.delete(item.Id);
        //};

        vm.close = function () {
            $modalInstance.dismiss("cancel");
        };
    }

} ());