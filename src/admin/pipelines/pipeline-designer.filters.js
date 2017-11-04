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