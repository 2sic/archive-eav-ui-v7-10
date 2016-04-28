
/*
 * This is the label-wrapper of a group-title, 
 * and in the html allows show/hide of the entire group
 * show-hide works over the options property to.collapseGroup
 */
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