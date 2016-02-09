
(function() {
	"use strict";

    angular.module("eavFieldTemplates")
        .config(function(formlyConfigProvider) {
            formlyConfigProvider.setWrapper({
                name: 'fieldGroup',
                templateUrl: "wrappers/field-group.html"
            });
        });
})();