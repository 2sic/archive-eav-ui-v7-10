
angular.module('EavServices')
  .factory('permissionsSvc',
    function(/*$http, eavConfig,*/ entitiesSvc, metadataSvc, svcCreator, contentTypeSvc) {

      // Construct a service for this specific targetGuid
      return function createSvc(appId, targetType, keyType, targetId) {
        var svc = {
          key: targetId,
          targetId: targetId,
          ctName: 'PermissionConfiguration',
          ctId: 0,
          targetType: targetType,
          keyType: keyType,
          ctSvc: contentTypeSvc(appId)
        };

        svc = angular.extend(svc,
          svcCreator.implementLiveList(function getAll() {
            return metadataSvc.getMetadata(svc.targetType, svc.keyType, svc.key, svc.ctName)
              .then(svc.updateLiveAll);
          }));

        // Get ID of this content-type 
        svc.ctSvc.getDetails(svc.ctName).then(function(result) {
          svc.ctId = result.data.Id;
        });

        // delete, then reload
        svc.delete = function del(id) {
          return entitiesSvc.delete(svc.ctName, id)
            .then(svc.liveListReload);
        };
        return svc;
      };
    });