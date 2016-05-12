angular.module("PipelineDesigner", [
        "PipelineDesigner.filters",
        "ngResource",
        "EavConfiguration",
        "EavServices",
        "eavTemplates",
        "eavNgSvcs",
        "EavAdminUi",
        "eavEditEntity"
    ])

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
    });

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