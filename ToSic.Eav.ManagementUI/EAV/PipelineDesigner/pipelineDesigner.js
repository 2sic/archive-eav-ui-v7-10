var pipelineDesigner = angular.module('pipelineDesinger', ['pipelineDesinger.filters', 'ngResource']);

pipelineDesigner.config(['$locationProvider', function ($locationProvider) {
	$locationProvider.html5Mode(true);
}]);

// Rise event ngRepeatFinished when ng-repeat has finished
// Source: http://stackoverflow.com/questions/15207788/calling-a-function-when-ng-repeat-has-finished
pipelineDesigner.directive('enablerenderfinishedevent', function () {
	return {
		restrict: 'A',
		link: function (scope) {
			if (scope.$last === true) {
				scope.$emit('ngRepeatFinished');
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
			case 'class':
				if (globalParts) {
					var classfqn = globalParts[0].match(/[^\.]+/g);
					return classfqn[classfqn.length - 1];
				}
		}

		return input;
	};
});