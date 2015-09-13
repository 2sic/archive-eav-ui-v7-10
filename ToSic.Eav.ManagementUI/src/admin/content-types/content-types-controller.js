(function () { 

    angular.module("ContentTypesApp")
        .controller("List", ContentTypeListController)
        .controller("Edit", ContentTypeEditController)
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

}());