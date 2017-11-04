
(function() {

    /*
        shared data state across various components
    */
    angular.module("PipelineDesigner").factory("queryDef",
        function(pipelineId, pipelineService, $q, $location, toastr, $filter) {

            var queryDef = {
                id: pipelineId, // injected from URL
                dsCount: 0,
                readOnly: true,
                data: null,


                // Test wether a DataSource is persisted on the Server
                dataSourceIsPersisted: function (dataSource) {
                    return dataSource.EntityGuid.indexOf("unsaved") === -1;
                },

                addDataSource: function (partAssemblyAndType, visualDesignerData, entityGuid) {
                    if (!visualDesignerData)
                        visualDesignerData = { Top: 100, Left: 100 };

                    var newDataSource = {
                        VisualDesignerData: visualDesignerData,
                        Name: $filter("typename")(partAssemblyAndType, "className"),
                        Description: "",
                        PartAssemblyAndType: partAssemblyAndType,
                        EntityGuid: entityGuid || "unsaved" + (queryDef.dsCount + 1)
                    };
                    // Extend it with a Property to it's Definition
                    newDataSource = angular.extend(newDataSource, pipelineService.getNewDataSource(queryDef.data, newDataSource));

                    queryDef.data.DataSources.push(newDataSource);
                },

                // save the current query
                save: function(/*successHandler*/) {
                    //var deferred = $q.defer();
                    queryDef.readOnly = true;

                    return pipelineService.savePipeline(queryDef.data.Pipeline, queryDef.data.DataSources)
                        .then(function(success) {
                                // Update PipelineData with data retrieved from the Server
                                queryDef.data.Pipeline = success.Pipeline;
                                queryDef.data.TestParameters = success.TestParameters;
                                queryDef.id = success.Pipeline.EntityId;
                                $location.search("PipelineId", success.Pipeline.EntityId);
                                queryDef.readOnly = !success.Pipeline.AllowEdit;
                                queryDef.data.DataSources = success.DataSources;
                                pipelineService.postProcessDataSources(queryDef.data);

                                // communicate to the user...
                                toastr.clear();
                                toastr.success("Pipeline " + success.Pipeline.EntityId + " saved and loaded",
                                    "Saved",
                                    { autoDismiss: true });

                                // successHandler(/*success*/);
                            },
                            function(reason) {
                                toastr.error(reason, "Save Pipeline failed");
                                queryDef.readOnly = false;
                                deferred.reject();
                            });
                    //    .then(function() {
                    //        deferred.resolve();
                    //    });

                    //return deferred;
                }
            }


            return queryDef;
        });


})();