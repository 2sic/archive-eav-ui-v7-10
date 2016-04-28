
(function () {
    /* jshint laxbreak:true */
	"use strict";

	var app = angular.module("eavEditEntity"); 

	// The controller for the main form directive
	app.controller("EditEntityFormCtrl", function editEntityCtrl(appId, $http, $scope, formlyConfig, contentTypeFieldSvc, $sce, debugState, customInputTypes, eavConfig) {

		var vm = this;
		vm.editInDefaultLanguageFirst = function () {
			return false; // ToDo: Use correct language information, e.g. eavLanguageService.currentLanguage != eavLanguageService.defaultLanguage && !$scope.entityId;
		};

		// The control object is available outside the directive
		// Place functions here that should be available from the parent of the directive
		vm.control = {
		    isValid: function () { return vm.formFields.length === 0 || vm.form && vm.form.$valid; },
		    isDirty: function () { return (vm.form && vm.form.$dirty); },
		    setPristine: function () { if (vm.form) vm.form.$setPristine(); },
            error: function () { return vm.form.$error; }
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
		            angular.forEach(result.data, function (e, i) {
		                // check in config input-type replacement map if the specified type should be replaced by another
		                //if (e.InputType && eavConfig.formly.inputTypeReplacementMap[e.InputType]) 
		                //    e.InputType = eavConfig.formly.inputTypeReplacementMap[e.InputType];


		                // review type and get additional configs!
		                e.InputType = vm.getType(e);
		                eavConfig.formly.inputTypeReconfig(e);  // provide custom overrides etc. if necessary

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

	    vm.initCustomJavaScript = function(field) {
	        var jsobject,
                cjs = field.Metadata.merged.CustomJavaScript;
	        if (!cjs) return;
	        if (cjs.indexOf("/* compatibility: 1.0 */") < 0) {
	            console.log("found custom js for field '" + field.StaticName + "', but didn't find correct version support; ignore");
	            return;
	        }

	        try {
	            var fn = new Function(cjs); // jshint ignore:line
	            jsobject = fn();
	        }
            catch (ex) {
                console.log("wasn't able to process the custom javascript for field '" + field.StaticName + "'. tried: " + cjs);
            }
	        if (jsobject === undefined || jsobject === null)
	            return;

	        var context = {
	            field: field,
	            formVm: vm,
	            formlyConfig: formlyConfig,
	            appId: appId,
	            module: app, // pass in this current module in case something complex is wanted
	        };

	        // now cjs should be the initiliazed object...
	        if (jsobject && jsobject.init)
	            jsobject.init(context);
	        

	    };

	    vm.registerAllFieldsFromReturnedDefinition = function raffrd(result) {
	        var lastGroupHeadingId = 0;
	        angular.forEach(result.data, function (e, i) {

	            if (e.Metadata.All === undefined)
	                e.Metadata.All = {};

	            vm.initCustomJavaScript(e);

	            var fieldType = e.InputType;

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
		vm.getType = function (attributeConfiguration) {
		    var e = attributeConfiguration;
		    var type = e.Type.toLowerCase();
		    var inputType = "";

		    // new: the All can - and should - have an input-type which doesn't change
		    // First look in Metadata.All if an InputType is defined (All should override the setting, which is not the case when using only merged)
		    if (e.InputType !== "unknown") // the input type of @All is here from the web service // Metadata.All && e.Metadata.All.InputType)
		        inputType = e.InputType;
		        // If not, look in merged
		    else if (e.Metadata.merged && e.Metadata.merged.InputType)
		        inputType = e.Metadata.merged.InputType;

		    if (inputType && inputType.indexOf("-") === -1) // has input-type, but missing main type, this happens with old types like string wysiyg
		        inputType = type + "-" + inputType;

		    var willBeRewrittenByConfig = (inputType && eavConfig.formly.inputTypeReplacementMap[inputType]);
		    if (!willBeRewrittenByConfig) {
		        // this type may have assets, so the definition may be late-loaded
		        var typeAlreadyRegistered = formlyConfig.getType(inputType);    // check if this input-type actually exists - so "string-i-made-this-up" will return undefined
		        var typeWillRegisterLaterWithAssets = (e.InputTypeConfig ? !!e.InputTypeConfig.Assets : false); // if it will load assets later, then it may still be defined then

		        // Use subtype 'default' if none is specified - or type does not exist
		        if (!inputType || (!typeAlreadyRegistered && !typeWillRegisterLaterWithAssets))
		            inputType = type + "-default";

		        // but re-check if it's in the config! since the name might have changed
		        willBeRewrittenByConfig = (inputType && eavConfig.formly.inputTypeReplacementMap[inputType]);
		    }

		    // check in config input-type replacement map if the specified type should be replaced by another
		    // like "string-wysiwyg" replaced by "string-wysiwyg-tinymce"
		    if (willBeRewrittenByConfig)
		        inputType = eavConfig.formly.inputTypeReplacementMap[inputType];

		    return (inputType);
		};
	});
    
	

})();