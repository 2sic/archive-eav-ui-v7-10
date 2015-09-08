// By default, eav-controls assume that all their parameters (appId, etc.) are instantiated by the bootstrapper
// but the "root" component must get it from the url
// Since different objects could be the root object (this depends on the initial loader), the root-one must have
// a connection to the Url, but only when it is the root
// So the trick is to just include this file - which will provide values for the important attribute
//
// As of now, it only supplies
// * appId
(function () {
    angular.module("InitParametersFromUrl", [])
        .factory('appId', function ($location) {
        	// this ties up the App-Id to the Url 
        	return $location.search().appid;
		})
		.factory('entityId', function($location) {
			// ToDo: $location.search() returns null
			return getQueryStringParam('entityid');
		})
		.factory('contentTypeName', function ($location) {
		    // ToDo: $location.search() returns null
			return getQueryStringParam('contenttypename');
	    })
	;

	/* Temp - until $location.search() does work again */
	var getQueryStringParam = function(name) {
		var url = location.href;
		name = name.replace(/[\[]/, "\\\[").replace(/[\]]/, "\\\]");
		var regexS = "[\\?&]" + name + "=([^&#]*)";
		var regex = new RegExp(regexS);
		var results = regex.exec(url);
		return results === null ? null : results[1];
	};
}());