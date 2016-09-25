
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
    .directive('stopEvent', function() {
        return {
            restrict: 'A',
            link: function(scope, element, attr) {
                if (attr && attr.stopEvent)
                    element.bind(attr.stopEvent, function(e) {
                        e.stopPropagation();
                    });
            }
        };
    })
    .directive('showDebugAvailability', function() {
        return {
            restrict: 'E',
            template: "<span class=\"low-priority\" uib-tooltip=\"{{ 'AdvancedMode.Info.Available' | translate }}\">"
                + "&pi;" // "<i icon=\"sunglasses\"></i>"
                + "</span><br/>"
        };
    })

    ;


})();