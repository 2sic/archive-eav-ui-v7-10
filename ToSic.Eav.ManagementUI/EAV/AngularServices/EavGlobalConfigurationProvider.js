// EavGlobalConfigurationProvider providers default global values for the EAV angular system
// The ConfigurationProvider in 2SexyContent is not the same as in the EAV project.
angular.module('eavGlobalConfigurationProvider', [])
	.factory('eavGlobalConfigurationProvider', function () {

		return {
			apiBaseUrl: "/api",
			defaultApiParams: {},
			dialogClass: "eavDialog",
			newItemUrl: "/EAVManagement.aspx?ManagementMode=NewItem&AttributeSetId=[AttributeSetId]&CultureDimension=[CultureDimension]&KeyNumber=[KeyNumber]&KeyGuid=[KeyGuid]&AssignmentObjectTypeId=[AssignmentObjectTypeId]",
			editItemUrl: "/EAVManagement.aspx?ManagementMode=EditItem&EntityId=[EntityId]&CultureDimension=2"
		};

	});