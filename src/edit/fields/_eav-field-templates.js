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
        "ui.tree",

        // testing for the entity-picker dropdown
        "ui.select",
        "ngSanitize"
    ]
)
    // important: order of use is backwards, so the last is around the second-last, etc.
    .constant("defaultFieldWrappers", [
        "eavLabel",
        "float-label",
        "bootstrapHasError",
        "disablevisually",
        "eavLocalization",
        "responsive",
        "collapsible",
        "hiddenIfNeeded"
    ])

    .constant("fieldWrappersWithPreview", [
        "eavLabel",
        "float-label",
        "bootstrapHasError",
        "disablevisually",
        "eavLocalization",
        "preview-default",
        "responsive",
        "collapsible",
        "hiddenIfNeeded"
    ])

    .constant("defaultFieldWrappersNoFloat", [
        "eavLabel",
        //"float-label",
        "bootstrapHasError",
        "disablevisually",
        "eavLocalization",
        //"preview-default",
        "responsive",
        "collapsible",
        "hiddenIfNeeded"
    ])

    .constant("fieldWrappersNoLabel", [
        //"eavLabel",
        //"float-label",
        "bootstrapHasError",
        "disablevisually",
        "eavLocalization",
        //"preview-default",
        "responsive",
        "no-label-space",
        "collapsible",
        "hiddenIfNeeded"
    ])
;