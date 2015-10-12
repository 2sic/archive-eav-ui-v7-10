/* global angular */
(function () {
	"use strict";

	var app = angular.module("eavEditEntity");

	// The controller for the main form directive
	app.controller("EditEntityWrapperCtrl", function editEntityCtrl($q, $http, $scope, items, $modalInstance, $window) {

	    var vm = this;
	    vm.itemList = items;

	    // this is the callback after saving - needed to close everything
	    vm.afterSave = function(result) {
	        if (result.status === 200)
	            vm.close(result);
	        else {
	            alert("Something went wrong - maybe parts worked, maybe not. Sorry :(");
	        }
	    };

	    vm.state = {
	        isDirty: function() {
	            throw "Inner control must override this function.";
	        }
	    };

	    vm.close = function (result) {
		    $modalInstance.close(result);
		};

	    var unsavedChangesText = "You have unsaved changes.";

	    vm.maybeLeave = function maybeLeave(e) {
	        if (vm.state.isDirty() && !confirm(unsavedChangesText + " Do you really want to exit?"))
	            e.preventDefault();
	    };

	    $scope.$on('modal.closing', vm.maybeLeave);
	    $window.addEventListener('beforeunload', function (e) {
	        if (vm.state.isDirty()) {
	            (e || window.event).returnValue = unsavedChangesText; //Gecko + IE
	            return unsavedChangesText; //Gecko + Webkit, Safari, Chrome etc.
	        }
	        return null;
	    });
	});

})();