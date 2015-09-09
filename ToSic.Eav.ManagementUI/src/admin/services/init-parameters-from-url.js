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
        .factory("appId", function() {
            return getQueryStringParam("appId");
        })
        .factory("entityId", function() {
            return getQueryStringParam("entityid");
        })
        .factory("contentTypeName", function() {
            return getQueryStringParam("contenttypename");
        })

        .factory("pipelineId", function () {
            return getQueryStringParam("pipelineId");
        })
        // This is a dummy object, because it's needed for dialogs
        .factory("$modalInstance", function() {
            return null;
        });

	/* Temp - until $location.search() does work again */
	var getQueryStringParam = function(name) {
		var url = location.href;
		name = name.replace(/[\[]/, "\\\[").replace(/[\]]/, "\\\]");
		var regexS = "[\\?&]" + name + "=([^&#]*)";
		var regex = new RegExp(regexS, "i");
		var results = regex.exec(url);
		return results === null ? null : results[1];
	};
}());