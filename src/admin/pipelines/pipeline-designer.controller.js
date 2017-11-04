// AngularJS Controller for the >>>> Pipeline Designer

(function () {
    /*jshint laxbreak:true */

    var editName = function (dataSource) {
        if (dataSource.ReadOnly) return;

        var newName = prompt("Rename DataSource", dataSource.Name);
        if (newName && newName.trim())
            dataSource.Name = newName.trim();
    };

    // Edit Description of a DataSource
    var editDescription = function (dataSource) {
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
        function (appId, pipelineId, $scope, pipelineService, $location, debugState, $timeout,
            ctrlS, $filter, toastrWithHttpErrorHandling, eavAdminDialogs, $log,
            eavConfig, $q, getUrlParamMustRefactor, queryDef, plumbGui) {
                "use strict";
                var vm = this;
                // Init
                vm.debug = debugState;
                vm.warnings = [];
                var toastr = toastrWithHttpErrorHandling;
                var waitMsg = toastr.info("This shouldn't take long", "Please wait...");

                function activate() {
                    // add ctrl+s to save
                    vm.saveShortcut = ctrlS(function() { vm.savePipeline(); });
                }
                activate();

                $scope.debug = false;

                $scope.queryDef = queryDef;

                pipelineService.setAppId(appId);


                // Get Data from PipelineService (Web API)
                pipelineService.getPipeline(queryDef.id)
                    .then(function(success) {
                        queryDef.data = success;

                        // If a new (empty) Pipeline is made, init new Pipeline
                        if (!queryDef.id || queryDef.data.DataSources.length === 1) {
                            queryDef.readOnly = false;
                            initNewPipeline();
                        } else {
                            // if read only, show message
                            queryDef.readOnly = !success.Pipeline.AllowEdit;
                            toastr.clear(waitMsg);
                            toastr.info(queryDef.readOnly
                                ? "This pipeline is read only"
                                : "You can now design the Pipeline. \nVisit 2sxc.org/help for more.",
                                "Ready", { autoDismiss: true });
                        }


                        refreshWarnings(queryDef.data, vm);
                    }, function(reason) {
                        toastr.error(reason, "Loading Pipeline failed");
                    });


                // init new jsPlumb Instance
                jsPlumb.ready(function () {
                    plumbGui.buildInstance();// can't do this before jsplumb is ready...
                });



                // make a DataSource with Endpoints, called by the datasource-Directive (which uses a $timeout)
                $scope.makeDataSource = function(dataSource, element) {
                    // suspend drawing and initialise
                	plumbGui.instance.batch(function () {

                		// make DataSources draggable. Must happen before makeSource()!
                		if (!queryDef.readOnly) {
                			plumbGui.instance.draggable(element, {
                				grid: [20, 20],
                				drag: $scope.dataSourceDrag
                			});
                		}

                        // Add Out- and In-Endpoints from Definition
                        var dataSourceDefinition = dataSource.Definition();
                        if (dataSourceDefinition !== null) {
                            // Add Out-Endpoints
                            angular.forEach(dataSourceDefinition.Out, function(name) {
                                plumbGui.addEndpoint(element, name, false);
                            });
                            // Add In-Endpoints
                            angular.forEach(dataSourceDefinition.In, function(name) {
                                plumbGui.addEndpoint(element, name, true);
                            });
                            // make the DataSource a Target for new Endpoints (if .In is an Array)
                            if (dataSourceDefinition.In) {
                                var targetEndpointUnlimited = plumbGui.targetEndpoint;
                                targetEndpointUnlimited.maxConnections = -1;
                                plumbGui.instance.makeTarget(element, targetEndpointUnlimited);
                            }

                            plumbGui.instance.makeSource(element, plumbGui.sourceEndpoint, { filter: ".ep .glyphicon" });
                        }

                        
                    });

                    queryDef.dsCount++;
                };

                // Initialize jsPlumb Connections once after all DataSources were created in the DOM
                $scope.$on("ngRepeatFinished", function() {
                    if (plumbGui.connectionsInitialized) return;

                    // suspend drawing and initialise
                    plumbGui.instance.batch(function() {
                        plumbGui.initWirings(queryDef.data.Pipeline.StreamWiring);
                    });
                    $scope.repaint(); // repaint so continuous connections are aligned correctly

                    plumbGui.connectionsInitialized = true;
                });


                // Init a new Pipeline with DataSources and Wirings from Configuration
                var initNewPipeline = function () {
                    var templateForNew = eavConfig.pipelineDesigner.defaultPipeline.dataSources;
                    angular.forEach(templateForNew, function(dataSource) {
                        queryDef.addDataSource(dataSource.partAssemblyAndType, dataSource.visualDesignerData, dataSource.entityGuid);
                    });

                    // Wait until all DataSources were created
                    var initWiringsListener = $scope.$on("ngRepeatFinished", function() {
                        plumbGui.connectionsInitialized = false;
                        plumbGui.initWirings(eavConfig.pipelineDesigner.defaultPipeline.streamWiring);
                        plumbGui.connectionsInitialized = true;

                        initWiringsListener(); // unbind the Listener
                    });
                };



                vm.addSelectedDataSource = function() {
                    var partAssemblyAndType = $scope.addDataSourceType.PartAssemblyAndType;
                    $scope.addDataSourceType = null; // reset dropdown
                    queryDef.addDataSource(partAssemblyAndType);
                    $scope.savePipeline();

                }

                // Add new DataSource
                //$scope.addDataSource = function(partAssemblyAndType, visualDesignerData, entityGuid) {
                //    if (!visualDesignerData)
                //        visualDesignerData = { Top: 100, Left: 100 };

                //    var newDataSource = {
                //        VisualDesignerData: visualDesignerData,
                //        Name: $filter("typename")(partAssemblyAndType, "className"),
                //        Description: "",
                //        PartAssemblyAndType: partAssemblyAndType,
                //        EntityGuid: entityGuid || "unsaved" + (queryDef.dsCount + 1)
                //    };
                //    // Extend it with a Property to it's Definition
                //    newDataSource = angular.extend(newDataSource, pipelineService.getNewDataSource(queryDef.data, newDataSource));

                //    queryDef.data.DataSources.push(newDataSource);
                //};

                // Delete a DataSource
                $scope.remove = function(index) {
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
                    if (!queryDef.dataSourceIsPersisted(dataSource)) {
                        $scope.savePipeline();
                        return;
                    }

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
                        vm.saveShortcut.unbind();
                        eavAdminDialogs.openEditItems([{ EntityId: queryDef.id }], function() {
                            pipelineService.getPipeline(queryDef.id)
                                .then(resetPlumbAndWarnings)
                                .then(vm.saveShortcut.rebind);
                        });

                    });
                };

                // #region Save Pipeline
                // Save Pipeline
                // returns a Promise about the saving state
                vm.savePipeline = $scope.savePipeline = function savePipeline() {
                    var waitMsg = toastr.info("This shouldn't take long", "Saving...");

                    plumbGui.pushPlumbConfigToQueryDef(plumbGui.instance);

                    return queryDef.save(/*resetPlumbAndWarnings*/).then(resetPlumbAndWarnings);
                };

                // Handle Pipeline Saved, success contains the updated Pipeline Data
                var resetPlumbAndWarnings = function(promise) {
                    // Reset jsPlumb, re-Init Connections
                    plumbGui.instance.reset();
                    plumbGui.connectionsInitialized = false;
                    refreshWarnings(queryDef.data, vm);
                    return promise;
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
                            warnings.push("Your test values has no moduleid specified. You probably want to check your test-values.");
                        testMid = matches[1];
                        var urlMid = getUrlParamMustRefactor("mid");
                        if (testMid !== urlMid)
                            warnings.push("Your test moduleid (" + testMid + ") is different from the current moduleid (" + urlMid + "). You probably want to check your test-values.");
                    }
                    catch(ex) {}
                }

                // Query the Pipeline
                $scope.queryPipeline = function (saveFirst) {
                    var query = function() {
                        // Query pipelineService for the result...
                        toastr.info("Running Query ...");

                        pipelineService.queryPipeline(queryDef.id).then(function(success) {
                            // Show Result in a UI-Dialog
                            toastr.clear();

                            var resolve = eavAdminDialogs.CreateResolve({ testParams: queryDef.data.Pipeline.TestParameters, result: success });
                            eavAdminDialogs.OpenModal("pipelines/query-stats.html", "QueryStats as vm", "lg", resolve);

                            $timeout(function() {
                                plumbGui.putEntityCountOnConnection(success);
                            });
                            $log.debug(success);
                        }, function(reason) {
                            toastr.error(reason, "Query failed");
                        });
                    };


                    // Ensure the Pipeline is saved
                    if (saveFirst)
                        $scope.savePipeline().then(query);
                    else
                        query();
                };

        });
})();