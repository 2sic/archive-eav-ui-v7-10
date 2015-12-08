
(function() {
	"use strict";

    angular.module("eavFieldTemplates")
        .config(function(formlyConfigProvider) {
            formlyConfigProvider.setWrapper({
                name: 'disablevisually',
                templateUrl: "wrappers/disablevisually.html"
            });
        });
})();