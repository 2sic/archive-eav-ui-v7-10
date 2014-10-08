pipelineDesigner.factory('uiNotification', ['toaster', function (toaster) {
	'use strict';

	var showNote = function (type, title, body, autoHide) {
		toaster.clear();
		toaster.pop(type, title, body, autoHide ? null : 0);
	};

	return {
		error: function (title, body, autoHide) {
			showNote('error', title, body, autoHide);
		},
		note: function (title, body, autoHide) {
			showNote('note', title, body, autoHide);
		},
		success: function (title, body, autoHide) {
			showNote('success', title, body, autoHide);
		}
	};
}]);