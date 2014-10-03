var pipelineDesigner = angular.module('pipelineDesinger', ['pipelineDesinger.filters', 'ngResource']);

pipelineDesigner.config(['$locationProvider', function ($locationProvider) {
	$locationProvider.html5Mode(true);
}]);

// datasource directive makes an element a DataSource with jsPlumb
pipelineDesigner.directive('datasource', function ($timeout) {
	return {
		restrict: 'A',
		link: function (scope, element) {
			scope.makeDataSource(scope.dataSource, element);
			if (scope.$last === true) {
				$timeout(function () {
					scope.$emit('ngRepeatFinished');
				});
			}
		}
	}
});

// Filters for "ClassName, AssemblyName"
angular.module('pipelineDesinger.filters', []).filter('typename', function () {
	return function (input, format) {
		var globalParts = input.match(/[^,\s]+/g);

		switch (format) {
			case 'classfqn':
				if (globalParts)
					return globalParts[0];
			case 'className':
				if (globalParts) {
					var classfqn = globalParts[0].match(/[^\.]+/g);
					return classfqn[classfqn.length - 1];
				}
		}

		return input;
	};
});