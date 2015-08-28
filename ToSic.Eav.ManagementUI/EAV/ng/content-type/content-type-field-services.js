angular.module('ContentTypeFieldServices', ['ng', 'eavNgSvcs', 'eavGlobalConfigurationProvider'])
    .factory('contentTypeFieldSvc', function ($http, eavGlobalConfigurationProvider, svcCreator, eavManagementSvc) {
        // start with a basic service which implement the live-list functionality
        var svc = svcCreator.implementLiveList(function liveListRetrieve() {
            return $http.get('eav/contenttype/getfields/', { params: { "appid": svc.appId, "staticName": svc.contentType.StaticName } });
        });

        svc.appId = 0;
            svc.contentType = null;

            svc.moveUp = function moveUp(item) {
                return $http.get('eav/contenttype/reorder', { params: { appid: svc.appId, contentTypeId: svc.contentType.Id, attributeId: item.Id, direction: 'up' } })
                    .then(svc.liveListReload);
            };
            svc.moveDown = function moveDown(item) {
                return $http.get('eav/contenttype/reorder', { params: { appid: svc.appId, contentTypeId: svc.contentType.Id, attributeId: item.Id, direction: 'down' } })
                    .then(svc.liveListReload);
            };

            svc.delete = function del(item) {
                return $http.delete('eav/contenttype/delete', { params: { appid: svc.appId, contentTypeId: svc.contentType.Id, attributeId: item.Id } })
                    .then(svc.liveListReload);
            }

            svc.add = function add(item) {
                return $http.post('eav/contenttype/addfield/', item, { params: { appid: svc.appId, contentTypeId: svc.contentType.Id } })
                    .then(svc.liveListReload);
            };

            svc.newItem = function newItem() {
                return {
                    Id: 0,
                    Type: "String",
                    StaticName: "",
                    IsTitle: false
                }
            };


            svc.delete = function del(item) {
                return $http.delete('eav/contenttype/deletefield', { params: { appid: svc.appId, contentTypeId: svc.contentType.Id, attributeId: item.Id } })
                    .then(svc.liveListReload);
            };


            return svc;
        })

;