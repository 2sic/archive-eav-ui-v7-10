/* global angular */
(function () {
	'use strict';

	var app = angular.module('eavEditEntity');

	// The controller for the main form directive
	app.controller('EditEntityWrapperCtrl', function editEntityCtrl($http, $scope, contentTypeName, entityId) {

		console.log(entityId);

		var vm = this;
		vm.contentTypeName = contentTypeName;
		vm.entityId = entityId;
		
		vm.registeredControls = [];
		vm.registerEditControl = function (control) {
			vm.registeredControls.push(control);
		};

		vm.isValid = function () {
			var valid = true;
			angular.forEach(vm.registeredControls, function (e, i) {
				if (!e.isValid())
					valid = false;
			});
			return valid;
		};

		vm.save = function () {
			var savePromises = [];
			angular.forEach(vm.registeredControls, function (e, i) {
				savePromises.push(e.save());
			});
			$q.all(savePromises).then(function () {
				alert("All save promises resolved!");
			});
		};
	});

})();