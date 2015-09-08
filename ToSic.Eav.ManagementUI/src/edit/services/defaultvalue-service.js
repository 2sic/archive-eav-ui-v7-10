/* global angular */
(function () {
	'use strict';

	angular.module('eavEditEntity').service('eavDefaultValueService', function () {
		// Returns a typed default value from the string representation
		return function parseDefaultValue(fieldConfig) {
			var e = fieldConfig;
			var d = e.templateOptions.settings.DefaultValue;

			switch (e.type.split('-')[0]) {
				case 'boolean':
					return d !== undefined ? d.toLowerCase() == 'true' : false;
				case 'datetime':
					return d !== undefined ? new Date(d) : null;
				case 'entity':
					return []; 
				case 'number':
					return null;
				default:
					return d ? d : "";
			}
		};
	});

})();