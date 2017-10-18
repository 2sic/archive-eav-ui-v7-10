
angular.module("EavServices")
    .factory("contentTypeSvc", function ($http, eavConfig, svcCreator, $translatePartialLoader, $translate) {
        return function appSpecificContentTypeSvc(appId, scope) {
            var svc = {};
            svc.scope = scope || eavConfig.contentType.defaultScope;
            svc.appId = appId;

            svc.retrieveContentTypes = function typeListRetrieve() {
                return $http.get("eav/contenttype/get/", { params: { "appid": svc.appId, "scope": svc.scope } });
            };

            svc = angular.extend(svc, svcCreator.implementLiveList(svc.retrieveContentTypes));

            svc.getDetails = function getDetails(contentTypeName, config) {
                return $http.get("eav/contenttype/GetSingle", angular.extend({}, config, {
                    params: { "appid": svc.appId, "contentTypeStaticName": contentTypeName }
                }))
                    .then(function (promise) {
                        // check if definition asks for external i18n, then add to loader
                        if (promise && promise.data && promise.data.I18nKey) {
                            $translatePartialLoader.addPart("content-types/" + promise.data.I18nKey);
                        }
                        return promise;
                    });
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
                return $http.post("eav/contenttype/save/", item, { params: { appid: svc.appId } })
                    .then(svc.liveListReload);
            };

            svc.delete = function del(item) {
                return $http.get("eav/contenttype/delete", { params: { appid: svc.appId, staticName: item.StaticName } })
                    .then(svc.liveListReload);
            };

            svc.setScope = function setScope(newScope) {
                svc.scope = newScope;
                svc.liveListReload();
            };

            svc.createGhost = function createGhost(sourceStaticName) {
                return $http.get("eav/contenttype/createghost", { params: { appid: svc.appId, sourceStaticName: sourceStaticName } })
                    .then(svc.liveListReload);
            };
            return svc;
        };

    });