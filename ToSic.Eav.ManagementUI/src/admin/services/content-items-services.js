angular.module("ContentItemsServices", ["eavNgSvcs", "EavConfiguration"])
    .factory("contentItemsSvc", function($http, entitiesSvc, eavManagementSvc, svcCreator) {
            return function createContentItemsSvc(appId, contentType /*, contentTypeId */) {
                var svc = {};
                svc.contentType = contentType;
                // seems not used: svc.contentTypeId = contentTypeId;
                svc.appId = appId;

                svc = angular.extend(svc, svcCreator.implementLiveList(function getAll() {
                    return $http.get("eav/entities/GetAllOfTypeForAdmin", { params: { appId: svc.appId, contentType: svc.contentType } });
                }));

                // delete, then reload
                svc.delete = function del(id) {
                    return entitiesSvc.delete(svc.contentType, id)
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