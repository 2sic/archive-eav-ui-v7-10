
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