angular.module("PipelineDesigner", ["PipelineDesigner.filters", "ngResource", "toaster", "eavGlobalConfigurationProvider", "eavDialogService", "PipelineService", "eavTemplates"]) 

// datasource directive makes an element a DataSource with jsPlumb
    .directive("datasource", function($timeout) {
        return {
            restrict: "A",
            link: function(scope, element) {
                // make this a DataSource when the DOM is ready
                $timeout(function() {
                    scope.makeDataSource(scope.dataSource, element);
                });
                if (scope.$last === true) {
                    $timeout(function() {
                        scope.$emit("ngRepeatFinished");
                    });
                }
            }
        };
    })

// Show Notifications using toaster
    .factory("uiNotification", [
        "toaster", function(toaster) {
            "use strict";

            var showNote = function(type, title, body, autoHide) {
                // wrap toaster in ready-Event because notes would't be show if teaster is used before
                angular.element(document).ready(function() {
                    toaster.clear();
                    toaster.pop(type, title, body, autoHide ? null : 0);
                });
            };

            return {
                clear: function() {
                    toaster.clear();
                },
                error: function(title, bodyOrError) {
                    var message;
                    // test whether bodyOrError is an Error from Web API
                    if (bodyOrError && bodyOrError.data && bodyOrError.data.Message) {
                        message = bodyOrError.data.Message;
                        if (bodyOrError.data.ExceptionMessage)
                            message += "\n" + bodyOrError.data.ExceptionMessage;
                    } else
                        message = bodyOrError;

                    showNote("error", title, message);
                },
                note: function(title, body, autoHide) {
                    showNote("note", title, body, autoHide);
                },
                success: function(title, body, autoHide) {
                    showNote("success", title, body, autoHide);
                },
                wait: function(title) {
                    showNote("note", title ? title : "Please wait ..", "This shouldn't take long", false);
                }
            };
        }
    ]);

// Filters for "ClassName, AssemblyName"
angular.module("PipelineDesigner.filters", []).filter("typename", function () {
	return function (input, format) {
		var globalParts = input.match(/[^,\s]+/g);

		switch (format) {
			case "classFullName":
				if (globalParts)
				    return globalParts[0];
			    break;
			case "className":
				if (globalParts) {
					var classFullName = globalParts[0].match(/[^\.]+/g);
					return classFullName[classFullName.length - 1];
				}
		}

		return input;
	};
});