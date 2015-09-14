(function () {
    angular.module("Eavi18n", [
        "pascalprecht.translate",
        "EavConfiguration"
    ])

    .config(function ($translateProvider, languages, $translatePartialLoaderProvider) {
            $translateProvider
                .preferredLanguage(languages.currentLanguage.split("-")[0])
                .useSanitizeValueStrategy("escape")
                .fallbackLanguage(languages.defaultLanguage.split("-")[0])

                .useLoader('$translatePartialLoader', {
                    urlTemplate: languages.i18nRoot + '{part}-{lang}.js' // '/i18n/{part}/{lang}.json'
                });

            $translatePartialLoaderProvider.addPart("admin").addPart("edit");
            //.useStaticFilesLoader({
            //	prefix: languages.i18nRoot + "admin-",
            //    suffix: ".js"
            //});
        });
})();