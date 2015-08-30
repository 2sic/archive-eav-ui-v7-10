(function () { // TN: this is a helper construct, research iife or read https://github.com/johnpapa/angularjs-styleguide#iife

    angular.module("ContentItemsApp", ['ContentItemsAppServices', 'eavGlobalConfigurationProvider'])
        .constant('createdBy', '2sic')          // just a demo how to use constant or value configs in AngularJS
        .constant('licence', 'MIT')             // these wouldn't be necessary, just added for learning exprience
        .controller("ContentItemsList", ContentItemsListController)
        ;

    function ContentItemsListController(contentItemsSvc, eavGlobalConfigurationProvider, appId, contentType, contentTypeId) {
        var vm = this;
        var svc = contentItemsSvc;
        svc.appId = appId;
        svc.contentType = contentType;
        svc.contentTypeId = contentTypeId;
        
        vm.new = function newItem() {
            window.open(eavGlobalConfigurationProvider.itemForm
                .getNewItemUrl(svc.contentTypeId));
        }

        vm.edit = function edit(item) {
            window.open(eavGlobalConfigurationProvider.itemForm
                .getEditItemUrl(item.Id));
        }

        vm.refresh = contentItemsSvc.liveListReload;
        vm.refresh();
        vm.items = contentItemsSvc.liveList();

        //contentTypeFieldSvc.appId = appId;

        vm.dynamicColumns = [];
        contentItemsSvc.getColumns().then(function (result) {
            var cols = result.data;
            for (var c = 0; c < cols.length; c++) {
                if (!cols[c].IsTitle)
                    vm.dynamicColumns.push(cols[c]);
            }
            // alert(result);
        });


        vm.tryToDelete = function tryToDelete(item) {
            if(confirm("Delete '" + 'title-unkwonn-yet' + "' (" + item.Id + ") ?"))
                contentItemsSvc.delete(item.Id)
        };

        vm.refresh = contentItemsSvc.liveListReload;

        vm.gridOptions = {
            showGridFooter: true
        };
    };

} ());