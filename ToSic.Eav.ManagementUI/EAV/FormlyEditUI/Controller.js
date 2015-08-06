﻿/* global angular */
(function () {
	'use strict';

	var app = angular.module('formlyExample', ['formly', 'formlyBootstrap', 'ui.bootstrap'], function config(formlyConfigProvider) {

		formlyConfigProvider.setType({
			name: 'string-default',
			template: '<input class="form-control" ng-model="model[options.key]">',
			wrapper: ['bootstrapLabel', 'bootstrapHasError']
		});

		formlyConfigProvider.setType({
			name: 'string-wysiwyg',
			template: '<textarea class="form-control" ng-model="model[options.key]"></textarea>',
			wrapper: ['bootstrapLabel', 'bootstrapHasError']
		});

		function _defineProperty(obj, key, value) { return Object.defineProperty(obj, key, { value: value, enumerable: true, configurable: true, writable: true }); }
		formlyConfigProvider.setType({
			name: 'string-dropdown',
			template: '<select class="form-control" ng-model="model[options.key]"></select>',
			wrapper: ['bootstrapLabel', 'bootstrapHasError'],
			defaultOptions: function defaultOptions(options) {

				// 2sxc dropdown field: Convert string configuration for dropdown values to object, which will be bound to the select
				if (!options.templateOptions.options && options.templateOptions.settings.DropdownValues) {
					var o = options.templateOptions.settings.DropdownValues;
					o = o.replace('\r', '').split('\n');
					o = o.map(function (e, i) {
						var s = e.split(':');
						return {
							name: s[0],
							value: s[1] ? s[1] : s[0]
						};
					});
					options.templateOptions.options = o;
				}

				var ngOptions = options.templateOptions.ngOptions || 'option[to.valueProp || \'value\'] as option[to.labelProp || \'name\'] group by option[to.groupProp || \'group\'] for option in to.options';
				return {
					ngModelAttrs: _defineProperty({}, ngOptions, {
						value: 'ng-options'
					})
				};
			}
		});

		formlyConfigProvider.setType({
			name: 'string-textarea',
			template: '<textarea class="form-control" ng-model="model[options.key]"></textarea>',
			wrapper: ['bootstrapLabel', 'bootstrapHasError'],
			defaultOptions: {
				ngModelAttrs: {
					'{{to.settings.RowCount}}': { value: 'rows' },
					cols: { attribute: 'cols' }
				}
			}
		});

		formlyConfigProvider.setType({
			name: 'number-default',
			template: '<input type="number" class="form-control" ng-model="model[options.key]">',
			wrapper: ['bootstrapLabel', 'bootstrapHasError'],
			defaultOptions: {
				ngModelAttrs: {
					'{{to.settings.Min}}': { value: 'min' },
					'{{to.settings.Max}}': { value: 'max' },
					'{{to.settings.Decimals ? "^[0-9]+(\.[0-9]{1," + to.settings.Decimals + "})?$" : null}}': { value: 'pattern' }
				}
			}
		});

		formlyConfigProvider.setType({
			name: 'boolean-default',
			template: "<div class=\"checkbox\">\n\t<label>\n\t\t<input type=\"checkbox\"\n           class=\"formly-field-checkbox\"\n\t\t       ng-model=\"model[options.key]\">\n\t\t{{to.label}}\n\t\t{{to.required ? '*' : ''}}\n\t</label>\n</div>\n",
			wrapper: ['bootstrapHasError']
		});


		/* Control: datetime-default */
		var attributes = [
			'date-disabled',
			'custom-class',
			'show-weeks',
			'starting-day',
			'init-date',
			'min-mode',
			'max-mode',
			'format-day',
			'format-month',
			'format-year',
			'format-day-header',
			'format-day-title',
			'format-month-title',
			'year-range',
			'shortcut-propagation',
			'datepicker-popup',
			'show-button-bar',
			'current-text',
			'clear-text',
			'close-text',
			'close-on-date-selection',
			'datepicker-append-to-body'
		];

		var bindings = [
		  'datepicker-mode',
		  'min-date',
		  'max-date'
		];

		var ngModelAttrs = {};

		angular.forEach(attributes, function (attr) {
			ngModelAttrs[camelize(attr)] = { attribute: attr };
		});

		angular.forEach(bindings, function (binding) {
			ngModelAttrs[camelize(binding)] = { bound: binding };
		});

		formlyConfigProvider.setType({
			name: 'datetime-default',
			wrapper: ['bootstrapLabel', 'bootstrapHasError'],
			template: '<input class="form-control" ng-model="model[options.key]" is-open="to.isOpen" datepicker-options="to.datepickerOptions" />',
			defaultOptions: {
				ngModelAttrs: ngModelAttrs,
				templateOptions: {
					addonLeft: {
						'class': 'glyphicon glyphicon-calendar',
						onClick: function (options, scope) {
							scope.to.isOpen = true;
						}
					},
					datepickerOptions: {},
					datepickerPopup: 'dd.MM.yyyy'
				}
			}
		});

		function camelize(string) {
			string = string.replace(/[\-_\s]+(.)?/g, function(match, chr) {
				return chr ? chr.toUpperCase() : '';
			});
			// Ensure 1st char is always lowercase
			return string.replace(/^([A-Z])/, function(match, chr) {
				return chr ? chr.toLowerCase() : '';
			});
		}

	});


	app.controller('MainCtrl', function MainCtrl($http) {
		var vm = this;
		vm.onSubmit = function onSubmit() {
			alert(JSON.stringify(vm.model), null, 2);
		};

		vm.model = {};

		vm.formFields = null;

		$http.get('/api/eav/ContentType/GetContentTypeConfiguration?appId=1&zoneId=1&contentTypeName=All Field Types')
		.then(function (result) {
			vm.debug = result;

			// Transform EAV content type configuration to formFields
			angular.forEach(result.data, function (e, i) {
				vm.formFields.push({
					key: e.StaticName,
					type: getType(e),
					templateOptions: {
						required: !!e.MetaData.Required,
						label: e.MetaData.Name,
						description: e.MetaData.Notes,
						settings: e.MetaData
					},
					hide: (e.MetaData.VisibleInEditUI ? !e.MetaData.VisibleInEditUI : false),
					defaultValue: convertDefaultValue(e)
				});
			});

		});

		// Returns the field type for an attribute configuration
		function getType(attributeConfiguration) {
			var e = attributeConfiguration;
			var type = e.Type;
			var subType = e.MetaData.InputType;

			// Use subtype 'default' if none is specified
			if (!subType)
				subType = 'default';

			// Special case: override type to string-textarea if multiline field should be used
			if (e.MetaData.RowCount > 1)
				subType = 'textarea';

			return (type + '-' + subType).toLowerCase();
		}

		// Returns a typed default value from the string representation
		function convertDefaultValue(attributeConfiguration) {
			var e = attributeConfiguration;

			if (!e.MetaData.DefaultValue)
				return null;

			switch (e.Type.toLowerCase()) {
				case 'boolean':
					return e.MetaData.DefaultValue.toLowerCase() == 'true';
				case 'datetime':
					return new Date(e.MetaData.DefaultValue);
				default:
					return e.MetaData.DefaultValue;
			}
		}

	});

})();