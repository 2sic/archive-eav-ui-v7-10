
(function () {
    /* jshint laxbreak:true */
	"use strict";

	var app = angular.module("eavEditEntity"); 

	// The controller for the main form directive
	app.controller("EditEntityFormCtrl", function editEntityCtrl(appId, $http, $scope, formlyConfig, contentTypeFieldSvc, $sce, debugState, customInputTypes) {

		var vm = this;
		vm.editInDefaultLanguageFirst = function () {
			return false; // ToDo: Use correct language information, e.g. eavLanguageService.currentLanguage != eavLanguageService.defaultLanguage && !$scope.entityId;
		};

		// The control object is available outside the directive
		// Place functions here that should be available from the parent of the directive
		vm.control = {
		    isValid: function () { return vm.formFields.length === 0 || vm.form && vm.form.$valid; },
		    isDirty: function () { return (vm.form && vm.form.$dirty); },
		    setPristine: function () { if(vm.form) vm.form.$setPristine(); }
		};

		// Register this control in the parent control
		if($scope.registerEditControl)
			$scope.registerEditControl(vm.control);

		vm.model = null;
		vm.entity = $scope.entity;

		vm.formFields = [];


		var loadContentType = function () {

		    contentTypeFieldSvc(appId, { StaticName: vm.entity.Type.StaticName }).getFields()
		        .then(function(result) {
		            vm.debug = result;

		            // Transform EAV content type configuration to formFields (formly configuration)

                    // first: add all custom types to re-load these scripts and styles
		            angular.forEach(result.data, function(e, i) {
		                if (e.InputTypeConfig)
		                    customInputTypes.addInputType(e);
		            });

		            // load all assets before continuing with formly-binding
		            var promiseToLoad = customInputTypes.loadWithPromise();
		            promiseToLoad.then(function(dependencyResult) {
		                vm.registerAllFieldsFromReturnedDefinition(result);
		            });


		        });
		};

	    vm.registerAllFieldsFromReturnedDefinition = function raffrd(result) {
	        var lastGroupHeadingId = 0;
	        angular.forEach(result.data, function (e, i) {

	            if (e.Metadata.All === undefined)
	                e.Metadata.All = {};

	            var fieldType = vm.getType(e);

	            // always remember the last heading so all the following fields know to look there for collapse-setting
	            var isFieldHeading = (fieldType === "empty-default");
	            if (isFieldHeading)
	                lastGroupHeadingId = i;

	            var nextField = {
	                key: e.StaticName,
	                type: fieldType,
	                templateOptions: {
	                    required: !!e.Metadata.All.Required,
	                    label: e.Metadata.All.Name === undefined ? e.StaticName : e.Metadata.All.Name,
	                    description: $sce.trustAsHtml(e.Metadata.All.Notes),
	                    settings: e.Metadata,
	                    header: $scope.header,
	                    canCollapse: lastGroupHeadingId > 0 && !isFieldHeading,
	                    fieldGroup: vm.formFields[lastGroupHeadingId],
	                    disabled: e.Metadata.All.Disabled,
	                    langReadOnly: false // Will be set by the language directive to override the disabled state
	                },
	                className: "type-" + e.Type.toLowerCase() + " input-" + fieldType + " field-" + e.StaticName.toLowerCase(),
	                hide: (e.Metadata.All.VisibleInEditUI === false ? !debugState.on : false),
	                expressionProperties: {
	                    // Needed for dynamic update of the disabled property
	                    'templateOptions.disabled': 'options.templateOptions.disabled' // doesn't set anything, just here to ensure formly causes update-binding
	                },
	                watcher: [
                        {
                            // changes when a entity becomes enabled / disabled
                            expression: function (field, scope, stop) {
                                return e.Metadata.All.Disabled ||
                                    (field.templateOptions.header.Group && field.templateOptions.header.Group.SlotIsEmpty) ||
                                    field.templateOptions.langReadOnly;
                            },
                            listener: function (field, newValue, oldValue, scope, stopWatching) {
                                field.templateOptions.disabled = newValue;
                            }
                        },
                        {
                            // handle collapse / open
                            expression: function (field, scope, stop) {
                                // only change values if it can collapse...
                                return (field.templateOptions.canCollapse) ? field.templateOptions.fieldGroup.templateOptions.collapseGroup : null;
                            },
                            listener: function (field, newValue, oldValue, scope, stopWatching) {
                                if (field.templateOptions.canCollapse)
                                    field.templateOptions.collapse = newValue;
                            }
                        }
	                ]
	            };

	            vm.formFields.push(nextField);
	        });
	    };


	    // Load existing entity if defined
		if (vm.entity !== null)
		    loadContentType();


		// Returns the field type for an attribute configuration
		vm.getType = function(attributeConfiguration) {
			var e = attributeConfiguration;
			var type = e.Type.toLowerCase();
			var inputType = "";

		    // new: the All can - and should - have an input-type which doesn't change
			if (e.Metadata.merged && e.Metadata.merged.InputType) {
			    inputType = e.Metadata.merged.InputType;
			}

			if (inputType && inputType.indexOf("-") === -1) // has input-type, but missing main type, this happens with old types like string wysiyg
		        inputType = type + "-" + inputType;

		    // this type may have assets, so the definition may be late-loaded
		    var typeAlreadyRegistered = formlyConfig.getType(inputType);
		    var typeWillLoadAssetsInAMoment = (e.InputTypeConfig ? !!e.InputTypeConfig.Assets : false);

			// Use subtype 'default' if none is specified - or type does not exist
		    if (!inputType || (!typeAlreadyRegistered && !typeWillLoadAssetsInAMoment))
		        inputType = type + "-default";

			return (inputType);
		};
	});
    
	

})();