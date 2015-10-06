/* 
 * Field: Boolean - Default
 */

angular.module("eavFieldTemplates")
    .config(function(formlyConfigProvider) {
        formlyConfigProvider.setType({
            name: "boolean-default",
            template: "<div class=\"checkbox\">\n\t<label>\n\t\t<input type=\"checkbox\"\n           class=\"formly-field-checkbox\"\n\t\t       ng-model=\"value.Value\">\n\t\t{{to.label}}\n\t\t{{to.required ? '*' : ''}}\n\t</label>\n</div>\n",
            wrapper: ["eavLabel", "bootstrapHasError", "eavLocalization"]
        });
    });