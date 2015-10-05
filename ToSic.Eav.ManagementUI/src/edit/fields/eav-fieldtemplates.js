
(function() {
	"use strict";

	/* This app registers all field templates for EAV in the angularjs eavFieldTemplates app */

	var eavFieldTemplates = angular.module("eavFieldTemplates", ["formly", "formlyBootstrap", "ui.bootstrap", "eavLocalization", "eavEditTemplates"])
        .config(function (formlyConfigProvider) {

	    formlyConfigProvider.setType({
	        name: "string-default",
	        template: "<input class=\"form-control\" ng-model=\"value.Value\">",
	        wrapper: ["eavLabel", "bootstrapHasError", "eavLocalization"]
	    });

	    formlyConfigProvider.setType({ name: "string-dropdown",
	        template: "<select class=\"form-control\" ng-model=\"value.Value\"></select>",
	        wrapper: ["eavLabel", "bootstrapHasError", "eavLocalization"],
	        defaultOptions: function defaultOptions(options) {
				
	            // DropDown field: Convert string configuration for dropdown values to object, which will be bound to the select
	            if (!options.templateOptions.options && options.templateOptions.settings.String.DropdownValues) {
	                var o = options.templateOptions.settings.String.DropdownValues;
	                o = o.replace("\r", "").split("\n");
	                o = o.map(function (e, i) {
	                    var s = e.split(":");
	                    return {
	                        name: s[0],
	                        value: s[1] ? s[1] : s[0]
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

	        }
	    });

	    formlyConfigProvider.setType({
	        name: "string-textarea",
	        template: "<textarea class=\"form-control\" ng-model=\"value.Value\"></textarea>",
	        wrapper: ["eavLabel", "bootstrapHasError", "eavLocalization"],
	        defaultOptions: {
	            ngModelAttrs: {
	                '{{to.settings.String.RowCount}}': { value: "rows" },
	                cols: { attribute: "cols" }
	            }
	        }
	    });

	    formlyConfigProvider.setType({
	        name: "number-default",
	        template: "<input type=\"number\" class=\"form-control\" ng-model=\"value.Value\">{{vm.isGoogleMap}}",
	        wrapper: ["eavLabel", "bootstrapHasError", "eavLocalization"],
	        defaultOptions: {
	            ngModelAttrs: {
	                '{{to.settings.Number.Min}}': { value: "min" },
	                '{{to.settings.Number.Max}}': { value: "max" },
	                '{{to.settings.Number.Decimals ? "^[0-9]+(\.[0-9]{1," + to.settings.Number.Decimals + "})?$" : null}}': { value: "pattern" }
	            }
	        },
	        controller: "FieldTemplate-NumberCtrl as vm"
	    });

	    formlyConfigProvider.setType({
	        name: "boolean-default",
	        template: "<div class=\"checkbox\">\n\t<label>\n\t\t<input type=\"checkbox\"\n           class=\"formly-field-checkbox\"\n\t\t       ng-model=\"value.Value\">\n\t\t{{to.label}}\n\t\t{{to.required ? '*' : ''}}\n\t</label>\n</div>\n",
	        wrapper: ["eavLabel", "bootstrapHasError", "eavLocalization"]
	    });

	    formlyConfigProvider.setType({
	        name: "datetime-default",
	        wrapper: ["eavLabel", "bootstrapHasError", "eavLocalization"],
	        template: "<div>" +
                "<div class=\"input-group\">" +
                    "<div class=\"input-group-addon\" style=\"cursor:pointer;\" ng-click=\"to.isOpen = true;\">" +
                        "<i class=\"glyphicon glyphicon-calendar\"></i>" +
                    "</div>" +
                    "<input class=\"form-control\" ng-model=\"value.Value\" is-open=\"to.isOpen\" datepicker-options=\"to.datepickerOptions\" datepicker-popup />" +
				    "<timepicker ng-show=\"to.settings.DateTime.UseTimePicker\" ng-model=\"value.Value\" show-meridian=\"ismeridian\"></timepicker>" +
                "</div>",
	        defaultOptions: {
	            templateOptions: {
	                datepickerOptions: {},
	                datepickerPopup: "dd.MM.yyyy"
	            }
	        }
		});

		formlyConfigProvider.setType({
		    name: "entity-default",
		    templateUrl: "fields/templates/entity-default.html",
		    wrapper: ["eavLabel", "bootstrapHasError"],
		    controller: "FieldTemplate-EntityCtrl"
		});

	    formlyConfigProvider.setWrapper({
	        name: 'eavLabel',
            templateUrl: "fields/eav-label.html"
	    });
	});

	eavFieldTemplates.controller("FieldTemplate-NumberCtrl", function () {
		var vm = this;
		// ToDo: Implement Google Map
	});


	eavFieldTemplates.controller("FieldTemplate-EntityCtrl", function ($scope, $http, $filter, $modal, appId) {

	    if (!$scope.to.settings.Entity)
	        $scope.to.settings.Entity = {};

	    $scope.availableEntities = [];

	    if ($scope.model[$scope.options.key] === undefined || $scope.model[$scope.options.key].Values[0].Value === "")
	        $scope.model[$scope.options.key] = { Values: [{ Value: [], Dimensions: {} }] };

	    $scope.chosenEntities = $scope.model[$scope.options.key].Values[0].Value;

	    $scope.addEntity = function () {
	        if ($scope.selectedEntity == "new")
	            $scope.openNewEntityDialog();
	        else
	            $scope.chosenEntities.push($scope.selectedEntity);
	        $scope.selectedEntity = "";
	    };

	    $scope.createEntityAllowed = function () {
	        return $scope.to.settings.Entity.EntityType !== null && $scope.to.settings.Entity.EntityType !== "";
	    };

	    $scope.openNewEntityDialog = function () {

	        var modalInstance = $modal.open({
	            template: "<div style=\"padding:20px;\"><edit-content-group edit=\"vm.edit\"></edit-content-group></div>",
	            controller: function (entityType) {
	                var vm = this;
	                vm.edit = { contentTypeName: entityType };
	            },
	            controllerAs: "vm",
	            resolve: {
	                entityType: function () {
	                    return $scope.to.settings.Entity.EntityType;
	                }
	            }
	        });

	        modalInstance.result.then(function () {
	            $scope.getAvailableEntities();
	        });

	    };

	    $scope.getAvailableEntities = function () {
	        $http({
	            method: "GET",
	            url: "eav/EntityPicker/getavailableentities",
	            params: {
	                contentTypeName: $scope.to.settings.Entity.EntityType,
	                appId: appId
	                // ToDo: dimensionId: $scope.configuration.DimensionId
	            }
	        }).then(function (data) {
	            $scope.availableEntities = data.data;
	        });
	    };

	    $scope.getEntityText = function (entityId) {
	        var entities = $filter("filter")($scope.availableEntities, { Value: entityId });
	        return entities.length > 0 ? entities[0].Text : "(Entity not found)";
	    };

	    $scope.remove = function (item) {
	        var index = $scope.chosenEntities.indexOf(item);
	        $scope.chosenEntities.splice(index, 1);
	    };

	    // Initialize entities
	    $scope.getAvailableEntities();

	});

})();