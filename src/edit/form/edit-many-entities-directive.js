﻿
(function () {
    "use strict";

    var app = angular.module("eavEditEntity");
    
    app.directive("eavEditEntities", function () {
        return {
            templateUrl: "form/edit-many-entities.html",
            restrict: "E",
            scope: {
                itemList: "=",
                afterSaveEvent: "=",
                state: "=",
                close: "=",
                partOfPage: "=",
                publishing: "="
            },
            controller: "EditEntities",
            controllerAs: "vm"
        };
    });
})();