/* 
 * Custom Fields skelleton to allow later, lazy loaded fields
 */
(function() {
    "use strict";
    var module = angular.module("eavCustomFields", ["oc.lazyLoad"])

        // here's a special trick from http://slides.com/gruizdevilla/late-registering-and-lazy-load-in-angularjs-en#/5
        .config(
            function(formlyConfigProvider, defaultFieldWrappers,
                $controllerProvider,
                $compileProvider,
                $filterProvider,
                $provide
            ) {
                module.IsLazyLoader = true;
                module.formlyConfigProvide = formlyConfigProvider;
                module.defaultFieldWrappers = defaultFieldWrappers;
                 
                 
                //module.controller = $controllerProvider.register;
                //module.directive = $compileProvider.directive;
                //module.filter = $filterProvider.register;
                //module.factory = $provide.factory;
                //module.service = $provide.service;

                //    // added by 2dm
                //module.config = $provide.config;
            })
    .config(function ($ocLazyLoadProvider) {
        $ocLazyLoadProvider.config({
            debug: true
        });
    });

    module.run(function(formlyConfig) {
        module.formlyConfig = formlyConfig;
    });
})();