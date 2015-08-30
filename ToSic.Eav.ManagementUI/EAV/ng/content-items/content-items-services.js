angular.module('ContentItemsAppServices', ['ng', 'eavNgSvcs', 'eavGlobalConfigurationProvider'])
    .factory('contentItemsSvc', function ($http, eavGlobalConfigurationProvider, entitiesSvc, eavManagementSvc, svcCreator) {
        var eavConf = eavGlobalConfigurationProvider;

        var svc = {};
            svc.contentType = "";// "PermissionConfiguration";
        // svc.ctId = 0;
        // svc.PermissionTargetGuid = '{00000000-0000-0000-0000-000000000000}';
        // svc.EntityAssignment = eavConf.metadataOfEntity;

        svc = angular.extend(svc, svcCreator.implementLiveList(function getAll() {
            //return entitiesSvc.get(svc.contentType);
            return $http.get('eav/entities/GetAllOfTypeForAdmin', { params: { appId: svc.appId, contentType: svc.contentType }});
            // return eavManagementSvc.getAssignedItems(svc.EntityAssignment, svc.PermissionTargetGuid, svc.ctName).then(svc.updateLiveAll);
        }));

        // Get ID of this content-type
        //eavManagementSvc.getContentTypeDefinition(svc.ctName).then(function (result) {
        //    svc.ctId = result.data.AttributeSetId;
        //});

        // delete, then reload
        svc.delete = function del(id) {
            return entitiesSvc.delete(svc.contentType, id)
                .then(svc.liveListReload);
        };

        return svc;
    })

;