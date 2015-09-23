
(function () {
	'use strict';


	/* This app handles all aspectes of the multilanguage features of the field templates */

	var eavLocalization = angular.module('eavLocalization', ['formly', "EavConfiguration"], function (formlyConfigProvider) {

		// Field templates that use this wrapper must bind to value.Value instead of model[...]
		formlyConfigProvider.setWrapper([
			{
				name: 'eavLocalization',
				templateUrl: 'localization/formly-localization-wrapper.html'
			}
		]);

	});

	eavLocalization.directive('eavLanguageSwitcher', function () {
		return {
			restrict: 'E',
			templateUrl: 'localization/language-switcher.html',
			controller: function($scope, languages) {
				$scope.languages = languages;
			}
		};
	});

	eavLocalization.directive('eavLocalizationScopeControl', function () {
		return {
			restrict: 'E',
			transclude: true,
			template: '',
			link: function (scope, element, attrs) {
			},
			controller: function ($scope, $filter, eavDefaultValueService, languages) { // Can't use controllerAs because of transcluded scope

				var scope = $scope;
				var langConf = languages;

				var initCurrentValue = function() {

					// Set base value object if not defined
					if (!scope.model[scope.options.key])
						scope.model[scope.options.key] = { Values: [] };

					var fieldModel = scope.model[scope.options.key];

					// If current language = default language and there are no values, create an empty value object
					if (langConf.currentLanguage == langConf.defaultLanguage) {
						if (fieldModel.Values.length === 0) {
						    var defaultValue = eavDefaultValueService(scope.options);
						    fieldModel.addVs(defaultValue, langConf.currentLanguage); // Assign default language dimension
						}
					}

				    // Assign default language if no dimension is set
					if (Object.keys(fieldModel.Values[0].Dimensions).length === 0)
					    fieldModel.Values[0].Dimensions[langConf.defaultLanguage] = false;

					var valueToEdit;

					// Decide which value to edit:
					// 1. If there is a value with current dimension on it, use it
					valueToEdit = $filter('filter')(fieldModel.Values, function(v, i) {
						return v.Dimensions[langConf.currentLanguage] !== undefined;
					})[0];

					// 2. Use default language value
					if (valueToEdit === undefined)
						valueToEdit = $filter('filter')(fieldModel.Values, function(v, i) {
							return v.Dimensions[langConf.defaultLanguage] !== undefined;
						})[0];

					// 3. Use the first value if there is only one
					if (valueToEdit === undefined) {
						if (fieldModel.Values.length > 1)
							throw "Default language value not found, but found multiple values - can't handle editing for " + $scope.options.key;
						// Use the first value
						valueToEdit = fieldModel.Values[0];
					}

					fieldModel._currentValue = valueToEdit;

					// Set scope variable 'value' to simplify binding
					scope.value = fieldModel._currentValue;

				    // Decide whether the value is writable or not
					var writable = (langConf.currentLanguage == langConf.defaultLanguage) ||
                        (scope.value && scope.value.Dimensions[langConf.currentLanguage] === false);

					scope.to.disabled = !writable;
				};

				initCurrentValue();

				// Handle language switch
				scope.langConf = langConf; // Watch does only work on scope variables
				scope.$watch('langConf.currentLanguage', function (newValue, oldValue) {
					if (oldValue === undefined || newValue == oldValue)
						return;
					initCurrentValue();
					console.log('switched language from ' + oldValue + ' to ' + newValue);
				});

				// ToDo: Could cause performance issues (deep watch array)...
				scope.$watch('model[options.key].Values', function(newValue, oldValue) {
					initCurrentValue();
				}, true);

				// The language menu must be able to trigger an update of the _currentValue property
				scope.model[scope.options.key]._initCurrentValue = initCurrentValue;
			}
		};
	});

	eavLocalization.directive('eavLocalizationMenu', function() {
		return {
			restrict: 'E',
			scope: {
				fieldModel: '=fieldModel',
				options: '=options'
			},
			templateUrl: 'localization/localization-menu.html',
			link: function (scope, element, attrs) { },
			controllerAs: 'vm',
			controller: function ($scope, languages) {
				var vm = this;
				vm.fieldModel = $scope.fieldModel;

				vm.isDefaultLanguage = function () { return languages.currentLanguage != languages.defaultLanguage; };
				vm.enableTranslate = function () { return true; };

				vm.actions = {
				    translate: function translate() {
				        vm.fieldModel.addVs($scope.value.Value, languages.currentLanguage, false);
				    },
				    linkDefault: function linkDefault() {
				        vm.fieldModel.removeLanguage(languages.currentLanguage);
				    }
				};

			}
		};
	});

	eavLocalization.directive('eavTimepickerWithoutTimezone', function () {
	    var directive = {
	        restrict: 'A',
	        require: ['ngModel'],
	        link: link
	    };
	    return directive;

	    function link(scope, element, attributes, modelController) {
	        scope.$watch(attributes.ngModel, function (newValue) {
	            //var dateString;
	            //var date;
	            //if (newValue instanceof Date) {
	            //    date = newValue;
	            //} else {
	            //    date = new Date(newValue.substring(0, 4), newValue.substring(5, 7), newValue.substring(8, 10), newValue.substring(11, 13), newValue.substring(14, 16), newValue.substring(17, 19));
	            //}
	            //dateString = Date.UTC(date.getFullYear(), date.getMonth(), date.getDate(), date.getHours(), date.getMinutes(), date.getSeconds()); //).toISOString().substring(0, 20);

	            //console.log(typeof newValue + "-" +  newValue + ">" +  new Date(dateString));
	            //attributes.ngModel = "Test";
	            //scope.$apply();
	            //console.log(newValue.toISOString().substring(0, 20));
	        });

	        //var modelController = controllers[0];

	        //// Called with date object when picked from the datepicker
	        //modelController.$parsers.push(function (viewValue) {
	        //    return viewValue.toISOString().substring(0, 20);
	        //});

	        //// Called with 'yyyy-mm-ddThh.mm.ssZ' string to format
	        //modelController.$formatters.push(function (modelValue) {
	        //    if (!modelValue) {
	        //        return "undefined";
	        //    }
	        //    var date = new Date(modelValue);
	        //    date.setMonth(0);
	        //    return date;
	        //});
	    }
	});
})();