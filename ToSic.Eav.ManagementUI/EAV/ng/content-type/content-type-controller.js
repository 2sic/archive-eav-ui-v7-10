(function () { // TN: this is a helper construct, research iife or read https://github.com/johnpapa/angularjs-styleguide#iife

    angular.module("ContentTypesApp", ['ContentTypeServices', 'ui.bootstrap', 'ContentTypeFieldServices', 'eavGlobalConfigurationProvider'])
        .constant('createdBy', '2sic')          // just a demo how to use constant or value configs in AngularJS
        .constant('licence', 'MIT')             // these wouldn't be necessary, just added for learning exprience
        .controller("List", ContentTypeListController)
        .controller("Edit", ContentTypeEditController)
        .controller("FieldList", ContentTypeFieldListController)
        .controller("FieldEdit", ContentTypeFieldEditController)
    ;



    function ContentTypeListController(contentTypeSvc, $modal, $location, eavGlobalConfigurationProvider) {
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

        vm.isGuid = function isGuid(txtToTest) {
            var patt = new RegExp(/[a-f0-9]{8}(?:-[a-f0-9]{4}){3}-[a-f0-9]{12}/i);
            return patt.test(txtToTest); // note: can't use the txtToTest.match because it causes infinite digest cycles
        }

        vm.permissions = function permissions(item) {
            if (!vm.isGuid(item.StaticName))
                return (alert('Permissions can only be set to Content-Types with Guid Identifiers'));
            window.open(
                eavGlobalConfigurationProvider.adminUrls.managePermissions(contentTypeSvc.appId, item.StaticName)
            );
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


    function ContentTypeFieldListController(contentTypeSvc, contentTypeFieldSvc, contentType, eavGlobalConfigurationProvider, eavManagementSvc, $modalInstance, $modal, $q) {
        var vm = this;

        contentTypeFieldSvc.appId = contentTypeSvc.appId;
        vm.contentType = contentTypeFieldSvc.contentType = contentType;

        contentTypeFieldSvc.liveListReset();
        vm.items = contentTypeFieldSvc.liveList();

        vm.add = function add() {
            //var item = contentTypeFieldSvc.newItem();
            modalInstance = $modal.open({
                animation: true,
                templateUrl: 'content-type-field-edit.html',
                controller: 'FieldEdit',
                controllerAs: 'vm'
                //size: 'sm'
            });

            modalInstance.result.then(function (items) {
                // contentTypeFieldSvc.add(item);
                var newList = [];
                for (var c = 0; c < items.length; c++)
                    if (items[c].StaticName)
                        newList.push(items[c]);
                contentTypeFieldSvc.addMany(newList, 0);
            });

        }


        vm.close = function () {
            $modalInstance.dismiss('cancel');
        };

        vm.moveUp = contentTypeFieldSvc.moveUp;
        vm.moveDown = contentTypeFieldSvc.moveDown;

        vm.tryToDelete = function tryToDelete(item) {
            if (item.IsTitle)
                return alert("Can't delete Title");
            if (confirm("Delete?")) {
                contentTypeFieldSvc.delete(item);
            }
        }

        vm.setTitle = contentTypeFieldSvc.setTitle;

        vm.createOrEditMetadata = function createOrEditMetadata(item, metadataType) {
            // todo: first, check if this metadata exists - to decide if new or edit

            metadataType = '@' + metadataType;
            // this is just demo-code...
            var exists = item.Metadata[metadataType] != null;

            var promise = (!exists) 
                ? vm.newUrl(metadataType, item)
                : vm.editUrl(item.Metadata[metadataType].EntityId);

            promise.then(function(url) {
                window.open(url);
            });
        }

        vm.newUrl = function newUrl(metadataType, item) {
            return eavManagementSvc.getContentTypeDefinition(metadataType).then(function (result) {
                var attSetId = result.data.AttributeSetId;
                var deferred = $q.defer();
                var res = eavGlobalConfigurationProvider.itemForm
                    .getNewItemUrl(attSetId, eavGlobalConfigurationProvider.metadataOfAttribute, { keyNumber: item.Id }, false);
                deferred.resolve(res);
                return deferred.promise;
            });
        }

        vm.editUrl = function editUrl(id) {
            var deferred = $q.defer();
            var result = eavGlobalConfigurationProvider.itemForm.getEditItemUrl(id, undefined, true);
            deferred.resolve(result);
            return deferred.promise;
        }

    }

    function ContentTypeFieldEditController(contentTypeFieldSvc, $modalInstance) {
        var vm = this;

        // prepare empty array of up to 10 new items to be added
        var nw = contentTypeFieldSvc.newItem;
        vm.items = [nw(), nw(), nw(), nw(), nw(), nw(), nw(), nw(), nw(), nw()];

        vm.item = contentTypeFieldSvc.newItem();
        vm.types = contentTypeFieldSvc.types.liveList();

        vm.ok = function () {
            $modalInstance.close(vm.items);
        };

        vm.cancel = function () {
            $modalInstance.dismiss('cancel');
        };
    }
}());