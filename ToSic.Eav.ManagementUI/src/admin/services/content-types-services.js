angular.module('ContentTypeServices', ['ng', 'eavNgSvcs', "EavConfiguration"])
    .factory('contentTypeSvc', function ($http, eavConfig, svcCreator) {
        return function appSpecificContentTypeSvc(appId) {
            var svc = {};
            svc.scope = eavConfig.contentType.defaultScope;
            svc.appId = appId;

            svc.retrieveContentTypes = function typeListRetrieve() {
                return $http.get('eav/contenttype/get/', { params: { "appid": svc.appId, "scope": svc.scope } });
            };

            svc = angular.extend(svc, svcCreator.implementLiveList(svc.retrieveContentTypes));

            svc.getDetails = function getDetails(contentTypeName) {
                return $http.get('eav/contenttype/get/', { params: { "appid": svc.appId, "contentTypeId": contentTypeName } });
            };

            svc.newItem = function newItem() {
                return {
                    StaticName: "",
                    Name: "",
                    Description: "",
                    Scope: eavConfig.contentType.defaultScope
                };
            };

            svc.save = function save(item) {
                return $http.post('eav/contenttype/save/', item, { params: { appid: svc.appId } })
                    .then(svc.liveListReload);
            };

            svc.delete = function del(item) {
                return $http.delete('eav/contenttype/delete', { params: { appid: svc.appId, staticName: item.StaticName } })
                    .then(svc.liveListReload);
            };

            svc.setScope = function setScope(newScope) {
                svc.scope = newScope;
                svc.liveListReload();
            };
            return svc;
        };
        //return svc;
    });