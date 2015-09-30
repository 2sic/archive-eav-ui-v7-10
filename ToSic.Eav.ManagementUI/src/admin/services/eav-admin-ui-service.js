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

angular.module("EavAdminUi", ["ng",
    "ui.bootstrap",         // for the $modal etc.
    "EavServices",
    "eavTemplates",         // Provides all cached templates
    "PermissionsApp",       // Permissions dialogs to manage permissions
    "ContentItemsApp",      // Content-items dialog - not working atm?
    "PipelineManagement",   // Manage pipelines
    "ContentImportApp",
    "ContentExportApp",
    "HistoryApp",            // the item-history app
	"eavEditEntity"			// the edit-app
])
    .factory("eavAdminDialogs", function ($modal, eavConfig, eavManagementSvc, contentTypeSvc, $window) {

        var svc = {};

        //#region Content Items dialogs
        svc.openContentItems = function oci(appId, staticName, itemId, closeCallback) {
                var resolve = svc.CreateResolve({ appId: appId, contentType: staticName, contentTypeId: itemId });
                return svc.OpenModal("content-items/content-items.html", "ContentItemsList as vm", "xlg", resolve, closeCallback);
            };
        //#endregion

        //#region content import export
            svc.openContentImport = function ocimp(appId, staticName, closeCallback) {
                var resolve = svc.CreateResolve({ appId: appId, contentType: staticName });
                return svc.OpenModal("content-import-export/content-import.html", "ContentImport as vm", "lg", resolve, closeCallback);
            };

            svc.openContentExport = function ocexp(appId, staticName, closeCallback) {
                var resolve = svc.CreateResolve({ appId: appId, contentType: staticName });
                return svc.OpenModal("content-import-export/content-export.html", "ContentExport as vm", "lg", resolve, closeCallback);
            };

        //#endregion

        //#region ContentType dialogs

            svc.openContentTypeEdit = function octe(item, closeCallback) {
                var resolve = svc.CreateResolve({ item: item });
                return svc.OpenModal("content-types/content-types-edit.html", "Edit as vm", "sm", resolve, closeCallback);
            };

            svc.openContentTypeFields = function octf(item, closeCallback) {
                var resolve = svc.CreateResolve({ contentType: item });
                return svc.OpenModal("content-types/content-types-fields.html", "FieldList as vm", "lg", resolve, closeCallback);
            };
        //#endregion
        
        //#region Item - new, edit
            svc.openItemNew = function oin(contentTypeName, closeCallback) {
                var resolve = svc.CreateResolve({ mode: "new", entityId: null, contentTypeName: contentTypeName });
                return svc.openItemEditWithEntityIdX(resolve, closeCallback);
            };

            svc.openItemEditWithEntityId = function oie(entityId, closeCallback) {
                var resolve = svc.CreateResolve({ mode: "edit", entityId: entityId, contentTypeName: null });
                return svc.openItemEditWithEntityIdX(resolve, closeCallback);
            };

            svc.openItemEditWithEntityIdX = function oieweix(resolve, callbacks) {
            	return svc.OpenModal("wrappers/edit-entity-wrapper.html", "EditEntityWrapperCtrl as vm", "lg", resolve, callbacks);
            };

            svc.openItemHistory = function ioh(entityId, closeCallback) {
                return svc.OpenModal("content-items/history.html", "History as vm", "lg",
                    svc.CreateResolve({ entityId: entityId }),
                    closeCallback);
            };
        //#endregion

        //#region Metadata - mainly new
            svc.openMetadataNew = function omdn(appId, targetType, targetId, metadataType, closeCallback) {
                var key = {};//, assignmentType;
                switch (targetType) {
                    case "entity":
                        key.keyGuid = targetId;
                        key.assignmentType = eavConfig.metadataOfEntity;
                        break;
                    case "attribute":
                        key.keyNumber = targetId;
                        key.assignmentType = eavConfig.metadataOfAttribute;
                        break;
                    default: throw "targetType unknown, only accepts entity or attribute";
                }
                // return eavManagementSvc.getContentTypeDefinition(metadataType)
                return contentTypeSvc(appId).getDetails(metadataType)
                    .then(function (result) {
                    //if (useDummyContentEditor) {
                        var resolve = svc.CreateResolve({ mode: "new", entityId: null, contentTypeName: metadataType });
                        alert(metadataType);
                        resolve = angular.extend(resolve, svc.CreateResolve(key));
                        return svc.openItemEditWithEntityIdX(resolve, { close: closeCallback });
                    //} else {

                    //    var attSetId = result.data.AttributeSetId;
                    //    var url = eavConfig.itemForm
                    //        .getNewItemUrl(attSetId, assignmentType, key, false);

                    //    return PromiseWindow.open(url).then(null, function(error) { if (error == "closed") closeCallback(); });
                    //}
                });
            };
        //#endregion

        //#region Permissions Dialog
            svc.openPermissionsForGuid = function opfg(appId, targetGuid, closeCallback) {
                var resolve = svc.CreateResolve({ appId: appId, targetGuid: targetGuid });
                return svc.OpenModal("permissions/permissions.html", "PermissionList as vm", "lg", resolve, closeCallback);
            };
        //#endregion

        //#region Pipeline Designer
            svc.editPipeline = function ep(appId, pipelineId, closeCallback) {
                var url = eavConfig.adminUrls.pipelineDesigner(appId, pipelineId);
                $window.open(url);
                return;
            };
        //#endregion



        //#region Internal helpers
            svc._attachCallbacks = function attachCallbacks(promise, callbacks) {
                if (typeof (callbacks) === "function") // if it's only one callback, use it for all close-cases
                    callbacks = { close: callbacks };
                return promise.result.then(callbacks.success || callbacks.close, callbacks.error || callbacks.close, callbacks.notify || callbacks.close);
            };

        // Will open a modal window. Has various specials, like
        // 1. If the templateUrl begins with "~/" - this will be re-mapped to the ng-app root. Only use this for not-inline stuff
        // 2. The controller can be written as "something as vm" and this will be split and configured corectly
            svc.OpenModal = function openModal(templateUrl, controller, size, resolveValues, callbacks) {
                var foundAs = controller.indexOf(" as ");
                var contAs = foundAs > 0 ?
                    controller.substring(foundAs + 4)
                    : null;
                if (foundAs > 0)
                    controller = controller.substring(0, foundAs);

                var modalInstance = $modal.open({
                    animation: true,
                    templateUrl: templateUrl,
                    controller: controller,
                    controllerAs: contAs,
                    size: size,
                    resolve: resolveValues
                });

                return svc._attachCallbacks(modalInstance, callbacks);
            };

        /// This will create a resolve-object containing return function()... for each property in the array
            svc.CreateResolve = function createResolve() {
                var fns = {}, list = arguments[0];
                for (var prop in list) 
                    if (list.hasOwnProperty(prop))
                        fns[prop] = svc._create1Resolve(list[prop]);
                return fns;
            };

            svc._create1Resolve = function (value) {
                return function () { return value; };
            };
        //#endregion


        return svc;
    })

;