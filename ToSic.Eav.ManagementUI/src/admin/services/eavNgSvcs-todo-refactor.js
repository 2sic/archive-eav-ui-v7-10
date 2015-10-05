/*  this file contains various eav-angular services
 *  1. the basic configuration enforcing html5 mode
 *  2. a management-dialog which simply gets the appid if in the url
 *  3. eavManagementSvc - provides some services to retrieve metadata and similar for eav-management dialogs
 *  4. svcCreator - a helper to quickly create services
 *  5. entitiesSvc - a service to get/delete entities
 */

angular.module("eavNgSvcs", ["ng"])

    /// Config to ensure that $location can work and give url-parameters
    .config(["$locationProvider", function ($locationProvider) {
            $locationProvider.html5Mode({
                enabled: true,
                requireBase: false
            });
        } ])

    /// Provide state-information related to the current open dialog
    .factory("eavManagementDialog", function($location){
        var result = {};
        var srch = $location.search();
        result.appId = srch.AppId || srch.appId || srch.appid;
        return result;
    })

    /// Management actions which are rather advanced metadata kind of actions
    .factory("eavManagementSvc", function($http, eavManagementDialog) {
        var svc = {};

        // Find all items assigned to a GUID
        svc.getAssignedItems = function getAssignedItems(assignedToId, keyGuid, contentTypeName) {
            return $http.get("eav/metadata/getassignedentities", {
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

    /// Standard entity commands like get one, many etc.
    .factory("entitiesSvc", function ($http, eavManagementDialog) {
        var svc = {};

        //svc.get = function get(contentType, id) {
        //    return id ?
        //        $http.get("eav/entities/getone", { params: { 'contentType': contentType, 'id': id, 'appId': eavManagementDialog.appId } })
        //        : $http.get("eav/entities/getentities", { params: { 'contentType': contentType, 'appId': eavManagementDialog.appId }});
        //};

		//svc.getMultiLanguage = function getMultiLanguage(appId, contentType, id) {
		//	return $http.get("eav/entities/getone", { params: { contentType: contentType, id: id, appId: appId, format: "multi-language" } });
		//};

		svc.getManyForEditing = function (appId, items) {
		    return $http.post("eav/entities/getmanyforediting", items, { params: { appId: appId } });
		};

		svc.saveMany = function (appId, items) {
		    // first clean up unnecessary nodes - just to make sure we don't miss-read the JSONs transferred
		    var removeTempValue = function(value, key) { delete value._currentValue; };
		    var itmCopy = angular.copy(items);
		    for (var ei = 0; ei < itmCopy.length; ei++)
		        angular.forEach(itmCopy[ei].Entity.Attributes, removeTempValue);

		    return $http.post("eav/entities/savemany", itmCopy, { params: { appId: appId } });
		};

        svc.delete = function del(type, id) {
            return $http.delete("eav/entities/delete", {
                params: {
                    'contentType': type,
                    'id': id,
                    'appId': eavManagementDialog.appId
                }
            });
        };

		svc.newEntity = function(contentTypeName) {
			return {
				Id: null,
				Guid: generateUUID(),
				Type: {
					StaticName: contentTypeName
				},
				Attributes: {},
                IsPublished: true
			};
		};
        
		svc.save = function save(appId, newData) {
		    return $http.post("eav/entities/save", newData, { params: { appId: appId } });
		};

        return svc;
    })

;

// Generate Guid - code from http://stackoverflow.com/a/8809472
function generateUUID() {
    var d = new Date().getTime();
    var uuid = 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
        var r = (d + Math.random() * 16) % 16 | 0;
        d = Math.floor(d / 16);
        return (c == 'x' ? r : (r & 0x3 | 0x8)).toString(16);
    });
    return uuid;
}