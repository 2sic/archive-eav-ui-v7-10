/* global angular */
(function () {
	'use strict';

	var app = angular.module('formlyExample', ['formly', 'formlyBootstrap'], function config(formlyConfigProvider) {

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
					o = o.map(function(e, i) {
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

		// ToDo: Finish number-default
		formlyConfigProvider.setType({
			name: 'number-default',
			template: '<input class="form-control" ng-model="model[options.key]">',
			wrapper: ['bootstrapLabel', 'bootstrapHasError']
		});

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
			angular.forEach(result.data, function (e,i) {
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
					defaultValue: e.MetaData.DefaultValue
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

	});

})();