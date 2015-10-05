(function () { // TN: this is a helper construct, research iife or read https://github.com/johnpapa/angularjs-styleguide#iife

    angular.module("ContentItemsApp", [
        "EavConfiguration",
        "EavAdminUi",
        "EavServices"
    ])
        .controller("ContentItemsList", ContentItemsListController)
    ;

    function ContentItemsListController(contentItemsSvc, eavConfig, appId, contentType, /*contentTypeId, */ eavAdminDialogs, $modalInstance) {
        var vm = this;
        var svc = contentItemsSvc(appId, contentType); //, contentTypeId);

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
            for (var c = 0; c < cols.length; c++) {
                if (!cols[c].IsTitle)
                    vm.dynamicColumns.push(cols[c]);
            }
        });

        vm.tryToDelete = function tryToDelete(item) {
            if (confirm("Delete '" + "title-unknown-yet" + "' (" + item.Id + ") ?"))
                svc.delete(item.Id);
        };
        vm.close = function () { $modalInstance.dismiss("cancel"); };

    }

} ());