// metadata
// retrieves metadata for an entity or an attribute

angular.module('EavServices')
  // Management actions which are rather advanced metadata kind of actions
  .factory('featuresSvc',
  function ($http, appId, $q) {
    var svc = {
      list: []
    };

      svc.getFeatures = function() {
        return $http.get('eav/system/features',
          {
            params: {
              appId: appId
            }
          });
      };

    svc.promise = svc.getFeatures();
    svc.promise.then(function(data) {
      svc.list = data.data;
    });

    svc.enabledNow = function(guid) {
      for (var i = 0; i < svc.list.length; i++)
        if (svc.list[i].id === guid)
          return svc.list[i].enabled;
      return false;
    };

    svc.enabled = function(guid) {
      return $q(function(resolve) {
        svc.promise.then(function() {
          resolve(svc.enabledNow(guid));
        });
      });
    };
      

      return svc;
    });