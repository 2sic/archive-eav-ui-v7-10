/*  this file contains various eav-angular services
 *  1....
 * todo
 * *** permissions
 * *** item edit with other parameters
 * *** metadat add with other params
 * *** 
 * How to use
 * call a command like openContentItems
 * 
 * the callbacks have the following structure
 * 1. .success (optional)
 * 2. .error (optional) 
 * 3. .notify (optional)
 * 4. .close (optional) --> this one is attached to all events if no primary handler is defined
 */

angular.module('EavAdminUI', ['ng'])
    .factory('adminDialogService', function($modal, eavGlobalConfigurationProvider, eavManagementSvc) {
        var svc = {};

        svc.dialogs = ["content-types", "content-items", "permissions"];

        svc.openContentTypesX = function octX(params, callbacks) {

        };

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
                var modalInstance = $modal.open({
                    animation: true,
                    templateUrl: 'content-type-fields.html',
                    controller: 'FieldList',
                    controllerAs: 'vm',
                    size: 'lg',
                    resolve: resolve
                });
                return svc._attachCallbacks(modalInstance, callbacks);
            };

            svc.openItemEditWithEntityId = function oie(entityId, closeCallback) {
                var url = eavGlobalConfigurationProvider.itemForm.getEditItemUrl(entityId, undefined, true);
                return PromiseWindow.open(url).then(null, function(error) { if(error == 'closed') closeCallback(); });
            };

            svc.openItemEditX = function oieX(resolve, callbacks) {

            };

            
            svc.openMetadataNewOnId = function omdni(metadataType, targetId, closeCallback) {
                return eavManagementSvc.getContentTypeDefinition(metadataType).then(function (result) {
                    var attSetId = result.data.AttributeSetId;
                    var url = eavGlobalConfigurationProvider.itemForm
                        .getNewItemUrl(attSetId, eavGlobalConfigurationProvider.metadataOfAttribute, { keyNumber: targetId }, false);

                    return PromiseWindow.open(url).then(null, function (error) { if (error == 'closed') closeCallback(); });
                });
            };
            
            svc._attachCallbacks = function attachCallbacks(promise, callbacks) {
                return promise.result.then(callbacks.success || callbacks.close, callbacks.error || callbacks.close, callbacks.notify || callbacks.close);
            };



        //// Retrieve extra content-type info
        //svc.getContentTypeDefinition = function getContentTypeDefinition(contentTypeName) {
        //    return $http.get('eav/contenttype/get', { params: { appId: eavManagementDialog.appId, contentTypeId: contentTypeName } });
        //}

        //// Find all items assigned to a GUID
        //svc.getAssignedItems = function getAssignedItems(assignedToId, keyGuid, contentTypeName) {
        //    return $http.get('eav/metadata/getassignedentities', { params: {
        //        appId: eavManagementDialog.appId,
        //        assignmentObjectTypeId: assignedToId,
        //        keyGuid: keyGuid,
        //        contentType: contentTypeName
        //    }
        //    });
        //}
        return svc;
    })

;