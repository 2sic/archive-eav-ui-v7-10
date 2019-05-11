angular
  .module("EavServices")
  // features Services
  // checks if a feature is enabled
  .factory("featuresSvc", function($http, appId, $q) {
    var svc = {
      list: [],
      ids: {
        pasteImage: "f6b8d6da-4744-453b-9543-0de499aa2352",
        pasteWysiwyg: "1b13e0e6-a346-4454-a1e6-2fb18c047d20"
      }
    };

    svc.getFeatures = function() {
      return $http.get("eav/system/features", {
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
        if (svc.list[i].id === guid) return svc.list[i].enabled;
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
