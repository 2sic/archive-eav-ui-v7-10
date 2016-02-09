/* service to manage input types */
(function () {
	"use strict";

    // notes: this has not been tested extensively
    // I'm guessing that it's not optimal yet, and I'm guessing that if the dialog is opened multiple times, that the list of dependencies just
    // keeps on growing and the UI might just get heavier with time ... must test once we have a few custom input types

	angular.module("eavEditEntity")
        .service("customInputTypes", function (eavConfig, toastr, formlyConfig, $q, $interval, $ocLazyLoad) {
            // Returns a typed default value from the string representation
            var svc = {};
            svc.inputTypesOnPage = {};
            svc.allLoaded = true;
            svc.assetsToLoad = [];

	        svc.addInputType = function addInputType(field) {
	            var config = field.InputTypeConfig;
                // check if anything defined - older configurations don't have anything and will default to string-default anyhow
	            if (config === undefined || config === null)
	                return;

	            svc.inputTypesOnPage[config.Type] = config;

	            svc.addToLoadQueue(config);
	        };

	        svc.addToLoadQueue = function loadNewAssets(config) {
	            if (config.Assets === undefined || config.Assets === null || !config.Assets) {
	                config.assetsLoaded = true;
	                return;
	            }

	            // split by new-line, ensuring nothing blank
	            var list = config.Assets.split("\n");

	            for (var a = 0; a < list.length; a++) {
	                var asset = list[a].trim();
	                if (asset.length > 5) { // ensure we skip empty lines etc.
	                    svc.assetsToLoad.push(svc.resolveSpecialPaths(asset));
	                }
	            }
	        };

	        // now create promise and wait for everything to load
	        svc.loadWithPromise = function loadWithPromise() {
	            return $ocLazyLoad.load(svc.assetsToLoad);

	        };

	        svc.resolveSpecialPaths = function resolve(url) {
	            url = url.replace(/\[System:Path\]/i, eavConfig.getUrlPrefix("system"))
	                .replace(/\[Zone:Path\]/i, eavConfig.getUrlPrefix("zone"))
	                .replace(/\[App:Path\]/i, eavConfig.getUrlPrefix("app"));
	            return url;
	        };

	        svc.checkDependencyArrival = function cda(typeName) {
	            return !!formlyConfig.getType(typeName);
	        };

	        return svc;
	    });

})();