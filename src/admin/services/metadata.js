// metadata
// retrieves metadata for an entity or an attribute

angular.module('EavServices')
  // Management actions which are rather advanced metadata kind of actions
  .factory('metadataSvc',
    function($http, appId) {
      var svc = {};

      // Find all items assigned to a GUID
      svc.getMetadata = function getMetadata(typeId, keyGuid, contentTypeName) {
        console.log('using deprecated getMetadata - try to migrate code to get2');
        return $http.get('eav/metadata/getassignedentities',
          {
            params: {
              appId: appId,
              assignmentObjectTypeId: typeId,
              keyType: 'guid',
              key: keyGuid,
              contentType: contentTypeName
            }
          });
      };


      svc.getMetadata2 = function(typeId, keyType, key, contentTypeName) {
        return $http.get('eav/metadata/getassignedentities',
          {
            params: {
              appId: appId,
              assignmentObjectTypeId: typeId,
              keyType: keyType, //"guid",
              key: key,
              contentType: contentTypeName
            }
          });
      };
      return svc;
    });