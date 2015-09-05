angular.module('ContentTypeFieldServices', ['ng', 'eavNgSvcs', 'eavGlobalConfigurationProvider'])
    .factory('contentTypeFieldSvc', function($http, eavGlobalConfigurationProvider, svcCreator, eavManagementSvc) {
        // start with a basic service which implement the live-list functionality
            var svc = {};
            svc.appId = 0;
            svc.contentType = null;

            svc.typeListRetrieve = function typeListRetrieve() {
                return $http.get('eav/contenttype/datatypes/', { params: { "appid": svc.appId } });
            };

            svc = angular.extend(svc, svcCreator.implementLiveList(function liveListRetrieve() {
                return $http.get('eav/contenttype/getfields/', { params: { "appid": svc.appId, "staticName": svc.contentType.StaticName } });
            }));
            
            svc.types = svcCreator.implementLiveList(svc.typeListRetrieve);



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
            };

            svc.addMany = function add(items, count) {
                return $http.get('eav/contenttype/addfield/', { params: items[count] })
                    .then(function() {
                        if (items.length == ++count)
                            svc.liveListReload();
                        else
                            svc.addMany(items, count);
                    });
            };

            svc.add = function addOne(item) {
                return $http.get('eav/contenttype/addfield/', { params: item })
                    .then(svc.liveListReload);
            };

            svc.newItemCount = 0;
            svc.newItem = function newItem() {
                return {
                    AppId: svc.appId,
                    ContentTypeId: svc.contentType.Id,
                    Id: 0,
                    Type: "String",
                    StaticName: "",
                    IsTitle: svc.liveList().length === 0,
                    SortOrder: svc.liveList().length + svc.newItemCount++
                };
            };


            svc.delete = function del(item) {
                if (item.IsTitle)
                    throw "Can't delete Title";
                return $http.delete('eav/contenttype/deletefield', { params: { appid: svc.appId, contentTypeId: svc.contentType.Id, attributeId: item.Id } })
                    .then(svc.liveListReload);
            };

            svc.setTitle = function setTitle(item) {
                return $http.get('eav/contenttype/setTitle', { params: { appid: svc.appId, contentTypeId: svc.contentType.Id, attributeId: item.Id } })
                    .then(svc.liveListReload);
            };


            return svc;
        })

;