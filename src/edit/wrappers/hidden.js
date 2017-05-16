
(function() {
	"use strict";

    angular.module("eavFieldTemplates")
        .config(function(formlyConfigProvider) {
            formlyConfigProvider.setWrapper({
                name: 'hiddenIfNeeded',
                templateUrl: "wrappers/hidden.html"
            });
        });
})();