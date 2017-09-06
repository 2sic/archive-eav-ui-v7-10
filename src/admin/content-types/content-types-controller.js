(function() {

    angular.module("ContentTypesApp")
        .controller("List", contentTypeListController);


    /// Manage the list of content-types
    function contentTypeListController(contentTypeSvc, eavAdminDialogs, appId, debugState, $translate, eavConfig) {
        var vm = this;
        var svc = contentTypeSvc(appId);

        vm.debug = debugState;

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

            eavAdminDialogs.openContentTypeEdit(item, vm.refresh);
        };

        vm.createGhost = function createGhost() {
            var sourceName = window.prompt("to create a ghost content-type enter source static name / id - this is a very advanced operation - read more about it on 2sxc.org/help?tag=ghost");
            if (!sourceName)
                return;
            svc.createGhost(sourceName);
        };

        vm.editFields = function editFields(item) {
            eavAdminDialogs.openContentTypeFields(item, vm.refresh);
        };

        vm.editItems = function editItems(item) {
            eavAdminDialogs.openContentItems(svc.appId, item.StaticName, item.Id, vm.refresh);
        };

        vm.addItem = function(contentType) {
            eavAdminDialogs.openItemNew(contentType, vm.refresh);
        };


        vm.liveEval = function admin() {
            $translate("General.Questions.SystemInput").then(function (msg) {
                var inp = prompt(msg);
                if(inp)
                    eval(inp); // jshint ignore:line
            });
        };

        // this is to change the scope of the items being shown
        vm.changeScope = function admin() {
            $translate("ContentTypes.Buttons.ChangeScopeQuestion").then(function (msg) {
                var inp = prompt(msg);
                if (inp)
                    svc.setScope(inp);
            });
        };

        vm.isGuid = function isGuid(txtToTest) {
            var patt = new RegExp(/[a-f0-9]{8}(?:-[a-f0-9]{4}){3}-[a-f0-9]{12}/i);
            return patt.test(txtToTest); // note: can't use the txtToTest.match because it causes infinite digest cycles
        };

        vm.permissions = function permissions(item) {
            return eavAdminDialogs.openPermissionsForGuid(svc.appId, item.StaticName, vm.refresh);
        };

        vm.openExport = function openExport(item) {
            return eavAdminDialogs.openContentExport(svc.appId, item.StaticName, vm.refresh);
        };

        vm.openImport = function openImport(item) {
            return eavAdminDialogs.openContentImport(svc.appId, item.StaticName, vm.refresh);
        };

        //#region metadata for this type - new 2016-09-07

        // Edit / Add metadata to a specific field
        vm.createOrEditMetadata = function createOrEditMetadata(item) {
            // assemble an array of items for editing
            var items = [vm.createItemDefinition(item, "ContentType")];
            eavAdminDialogs.openEditItems(items, svc.liveListReload);
        };
        
        vm.createItemDefinition = function createItemDefinition(item, metadataType) {
            var title = "ContentType Metadata"; // todo: i18n
            return item.Metadata  // check if it already has metadata
                ? { EntityId: item.Metadata.Id, Title: title }  // if defined, return the entity-number to edit
                : {
                    ContentTypeName: metadataType,        // otherwise the content type for new-assegnment
                    Metadata: {
                        Key: item.StaticName,
                        KeyType: "string",
                        TargetType: eavConfig.metadataOfContentType
                    },
                    Title: title,
                    Prefill: { Label: item.Name, Description: item.Description }
                };
        };
        //#endregion
    }
}());