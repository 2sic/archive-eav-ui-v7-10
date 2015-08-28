(function () { // TN: this is a helper construct, research iife or read https://github.com/johnpapa/angularjs-styleguide#iife

    angular.module("ContentTypesApp", ['ContentTypeServices', 'ui.bootstrap', 'ContentTypeFieldServices'])
        .constant('createdBy', '2sic')          // just a demo how to use constant or value configs in AngularJS
        .constant('licence', 'MIT')             // these wouldn't be necessary, just added for learning exprience
        .controller("List", ContentTypeListController)
        .controller("Edit", ContentTypeEditController)
        .controller("FieldList", ContentTypeFieldListController)
        .controller("FieldEdit", ContentTypeFieldEditController)
    ;



    function ContentTypeListController(contentTypeSvc, $modal, $location) {
        var vm = this;

        contentTypeSvc.appId = $location.search().appid;

        vm.items = contentTypeSvc.liveList();

        vm.tryToDelete = function tryToDelete(title, entityId) {
            var ok = confirm("Delete '" + title + "' (" + entityId + ") ?");
            if (ok)
                contentTypeSvc.delete(entityId)
        };

        vm.edit = function edit(item) {
            if (item === undefined)
                item = contentTypeSvc.newItem();

            modalInstance = $modal.open({
                animation: true, 
                templateUrl: 'content-type-edit.html',
                controller: 'Edit',
                controllerAs: 'vm',
                size: 'sm',
                resolve: {
                    item: function () {
                        return item;
                    }
                }
            });

            modalInstance.result.then(function (item) {
                contentTypeSvc.save(item);
            });
        };

        vm.editFields = function editFields(item) {

            if (item === undefined)
                return;

            modalInstance = $modal.open({
                animation: true,
                templateUrl: 'content-type-fields.html',
                controller: 'FieldList',
                controllerAs: 'vm',
                size: 'lg',
                resolve: {
                    contentType: function () {
                        return item;
                    }
                }
            });

            modalInstance.result.then(function (item) {
                // contentTypeSvc.save(item);
            });            
        }
        
        

        vm.refresh = function refresh() {
            contentTypeSvc.liveListReload();
        }

        vm.tryToDelete = function tryToDelete(item) {
            if (confirm("Delete?")) {
                contentTypeSvc.delete(item);
            }
        }

        vm.goTo = function goTo(target, item) {
            switch(target) {
                case 'permissions':
                    alert('todo');
                default:
                    alert("can't find target");
            }
        }

        vm.getUrl = function (mode, id) {
            alert('not implemented yet - should change dialog...');
            switch (mode) {
                case 'export':
                    return eavConf.itemForm.getNewItemUrl(svc.ctId, svc.EntityAssignment, { keyGuid: svc.PermissionTargetGuid }, false);
                case 'import':
                    return eavConf.itemForm.getEditItemUrl(id, undefined, true);
            }
        };
    };

    function ContentTypeEditController(contentTypeSvc, item, $modalInstance) {
        var vm = this;
        
        vm.item = item;

        vm.ok = function () {
            $modalInstance.close(vm.item);
        };

        vm.cancel = function () {
            $modalInstance.dismiss('cancel');
        };
    }

    function ContentTypeFieldEditController(contentTypeFieldSvc, item, $modalInstance) {
        var vm = this;

        vm.item = item;

        vm.ok = function () {
            $modalInstance.close(vm.item);
        };

        vm.cancel = function () {
            $modalInstance.dismiss('cancel');
        };
    }

    function ContentTypeFieldListController(contentTypeSvc, contentTypeFieldSvc, contentType, $modalInstance, $modal) {
        var vm = this;

        contentTypeFieldSvc.appId = contentTypeSvc.appId;
        vm.contentType = contentTypeFieldSvc.contentType = contentType;

        contentTypeFieldSvc.liveListReset();
        vm.items = contentTypeFieldSvc.liveList();

        vm.add = function add() {
            var item = contentTypeFieldSvc.newItem();
            modalInstance = $modal.open({
                animation: true,
                templateUrl: 'content-type-field-edit.html',
                controller: 'FieldEdit',
                controllerAs: 'vm',
                size: 'sm',
                resolve: {
                    item: function () {
                        return item;
                    }
                }
            });

            modalInstance.result.then(function (item) {
                // contentTypeSvc.save(item);
            });

        }
        //vm.ok = function () {
        //    $modalInstance.close(vm.contentType);
        //};

        vm.close = function () {
            $modalInstance.dismiss('cancel');
        };

        vm.moveUp = contentTypeFieldSvc.moveUp;
        vm.moveDown = contentTypeFieldSvc.moveDown;


    }
}());