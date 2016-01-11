(function() {

    angular.module("LazyLoadAgGrid", [
            //"SxcAdminUi",
            //"EavAdminUi",
            "oc.lazyLoad"
        ])
        //.controller("DialogHost", DialogHostController);
        .run(function ($ocLazyLoad) {
            preLoadAgGrid($ocLazyLoad);
        });

    function preLoadAgGrid($ocLazyLoad) {
        return $ocLazyLoad.load([
            "/dist/lib/ag-grid/ag-grid.min.js",
            "/dist/lib/ag-grid/ag-grid.min.css"

            //$2sxc.debug.renameScript("../sxc-develop/sxc-develop.min.js")
        ]);

    }
})();