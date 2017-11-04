
(function() {

    angular.module("PipelineDesigner")
        /*
         shared data state across various components
        */
        .factory("queryDef",
        function (pipelineId, pipelineService, $q) {
            var deferred = $q.defer();

                var queryDef = {
                    id: pipelineId, // injected from URL
                    dsCount: 0,
                    readOnly: true,
                    data: null,
                    save: function (successHandler) {
                        pipelineService.savePipeline(queryDef.data.Pipeline, queryDef.data.DataSources)
                            .then(successHandler,
                                function (reason) {
                                    toastr.error(reason, "Save Pipeline failed");
                                    queryDef.readOnly = false;
                                    deferred.reject();
                                })
                            .then(function () {
                                deferred.resolve();
                            });
                    }
                }


                return queryDef;
            });


})();