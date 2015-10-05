/*jshint laxbreak:true */
(function () {
    angular.module("ContentTypesApp")
        .controller("FieldList", contentTypeFieldListController)
        .controller("FieldsAdd", contentTypeFieldAddController)
    ;

    /// The controller to manage the fields-list
    function contentTypeFieldListController(appId, contentTypeFieldSvc, contentType, $modalInstance, $modal, eavAdminDialogs, $translate, eavConfig) {
        var vm = this;
        var svc = contentTypeFieldSvc(appId, contentType);

        // to close this dialog
        vm.close = function () {
            $modalInstance.dismiss("cancel");
        };

        vm.items = svc.liveList();

        // Open an add-dialog, and add them if the dialog is closed
        vm.add = function add() {
            $modal.open({
                animation: true,
                templateUrl: "content-types/content-types-field-edit.html",
                controller: "FieldsAdd",
                controllerAs: "vm",
                resolve: {
                    svc: function() { return svc; }
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
            // assemble an array of 2 items for editing
            var items = [vm.createItemDefinition(item, "All"), vm.createItemDefinition(item, metadataType)];
            eavAdminDialogs.openEditItems(items, svc.liveListReload);
        };

        vm.createItemDefinition = function createItemDefinition(item, metadataType) {
            var title = metadataType == "All" ? "General" : metadataType;
            return item.Metadata[metadataType] !== undefined
                ? { EntityId: item.Metadata[metadataType].Id, Title: title }  // if defined, return the entity-number to edit
                : {
                    ContentTypeName: "@" + metadataType,        // otherwise the content type for new-assegnment
                    Metadata: {
                        Key: item.Id,
                        KeyType: "number",
                        TargetType: eavConfig.metadataOfAttribute
                    },
                    Title: title
                };      
        };
    }

    /// This is the main controller for adding a field
    /// Add is a standalone dialog, showing 10 lines for new field names / types
    function contentTypeFieldAddController(svc, $modalInstance) {
        var vm = this;

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
            $modalInstance.close();
        };

        vm.close = function() { $modalInstance.dismiss("cancel"); };
    }
}());