/*
 * This wrapper should be around all fields, so that they can float the label 
 */
(function () {
    "use strict";
    angular.module("eavFieldTemplates")
        .config(function (formlyConfigProvider) {
            formlyConfigProvider.setWrapper({
                name: 'preview-default',
                templateUrl: "wrappers/preview-default.html"
            });
        });
})();