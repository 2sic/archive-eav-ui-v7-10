(function () { // TN: this is a helper construct, research iife or read https://github.com/johnpapa/angularjs-styleguide#iife

    angular.module("ContentTypesApp", ['ContentTypeServices', 'ui.bootstrap', 'ContentTypeFieldServices', 'eavGlobalConfigurationProvider', 'ContentItemsApp', 'EavAdminUI', 'PermissionsApp'])
        .constant('createdBy', '2sic')          // just a demo how to use constant or value configs in AngularJS
        .constant('license', 'MIT')             // these wouldn't be necessary, just added for learning exprience
        .controller("List", ContentTypeListController)
        .controller("Edit", ContentTypeEditController)
        .controller("FieldList", ContentTypeFieldListController)
        .controller("FieldsAdd", ContentTypeFieldAddController)
    ;


    /// Manage the list of content-types
    function ContentTypeListController(contentTypeSvc, $location, eavAdminDialogs, eavGlobalConfigurationProvider) {
        var vm = this;

        contentTypeSvc.appId = $location.search().appid;

        vm.items = contentTypeSvc.liveList();
        vm.isLoaded = function isLoaded() { return vm.items.isLoaded; }

        vm.tryToDelete = function tryToDelete(title, entityId) {
            var ok = confirm("Delete '" + title + "' (" + entityId + ") ?");
            if (ok)
                contentTypeSvc.delete(entityId)
        };

        vm.edit = function edit(item) {
            if (item === undefined)
                item = contentTypeSvc.newItem();
            eavAdminDialogs.openContentTypeEdit(item, vm.refresh);
        };

        vm.editFields = function editFields(item) {
            if (item === undefined)
                return;
            eavAdminDialogs.openContentTypeFields(item, vm.refresh);
        }
        
        vm.editItems = function editItems(item) {
            if (item === undefined)
                return;
            eavAdminDialogs.openContentItems(contentTypeSvc.appId, item.StaticName, item.Id, vm.refresh);
        }
        

        vm.refresh = contentTypeSvc.liveListReload;

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
            eavAdminDialogs.openPermissionsForGuid(contentTypeSvc.appId, item.StaticName, vm.refresh);
            //window.open(
            //    eavGlobalConfigurationProvider.adminUrls.managePermissions(contentTypeSvc.appId, item.StaticName)
            //);
        }

    };

    /// Edit or add a content-type
    function ContentTypeEditController(contentTypeSvc, item, $modalInstance) {
        var vm = this;
        
        vm.item = item;

        vm.ok = function () {
            contentTypeSvc.save(item);
            $modalInstance.close(vm.item);
        };

        vm.cancel = function () {
            $modalInstance.dismiss('cancel');
        };
    }

    /// The controller to manage the fields-list
    function ContentTypeFieldListController(contentTypeSvc, contentTypeFieldSvc, contentType, eavGlobalConfigurationProvider, eavManagementSvc, $modalInstance, $modal, $q, eavAdminDialogs) {
        var vm = this;

        contentTypeFieldSvc.appId = contentTypeSvc.appId;
        vm.contentType = contentTypeFieldSvc.contentType = contentType;

        // to close this dialog
        vm.close = function () {
            $modalInstance.dismiss('cancel');
        };

        // Reset & reload the list - initial reset important, because it could still have the previous list cached
        contentTypeFieldSvc.liveListReset();
        vm.items = contentTypeFieldSvc.liveList();

        // Open an add-dialog, and add them if the dialog is closed
        vm.add = function add() {
            modalInstance = $modal.open({
                animation: true,
                templateUrl: 'content-type-field-edit.html',
                controller: 'FieldsAdd',
                controllerAs: 'vm'
            });

            modalInstance.result.then(function (items) {
                var newList = [];
                for (var c = 0; c < items.length; c++)
                    if (items[c].StaticName)
                        newList.push(items[c]);
                contentTypeFieldSvc.addMany(newList, 0);
            });

        }


        // Actions like moveUp, Down, Delete, Title
        vm.moveUp = contentTypeFieldSvc.moveUp;
        vm.moveDown = contentTypeFieldSvc.moveDown;
        vm.setTitle = contentTypeFieldSvc.setTitle;

        vm.tryToDelete = function tryToDelete(item) {
            if (item.IsTitle)
                return alert("Can't delete Title");
            if (confirm("Delete?")) {
                contentTypeFieldSvc.delete(item);
            }
        }

        // Edit / Add metadata to a specific fields
        vm.createOrEditMetadata = function createOrEditMetadata(item, metadataType) {
            metadataType = '@' + metadataType;
            var exists = item.Metadata[metadataType] != null;

            if (exists) {
                eavAdminDialogs.openItemEditWithEntityId(
                    item.Metadata[metadataType].EntityId,
                    contentTypeFieldSvc.liveListReload);
            } else {
                eavAdminDialogs.openMetadataNew('attribute', item.Id, metadataType,
                    contentTypeFieldSvc.liveListReload);
            }
        }
    }

    /// This is the main controller for adding a field
    /// Add is a standalone dialog, showing 10 lines for new field names / types
    function ContentTypeFieldAddController(contentTypeFieldSvc, $modalInstance) {
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