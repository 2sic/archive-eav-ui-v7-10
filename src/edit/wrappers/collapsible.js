/*
 * This wrapper should be around all fields, so that they can collapse 
 * when a field-group-title requires collapsing
 */
(function () {
    "use strict";

    angular.module("eavFieldTemplates")
        .config(function (formlyConfigProvider) {
            formlyConfigProvider.setWrapper({
                name: 'collapsible',
                templateUrl: "wrappers/collapsible.html"
            });
        });
})();