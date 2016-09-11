/* 
 * Field: String - Dropdown
 */

angular.module("eavFieldTemplates")
    .config(function(formlyConfigProvider, defaultFieldWrappers) {

        formlyConfigProvider.setType({
            name: "string-contenttype",
            templateUrl: "fields/string/string-contenttype.html",
            wrapper: defaultFieldWrappers,
            controller: "FieldTemplate-String-ContentType"
            //,
            //defaultOptions: function defaultOptions(options) {
            //
            //    // DropDown field: Convert string configuration for dropdown values to object, which will be bound to the select
            //    if (options.templateOptions.settings && options.templateOptions.settings.merged && options.templateOptions.settings.merged.DropdownValues) {
            //        var o = options.templateOptions.settings.merged.DropdownValues;
            //        o = o.replace(/\r/g, "").split("\n");
            //        o = o.map(function (e, i) {
            //            var s = e.split(":");
            //            return {
            //                name: s[0],
            //                value: (s[1] || s[1] === '') ? s[1] : s[0]
            //            };
            //        });
            //        options.templateOptions.options = o;
            //    }
            //
            //    function _defineProperty(obj, key, value) { return Object.defineProperty(obj, key, { value: value, enumerable: true, configurable: true, writable: true }); }
            //
            //    var ngOptions = options.templateOptions.ngOptions || "option[to.valueProp || 'value'] as option[to.labelProp || 'name'] group by option[to.groupProp || 'group'] for option in to.options";
            //    return {
            //        ngModelAttrs: _defineProperty({}, ngOptions, {
            //            value: "ng-options"
            //        })
            //    };
            //
            //}
        });

    })
    .controller("FieldTemplate-String-ContentType", function($scope, contentTypeSvc, appId) { //, $http, $filter, $translate, $modal, eavAdminDialogs, eavDefaultValueService) {
        // ensure settings are merged
        if (!$scope.to.settings.merged)
            $scope.to.settings.merged = {};

        // create initial list for binding
        $scope.contentTypes = [];

        var svc = contentTypeSvc(appId);
        svc.retrieveContentTypes().then(function(result) {
            $scope.contentTypes = result.data;
        });

    });