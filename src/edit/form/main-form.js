/* global angular */
(function () {
	"use strict";

	var app = angular.module("eavEditEntity");

	// The controller for the main form directive
	app.controller("EditEntityWrapperCtrl", function editEntityCtrl($q, $http, $scope, items, $modalInstance, $window, $translate, toastr) {

	    var vm = this;
	    vm.itemList = items;

	    // this is the callback after saving - needed to close everything
	    vm.afterSave = function(result) {
	        if (result.status === 200)
	            vm.close(result);
	        else {
	            alert($translate.instant("Errors.UnclearError"));
	        }
	    };

	    vm.state = {
	        isDirty: function() {
	            throw $translate.instant("Errors.InnerControlMustOverride");
	        }
	    };

	    vm.close = function (result) {
		    $modalInstance.close(result);
		};


	    //vm.maybeLeave = function maybeLeave(e) {
	    //    if (vm.state.isDirty() && !confirm(unsavedChangesText + " " + $translate.instant("Message.ExitOk")))
	    //        e.preventDefault();
	    //};

	    //vm.showMaybeLeaveToaster = function(e) {
	    //    var quickQuit = "<div>"
        //        + $translate.instant("Errors.UnsavedChanges")
	    //        + "<button type=\"button\" id=\"save\" class=\"btn btn-primary\" ><i class=\"icon-ok\"></i>save</button> &nbsp;"
	    //        + "<button type=\"button\" id=\"quit\" class=\"btn btn-default\" ><i class= \"icon-cancel\"></i>don't save</button>"
	    //        + "</div>";
	    //    toastr.warning(quickQuit, "leaving i18n?", {
	    //        allowHtml: true,
	    //        closeButton: true,
	    //        tapToDismiss: false,
	    //        autoDismiss: false,
	    //        progressBar: true,
	    //        timeOut: 50000,
	    //        extendedTimeOut: 50000,
	    //        onTap: vm.handleLeaveToasterTap,
	    //        onShown: function (toast) {
	    //            var div = toast.el[0].children[1].children[1].children[0];
	    //            var save = div.children[0];
	    //            var quit = div.children[1];
	    //            save.onclick = function () { alert("save!") };
	    //            quit.onclick = function () { alert("quit!!!!!") };
	    //        }
	    //    });

	    //};


	    //$scope.$on('modal.closing', vm.maybeLeave);

	    $window.addEventListener('beforeunload', function (e) {
	        var unsavedChangesText = $translate.instant("Errors.UnsavedChanges");
	        if (vm.state.isDirty()) {
	            (e || window.event).returnValue = unsavedChangesText; //Gecko + IE
	            return unsavedChangesText; //Gecko + Webkit, Safari, Chrome etc.
	        }
	        return null;
	    });
	});

})();