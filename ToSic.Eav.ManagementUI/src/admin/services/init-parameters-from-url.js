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
            return getParameterByName("appId");
        })
        .factory("zoneId", function () {
            return getParameterByName("zoneId");
        })
        .factory("entityId", function () {
            return getParameterByName("entityid");
        })
        .factory("contentTypeName", function() {
            return getParameterByName("contenttypename");
        })

        .factory("pipelineId", function () {
            return getParameterByName("pipelineId");
        })
        .factory("dialog", function () {
            return getParameterByName("dialog");
        })
        // This is a dummy object, because it's needed for dialogs
        .factory("$modalInstance", function() {
            return null;
        });

    function getParameterByName(name) {
        name = name.replace(/[\[]/, "\\[").replace(/[\]]/, "\\]");
        var searchRx = new RegExp("[\\?&]" + name + "=([^&#]*)", "i");
        var results = searchRx.exec(location.search);

        if (results === null) {
            var hashRx = new RegExp("[#&]" + name + "=([^&#]*)", "i");
            results = hashRx.exec(location.hash);
        }

        // if nothing found, try normal URL because DNN places parameters in /key/value notation
        if (results === null) {
            // Otherwise try parts of the URL
            var matches = window.location.pathname.match(new RegExp("/" + name + "/([^/]+)", "i"));

            // Check if we found anything, if we do find it, we must reverse the results so we get the "last" one in case there are multiple hits
            if (matches !== null && matches.length > 1)
                results = matches.reverse()[0];
        } else
            results = results[1];

        return results === null ? "" : decodeURIComponent(results.replace(/\+/g, " "));
    }
}());