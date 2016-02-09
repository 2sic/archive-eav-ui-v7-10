// Init the main eav services module
angular.module("EavServices", [
    "ng",                   // Angular for $http etc.
    "EavConfiguration",     // global configuration
    "pascalprecht.translate",
    "ngResource",           // only needed for the pipeline-service, maybe not necessary any more?
    "ngAnimate",
    "toastr"
]);