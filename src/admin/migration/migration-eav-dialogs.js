// This is a temporary bridge component
// it lets us open the new angular UIs which are in a separate solution
//
// The goal is to one day move all dialogs into that system
// but until that is done, we'll have a hybrid situation
(function () {

  angular.module('Migration')
    .factory('eavNgDialogs', function (
      zoneId,
      appId,
      debugState,
      enableAppFeatures,
      getUrlParamMustRefactor
    ) {

      var svc = {};

      svc.ngRoot = '../ng-admin/ui.html';

      svc.assembleInitValues = function () {
        var url = getUrlParamMustRefactor;
        return '?z=' + zoneId
          + '&a=' + appId
          //+ '&t=0'
          + '&p=' + url('tid')
          + '&c=' + url('cbid')
          + '&d=' + debugState.on
          + '&i=' + url('mid')
          + '&lc=' + url('langs')
          + '&lui=' + url('langs')
          + '&lp=' + url('langpri')
          + '&fa=' + enableAppFeatures
          + '&fd=' + url('user%5BcanDesign%5D')
          + '&fc=' + url('user%5BcanDevelop%5D')
          + '&fs=' + 'false'
          + '&pop=' + url('partOfPage')
          + '&rtt=' + url('portalroot')
          + '&rta=' + url('approot')
          + '&rtw=' + url('websiteroot')
          + '&systype=' + 'dnn'
          + '&sxcver=' + url('sxcver')
          //+ '&sysver=' 
          ;
      };

      svc.open = function (route, callback) {
        alert("this feature is still beta and doesn't work yet");
        var path = svc.ngRoot + '#' + route + svc.assembleInitValues();
        return window.$2sxc.totalPopup.open(path, callback);
      };

      return svc;
    });
}());