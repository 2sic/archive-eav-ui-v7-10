
(function () {
    /* jshint laxbreak:true*/

angular.module('EavDirectives')
    .directive('icon', function() {
        return {
            restrict: 'A',
            replace: false,
            transclude: false,
            link: function postLink(scope, elem, attrs) {
                var icn = attrs.icon;
                elem.addClass('glyphicon glyphicon-' + icn);
            }
        };
    });


})();