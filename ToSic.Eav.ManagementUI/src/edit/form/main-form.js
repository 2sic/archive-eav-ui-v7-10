/* global angular */
(function () {
	"use strict";

	var app = angular.module("eavEditEntity");

	// The controller for the main form directive
	app.controller("EditEntityWrapperCtrl", function editEntityCtrl($q, $http, $scope, items, $modalInstance, $window, $filter) {

	    var vm = this;
	    vm.itemList = items;
	    var translate = $filter("translate");

	    // this is the callback after saving - needed to close everything
	    vm.afterSave = function(result) {
	        if (result.status === 200)
	            vm.close(result);
	        else {
	            alert(translate("Errors.UnclearError"));
	        }
	    };

	    vm.state = {
	        isDirty: function() {
	            throw translate("Errors.InnerControlMustOverride");
	        }
	    };

	    vm.close = function (result) {
		    $modalInstance.close(result);
		};

	    var unsavedChangesText = translate("Errors.UnsavedChanges");

	    vm.maybeLeave = function maybeLeave(e) {
	        if (vm.state.isDirty() && !confirm(unsavedChangesText + " " + translate("Message.ExitOk")))
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