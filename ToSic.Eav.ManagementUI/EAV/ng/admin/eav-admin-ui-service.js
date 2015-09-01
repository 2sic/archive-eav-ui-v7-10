/*  this file contains a service to handle 
 * How it works
 * This service tries to open a modal dialog if it can, otherwise a new window returning a promise to allow
 * ...refresh when the window close. 
 * 
 * In most cases there is a nice command to open something, like openItemEditWithEntityId(id, callback)
 * ...and there is also a more advanced version where you could specify more closely what you wanted
 * ...usually ending with an X, so like openItemEditWithEntityIdX(resolve, callbacks)
 * 
 * the simple callback is 1 function (usually to refresh the main list), the complex callbacks have the following structure
 * 1. .success (optional)
 * 2. .error (optional) 
 * 3. .notify (optional)
 * 4. .close (optional) --> this one is attached to all events if no primary handler is defined 
 *  
 * How to use
 * 1. you must already include all js files in your main app - so the controllers you'll need must be preloaded
 * 2. Your main app must also  declare the other apps as dependencies, so angular.module('yourname', ['dialog 1', 'diolag 2'])
 * 3. your main app must also need this ['EavAdminUI']
 * 4. your controller must require eavAdminDialogs
 * 5. Then you can call such a dialog
 */


// Todo
// 1. Import / Export
// 2. Pipeline Designer

angular.module('EavAdminUi', ['ng'])
    .factory('eavAdminDialogs', function($modal, eavGlobalConfigurationProvider, eavManagementSvc) {
        var svc = {};

        //#region Content Items dialogs
            svc.openContentItems = function oci(appId, staticName, itemId, closeCallback) {
                return svc.openContentItemsX({
                    appId: function() { return appId; },
                    contentType: function() { return staticName; },
                    contentTypeId: function() { return itemId; }
                }, { close: closeCallback });
            };

            svc.openContentItemsX = function ociX(resolve, callbacks) {
                var modalInstance = $modal.open({
                    animation: true,
                    templateUrl: '/eav/ng/content-items/content-items.html',
                    controller: 'ContentItemsList',
                    controllerAs: 'vm',
                    size: 'lg',
                    resolve: resolve
                });

                return svc._attachCallbacks(modalInstance, callbacks);
            };

        //#endregion

        //#region ContentType dialogs
            svc.openContentTypeEdit = function octe(item, closeCallback) {
                return svc.openContentTypeEditX({
                    item: function() { return item; }
                }, { close: closeCallback });
            };

            svc.openContentTypeEditX = function octeX(resolve, callbacks) {
                var modalInstance = $modal.open({
                    animation: true,
                    templateUrl: 'content-type-edit.html',
                    controller: 'Edit',
                    controllerAs: 'vm',
                    size: 'sm',
                    resolve: resolve
                });

                return svc._attachCallbacks(modalInstance, callbacks);
            };

            svc.openContentTypeFields = function octf(item, closeCallback) {
                return svc.openContentTypeFieldsX({
                    contentType: function() { return item; }
                }, { close: closeCallback });
            };

            svc.openContentTypeFieldsX = function octfX(resolve, callbacks) {
                //resolve.close = function() {
                //    alert('closing');
                //}
                resolve._modalInstance = $modal.open({
                    animation: true,
                    templateUrl: 'content-type-fields.html',
                    controller: 'FieldList',
                    controllerAs: 'vm',
                    size: 'lg',
                    resolve: resolve
                });
                return svc._attachCallbacks(resolve._modalInstance, callbacks);
            };
        //#endregion

        //#region Item - new, edit
            svc.openItemNew = function oin(contentTypeId, closeCallback) {
                var url = eavGlobalConfigurationProvider.itemForm.getNewItemUrl(contentTypeId);
                return PromiseWindow.open(url).then(null, function (error) { if (error == 'closed') closeCallback(); });
            };

            svc.openItemEditWithEntityId = function oie(entityId, closeCallback) {
                var url = eavGlobalConfigurationProvider.itemForm.getEditItemUrl(entityId, undefined, true);
                return PromiseWindow.open(url).then(null, function(error) { if(error == 'closed') closeCallback(); });
            };

        //#endregion

        //#region Metadata - mainly new
            svc.openMetadataNew = function omdn(targetType, targetId, metadataType, closeCallback) {
                var key = {}, assignmentType = 0;
                switch (targetType) {
                    case 'entity':
                        key.keyGuid = targetId;
                        assignmentType = eavGlobalConfigurationProvider.metadataOfEntity;
                        break;
                    case 'attribute':
                        key.keyNumber = targetId;
                        assignmentType = eavGlobalConfigurationProvider.metadataOfAttribute;
                        break;
                    default: throw 'targetType unknown, only accepts entity or attribute';
                }
                return eavManagementSvc.getContentTypeDefinition(metadataType).then(function (result) {
                    var attSetId = result.data.AttributeSetId;
                    var url = eavGlobalConfigurationProvider.itemForm
                        .getNewItemUrl(attSetId, assignmentType, key, false);

                    return PromiseWindow.open(url).then(null, function (error) { if (error == 'closed') closeCallback(); });
                });
            };
        //#endregion

        //#region Permissions Dialog
            svc.openPermissionsForGuid = function opfg(appId, targetGuid, closeCallback) {
                return svc.openPermissionsForGuidX({
                    appId: function() { return appId },
                    targetGuid: function() { return targetGuid }
                }, { close: closeCallback });


                // window-mode
                //var url = eavGlobalConfigurationProvider.adminUrls.managePermissions(appId, contentTypeName);
                //return PromiseWindow.open(url).then(null, function (error) { if (error == 'closed') closeCallback(); });
            };

            svc.openPermissionsForGuidX = function opfgX(resolve, callbacks) {
                var modalInstance = $modal.open({
                    animation: true,
                    templateUrl: '/eav/ng/permissions/permissions.html',
                    controller: 'PermissionList',
                    controllerAs: 'vm',
                    size: 'lg',
                    resolve: resolve
                });
                return svc._attachCallbacks(modalInstance, callbacks);
            };
        //#endregion

        //#region Export / Import content Types

        svc.openContentExport = function oce(appId, closeCallback) {
            var url = eavGlobalConfigurationProvider.adminUrls.exportContent(appId);
            window.open(url);
        };
        svc.openContentImport = function oci(appId, closeCallback) {
            var url = eavGlobalConfigurationProvider.adminUrls.importContent(appId);
            window.open(url);
        };
        //#endregion

        //#region Internal helpers
            svc._attachCallbacks = function attachCallbacks(promise, callbacks) {
                return promise.result.then(callbacks.success || callbacks.close, callbacks.error || callbacks.close, callbacks.notify || callbacks.close);
            };
        //#endregion


        return svc;
    })

;