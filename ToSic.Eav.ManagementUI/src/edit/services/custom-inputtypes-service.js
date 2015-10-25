/* global angular */
(function () {
	"use strict";

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

                // only add one if it has not been added yet
	            if (svc.inputTypesOnPage[config.Type] !== undefined)
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
	                    // add to document
	                    //if (svc.addHeadJsOrCssTag(svc.resolveSpecialPaths(asset))) {
	                    //    // note: returns true if it has JS, in which case we monitor for success before binding the form
	                    //    // config.hasJsAssets = true;
	                    //    config.assetsLoaded = false;
	                    //}
	                }
	            }
	        };

	        // now create promise and wait for everything to load
	        svc.loadWithPromise = function loadWithPromise() {
	            return $ocLazyLoad.load(svc.assetsToLoad);
	            //var cycleDuration = 100;
	            //return $q(function(resolve, reject) {
	            //    var msg = toastr.info("todo translate: loading custom input types");

	            //    return $ocLazyLoad.load(svc.assetsToLoad);

	                //var totalTime = 0;
	                //var maxTime = 2000;
	                //var finishNow = false;
	                //svc.checkloop = $interval(function() {
	                //    var allLoaded = true;
	                //    angular.forEach(svc.inputTypesOnPage, function(config, type) { 
	                //        if (!config.assetsLoaded) {
	                //            if (svc.checkDependencyArrival(config.Type)) {
	                //                config.assetsLoaded = true;
	                //            } else
	                //                allLoaded = false;
	                //        }
	                //    });

	                //    if (allLoaded) {
	                //        toastr.clear(msg);
	                //        toastr.info("todo translate: all loadded");
	                //        finishNow = true;
	                //    } else {
	                //        totalTime += cycleDuration;
	                //        if (totalTime > maxTime) {
	                //            toastr.clear(msg);
	                //            toastr.warning("todo translate - not able to load all types within " + maxTime / 1000 + " seconds, will continue without. See 2sxc.org/help?tag=custom-input", "todo translate error");
	                //            finishNow = true;
	                //        }
	                //    }

                    //    if (finishNow) {
                    //        $interval.cancel(svc.checkloop);
	                //        resolve();
                    //    }
	                //}, cycleDuration);

	            //});
	        };

	        svc.resolveSpecialPaths = function resolve(url) {
	            url = url.replace(/\[System:Path\]/i, eavConfig.getUrlPrefix("system"))
	                .replace(/\[Zone:Path\]/i, eavConfig.getUrlPrefix("zone"))
	                .replace(/\[App:Path\]/i, eavConfig.getUrlPrefix("app"));
	            return url;
	        };

	        svc.addHeadJsOrCssTag = function (url) {
	            url = url.trim();
	            if (url.indexOf(".js") > 0) {
	                var oHead = document.getElementsByTagName("HEAD").item(0);
	                var oScript = document.createElement("script");
	                oScript.type = "text/javascript";
	                oScript.src = url;
	                oHead.appendChild(oScript);
	                return true;
	            } else if (url.indexOf(".css") > 0) {
	                alert("css include not implemented yet");
	                return false;
	            }
	        };

	        svc.checkDependencyArrival = function cda(typeName) {
	            return !!formlyConfig.getType(typeName);
	        };

	        return svc;
	    });

})();