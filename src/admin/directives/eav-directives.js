
(function () {
    /* jshint laxbreak:true*/

angular.module("EavDirectives", [])
    .directive("icon", function() {
        return {
            restrict: "A",
            replace: false,
            transclude: false,
            link: function postLink(scope, elem, attrs) {
                var icn = attrs.icon;
                elem.addClass("glyphicon glyphicon-" + icn);
            }
        };
    })
    .directive("stopEvent", function() {
        return {
            restrict: "A",
            link: function(scope, element, attr) {
                if (attr && attr.stopEvent)
                    element.bind(attr.stopEvent, function(e) {
                        e.stopPropagation();
                    });
            }
        };
    })
    .directive("showDebugAvailability", function (eavConfig) {
        return {
            restrict: "E",
            scope: {},
            template: "<span class=\"debug-indicator low-priority\" ng-class='{ \"debug-enabled\": debugState.on }' "
            + "uib-tooltip=\"{{ 'AdvancedMode.Info.Available' | translate }} \n" + eavConfig.versionInfo + "\" "
            + "ng-click='askForLogging()'>"
                + "&pi;"
            + "</span><br/>",
            controller: ["$scope", "debugState", "toastr", function ($scope, debugState, toastr) {
                $scope.debugState = debugState;

                function askLogging() {
                    var duration = prompt("enable extended logging? type desired duration in minutes:", 1);
                    if (duration === null || duration === undefined) return;
                    debugState.enableExtendedLogging(duration).then(function (res) {
                        console.log(res.data);
                        toastr.info(res.data, { timeOut: 1000 });
                    });
                }


                $scope.askForLogging = function () {
                    if (!debugState.on) return;
                    askLogging();
                };
            }]
        };
    })

    ;


})();