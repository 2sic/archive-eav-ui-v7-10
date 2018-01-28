
angular.module('EavServices')
    .factory('contentItemsSvc', function ($http, entitiesSvc, metadataSvc, svcCreator) {
        return function(appId, contentType) {
            var svc = {
                contentType: contentType,
                appId: appId,
                importItem: importItem
            };

            svc = angular.extend(svc,
                svcCreator.implementLiveList(function getAll() {
                    return $http.get('eav/entities/GetAllOfTypeForAdmin',
                        { params: { appId: svc.appId, contentType: svc.contentType } });
                }));

            // delete, then reload
            svc.delete = function(id, tryForce) {
                return entitiesSvc.delete(svc.contentType, id, tryForce);
            };

            // todo: should use the ContentTypeService instead
            svc.getColumns = function() {
                return $http.get('eav/contenttype/getfields/',
                    { params: { appid: svc.appId, staticName: svc.contentType } });
            };

            function importItem(args) {
                return $http.post('eav/contentimport/import', { AppId: svc.appId, ContentBase64: args.File.base64 });
            }


            return svc;
        };
    });