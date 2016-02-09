/*jshint laxbreak:true */
(function() {

    angular.module("PipelineDesigner")
        .controller("QueryStats", function (testParams, result, $modalInstance) {
                var vm = this;
                var success = result;
                vm.testParameters = testParams.split("\n");
                vm.timeUsed = success.QueryTimer.Milliseconds;
                vm.ticksUsed = success.QueryTimer.Ticks;
                vm.result = success.Query;

                vm.sources = success.Sources;
                vm.streams = success.Streams;

                vm.connections = "todo";


                vm.close = function () {
                    $modalInstance.dismiss("cancel");
                };



            }
        );
})();