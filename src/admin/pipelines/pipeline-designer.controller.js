// AngularJS Controller for the >>>> Pipeline Designer

(function() {
    /*jshint laxbreak:true */

    var editName = function(dataSource) {
        if (dataSource.ReadOnly) return;

        var newName = prompt("Rename DataSource", dataSource.Name);
        if (newName && newName.trim())
            dataSource.Name = newName.trim();
    };

    // Edit Description of a DataSource
    var editDescription = function(dataSource) {
        if (dataSource.ReadOnly) return;

        var newDescription = prompt("Edit Description", dataSource.Description);
        if (newDescription && newDescription.trim())
            dataSource.Description = newDescription.trim();
    };

    // helper method because we don't have jQuery any more to find the offset
    function getElementOffset(element) {
        var de = document.documentElement;
        var box = element.getBoundingClientRect();
        var top = box.top + window.pageYOffset - de.clientTop;
        var left = box.left + window.pageXOffset - de.clientLeft;
        return { top: top, left: left };
    }


    angular.module("PipelineDesigner")
        .controller("PipelineDesignerController",
            function(appId,
                pipelineId,
                $scope,
                pipelineService,
                $location,
                debugState,
                $timeout,
                ctrlS,
                $filter,
                toastrWithHttpErrorHandling,
                eavAdminDialogs,
                $log,
                eavConfig,
                $q,
                getUrlParamMustRefactor,
                queryDef,
                plumbGui) {


                "use strict";
                // Init
                var vm = this;
                vm.debug = debugState;
                vm.warnings = [];
                var toastr = toastrWithHttpErrorHandling;
                $scope.debug = false;
                $scope.queryDef = queryDef;
                pipelineService.setAppId(appId);

                // fully re-initialize a query (at start, or later re-load)
                vm.reInitQuery = function() {
                    // Get Data from PipelineService (Web API)
                    var waitMsg = toastr.info("This shouldn't take long", "Loading...");
                    return queryDef.loadQuery()
                        .then(function() {
                                toastr.clear(waitMsg);
                                refreshWarnings(queryDef.data, vm);
                            },
                            function(reason) {
                                toastr.error(reason, "Loading query failed");
                            });
                };

                function activate() {
                    // add ctrl+s to save
                    vm.saveShortcut = ctrlS(function() { vm.savePipeline(); }); 
                    vm.reInitQuery();
                }

                activate();


                // make a DataSource with Endpoints, called by the datasource-Directive (which uses a $timeout)
                $scope.makeDataSource = function (dataSource, element) {
                    plumbGui.makeSource(dataSource, element, $scope.dataSourceDrag);
                    queryDef.dsCount++; // unclear what this is for, probably to name/number new sources
                };



                // Initialize jsPlumb Connections once after all DataSources were created in the DOM
                $scope.$on("ngRepeatFinished",
                    function() {
                        if (plumbGui.connectionsInitialized) return;

                        plumbGui.instance.batch(plumbGui.initWirings); // suspend drawing and initialise
                        $scope.repaint(); // repaint so continuous connections are aligned correctly

                        plumbGui.connectionsInitialized = true;
                    });


                vm.addSelectedDataSource = function() {
                    var partAssemblyAndType = $scope.addDataSourceType.PartAssemblyAndType;
                    queryDef.addDataSource(partAssemblyAndType, null, null, $scope.addDataSourceType.Name);
                    $scope.addDataSourceType = null; // reset dropdown
                    $scope.savePipeline();
                };

                // Delete a DataSource
                vm.remove = function(index) {
                    var dataSource = queryDef.data.DataSources[index];
                    if (!confirm("Delete DataSource \"" + (dataSource.Name || "(unnamed)") + "\"?")) return;
                    var elementId = plumbGui.dataSrcIdPrefix + dataSource.EntityGuid;
                    plumbGui.instance.selectEndpoints({ element: elementId }).remove();
                    queryDef.data.DataSources.splice(index, 1);
                };

                // Edit name & description of a DataSource
                $scope.editName = editName;
                $scope.editDescription = editDescription;

                // Update DataSource Position on Drag
                $scope.dataSourceDrag = function(draggedWrapper) {
                    var offset = getElementOffset(draggedWrapper.el);
                    var dataSource = plumbGui.findDataSourceOfElement(draggedWrapper.el);
                    $scope.$apply(function() {
                        dataSource.VisualDesignerData.Top = Math.round(offset.top);
                        dataSource.VisualDesignerData.Left = Math.round(offset.left);
                    });
                };

                // Configure a DataSource
                $scope.configureDataSource = function(dataSource) {
                    if (dataSource.ReadOnly) return;

                    // Ensure dataSource Entity is saved
                    if (!queryDef.dataSourceIsPersisted(dataSource)) 
                        $scope.savePipeline();
                    else
                        pipelineService.editDataSourcePart(dataSource, queryDef.data.InstalledDataSources);
                };


                // Show/Hide Endpoint Overlays
                $scope.showEndpointOverlays = true;
                $scope.toggleEndpointOverlays = function() {
                    $scope.showEndpointOverlays = !$scope.showEndpointOverlays;

                    var endpoints = plumbGui.instance.selectEndpoints();
                    if ($scope.showEndpointOverlays)
                        endpoints.showOverlays();
                    else
                        endpoints.hideOverlays();
                };

                // Edit Pipeline Entity
                $scope.editPipelineEntity = function() {
                    // save Pipeline, then open Edit Dialog
                    $scope.savePipeline().then(function() {
                        vm.saveShortcut.unbind();// disable ctrl+s
                        eavAdminDialogs.openEditItems([{ EntityId: queryDef.id }],
                            function (success) {
                                console.log("testing", success);
                                vm.reInitQuery()
                                    .then(resetPlumbAndWarnings) // reset jsplumb
                                    .then(vm.saveShortcut.rebind);// re-enable ctrl+s
                            });

                    });
                };

                // #region Save Pipeline
                // Handle Pipeline Saved, success contains the updated Pipeline Data
                function resetPlumbAndWarnings(promise) {
                    // Reset jsPlumb, re-Init Connections
                    plumbGui.instance.reset();
                    plumbGui.connectionsInitialized = false;
                    refreshWarnings(queryDef.data, vm);
                    return promise;
                }


                // Save Pipeline
                // returns a Promise about the saving state
                vm.savePipeline = $scope.savePipeline = function savePipeline() {
                    toastr.info("This shouldn't take long", "Saving...");
                    plumbGui.pushPlumbConfigToQueryDef(plumbGui.instance);
                    return queryDef.save()
                        .then(resetPlumbAndWarnings);
                };


                // #endregion

                // Repaint jsPlumb
                $scope.repaint = function() { plumbGui.instance.repaintEverything(); };

                // Show/Hide Debug info
                $scope.toogleDebug = function() { $scope.debug = !$scope.debug; };

                // check if there are special warnings the developer should know
                // typically when the test-module-id is different from the one we're currently
                // working on, or if no test-module-id is provided
                // note: this should actually be external code, and injected later on
                // reason is that it's actually testing for a 2sxc-variable mid
                function refreshWarnings(pipelineData, vm) {
                    var regex = /^\[module:moduleid\]=([0-9]*)$/gmi; // capture the mod-id
                    var testParams, testMid;
                    var warnings = vm.warnings = [];
                    try { // catch various not-initialized errors
                        testParams = pipelineData.Pipeline.TestParameters;
                        var matches = regex.exec(testParams);
                        if (!matches || matches.length === 0)
                            warnings.push(
                                "Your test values has no moduleid specified. You probably want to check your test-values.");
                        testMid = matches[1];
                        var urlMid = getUrlParamMustRefactor("mid");
                        if (testMid !== urlMid)
                            warnings.push("Your test moduleid (" +
                                testMid +
                                ") is different from the current moduleid (" +
                                urlMid +
                                "). You probably want to check your test-values.");
                    } catch (ex) { }
                }

                // Query the Pipeline
                $scope.queryPipeline = function(saveFirst) {
                    function runQuery() {
                        // Query pipelineService for the result...
                        toastr.info("Running Query ...");

                        pipelineService.queryPipeline(queryDef.id).then(function(success) {
                                // Show Result in a UI-Dialog
                                toastr.clear();

                                var resolve = eavAdminDialogs.CreateResolve({
                                    testParams: queryDef.data.Pipeline.TestParameters,
                                    result: success
                                });
                                eavAdminDialogs.OpenModal("pipelines/query-stats.html",
                                    "QueryStats as vm",
                                    "lg",
                                    resolve);

                                $timeout(function() {
                                    plumbGui.putEntityCountOnConnection(success);
                                });
                                $log.debug(success);
                            },
                            function(reason) {
                                toastr.error(reason, "Query failed");
                            });
                    }

                    // Ensure the Pipeline is saved
                    if (saveFirst)
                        $scope.savePipeline().then(runQuery);
                    else
                        runQuery();
                };

                vm.typeInfo = queryDef.dsTypeInfo;
            });
})();