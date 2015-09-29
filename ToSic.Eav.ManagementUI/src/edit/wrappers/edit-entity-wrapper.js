/* global angular */
(function () {
	'use strict';

	var app = angular.module('eavEditEntity');

	// The controller for the main form directive
	app.controller('EditEntityWrapperCtrl', function editEntityCtrl($q, $http, $scope, contentTypeName, entityId, $modalInstance) {

		var vm = this;
		vm.editPackageRequest = {
            type: 'entities',
            entities: [{
		        contentTypeName: contentTypeName,
		        entityId: entityId
		    }]
		};
		
		vm.close = function () {
		    $modalInstance.dismiss("cancel");
		};
	});

})();