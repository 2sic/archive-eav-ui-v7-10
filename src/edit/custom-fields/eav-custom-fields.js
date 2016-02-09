/* 
 * Custom Fields skelleton to allow later, lazy loaded fields
 */
(function() {
    "use strict";
    var module = angular.module("eavCustomFields", ["oc.lazyLoad"])

    .config(function ($ocLazyLoadProvider) {
        $ocLazyLoadProvider.config({
            debug: true
        });
    });

})();