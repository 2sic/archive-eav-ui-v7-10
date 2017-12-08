/* 
 * Field: String - Dropdown
 */

angular.module("eavFieldTemplates")
    .config(function (formlyConfigProvider, defaultFieldWrappers) {

        formlyConfigProvider.setType({
            name: "string-dropdown",
            templateUrl: "fields/string/string-dropdown.html",
            wrapper: defaultFieldWrappers,
            defaultOptions: function defaultOptions(options) {
                
                // DropDown field: Convert string configuration for dropdown values to object, which will be bound to the select
                if (options.templateOptions.settings && options.templateOptions.settings.merged && options.templateOptions.settings.merged.DropdownValues) {
                    var o = options.templateOptions.settings.merged.DropdownValues;
                    o = o.replace(/\r/g, "").split("\n");
                    o = o.map(function (e, i) {
                        var s = e.split(":"),
                            maybeWantedEmptyVal = s[1],
                            key = s.shift(), // take first, shrink the array
                            val = s.join(":");

                        return {
                            name: key,
                            value: (val || maybeWantedEmptyVal === '') ? val : key
                        };
                    });
                    options.templateOptions.options = o;
                }
                
                function _defineProperty(obj, key, value) { return Object.defineProperty(obj, key, { value: value, enumerable: true, configurable: true, writable: true }); }

                var ngOptions = options.templateOptions.ngOptions || "option[to.valueProp || 'value'] as option[to.labelProp || 'name'] group by option[to.groupProp || 'group'] for option in to.options";
                return {
                    ngModelAttrs: _defineProperty({}, ngOptions, {
                        value: "ng-options"
                    })
                };

            },
            controller: "FieldTemplate-String-DropDown"
        });
    }).controller("FieldTemplate-String-DropDown", function ($scope, $timeout) { //, $http, $filter, $translate, $uibModal, eavAdminDialogs, eavDefaultValueService) {
        
        $timeout(function () {
            $scope.freeTextMode = false;
            if ($scope.to.settings.merged.EnableTextEntry && $scope.value && $scope.value.Value) {
                if ($scope.to.options.filter(function (e) { return e.value == $scope.value.Value; }).length === 0)
                    $scope.freeTextMode = true;
            }
        }, 1);
    });