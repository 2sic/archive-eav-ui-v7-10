/* global angular */
(function () {
	"use strict";

	angular.module("eavEditEntity")
        .service("eavDefaultValueService", function () {
		// Returns a typed default value from the string representation
		return function parseDefaultValue(fieldConfig) {
			var e = fieldConfig;
			var d = e.templateOptions.settings.All.DefaultValue;

		    if (e.templateOptions.header.Prefill && e.templateOptions.header.Prefill[e.key]) {
			    d = e.templateOptions.header.Prefill[e.key];
			}

			switch (e.type.split("-")[0]) {
				case "boolean":
					return d !== undefined && d !== null ? d.toLowerCase() === "true" : false;
				case "datetime":
					return d !== undefined && d !== null && d !== "" ? new Date(d) : null;
			    case "entity":
			        if (!(d !== undefined && d !== null && d !== ""))
			            return []; // no default value

			        // 3 possibilities
			        if (d.constructor === Array) return d;  // possibility 1) an array

                    // for possibility 2 & 3, do some variation checking
			        if (d.indexOf("{") > -1) // string has { } characters, we must switch them to quotes
			            d = d.replace(/[\{\}]/g, "\"");

			        if (d.indexOf(",") !== -1 && d.indexOf("[") === -1) // list but no array, add brackets
			            d = "[" + d + "]";

			        return (d.indexOf("[") === 0) // possibility 2) an array with guid strings
			            ? JSON.parse(d) // if it's a string containing an array
			            : [d.replace(/"/g, "")]; //  possibility 3) just a guid string, but might have quotes
                        
				case "number":
				    return d !== undefined && d !== null && d !== "" ? Number(d) : "";
                default:
					return d ? d : "";
			}
		};
	});

})();