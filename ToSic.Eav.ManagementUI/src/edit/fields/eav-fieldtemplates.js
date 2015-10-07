
(function() {
	"use strict";

	/* This app registers all field templates for EAV in the angularjs eavFieldTemplates app */

	var eavFieldTemplates = angular.module("eavFieldTemplates", ["formly", "formlyBootstrap", "ui.bootstrap", "eavLocalization", "eavEditTemplates", "ui.tree"])
        .config(function (formlyConfigProvider) {


	    formlyConfigProvider.setWrapper({
	        name: 'eavLabel',
            templateUrl: "fields/eav-label.html"
	    });
	});

	eavFieldTemplates.controller("FieldTemplate-NumberCtrl", function () {
		var vm = this;
		// ToDo: Implement Google Map
	});



})();