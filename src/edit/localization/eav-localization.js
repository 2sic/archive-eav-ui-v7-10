
(function () {
	"use strict";


	/* This app handles all aspectes of the multilanguage features of the field templates */

	var eavLocalization = angular.module("eavLocalization", ["formly", "EavConfiguration"], function (formlyConfigProvider) {

		// Field templates that use this wrapper must bind to value.Value instead of model[...]
		formlyConfigProvider.setWrapper([
			{
				name: "eavLocalization",
				templateUrl: "localization/formly-localization-wrapper.html"
			}
		]);

	});

	eavLocalization.directive("eavLanguageSwitcher", function () {
		return {
			restrict: "E",
			templateUrl: "localization/language-switcher.html",
			controller: function($scope, languages) {
				$scope.languages = languages;
			},
			scope: {
			    isDisabled: "=isDisabled"
			}
		};
	});

	eavLocalization.directive("eavLocalizationScopeControl", function () {
		return {
			restrict: "E",
			transclude: true,
			template: "",
			link: function (scope, element, attrs) {
			},
			controller: function ($scope, $filter, $translate, eavDefaultValueService, languages) { // Can't use controllerAs because of transcluded scope

				var scope = $scope;
				var langConf = languages;

				var initCurrentValue = function() {

					// Set base value object if not defined
					if (!scope.model[scope.options.key])
						scope.model.addAttribute(scope.options.key);

					var fieldModel = scope.model[scope.options.key];

					// If current language = default language and there are no values, create an empty value object
					if (fieldModel.Values.length === 0) {
					    if (langConf.currentLanguage == langConf.defaultLanguage) {
					        var defaultValue = eavDefaultValueService(scope.options);
                            // Add default language if we are in a ml environment, else don't add any
					        var languageToAdd = langConf.languages.length > 0 ? langConf.currentLanguage : null;
					        fieldModel.addVs(defaultValue, languageToAdd);
					    }
					    else { // There are no values - value must be edited in default language first
					        return;
					    }
					}

				    // Assign default language if no dimension is set - new: if multiple languages are in use!!!
					if (Object.keys(fieldModel.Values[0].Dimensions).length === 0)
                        if(langConf.languages.length > 0)
				            fieldModel.Values[0].Dimensions[langConf.defaultLanguage] = false; // set to "not-read-only"

					var valueToEdit;

					// Decide which value to edit:
					// 1. If there is a value with current dimension on it, use it
					valueToEdit = $filter("filter")(fieldModel.Values, function(v, i) {
						return v.Dimensions[langConf.currentLanguage] !== undefined;
					})[0];

					// 2. Use default language value
					if (valueToEdit === undefined)
						valueToEdit = $filter("filter")(fieldModel.Values, function(v, i) {
							return v.Dimensions[langConf.defaultLanguage] !== undefined;
						})[0];

					// 3. Use the first value if there is only one
					if (valueToEdit === undefined) {
						if (fieldModel.Values.length > 1)
						    throw $translate.instant("Errors.DefLangNotFound") + " " + $scope.options.key;
						// Use the first value
						valueToEdit = fieldModel.Values[0];
					}

					fieldModel._currentValue = valueToEdit;

					// Set scope variable 'value' to simplify binding
					scope.value = fieldModel._currentValue;

				    // Decide whether the value is writable or not
					var writable = (langConf.currentLanguage == langConf.defaultLanguage) ||
                        (scope.value && scope.value.Dimensions[langConf.currentLanguage] === false);

					scope.to.langReadOnly = !writable;
				};

				initCurrentValue();

				// Handle language switch
				scope.langConf = langConf; // Watch does only work on scope variables
				scope.$watch("langConf.currentLanguage", function (newValue, oldValue) {
					if (oldValue === undefined || newValue == oldValue)
						return;
					initCurrentValue();
				});

				// ToDo: Could cause performance issues (deep watch array)...
				scope.$watch("model[options.key].Values", function(newValue, oldValue) {
					initCurrentValue();
				}, true);

				// The language menu must be able to trigger an update of the _currentValue property
				scope.model[scope.options.key]._initCurrentValue = initCurrentValue;
			}
		};
	});

	eavLocalization.directive("eavLocalizationMenu", function () {
		return {
			restrict: "E",
			scope: {
				fieldModel: "=fieldModel",
				options: "=options",
				value: "=value",
				index: "=index",
                formModel: "=formModel"
			},
			templateUrl: "localization/localization-menu.html",
			link: function (scope, element, attrs) { },
			controllerAs: "vm",
			controller: function ($scope, languages, $translate) {

			    var vm = this;
			    var lblDefault = $translate.instant("LangMenu.UseDefault");
			    var lblIn = $translate.instant("LangMenu.In");

				vm.fieldModel = $scope.fieldModel;
				vm.languages = languages;
				vm.hasLanguage = function(languageKey) {
				    return vm.fieldModel.getVsWithLanguage(languageKey) !== null;
				};

				vm.isDefaultLanguage = function () { return languages.currentLanguage != languages.defaultLanguage; };
				vm.enableTranslate = function () { return vm.fieldModel.getVsWithLanguage(languages.currentLanguage) === null; };

				vm.infoMessage = function () {
				    if (Object.keys($scope.value.Dimensions).length === 1 && $scope.value.Dimensions[languages.defaultLanguage] === false)
				        return lblDefault;
				    if (Object.keys($scope.value.Dimensions).length === 1 && $scope.value.Dimensions[languages.currentLanguage] === false)
				        return "";
				    return $translate.instant("LangMenu.In", { languages: Object.keys($scope.value.Dimensions).join(", ") });
				    // "in " + Object.keys($scope.value.Dimensions).join(", ");
				};

				vm.tooltip = function () {
				    var editableIn = [];
				    var usedIn = [];
				    angular.forEach($scope.value.Dimensions, function (value, key) {
				        (value ? usedIn : editableIn).push(key);
				    });
				    var tooltip = $translate.instant("LangMenu.EditableIn", { languages: editableIn.join(", ") }); // "editable in " + editableIn.join(", ");
				    if (usedIn.length > 0)
				        tooltip += $translate.instant("LangMenu.AlsoUsedIn", { languages: usedIn.join(", ") });// ", also used in " + usedIn.join(", ");
				    return tooltip;
				};

				vm.actions = {
				    toggleTranslate: function toggleTranslate() {
				        if (vm.enableTranslate())
				            vm.actions.translate();
				        else
				            vm.actions.linkDefault();
				    },
				    translate: function translate() {
				        if (vm.enableTranslate()) {
				            vm.fieldModel.removeLanguage(languages.currentLanguage);
				            vm.fieldModel.addVs($scope.value.Value, languages.currentLanguage, false);
				        }
				    },
				    linkDefault: function linkDefault() {
				        vm.fieldModel.removeLanguage(languages.currentLanguage);
				    },
				    autoTranslate: function (languageKey) {
				        // Google translate is not implemented yet, because
                        // there is no longer a free api.
				        alert($translate.instant("LangMenu.NotImplemented"));
				    },
				    copyFrom: function (languageKey) {
				        if ($scope.options.templateOptions.disabled)
				            alert($translate.instant("LangMenu.CopyNotPossible"));
				        else {
				            var value = vm.fieldModel.getVsWithLanguage(languageKey).Value;
				            if (vs === null || vs === undefined)
				                console.log($scope.options.key + ": Can't copy value from " + languageKey + ' because that value does not exist.');
				            else
				                $scope.value.Value = value;
				        }
				    },
				    useFrom: function (languageKey) {
				        var vs = vm.fieldModel.getVsWithLanguage(languageKey);
				        if (vs === null || vs === undefined)
				            console.log($scope.options.key + ": Can't use value from " + languageKey + ' because that value does not exist.');
				        else {
				            vm.fieldModel.removeLanguage(languages.currentLanguage);
				            vs.setLanguage(languages.currentLanguage, true);
				        }
				    },
				    shareFrom: function (languageKey) {
				        var vs = vm.fieldModel.getVsWithLanguage(languageKey);
				        if (vs === null || vs === undefined)
				            console.log($scope.options.key + ": Can't share value from " + languageKey + ' because that value does not exist.');
				        else {
				            vm.fieldModel.removeLanguage(languages.currentLanguage);
				            vs.setLanguage(languages.currentLanguage, false);
				        }
				    },
				    all: {
				        translate: function translate() {
				            forAllMenus('translate');
				        },
				        linkDefault: function linkDefault() {
				            forAllMenus('linkDefault');
				        },
				        copyFrom: function (languageKey) {
				            forAllMenus('copyFrom', languageKey);
				        },
				        useFrom: function (languageKey) {
				            forAllMenus('useFrom', languageKey);
				        },
				        shareFrom: function (languageKey) {
				            forAllMenus('shareFrom', languageKey);
				        }
				    }
				};

			    // Collect all localizationMenus (to enable "all" actions)
				if ($scope.formModel.localizationMenus === undefined)
				    $scope.formModel.localizationMenus = [];
				$scope.formModel.localizationMenus.push(vm.actions);

				var forAllMenus = function (action, languageKey) {
				    for (var i = 0; i < $scope.formModel.localizationMenus.length; i++) {
				        $scope.formModel.localizationMenus[i][action](languageKey);
				    }
				};
			}
		};
	});

	eavLocalization.directive("eavTreatTimeUtc", function () {
	    var directive = {
	        restrict: "A",
	        require: ["ngModel"],
            compile: compile,
	        link: link
	    };
	    return directive;

	    function compile(element, attributes) {

	    }

	    function link(scope, element, attributes, modelController) {     
	        modelController[0].$formatters.push(function (modelValue) {

	            return modelValue;
	        });

	        modelController[0].parsers.push(function (viewValue) {

	            return viewValue;
	        });
	    }
	});
})();