/* 
 * Field: Empty - Default: this is usually a title/group section
 */

angular.module("eavFieldTemplates")
    .config(function(formlyConfigProvider) {
        formlyConfigProvider.setType({
            name: "empty-default",
            templateUrl: "fields/empty/empty-default.html",
            wrapper: ["fieldGroup"]
        });
    });