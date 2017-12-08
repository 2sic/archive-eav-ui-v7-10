/* 
 * Field: Entity - Query
 * 
 */

angular.module("eavFieldTemplates")
    .config(function (formlyConfigProvider, defaultFieldWrappers) {
        formlyConfigProvider.setType({
            name: "string-dropdown-query",
            templateUrl: "fields/entity/entity-default.html",
            wrapper: defaultFieldWrappers,
            controller: "FieldTemplate-StringDropDownQueryCtrl"
        });
    })
    .controller("FieldTemplate-StringDropDownQueryCtrl", function ($controller, $scope, fieldMask, $q, query, $timeout, eavDefaultValueService) {

        // Define options because these options are needed by the base template but not provided since the
        // type of this field is string and not entity
        $scope.to.settings.merged.EnableRemove = true;
        $scope.to.settings.merged.EnableAddExisting = true; // enable manual select existing
        $scope.to.settings.merged.EnableCreate = false;         // disable manual create
        $scope.to.settings.merged.EnableEdit = false;
        $scope.to.settings.merged.EntityType = "";
        var separator = $scope.to.settings.merged.Separator;

        $scope.modes = { freeTextMode: false };

        // use "inherited" controller just like described in http://stackoverflow.com/questions/18461263/can-an-angularjs-controller-inherit-from-another-controller-in-the-same-module
        $controller("FieldTemplate-EntityQueryCtrl", { $scope: $scope });

        $scope.entityTextDefault = null; // display value instead of "not found" message when selected value does not exist

        // We don't need to initialize the value because we're saving it as string instead of relationships
        $scope.initValue = function () {};

        $scope.queryEntityMapping = function (entity) {
            return { Value: entity[$scope.to.settings.merged.Value], Text: entity[$scope.to.settings.merged.Label], Id: entity.Id };
        };


        // Override binding - we must save values as string instead
        $scope.bindModel = function () {
            var values = $scope.value.Value;
            if (!values) values = "";
            if (values === "") $scope.chosenEntities = [];
            else $scope.chosenEntities = values.split(separator);
        };
        
        $scope.$watch('chosenEntities', function (newValue, oldValue) {
            if(newValue && newValue.map)
                $scope.value.Value = newValue.join(separator);
        }, true);

        // Refresh entity list when free text mode is left
        $scope.$watch('modes.freeTextMode', function (newValue, oldValue) {
            if (newValue === false && oldValue === true)
                $scope.bindModel();
        }, true);
        
    });
