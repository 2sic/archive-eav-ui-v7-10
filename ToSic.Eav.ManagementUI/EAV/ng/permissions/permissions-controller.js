(function () { // TN: this is a helper construct, research iife or read https://github.com/johnpapa/angularjs-styleguide#iife

    angular.module("PermissionsApp", ['PermissionsServices', 'eavGlobalConfigurationProvider'])
        .constant('createdBy', '2sic')          // just a demo how to use constant or value configs in AngularJS
        .constant('licence', 'MIT')             // these wouldn't be necessary, just added for learning exprience
        .controller("Admin", AdminController)
        ;

    function AdminController(permissionsSvc, eavGlobalConfigurationProvider, $location) {
        var vm = this;
        var svc = permissionsSvc;
        permissionsSvc.PermissionTargetGuid = $location.search().Target;

        vm.newUrl = function newUrl() {
            return eavGlobalConfigurationProvider.itemForm
                .getNewItemUrl(svc.ctId, svc.EntityAssignment, { keyGuid: svc.PermissionTargetGuid }, false);
        }
        vm.editUrl = function editUrl(id) {
            return eavGlobalConfigurationProvider.itemForm
                .getEditItemUrl(id, undefined, true);
        }


        vm.permissions = permissionsSvc.liveList();
        
        vm.tryToDelete = function tryToDelete(title, entityId) {
            var ok = confirm("Delete '" + title + "' (" + entityId + ") ?");
            if(ok)
                permissionsSvc.delete(entityId)
        };

        vm.refresh = function refresh() {
            permissionsSvc.getAll();
        }
    };

} ());