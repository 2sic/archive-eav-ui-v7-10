
(function() {
	"use strict";

    angular.module("eavFieldTemplates")
        .config(function(formlyConfigProvider) {
            formlyConfigProvider.setWrapper({
                name: 'eavLabel',
                templateUrl: "wrappers/eav-label.html"
            });
        });
})();