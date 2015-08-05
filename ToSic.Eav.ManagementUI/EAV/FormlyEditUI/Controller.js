/* global angular */
(function () {
	'use strict';

	var app = angular.module('formlyExample', ['formly', 'formlyBootstrap'], function config(formlyConfigProvider) {

		formlyConfigProvider.setType({
			name: 'string-text',
			extends: 'input'
		});

		formlyConfigProvider.setType({
			name: 'string-dropdown',
			extends: 'select'
		});

		formlyConfigProvider.setType({
			name: 'string-wysiwyg',
			extends: 'input'
		});



	});


	app.controller('MainCtrl', function MainCtrl($http) {
		var vm = this;
		vm.onSubmit = function onSubmit() {
			alert(JSON.stringify(vm.model), null, 2);
		};

		vm.model = {};

		vm.formFields = null;

		$http.get('/api/eav/ContentType/GetContentTypeConfiguration?appId=1&zoneId=1&contentTypeName=All Field Types')
		.then(function (data) {
			vm.debug = data;
		});

	});


	app.directive('exampleDirective', function () {
		return {
			templateUrl: 'example-directive.html'
		};
	});
})();