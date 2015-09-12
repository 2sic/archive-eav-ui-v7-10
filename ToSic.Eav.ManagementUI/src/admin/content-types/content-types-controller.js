(function () { // TN: this is a helper construct, research iife or read https://github.com/johnpapa/angularjs-styleguide#iife

    angular.module("ContentTypesApp", [
        "ContentTypeServices",
        "ContentTypeFieldServices",
        "EavAdminUi",
        "Eavi18n"])
        .constant("createdBy", "2sic")          // just a demo how to use constant or value configs in AngularJS
        .constant("license", "MIT")             // these wouldn't be necessary, just added for learning exprience
        .controller("List", ContentTypeListController)
        .controller("Edit", ContentTypeEditController)
        .controller("FieldList", ContentTypeFieldListController)
        .controller("FieldsAdd", ContentTypeFieldAddController)
    ;


    /// Manage the list of content-types
    function ContentTypeListController(contentTypeSvc, eavAdminDialogs, appId, $translate) {
        var vm = this;
        var svc = contentTypeSvc(appId);

        vm.items = svc.liveList();
        vm.refresh = svc.liveListReload;

        vm.tryToDelete = function tryToDelete(item) {
            $translate("General.Questions.Delete", { target: "'" + item.Name + "' (" + item.Id + ")"}).then(function(msg) {
                if(confirm(msg))
                    svc.delete(item);
            });
        };

        vm.edit = function edit(item) {
            if (item === undefined)
                item = svc.newItem();

            var resolve = eavAdminDialogs.CreateResolve({ item: item, svc: svc });
            return eavAdminDialogs.OpenModal("content-types/content-types-edit.html", "Edit as vm", "sm", resolve);
        };

        vm.editFields = function editFields(item) {
            eavAdminDialogs.openContentTypeFields(item, vm.refresh);
        };

        vm.editItems = function editItems(item) {
            eavAdminDialogs.openContentItems(svc.appId, item.StaticName, item.Id, vm.refresh);
        };

        vm.liveEval = function admin() {
            $translate("General.Questions.SystemInput").then(function (msg) {
                var inp = prompt(msg);
                if(inp)
                    eval(inp); // jshint ignore:line
            });
        };

        vm.isGuid = function isGuid(txtToTest) {
            var patt = new RegExp(/[a-f0-9]{8}(?:-[a-f0-9]{4}){3}-[a-f0-9]{12}/i);
            return patt.test(txtToTest); // note: can't use the txtToTest.match because it causes infinite digest cycles
        };

        vm.permissions = function permissions(item) {
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
    function ContentTypeEditController(appId, svc, item, $modalInstance) {
        var vm = this;
        
        vm.item = item;

        vm.ok = function () {
            svc.save(item);
            $modalInstance.close(vm.item);
        };

        vm.close = function () {
            $modalInstance.dismiss("cancel");
        };
    }

    /// The controller to manage the fields-list
    function ContentTypeFieldListController(appId, contentTypeFieldSvc, contentType, $modalInstance, $modal, eavAdminDialogs, $translate) {
        var vm = this;
        var svc = contentTypeFieldSvc(appId, contentType);

        // to close this dialog
        vm.close = function () {
            $modalInstance.dismiss("cancel");
        };

        // Reset & reload the list - initial reset important, because it could still have the previous list cached
        // svc.liveListReset();
        vm.items = svc.liveList();

        // Open an add-dialog, and add them if the dialog is closed
        vm.add = function add() {
            $modal.open({
                animation: true,
                templateUrl: "content-types/content-types-field-edit.html",
                controller: "FieldsAdd",
                controllerAs: "vm",
                resolve: {
                    dataSvc: function() { return svc; }
                }
            });
        };


        // Actions like moveUp, Down, Delete, Title
        vm.moveUp = svc.moveUp;
        vm.moveDown = svc.moveDown;
        vm.setTitle = svc.setTitle;

        vm.tryToDelete = function tryToDelete(item) {
            if (item.IsTitle) 
                return $translate(["General.Messages.CantDelete", "General.Terms.Title"], {target:"{0}"}).then(function (translations) {
                    alert(translations["General.Messages.CantDelete"].replace("{0}", translations["General.Terms.Title"]));
                });

            $translate("General.Questions.Delete", { target: "'" + item.StaticName + "' (" + item.Id + ")" }).then(function(msg) {
                if (confirm(msg))
                    svc.delete(item);
            });
        };

        // Edit / Add metadata to a specific fields
        vm.createOrEditMetadata = function createOrEditMetadata(item, metadataType) {
            var exists = item.Metadata[metadataType] !== undefined;

            if (exists) {
                eavAdminDialogs.openItemEditWithEntityId(
                    item.Metadata[metadataType].EntityId,
                    svc.liveListReload);
            } else {
                eavAdminDialogs.openMetadataNew(appId, "attribute", item.Id, metadataType,
                    svc.liveListReload);
            }
        };
    }

    /// This is the main controller for adding a field
    /// Add is a standalone dialog, showing 10 lines for new field names / types
    function ContentTypeFieldAddController(dataSvc, $modalInstance) {
        var vm = this;
        var svc = dataSvc;// (appId, contentType);

        // prepare empty array of up to 10 new items to be added
        var nw = svc.newItem;
        vm.items = [nw(), nw(), nw(), nw(), nw(), nw(), nw(), nw(), nw(), nw()];

        vm.item = svc.newItem();
        vm.types = svc.types.liveList();

        vm.ok = function () {
            var items = vm.items;
            var newList = [];
            for (var c = 0; c < items.length; c++)
                if (items[c].StaticName)
                    newList.push(items[c]);
            svc.addMany(newList, 0);
            $modalInstance.close();//vm.items);
        };

        vm.close = function() { $modalInstance.dismiss("cancel"); };
    }
}());