(function () { // TN: this is a helper construct, research iife or read https://github.com/johnpapa/angularjs-styleguide#iife

    angular.module("ContentTypesApp", ['ContentTypeServices', 'ContentTypeFieldServices', 'eavGlobalConfigurationProvider', 'EavAdminUi'])
        .constant('createdBy', '2sic')          // just a demo how to use constant or value configs in AngularJS
        .constant('license', 'MIT')             // these wouldn't be necessary, just added for learning exprience
        .controller("List", ContentTypeListController)
        .controller("Edit", ContentTypeEditController)
        .controller("FieldList", ContentTypeFieldListController)
        .controller("FieldsAdd", ContentTypeFieldAddController)
    ;


    /// Manage the list of content-types
    function ContentTypeListController(contentTypeSvc, eavAdminDialogs, appId) {
        var vm = this;
        var svc = contentTypeSvc(appId);

        vm.items = svc.liveList();
        vm.isLoaded = function isLoaded() { return vm.items.isLoaded; };

        vm.tryToDelete = function tryToDelete(title, entityId) {
            var ok = confirm("Delete '" + title + "' (" + entityId + ") ?");
            if (ok)
                svc.delete(entityId);
        };

        vm.edit = function edit(item) {
            if (item === undefined)
                item = svc.newItem();
            eavAdminDialogs.openContentTypeEdit(item, vm.refresh);
        };

        vm.editFields = function editFields(item) {
            eavAdminDialogs.openContentTypeFields(item, vm.refresh);
        };

        vm.editItems = function editItems(item) {
            eavAdminDialogs.openContentItems(svc.appId, item.StaticName, item.Id, vm.refresh);
        };
        

        vm.refresh = svc.liveListReload;

        vm.tryToDelete = function tryToDelete(item) {
            if (confirm("Delete?")) 
                svc.delete(item);
        };
        vm.liveEval = function admin() {
            var inp = prompt("This is for very advanced operations. Only use this if you know what you're doing. \n\n Enter admin commands:");
            if (inp)
                eval(inp); // jshint ignore:line
        };

        vm.isGuid = function isGuid(txtToTest) {
            var patt = new RegExp(/[a-f0-9]{8}(?:-[a-f0-9]{4}){3}-[a-f0-9]{12}/i);
            return patt.test(txtToTest); // note: can't use the txtToTest.match because it causes infinite digest cycles
        };

        vm.permissions = function permissions(item) {
            if (!vm.isGuid(item.StaticName))
                return (alert('Permissions can only be set to Content-Types with Guid Identifiers'));
            return eavAdminDialogs.openPermissionsForGuid(svc.appId, item.StaticName, vm.refresh);
        };

        vm.openExport = function openExport() {
            return eavAdminDialogs.openContentExport(svc.appId);
        };
        vm.openImport = function openImport() {
            return eavAdminDialogs.openContentImport(svc.appId);
        };
    }

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
        // todo: must get appId in and 
        // todo: change to contentTypeSvc(appId)

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
                templateUrl: 'content-types/content-types-field-edit.html',
                controller: 'FieldsAdd',
                controllerAs: 'vm'
            });

            modalInstance.result.then(function(items) {
                var newList = [];
                for (var c = 0; c < items.length; c++)
                    if (items[c].StaticName)
                        newList.push(items[c]);
                contentTypeFieldSvc.addMany(newList, 0);
            });

        };


        // Actions like moveUp, Down, Delete, Title
        vm.moveUp = contentTypeFieldSvc.moveUp;
        vm.moveDown = contentTypeFieldSvc.moveDown;
        vm.setTitle = contentTypeFieldSvc.setTitle;

        vm.tryToDelete = function tryToDelete(item) {
            if (item.IsTitle)
                return alert("Can't delete Title");
            if (confirm("Delete?")) {
                return contentTypeFieldSvc.delete(item);
            }
        };

        // Edit / Add metadata to a specific fields
        vm.createOrEditMetadata = function createOrEditMetadata(item, metadataType) {
            metadataType = '@' + metadataType;
            var exists = item.Metadata[metadataType] !== null;

            if (exists) {
                eavAdminDialogs.openItemEditWithEntityId(
                    item.Metadata[metadataType].EntityId,
                    contentTypeFieldSvc.liveListReload);
            } else {
                eavAdminDialogs.openMetadataNew('attribute', item.Id, metadataType,
                    contentTypeFieldSvc.liveListReload);
            }
        };
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