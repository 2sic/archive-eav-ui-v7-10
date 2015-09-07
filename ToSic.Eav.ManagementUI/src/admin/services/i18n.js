(function () {
    angular.module("Eavi18n", [
        "pascalprecht.translate",
        "eavGlobalConfigurationProvider"
    ])

    .config(function ($translateProvider, languages) {
        $translateProvider
          .preferredLanguage(languages.preferredLanguage())
          .useSanitizeValueStrategy("escape")
          .fallbackLanguage(languages.fallbackLanguage())
          .useStaticFilesLoader({
              prefix: languages.i18nRoot() + "eav-core-",
              suffix: ".json"
          });
    });
})();