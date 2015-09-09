(function () { // TN: this is a helper construct, research iife or read https://github.com/johnpapa/angularjs-styleguide#iife

    angular.module("HistoryApp", [
        "ContentItemsAppServices",
        "eavGlobalConfigurationProvider",
        "HistoryServices",
        "eavTemplates",
    "Eavi18n"
    ])//, 'EavAdminUi'])
        .controller("History", HistoryController)
        .controller("HistoryDetails", HistoryDetailsController)
        ;

    function HistoryController(appId, entityId, historySvc, $modalInstance, $modal) {
        var vm = this;
        var svc = historySvc(appId, entityId);
        vm.entityId = entityId;
        vm.items = svc.liveList();

        vm.close = function () { $modalInstance.dismiss("cancel"); };

        vm.details = function(item) {
            $modal.open({
                animation: true,
                templateUrl: "content-items/history-details.html",
                controller: "HistoryDetails",
                controllerAs: "vm",
                resolve: {
                    changeId: function() { return item.ChangeId; },
                    dataSvc: function() { return svc; }
                }
            });
        };
    }

    function HistoryDetailsController(changeId, dataSvc, $modalInstance) {
        var vm = this;
        alert("not implemented yet");
        var svc = dataSvc;

        svc.getVersionDetails(changeId).then(function(result) {
            alert(result.data);
            vm.items = result.data;
        });
        // vm.items = svc.liveList();

        vm.close = function () { $modalInstance.dismiss("cancel"); };
    }
} ());