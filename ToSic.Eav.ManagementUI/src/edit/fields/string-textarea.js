/* 
 * Field: String - Textarea
 */

angular.module("eavFieldTemplates")
    .config(function (formlyConfigProvider, defaultFieldWrappers) {

        formlyConfigProvider.setType({
            name: "string-textarea",
            template: "<textarea class=\"form-control\" ng-model=\"value.Value\"></textarea>",
            wrapper: defaultFieldWrappers,
            defaultOptions: {
                ngModelAttrs: {
                    '{{to.settings.String.RowCount}}': { value: "rows" },
                    cols: { attribute: "cols" }
                }
            }
        });


    });