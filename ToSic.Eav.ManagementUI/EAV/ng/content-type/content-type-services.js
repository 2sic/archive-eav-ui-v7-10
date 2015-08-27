angular.module('ContentTypeServices', ['ng', 'eavNgSvcs', 'eavGlobalConfigurationProvider'])
    .factory('contentTypeSvc', function ($http, eavGlobalConfigurationProvider, eavManagementSvc) {
            var svc = {};
            var eavConf = eavGlobalConfigurationProvider;
            svc.appId = 0;

            // Cached list
            svc._all = [];
            svc.allLive = function getAllLive() {
                if (svc._all.length == 0)
                    svc.getAll();
                return svc._all;
            };

            // use a promise-result to re-fill the live list of all items, return the promise again
            svc.updateLiveAll = function updateLiveAll(result) {
                svc._all.length = 0; // clear
                for (var i = 0; i < result.data.length; i++)
                    svc._all.push(result.data[i]);
                return result;
            };

            svc.getAll = function getAll() {
                return $http.get('eav/contenttype/get/', { params: { "appid": svc.appId, "scope": eavConf.contentType.defaultScope } })
                    .then(svc.updateLiveAll); 
            };

            svc.save = function save(item) {
                return $http.post('eav/contenttype/save/', item, { params: { appid: svc.appId } })
                    .then(svc.getAll);
            };

            svc.newItem = function newItem() {
                return {
                    StaticName: "",
                    Name: "",
                    Description: "",
                    Scope: eavConf.contentType.defaultScope
                }
            };


            svc.delete = function del(item) {
                return $http.delete('eav/contenttype/delete', { params: { appid: svc.appId, staticName: item.StaticName } })
                    .then(svc.getAll);
            };


            return svc;
        })

;