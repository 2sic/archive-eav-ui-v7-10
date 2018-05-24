
(function () {
    /* jshint laxbreak:true*/

angular.module('EavDirectives')
    .directive('showDebugAvailability', function (eavConfig) {
        return {
            restrict: 'E',
            scope: {},
            template: "<span class=\"debug-indicator low-priority\" ng-class='{ \"debug-enabled\": debugState.on }' "
            + "uib-tooltip=\"{{ 'AdvancedMode.Info.Available' | translate }} \n" + eavConfig.versionInfo + '" '
            + "ng-click='askForLogging()'>"
                + '&pi;'
            + '</span><br/>',
            controller: ['$scope', 'debugState', 'toastr', function ($scope, debugState, toastr) {
                $scope.debugState = debugState;

                function askLogging() {
                    var duration = prompt('enable extended logging? type desired duration in minutes:\n\n' +
                      'note: try the new insights instead, see 2sxc.org/help?tag=insights', 1);
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
    });


})();