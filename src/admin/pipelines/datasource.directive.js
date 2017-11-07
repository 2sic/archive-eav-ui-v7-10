

angular.module("PipelineDesigner")
    // datasource directive makes an element a DataSource with jsPlumb
    .directive('datasource', function ($timeout) {
        return {
            restrict: 'A',
            link: function (scope, element) {
                // make this a DataSource when the DOM is ready
                $timeout(function () {
                    scope.makeDataSource(scope.set.ds/*.dataSource*/, element);
                });
                if (scope.$last === true) {
                    $timeout(function () {
                        scope.$emit("ngRepeatFinished");
                    });
                }
            }
        };
    });
