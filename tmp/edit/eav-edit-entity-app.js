/* Main object with dependencies, used in wrappers and other places */
(function () {
	"use strict";

    angular.module("eavEditEntity", [
        "formly",
        "ui.bootstrap",
        "uiSwitch",
        "toastr",
        "ngAnimate",
        "EavServices",
        "eavEditTemplates",
        "eavFieldTemplates",
        "eavCustomFields",

        // new...
        "oc.lazyLoad"
    ]);



})();