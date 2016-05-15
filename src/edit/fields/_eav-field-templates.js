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
    // important: order of use is backwards, so the last is around the second-last, etc.
    .constant("defaultFieldWrappers", [
        "eavLabel",
        "float-label",
        "bootstrapHasError",
        "disablevisually",
        "eavLocalization",
        "collapsible"
    ])

    .constant("fieldWrappersWithPreview", [
        "eavLabel",
        "float-label",
        "bootstrapHasError",
        "disablevisually",
        "eavLocalization",
        "preview-default",
        "collapsible"
    ])
;