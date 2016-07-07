/*
 * This wrapper should be around all fields, so that they can float the label 
 */
(function () {
    "use strict";
    angular.module("eavFieldTemplates")
        .config(function (formlyConfigProvider) {
            formlyConfigProvider.setWrapper({
                name: 'no-label-space',
                templateUrl: "wrappers/no-label-space.html"
            });
        });
})();