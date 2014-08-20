// EavGlobalConfigurationProvider providers default global values for the EAV angular system
// The ConfigurationProvider in 2SexyContent is not the same as in the EAV project.

(function () {
    angular.module('2sic-EAV')
        .factory('eavGlobalConfigurationProvider', function () {

            return {
                apiBaseUrl: "/api",
                defaultApiParams: {},
                dialogClass: "eavDialog"
            };

        });
})();