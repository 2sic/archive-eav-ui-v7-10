// metadata
// retrieves metadata for an entity or an attribute

angular.module('EavServices')
  // Management actions which are rather advanced metadata kind of actions
  .factory('metadataSvc',
    function($http, appId) {
      var svc = {};

      // Find all items assigned to a GUID
      //svc.getMetadataOld = function getMetadata(typeId, keyGuid, contentTypeName) {
      //  console.log('using deprecated getMetadata - try to migrate code to get2');
      //  return $http.get('eav/metadata/get',
      //    {
      //      params: {
      //        appId: appId,
      //        targetType: typeId,
      //        keyType: 'guid',
      //        key: keyGuid,
      //        contentType: contentTypeName
      //      }
      //    });
      //};


      svc.getMetadata = function(typeId, keyType, key, contentTypeName) {
        return $http.get('eav/metadata/get',
          {
            params: {
              appId: appId,
              targetType: typeId,
              keyType: keyType,
              key: key,
              contentType: contentTypeName
            }
          });
      };
      return svc;
    });