/* 
 * Field-Templates app initializer
 */

angular.module("eavFieldTemplates",
    [
        "formly",
        "formlyBootstrap",
        "ui.bootstrap",
        "eavLocalization",
        "eavEditTemplates",
        "ui.tree"
    ]
)
    .constant("defaultFieldWrappers", ["collapsible", "eavLabel", "bootstrapHasError", "eavLocalization"])
;