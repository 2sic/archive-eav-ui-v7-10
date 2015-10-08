
(function() {
	"use strict";

	/* This app registers all field templates for EAV in the angularjs eavFieldTemplates app */

	var eavFieldTemplates = angular.module("eavFieldTemplates")
        .config(function (formlyConfigProvider) {


	    //formlyConfigProvider.setWrapper({
	    //    name: 'eavLabel',
        //    templateUrl: "wrappers/eav-label.html"
	    //});
	});

	eavFieldTemplates.controller("FieldTemplate-NumberCtrl", function () {
		var vm = this;
		// ToDo: Implement Google Map
	});



})();