
(function () {
	'use strict';

	angular.module('eavEditEntity')
        .directive('eavEditEntityForm', function () {
		return {
		    templateUrl: 'form/edit-single-entity.html',
			restrict: 'E',
			scope: {
				contentTypeName: '@contentTypeName',
				entity: '=entity',
				registerEditControl: '=registerEditControl'
			},
			controller: 'EditEntityFormCtrl',
			controllerAs: 'vm'
		};
	});
	

})();