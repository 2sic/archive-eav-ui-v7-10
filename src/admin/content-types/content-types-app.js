// This is the main declaration for the app ContentTypesApp
(function () {

    angular.module("ContentTypesApp", [
        "EavServices",
        "EavAdminUi",
        "EavDirectives"
        ])
        .constant("license", {
            createdBy: "2sic internet solutions",
            license: "MIT"
            })             
    ;  
}()); 