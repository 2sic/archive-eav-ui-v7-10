/* 
 * Field: DateTime - Default
 */

angular.module("eavFieldTemplates")
    .config(function (formlyConfigProvider, defaultFieldWrappers) {

        formlyConfigProvider.setType({
            name: "datetime-default",
            wrapper: defaultFieldWrappers,
            template: "<div>" +
                "<div class=\"input-group\">" +
                    "<div class=\"input-group-addon\" style=\"cursor:pointer;\" ng-click=\"to.isOpen = true;\">" +
                        "<i class=\"glyphicon glyphicon-calendar\"></i>" +
                    "</div>" +
                    "<input class=\"form-control input-lg\" ng-model=\"value.Value\" is-open=\"to.isOpen\" datepicker-options=\"to.datepickerOptions\" datepicker-popup />" +
				    "<timepicker ng-show=\"to.settings.merged.UseTimePicker\" ng-model=\"value.Value\" show-meridian=\"ismeridian\"></timepicker>" +
                "</div>",
            defaultOptions: {
                templateOptions: {
                    datepickerOptions: {},
                    datepickerPopup: "dd.MM.yyyy"
                }
            },
            link: function (scope, el, attrs) {
                // Server delivers value as string, so convert it to UTC date
                function convertDateToUTC(date) { return new Date(date.getUTCFullYear(), date.getUTCMonth(), date.getUTCDate(), date.getUTCHours(), date.getUTCMinutes(), date.getUTCSeconds()); }
                if (scope.value && scope.value.Value && typeof (scope.value.Value) === 'string')
                    scope.value.Value = convertDateToUTC(new Date(scope.value.Value));
            }
        });

    });