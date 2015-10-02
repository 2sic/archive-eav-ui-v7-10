/* global angular */
(function () {
	"use strict";

	var app = angular.module("eavEditEntity");

	// The controller for the main form directive
	app.controller("EditEntityWrapperCtrl", function editEntityCtrl($q, $http, $scope, contentTypeName, entityId, $modalInstance) {

		var vm = this;
		vm.itemList =
                [{
		        ContentTypeName: contentTypeName,
		        EntityId: entityId
                    // todo: metadata infos too...
		    }]
	    ;

        // this is the callback after saving - needed to close everything
		vm.afterSave = function (result) {
		    if (result.status === 200)
		        vm.close();
		    else {
		        alert("Something went wrong - maybe parts worked, maybe not. Sorry :("); 
		    }

		};
		
		vm.close = function () {
		    $modalInstance.dismiss("cancel");
		};
	});

})();