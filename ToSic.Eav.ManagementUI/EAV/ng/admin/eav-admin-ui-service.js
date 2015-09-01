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
    .factory('eavAdminDialogs', function($modal, eavGlobalConfigurationProvider, eavManagementSvc) {
        var svc = {};

        svc.dialogs = ["content-types", "content-items", "permissions"];

        //svc.openContentTypesX = function octX(params, callbacks) {

        //};

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

            //svc.openItemEditX = function oieX(resolve, callbacks) {

            //};

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


                //return svc.openMetadataNewX(metadataType, targetType, key, closeCallback);
            };
            
            //svc.openMetadataNewForId = function omdni(metadataType, mode, targetId, closeCallback) {
            //    if (mode != 'attribute') throw 'mode must be attribute';
            //    return svc.openMetadataNewX(metadataType, mode, { keyNumber: targetId }, closeCallback);
            //    //return eavManagementSvc.getContentTypeDefinition(metadataType).then(function (result) {
            //    //    var attSetId = result.data.AttributeSetId;
            //    //    var url = eavGlobalConfigurationProvider.itemForm
            //    //        .getNewItemUrl(attSetId, eavGlobalConfigurationProvider.metadataOfAttribute, { keyNumber: targetId }, false);

            //    //    return PromiseWindow.open(url).then(null, function (error) { if (error == 'closed') closeCallback(); });
            //    //});
            //};

            //svc.openMetadataNewForGuid = function omdng(metadataType, mode, targetGuid, closeCallback) {
            //    if (mode != 'entity') throw 'mode must be entity';
            //    return svc.openMetadataNewX(metadataType, mode, { keyGuid: targetGuid }, closeCallback);
            //};

            //svc.openMetadataNewX = function omdnX(metadataType, mode, key, closeCallback) {
            //    var assignmentType = mode == 'attribute'
            //        ? eavGlobalConfigurationProvider.metadataOfAttribute
            //        : eavGlobalConfigurationProvider.metadataOfEntity;
            //    return eavManagementSvc.getContentTypeDefinition(metadataType).then(function (result) {
            //        var attSetId = result.data.AttributeSetId;
            //        var url = eavGlobalConfigurationProvider.itemForm
            //            .getNewItemUrl(attSetId, assignmentType, key, false);

            //        return PromiseWindow.open(url).then(null, function (error) { if (error == 'closed') closeCallback(); });
            //    });
            //};

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



        // Internal helpers
            svc._attachCallbacks = function attachCallbacks(promise, callbacks) {
                return promise.result.then(callbacks.success || callbacks.close, callbacks.error || callbacks.close, callbacks.notify || callbacks.close);
            };



        return svc;
    })

;