(function () {
    angular.module("Eavi18n", [
        "pascalprecht.translate",
        "EavConfiguration"
    ])

    .config(function ($translateProvider, languages, $translatePartialLoaderProvider) {
            $translateProvider
                .preferredLanguage(languages.currentLanguage.split("-")[0])
                .useSanitizeValueStrategy("escapeParameters")//"escape")
                .fallbackLanguage(languages.defaultLanguage.split("-")[0])

                .useLoader("$translatePartialLoader", {
                    urlTemplate: languages.i18nRoot + "{part}-{lang}.js" 
                })
                .useLoaderCache(true);              // should cache json
            $translatePartialLoaderProvider         // these parts are always required
                .addPart("admin")
                .addPart("edit");   
    })

    // ensure that adding parts will load the missing files
    .run(function ($rootScope, $translate) {
        $rootScope.$on("$translatePartialLoaderStructureChanged", function () {
            $translate.refresh();
        });
    });
})();