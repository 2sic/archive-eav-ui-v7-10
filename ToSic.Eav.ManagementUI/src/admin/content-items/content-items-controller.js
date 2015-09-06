(function () { // TN: this is a helper construct, research iife or read https://github.com/johnpapa/angularjs-styleguide#iife

    angular.module("ContentItemsApp", ['ContentItemsAppServices', 'eavGlobalConfigurationProvider', 'EavAdminUi'])
        .constant('createdBy', '2sic')          // just a demo how to use constant or value configs in AngularJS
        .constant('license', 'MIT')             // these wouldn't be necessary, just added for learning exprience
        .controller("ContentItemsList", ContentItemsListController)
    ;

    function ContentItemsListControllerX() { }

    function ContentItemsListController(contentItemsSvc, eavGlobalConfigurationProvider, appId, contentType, contentTypeId, eavAdminDialogs) {
        var vm = this;
        var svc = contentItemsSvc(appId, contentType, contentTypeId);

        vm.add = function add() {
            eavAdminDialogs.openItemNew(svc.contentTypeId, svc.liveListReload);
        };

        vm.edit = function(item) {
            eavAdminDialogs.openItemEditWithEntityId(item.Id, svc.liveListReload);
        };

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
            if (confirm("Delete '" + 'title-unkwonn-yet' + "' (" + item.Id + ") ?"))
                svc.delete(item.Id);
        };
    }

} ());