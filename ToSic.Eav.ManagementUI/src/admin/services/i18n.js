/* The main component for language inclusion
 * Ensure the dependencies work, that the url-schema is prepared etc.
 * 
 */

(function () {
    angular.module("EavServices")

    .config(function ($translateProvider, languages, $translatePartialLoaderProvider) {
            $translateProvider
                .preferredLanguage(languages.currentLanguage.split("-")[0])
                .useSanitizeValueStrategy("escapeParameters")   // this is very important to allow html in the JSON files
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
    })

    .factory("translate", function($filter) {
                return $filter("translate");
            })
    ;
})();