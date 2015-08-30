angular.module('ContentItemsAppServices', ['ng', 'eavNgSvcs', 'eavGlobalConfigurationProvider'])
    .factory('contentItemsSvc', function ($http, entitiesSvc, eavManagementSvc, svcCreator) {
        var svc = {};
        svc.contentType = "";

        svc = angular.extend(svc, svcCreator.implementLiveList(function getAll() {
            return $http.get('eav/entities/GetAllOfTypeForAdmin', { params: { appId: svc.appId, contentType: svc.contentType }});
        }));

        // delete, then reload
        svc.delete = function del(id) {
            return entitiesSvc.delete(svc.contentType, id)
                .then(svc.liveListReload);
        };

        svc.getColumns = function getColumns() {
            return $http.get('eav/contenttype/getfields/', { params: { "appid": svc.appId, "staticName": svc.contentType } });
        };

        return svc;
    })

;