angular.module("HistoryServices", ["ng", "eavNgSvcs"])//, "EavConfiguration"])
    .factory("historySvc", function($http, svcCreator) { //, eavConfig, entitiesSvc, eavManagementSvc, contentTypeSvc) {
        //var eavConf = eavConfig;

        // Construct a service for this specific targetGuid
        return function createSvc(appId, entityId) {
            var svc = {
                appId: appId,
                entityId: entityId
            };

            // When we get the list, reverse-number the results to give it a nice version number
            svc = angular.extend(svc, svcCreator.implementLiveList(function getAll() {
                return $http.get("eav/entities/history", { params: { "appId": svc.appId, "entityId": svc.entityId } })
                .then(function(result) {
                    var list = result.data;
                        for (var i = 0; i < list.length; i++)
                            list[i].VirtualVersion = list.length - i;
                        return result;
                    });
            }));

            svc.getVersionDetails = function getVersionDetails(changeId) {
                return $http.get("eav/entities/historydetails", { params: { "appId": svc.appId, "entityId": svc.entityId, "changeId": changeId } });
            };

            return svc;
        };
    });