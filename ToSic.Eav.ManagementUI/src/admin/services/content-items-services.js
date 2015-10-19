
angular.module("EavServices")
    .factory("contentItemsSvc", function($http, entitiesSvc, metadataSvc, svcCreator) {
            return function createContentItemsSvc(appId, contentType) {
                var svc = {};
                svc.contentType = contentType;

                svc.appId = appId;

                svc = angular.extend(svc, svcCreator.implementLiveList(function getAll() {
                    return $http.get("eav/entities/GetAllOfTypeForAdmin", { params: { appId: svc.appId, contentType: svc.contentType } });
                }));

                // delete, then reload
                svc.delete = function del(id) {
                    return entitiesSvc.delete(svc.contentType, id) // for now must work with get :( - delete doesn't work well in dnn
                        .then(svc.liveListReload);
                };

                // todo: should use the ContentTypeService instead
                svc.getColumns = function getColumns() {
                    return $http.get("eav/contenttype/getfields/", { params: { "appid": svc.appId, "staticName": svc.contentType } });
                };

                return svc;
            };
        }
    );