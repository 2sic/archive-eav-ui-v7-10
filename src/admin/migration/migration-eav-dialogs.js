// This is a temporary bridge component
// it lets us open the new angular UIs which are in a separate solution
//
// The goal is to one day move all dialogs into that system
// but until that is done, we'll have a hybrid situation
(function() {
  var shortMap = [
    ['z', 'zoneId'],
    ['a', 'appId'],
    ['p', 'tid'],
    ['c', 'cbid'],
    ['d', 'debug'],
    ['i', 'mid'],
    ['l', 'lang'],
    ['lc', 'langs'],
    ['lp', 'langpri'],
    ['pop', 'partOfPage'],
    ['rtt', 'portalroot'],
    ['rtw', 'websiteroot']
  ];

  function lengthenParams(url) {
    for (var i = 0; i < shortMap.length; i++) {
      url = url.replace('&' + shortMap[i][0] + '=', '&' + shortMap[i][1] + '=');
    }
    return url;
  }

  angular
    .module('Migration')
    .factory('eavNgDialogs', function(
      zoneId,
      appId,
      debugState,
      enableAppFeatures,
      getUrlParamMustRefactor,
      $window
    ) {
      var svc = {};

      svc.ngRoot = '../ng-admin/ui.html';
      svc.ngEditRoot = '../ng-edit/ui.html';

      svc.openEdit = function(params, callback) {
        var path =
          svc.ngEditRoot +
          svc.paramsToBreakCache() +
          '#' +
          svc.assembleInitValues(false) +
          '&' +
          params;

        if (window.event && window.event.shiftKey) {
          if (callback) callback();
          return $window.open(path);
        } else {
          return window.$2sxc.totalPopup.open(path, callback);
        }
      };

      svc.openAdmin = function(route, callback) {
        var path =
          svc.ngRoot +
          svc.paramsToBreakCache() +
          '#' +
          route +
          svc.assembleInitValues();
        return window.$2sxc.totalPopup.open(path, callback);
      };

      svc.paramsToBreakCache = function() {
        return '?sxcver=' + getUrlParamMustRefactor('sxcver');
      };

      svc.assembleInitValues = function(short) {
        short = short || false;
        var url = getUrlParamMustRefactor;
        var result =
          '&z=' +
          zoneId +
          '&a=' +
          appId +
          //+ '&t=0'
          '&p=' +
          url('tid') +
          '&c=' +
          url('cbid') +
          '&d=' +
          debugState.on +
          '&i=' +
          url('mid') +
          '&l=' +
          url('lang') +
          '&lc=' +
          url('langs') +
          '&lp=' +
          url('langpri') +
          '&fs=' +
          'false' +
          '&pop=' +
          url('partOfPage') +
          '&rtt=' +
          url('portalroot') +
          '&rta=' +
          url('approot') +
          '&rtw=' +
          url('websiteroot') +
          '&systype=' +
          'dnn' +
          '&sxcver=' +
          url('sxcver');

        console.log('result before adding', result);
        var addon = short
          ? '&fa=' +
            enableAppFeatures +
            '&lui=' +
            url('langs') +
            '&fd=' +
            url('user%5BcanDesign%5D') +
            '&fc=' +
            url('user%5BcanDevelop%5D')
          : '&user=' + url('user');
        result = result + addon;

        if (!short) result = lengthenParams(result);
        return result;
      };

      return svc;
    });
})();
