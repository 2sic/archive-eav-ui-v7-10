
(function () {
	"use strict";

	var app = angular.module("eavEditEntity"); 

	// The controller for the main form directive
    app.controller("EditEntityFormCtrl", function editEntityCtrl(appId, $http, $scope, formlyConfig, contentTypeFieldSvc, $sce) {

		var vm = this;
		vm.editInDefaultLanguageFirst = function () {
			return false; // ToDo: Use correct language information, e.g. eavLanguageService.currentLanguage != eavLanguageService.defaultLanguage && !$scope.entityId;
		};

		// The control object is available outside the directive
		// Place functions here that should be available from the parent of the directive
		vm.control = {
			isValid: function() { return vm.form.$valid; }
		};

		// Register this control in the parent control
		if($scope.registerEditControl)
			$scope.registerEditControl(vm.control);

		vm.model = null;
		vm.entity = $scope.entity;

		vm.formFields = null;


		var loadContentType = function () {

		    contentTypeFieldSvc(appId, { StaticName: vm.entity.Type.StaticName }).getFields()
			.then(function (result) {
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
			                description: $sce.trustAsHtml(e.Metadata.All.Notes),
			                settings: e.Metadata,
                            header: $scope.header
			            },
			            hide: (e.Metadata.All.VisibleInEditUI ? !e.Metadata.All.VisibleInEditUI : false),
			            expressionProperties: {
			                'templateOptions.disabled': "options.templateOptions.disabled" // Needed for dynamic update of the disabled property
			            }
			        });
			    });
			});
		};

	    // Load existing entity if defined
		if (vm.entity !== null)
		    loadContentType();


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