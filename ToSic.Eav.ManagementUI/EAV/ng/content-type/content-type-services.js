angular.module('ContentTypeServices', ['ng', 'eavNgSvcs', 'eavGlobalConfigurationProvider'])
    .factory('contentTypeSvc', function($http, eavGlobalConfigurationProvider, svcCreator, eavManagementSvc) {

        // start with a basic service which implement the live-list functionality
        var svc = svcCreator.implementLiveList(function liveListRetrieve() {
            return $http.get('eav/contenttype/get/', { params: { "appid": svc.appId, "scope": eavGlobalConfigurationProvider.contentType.defaultScope } });
        });

        svc.appId = 0;

        svc.newItem = function newItem() {
            return {
                StaticName: "",
                Name: "",
                Description: "",
                Scope: eavGlobalConfigurationProvider.contentType.defaultScope
            }
        };


        svc.save = function save(item) {
            return $http.post('eav/contenttype/save/', item, { params: { appid: svc.appId } })
                .then(svc.liveListReload);
        };

        svc.delete = function del(item) {
            return $http.delete('eav/contenttype/delete', { params: { appid: svc.appId, staticName: item.StaticName } })
                .then(svc.liveListReload);
        };


        return svc;
    });