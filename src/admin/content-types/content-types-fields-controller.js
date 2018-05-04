/*jshint laxbreak:true */
(function () {
    angular.module("ContentTypesApp")
        .controller("FieldList", contentTypeFieldListController)
        ;

    /// The controller to manage the fields-list
    function contentTypeFieldListController(appId, contentTypeFieldSvc, contentType, $uibModalInstance, $uibModal, eavAdminDialogs, $filter, $translate, eavConfig, $scope) {
        var vm = this;
        var svc = contentTypeFieldSvc(appId, contentType);

        // to close this dialog
        vm.close = function () {
            $uibModalInstance.dismiss("cancel");
        };

        vm.items = svc.liveList();

        vm.orderList = function () {
            var orderList = [];
            vm.items.map(function (e, i) {
                orderList.push(e.Id);
            });
            return orderList;
        };

        vm.treeOptions = {
            dropped: function () {
                vm.dragEnabled = false; // Disable drag while updating (causes strange effects like duplicate items)
                svc.reOrder(vm.orderList()).then(function () {
                    vm.dragEnabled = true;
                });
            }
        };

        vm.dragEnabled = true;

        // Open an add-dialog, and add them if the dialog is closed
        vm.add = function add() {
            $uibModal.open({
                animation: true,
                templateUrl: "content-types/content-types-fields-add.html",
                controller: "FieldsAdd",
                controllerAs: "vm",
                size: "lg",
                resolve: {
                    svc: function () { return svc; }
                }
            });
        };

        vm.edit = function edit(item) {
            $uibModal.open({
                animation: true,
                templateUrl: "content-types/content-types-field-edit.html",
                controller: "FieldEdit",
                controllerAs: "vm",
                size: "lg",
                resolve: {
                    svc: function () { return svc; },
                    item: function () { return item; }
                }
            });

        };

        vm.inputTypeTooltip = function (inputType) {
            if (inputType !== "unknown")
                return inputType;

            return "unknown means it's using an old definition for input-types - edit it to use the new definition";
        };

        // Actions like moveUp, Down, Delete, Title
        //vm.moveUp = svc.moveUp;
        //vm.moveDown = svc.moveDown;
        vm.setTitle = svc.setTitle;

        vm.tryToDelete = function tryToDelete(item) {
            if (item.IsTitle)
                return $translate(["General.Messages.CantDelete", "General.Terms.Title"], { target: "{0}" }).then(function (translations) {
                    alert(translations["General.Messages.CantDelete"].replace("{0}", translations["General.Terms.Title"]));
                });

            return $translate("General.Questions.Delete", { target: "'" + item.StaticName + "' (" + item.Id + ")" }).then(function (msg) {
                if (confirm(msg))
                    svc.delete(item);
            });
        };

        vm.rename = function rename(item) {
            $translate("General.Questions.Rename", { target: "'" + item.StaticName + "' (" + item.Id + ")" }).then(function (msg) {
                var newName = prompt(msg);
                if (newName)
                    svc.rename(item, newName);
            });
      };

      vm.permissions = function(item) {
        console.log(item);
        eavAdminDialogs.openPermissions(appId, eavConfig.metadataOfAttribute, "number", item.Id, svc.liveListReload);
      };

        // Edit / Add metadata to a specific fields
        vm.createOrEditMetadata = function createOrEditMetadata(item, metadataType) {
            // assemble an array of 2 items for editing
            var items = [
                vm.createItemDefinition(item, "All"),
                vm.createItemDefinition(item, metadataType),
                vm.createItemDefinition(item, item.InputType)
            ];
            eavAdminDialogs.openEditItems(items, svc.liveListReload);
        };

        vm.createItemDefinition = function createItemDefinition(item, metadataType) {
            var title = metadataType === "All" ? $translate.instant("DataType.All.Title") : metadataType;
            return item.Metadata[metadataType] !== undefined
                ? { EntityId: item.Metadata[metadataType].Id, Title: title }  // if defined, return the entity-number to edit
                : {
                    ContentTypeName: "@" + metadataType,        // otherwise the content type for new-assegnment
                    Metadata: {
                        Key: item.Id,
                        KeyType: "number",
                        TargetType: eavConfig.metadataOfAttribute
                    },
                    Title: title,
                    Prefill: { Name: item.StaticName }
                };
        };
    }

}());