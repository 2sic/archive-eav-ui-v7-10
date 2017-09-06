
angular.module('EavServices')
    .factory('contentItemsSvc', function ($http, entitiesSvc, metadataSvc, svcCreator) {
        return function (appId, contentType) {
            var svc = {};
            svc.contentType = contentType;
            svc.appId = appId;

            svc = angular.extend(svc, svcCreator.implementLiveList(function getAll() {
                return $http.get('eav/entities/GetAllOfTypeForAdmin', { params: { appId: svc.appId, contentType: svc.contentType } });
            }));
            
            // delete, then reload
            svc.delete = function (id, tryForce) {
                return entitiesSvc.delete(svc.contentType, id, tryForce);
                //.then(svc.liveListReload, null);
                //});
            };
            
            // todo: should use the ContentTypeService instead
            svc.getColumns = function () {
                return $http.get("eav/contenttype/getfields/", { params: { appid: svc.appId, staticName: svc.contentType } });
            };

            return svc;
        };
    });