/* global angular */
(function () {
	"use strict";

	angular.module("eavEditEntity")
        .service("customInputTypes", function (eavConfig) {
            // Returns a typed default value from the string representation
            var svc = {};
	        var inputTypesOnPage = {};

	        svc.addInputType = function addInputType(config) {
                // check if anything defined - older configurations don't have anything and will default to string-default anyhow
	            if (config === undefined || config === null)
	                return;

                // only add one if it has not been added yet
	            if (inputTypesOnPage[config.Type] !== undefined)
	                return;

	            inputTypesOnPage[config.Type] = config;

	            svc.loadNewAssets(config);
	        };

	        svc.loadNewAssets = function loadNewAssets(config) {
	            if (config.Assets === undefined || config.Assets === null || !config.Assets)
	                return;

	            // todo: split by new-line
	            var list = config.Assets.split("\n");//.replace(/\n/g, "~").split("~");

	            // todo: add to document
                for(var a = 0; a < list.length; a++)
                    svc.addHeadJsOrCssTag(svc.resolveSpecialPaths(list[a]));
	        };

	        svc.resolveSpecialPaths = function resolve(url) {
	            url = url.replace(/\[System:Path\]/i, eavConfig.getUrlPrefix("system"))
	                .replace(/\[Zone:Path\]/i, eavConfig.getUrlPrefix("zone"))
	                .replace(/\[App:Path\]/i, eavConfig.getUrlPrefix("app"));
	            return url;
	        };

	        svc.addHeadJsOrCssTag = function(url) {
	            if (url.indexOf(".js") > 0) {
	                var oHead = document.getElementsByTagName("HEAD").item(0);
	                var oScript = document.createElement("script");
	                oScript.type = "text/javascript";
	                oScript.src = url;
	                oHead.appendChild(oScript);
	            } else if (url.indexOf(".css") > 0) {
	                alert("css include not implemented yet");
	            }
	        };

	        return svc;
	    });

})();