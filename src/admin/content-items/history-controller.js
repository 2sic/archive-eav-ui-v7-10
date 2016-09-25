(function () { // TN: this is a helper construct, research iife or read https://github.com/johnpapa/angularjs-styleguide#iife

    angular.module("HistoryApp", [
        "EavServices",
        "EavConfiguration",
        "eavTemplates",
    ])
        .controller("History", HistoryController)
        .controller("HistoryDetails", HistoryDetailsController)
        ;

    function HistoryController(appId, entityId, historySvc, $uibModalInstance, $uibModal) {
        var vm = this;
        var svc = historySvc(appId, entityId);
        vm.entityId = entityId;
        vm.items = svc.liveList();

        vm.close = function () { $uibModalInstance.dismiss("cancel"); };

        vm.details = function(item) {
            $uibModal.open({
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

    function HistoryDetailsController(changeId, dataSvc, $uibModalInstance) {
        var vm = this;
        alert("not implemented yet");
        var svc = dataSvc;

        svc.getVersionDetails(changeId).then(function(result) {
            alert(result.data);
            vm.items = result.data;
        });
        // vm.items = svc.liveList();

        vm.close = function () { $uibModalInstance.dismiss("cancel"); };
    }
} ());