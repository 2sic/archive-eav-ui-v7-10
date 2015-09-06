angular.module('PermissionsServices', ['ng', 'eavNgSvcs', 'eavGlobalConfigurationProvider'])
    .factory('permissionsSvc', function($http, eavGlobalConfigurationProvider, entitiesSvc, eavManagementSvc, svcCreator, contentTypeSvc) {
        var eavConf = eavGlobalConfigurationProvider;

        // Construct a service for this specific targetGuid
        return function createSvc(appId, permissionTargetGuid) {
            var svc = {
                PermissionTargetGuid: permissionTargetGuid,
                ctName: "PermissionConfiguration",
                ctId: 0,
                EntityAssignment: eavConf.metadataOfEntity,
                ctSvc: contentTypeSvc(appId)
            };

            svc = angular.extend(svc, svcCreator.implementLiveList(function getAll() {
                return eavManagementSvc.getAssignedItems(svc.EntityAssignment, svc.PermissionTargetGuid, svc.ctName).then(svc.updateLiveAll);
            }));

            // Get ID of this content-type 
            // todo refactor - this sholud be in the conetnttype conroller
            //eavManagementSvc.getContentTypeDefinition(svc.ctName).then(function (result) {
            svc.ctSvc.getDetails(svc.ctName).then(function (result) {
                svc.ctId = result.data.AttributeSetId;
            });

            // delete, then reload
            svc.delete = function del(id) {
                return entitiesSvc.delete(svc.ctName, id)
                    .then(svc.liveListReload);
            };
            return svc;
        };
    });