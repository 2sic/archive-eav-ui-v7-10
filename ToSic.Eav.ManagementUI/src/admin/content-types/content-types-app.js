// This is the main declaration for the app ContentTypesApp
(function () {

    angular.module("ContentTypesApp", [
        "ContentTypeServices",
        "ContentTypeFieldServices",
        "EavAdminUi",
        "Eavi18n",
        "EavDirectives"
        ])
        .constant("license", {
            createdBy: "2sic internet solutions",
            license: "MIT"
            })             
    ;
}());