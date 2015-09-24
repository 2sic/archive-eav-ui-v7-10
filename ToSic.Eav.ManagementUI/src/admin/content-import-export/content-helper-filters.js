(function () {
    angular.module("ContentHelperFilters", []);


    angular.module("ContentHelperFilters").filter("isoLangCode", function () {
        return function (str) {
            if (str.length != 5)
                return str;
            return str.substring(0, 2).toLowerCase() + "-" + str.substring(3, 5).toUpperCase();
        };
    });
}());