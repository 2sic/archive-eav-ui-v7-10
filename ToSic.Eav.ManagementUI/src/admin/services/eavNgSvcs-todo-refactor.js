/*  this file contains various eav-angular services
 *  1. the basic configuration enforcing html5 mode
 *  2. a management-dialog which simply gets the appid if in the url
 *  3. eavManagementSvc - provides some services to retrieve metadata and similar for eav-management dialogs
 *  4. svcCreator - a helper to quickly create services
 *  5. entitiesSvc - a service to get/delete entities
 */

angular.module('eavNgSvcs', ['ng'])

    /// Config to ensure that $location can work and give url-parameters
    .config(['$locationProvider', function ($locationProvider) {
            $locationProvider.html5Mode({
                enabled: true,
                requireBase: false
            });
        } ])

    /// Provide state-information related to the current open dialog
    .factory('eavManagementDialog', function($location){
        var result = {};
        var srch = $location.search();
        result.appId = srch.AppId || srch.appId || srch.appid;
        return result;
    })

    /// Management actions which are rather advanced metadata kind of actions
    .factory('eavManagementSvc', function($http, eavManagementDialog) {
        var svc = {};

        // Retrieve extra content-type info
        svc.getContentTypeDefinition = function getContentTypeDefinition(contentTypeName) {
            alert('using the wrong method - should use the content-type controller. Will work for now, change code please');
            return $http.get('eav/contenttype/get', { params: { appId: eavManagementDialog.appId, contentTypeId: contentTypeName } });
        };

        // Find all items assigned to a GUID
        svc.getAssignedItems = function getAssignedItems(assignedToId, keyGuid, contentTypeName) {
            return $http.get('eav/metadata/getassignedentities', {
                params: {
                    appId: eavManagementDialog.appId,
                    assignmentObjectTypeId: assignedToId,
                    keyType: "guid",
                    key: keyGuid,
                    contentType: contentTypeName
                }
            });
        };
        return svc;
    })

    // This is a helper-factory to create services which manage one live list
    // check examples with the permissions-service or the content-type-service how we use it
    .factory('svcCreator', function() {
        var creator = {};

        // construct a object which has liveListCache, liveListReload(), liveListReset(),  
        creator.implementLiveList = function(getLiveList) {
            var t = {};

            t.liveListCache = [];                   // this is the cached list
            t.liveListCache.isLoaded = false;

            t.liveList = function getAllLive() {
                if (t.liveListCache.length === 0)
                    t.liveListReload();
                return t.liveListCache;
            };

            // use a promise-result to re-fill the live list of all items, return the promise again
            t._liveListUpdateWithResult = function updateLiveAll(result) {
                t.liveListCache.length = 0; // clear
                for (var i = 0; i < result.data.length; i++)
                    t.liveListCache.push(result.data[i]);
                t.liveListCache.isLoaded = true;
                return result;
            };

            t.liveListSourceRead = getLiveList;

            t.liveListReload = function getAll() {
                return t.liveListSourceRead()
                    .then(t._liveListUpdateWithResult);
            };

            t.liveListReset = function resetList() {
                t.liveListCache = [];
            };

            return t;
        };
        return creator;

    })

    /// Standard entity commands like get one, many etc.
    .factory('entitiesSvc', function ($http, eavManagementDialog) {
        var svc = {};

        svc.get = function get(contentType, id) {
            return id ?
                $http.get("eav/entities/getone", { params: { 'contentType': contentType, 'id': id, 'appId': eavManagementDialog.appId } })
                : $http.get("eav/entities/getentities", { params: { 'contentType': contentType, 'appId': eavManagementDialog.appId }});
        };

		svc.getMultiLanguage = function getMultiLanguage(appId, contentType, id) {
			return $http.get("eav/entities/getone", { params: { contentType: contentType, id: id, appId: appId, format: 'multi-language' } });
		};

        svc.delete = function del(type, id) {
            return $http.delete('eav/entities/delete', {
                params: {
                    'contentType': type,
                    'id': id,
                    'appId': eavManagementDialog.appId
                }
            });
        };

		svc.newEntity = function() {
			return {
				Id: null,
				Guid: null,
				Type: {
					Name: $scope.contentTypeName
				},
				Attributes: {}
			};
		};
        
        return svc;
    })

;