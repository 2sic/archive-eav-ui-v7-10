(function () { // TN: this is a helper construct, research iife or read https://github.com/johnpapa/angularjs-styleguide#iife

    angular.module("ContentItemsApp", [
        "EavConfiguration",
        "EavAdminUi",
        "EavServices"
    ])
        .controller("ContentItemsList", ContentItemsListController)
    ;

    function ContentItemsListController(contentItemsSvc, eavConfig, appId, contentType, eavAdminDialogs, debugState, $uibModalInstance) {
        var vm = this;
        vm.debug = debugState;

        var svc = contentItemsSvc(appId, contentType); 

        // config
        vm.maxDynamicColumns = 10;

        vm.add = function add() {
            eavAdminDialogs.openItemNew(contentType, svc.liveListReload);
        };

        vm.edit = function(item) {
            eavAdminDialogs.openItemEditWithEntityId(item.Id, svc.liveListReload);
        };

        vm.refresh = svc.liveListReload;

        vm.items = svc.liveList();

        vm.dynamicColumns = [];
        svc.getColumns().then(function (result) {
            var cols = result.data;
            for (var c = 0; c < cols.length && c < vm.maxDynamicColumns; c++) {
                if (!cols[c].IsTitle)
                    vm.dynamicColumns.push(cols[c]);
            }
        });

        vm.tryToDelete = function tryToDelete(item) {
            if (confirm("Delete '" + "title-unknown-yet" + "' (" + item.RepositoryId + ") ?"))
                svc.delete(item.RepositoryId);
        };

        vm.openDuplicate = function openDuplicate(item) {
            var items = [
                {
                    ContentTypeName: contentType,
                    DuplicateEntity: item.Id
                }
            ];
            eavAdminDialogs.openEditItems(items, svc.liveListReload);

        };

        vm.close = function () { $uibModalInstance.dismiss("cancel"); };

    }

} ());