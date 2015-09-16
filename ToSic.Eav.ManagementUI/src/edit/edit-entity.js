/* global angular */
(function () {
	"use strict";

	var app = angular.module("eavEditEntity", [
        "formly",
        "eavFieldTemplates",
        "eavNgSvcs",
        "EavServices",
        "eavEditTemplates"]);

	// Main directive that renders an entity edit form
	app.directive("eavEditEntity", function() {
		return {
			templateUrl: "edit-entity.html",
			restrict: "E",
			scope: {
				contentTypeName: "@contentTypeName",
				entityId: "@entityId",
				registerEditControl: "=registerEditControl"
			},
			controller: "EditEntityCtrl",
			controllerAs: "vm"
		};
	});

	// The controller for the main form directive
	app.controller("EditEntityCtrl", function editEntityCtrl($http, $scope, formlyConfig, contentTypeFieldSvc, entitiesSvc) {

		var vm = this;
		vm.editInDefaultLanguageFirst = function () {
			return false; //eavLanguageService.currentLanguage != eavLanguageService.defaultLanguage && !$scope.entityId;
		};

		vm.save = function() {
			alert("Saving not implemented yet!");
		};

		// The control object is available outside the directive
		// Place functions here that should be available from the parent of the directive
		vm.control = {
			isValid: function() { return vm.form.$valid; },
			save: vm.save
		};

		// Register this control in the parent control
		if($scope.registerEditControl)
			$scope.registerEditControl(vm.control);

		vm.model = null;
		vm.entity = null;

		vm.formFields = null;

		contentTypeFieldSvc(1, { StaticName: $scope.contentTypeName }).getFields() // ToDo: use correct AppId
			.then(function(result) {
				vm.debug = result;

				// Transform EAV content type configuration to formFields (formly configuration)
				angular.forEach(result.data, function (e, i) {

					if (e.Metadata.All === undefined)
						e.Metadata.All = {};

					vm.formFields.push({
						key: e.StaticName,
						type: getType(e),
						templateOptions: {
							required: !!e.Metadata.All.Required,
							label: e.Metadata.All.Name === undefined ? e.StaticName : e.Metadata.All.Name,
							description: e.Metadata.All.Notes,
							settings: e.Metadata
						},
						hide: (e.Metadata.All.VisibleInEditUI ? !e.Metadata.All.VisibleInEditUI : false),
						//defaultValue: parseDefaultValue(e)
						expressionProperties: {
							'templateOptions.disabled': "options.templateOptions.disabled" // Needed for dynamic update of the disabled property
						}
					});
				});
			});

		// Load existing entity if defined
		if ($scope.entityId) {
			entitiesSvc.getMultiLanguage(1, $scope.contentTypeName, $scope.entityId) // ToDo: Use correct App-Id
				.then(function (result) {
					vm.entity = result.data;
				});
		} else {
			vm.entity = entitiesSvc.createEntity();
		}

		// Returns the field type for an attribute configuration
		var getType = function(attributeConfiguration) {
			var e = attributeConfiguration;
			var type = e.Type.toLowerCase();
			var subType = e.Metadata.String !== undefined ? e.Metadata.String.InputType : null;

			subType = subType ? subType.toLowerCase() : null;

			// Special case: override subtype for string-textarea
			if (type === "string" && e.Metadata.String !== undefined && e.Metadata.String.RowCount > 1)
				subType = "textarea";

			// Use subtype 'default' if none is specified - or type does not exist
			if (!subType || !formlyConfig.getType(type + "-" + subType))
				subType = "default";

			return (type + "-" + subType);
		};
	});

	

})();