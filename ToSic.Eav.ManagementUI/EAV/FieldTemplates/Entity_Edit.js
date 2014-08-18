(function() {
    angular.module('2sic-EAV', ['ui.tree'])
        .controller('EntityEditCtrl', function($scope) {
            $scope.configuration = {};
            $scope.selectedEntity = "";
            $scope.entityIds = function () {
                return $scope.configuration.SelectedEntities.join(',');
            };
            $scope.AddEntity = function () {
                $scope.configuration.SelectedEntities.push(parseInt($scope.selectedEntity));
                $scope.selectedEntity = "";
            };

            $scope.CreateEntity = function () {
                
            };
        }).directive('openDialog', function () {
            // Directive to open a jQuery UI modal dialog
            return {
                restrict: 'A',
                link: function(scope, elem, attr, ctrl) {
                    var dialogId = '#' + attr.openDialog;
                    elem.bind('click', function(e) {
                        $(dialogId).dialog({
                            autoOpen: false,
                            modal: true,
                            buttons: {
                                Cancel: function() {
                                    $(this).dialog('close');
                                }
                            }
                        }).dialog('open');
                    });
                }
            };
    });

})();