(function () { // TN: this is a helper construct, research iife or read https://github.com/johnpapa/angularjs-styleguide#iife

    angular.module("ContentTypesApp", ['ContentTypeServices', 'ui.bootstrap'])
        .constant('createdBy', '2sic')          // just a demo how to use constant or value configs in AngularJS
        .constant('licence', 'MIT')             // these wouldn't be necessary, just added for learning exprience
        .controller("List", ContentTypeListController)
        .controller("Edit", ContentTypeEditController)
    ;



    function ContentTypeListController(contentTypeSvc, $modal, $location) {
        var vm = this;

        contentTypeSvc.appId = $location.search().appid;
        // permissionsSvc.PermissionTargetGuid = $location.search().Target;

        vm.getUrl = contentTypeSvc.getUrl;

        // debugger;

        vm.items = contentTypeSvc.allLive();

        vm.tryToDelete = function tryToDelete(title, entityId) {
            var ok = confirm("Delete '" + title + "' (" + entityId + ") ?");
            if (ok)
                contentTypeSvc.delete(entityId)
        };

        vm.edit = function edit(item) {
            // debugger;
            if (item === undefined)
                item = contentTypeSvc.newItem();

            modalInstance = $modal.open({
                animation: true, //$scope.animationsEnabled,
                templateUrl: 'content-type-edit.html',
                controller: 'Edit',
                controllerAs: 'vm',
                size: 'sm',
                resolve: {
                    item: //item
                        function () {
                        return item;
                    }
                }
            });

            modalInstance.result.then(function (selectedItem) {
                contentTypeSvc.save(item);
            }//, 
            //function () {
            //    $log.info('Modal dismissed at: ' + new Date());
            //}
            );
        };
        
        

        vm.refresh = function refresh() {
            contentTypeSvc.getAll();
        }

        //vm.create = function create() {
        //    alert('todo');
        //}

        vm.tryToDelete = function tryToDelete(item) {
            if (confirm("Delete?")) {
                contentTypeSvc.delete(item);
            }
        }
    };

    function ContentTypeEditController(contentTypeSvc, item, $modalInstance) {
        var vm = this;
        
        vm.item = item;
        //var contentTypeGuid = item.Guid;
        //if (contentTypeGuid !== null) {
        //    vm.contentTypeGuid = contentTypeGuid;

        //    // todo: load from svc
        //    contentTypeSvc.get(contentTypeGuid);
        //}

        vm.ok = function () {
            $modalInstance.close(vm.item);
        };

        vm.cancel = function () {
            $modalInstance.dismiss('cancel');
        };

        vm.save = function save() {
            //contentTypeSvc.create
        }
    }

}());