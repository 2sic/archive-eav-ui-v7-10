// metadata
// retrieves metadata for an entity or an attribute

angular.module("EavServices")
    /// Management actions which are rather advanced metadata kind of actions
    .factory("metadataSvc", function($http, appId) {
        var svc = {};

        // Find all items assigned to a GUID
        svc.getMetadata = function getMetadata(assignedToId, keyGuid, contentTypeName) {
            return $http.get("eav/metadata/getassignedentities", {
                params: {
                    appId: appId,
                    assignmentObjectTypeId: assignedToId,
                    keyType: "guid",
                    key: keyGuid,
                    contentType: contentTypeName
                }
            });
        };
        return svc;
    });