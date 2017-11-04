// AngularJS Controller for the >>>> Pipeline Designer

(function () {
    /*jshint laxbreak:true */

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

                    // If connection on Out-DataSource was removed, remove custom Endpoint
                    plumbGui.instance.bind("connectionDetached", function(info) {
                        if (info.targetId === plumbGui.dataSrcIdPrefix + "Out") {
                            var element = angular.element(info.target);
                            var fixedEndpoints = plumbGui.findDataSourceOfElement(element).dataSource.Definition().In;
                            var label = info.targetEndpoint.getOverlay("endpointLabel").label;
                            if (fixedEndpoints.indexOf(label) === -1) {
                                $timeout(function() {
                                    plumbGui.instance.deleteEndpoint(info.targetEndpoint);
                                });
                            }
                        }
                    });

                    // If a new connection is created, ask for a name of the In-Stream
                    plumbGui.instance.bind("connection", function(info) {
                        if (!$scope.connectionsInitialized) return;

                        // Repeat until a valid Stream-Name is provided by the user
                        var repeatCount = 0;
                        var endpointHandling = function(endpoint) {
                            var label = endpoint.getOverlay("endpointLabel").getLabel();
                            if (label === labelPrompt && info.targetEndpoint.id !== endpoint.id && angular.element(endpoint.canvas).hasClass("targetEndpoint"))
                                targetEndpointHavingSameLabel = endpoint;
                        };
                        while (true) {
                            repeatCount++;

                            var promptMessage = "Please name the Stream";
                            if (repeatCount > 1)
                                promptMessage += ". Ensure the name is not used by any other Stream on this DataSource.";

                            var endpointLabel = info.targetEndpoint.getOverlay("endpointLabel");
                            var labelPrompt = prompt(promptMessage, endpointLabel.getLabel());
                            if (labelPrompt)
                                endpointLabel.setLabel(labelPrompt);
                            else
                                continue;

                            // Check if any other Target-Endpoint has the same Stream-Name (Label)
                            var endpoints = plumbGui.instance.getEndpoints(info.target.id);
                            var targetEndpointHavingSameLabel = null;

                            angular.forEach(endpoints, endpointHandling);
                            if (targetEndpointHavingSameLabel)
                                continue;

                            break;
                        }
                    });
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
                $scope.connectionsInitialized = false;
                $scope.$on("ngRepeatFinished", function() {
                    if ($scope.connectionsInitialized) return;

                    // suspend drawing and initialise
                    plumbGui.instance.batch(function() {
                        plumbGui.initWirings(queryDef.data.Pipeline.StreamWiring);
                    });
                    $scope.repaint(); // repaint so continuous connections are aligned correctly

                    $scope.connectionsInitialized = true;
                });


                // Init a new Pipeline with DataSources and Wirings from Configuration
                var initNewPipeline = function () {
                    var templateForNew = eavConfig.pipelineDesigner.defaultPipeline.dataSources;
                    angular.forEach(templateForNew, function(dataSource) {
                        $scope.addDataSource(dataSource.partAssemblyAndType, dataSource.visualDesignerData, false, dataSource.entityGuid);
                    });

                    // Wait until all DataSources were created
                    var initWiringsListener = $scope.$on("ngRepeatFinished", function() {
                        $scope.connectionsInitialized = false;
                        plumbGui.initWirings(eavConfig.pipelineDesigner.defaultPipeline.streamWiring);
                        $scope.connectionsInitialized = true;

                        initWiringsListener(); // unbind the Listener
                    });
                };

                // Add new DataSource
                $scope.addDataSource = function(partAssemblyAndType, visualDesignerData, autoSave, entityGuid) {
                    if (!partAssemblyAndType) {
                        partAssemblyAndType = $scope.addDataSourceType.PartAssemblyAndType;
                        $scope.addDataSourceType = null;
                    }
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

                    if (autoSave !== false)
                        $scope.savePipeline();
                };

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
                    if (!dataSourceIsPersisted(dataSource)) {
                        $scope.savePipeline();
                        return;
                    }

                    pipelineService.editDataSourcePart(dataSource, queryDef.data.InstalledDataSources);

                };

                // Test wether a DataSource is persisted on the Server
                var dataSourceIsPersisted = function(dataSource) {
                    return dataSource.EntityGuid.indexOf("unsaved") === -1;
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
                                .then(pipelineSaved)
                                .then(vm.saveShortcut.rebind);
                        });

                    });
                };

                // #region Save Pipeline
                // Save Pipeline
                // returns a Promise about the saving state
                vm.savePipeline = $scope.savePipeline = function (successHandler) {
                    var waitMsg = toastr.info("This shouldn't take long", "Saving...");
                    queryDef.readOnly = true;

                    plumbGui.pushPlumbConfigToQueryDef(plumbGui.instance);

                    var deferred = $q.defer();

                    if (typeof successHandler === "undefined") // set default success Handler
                        successHandler = pipelineSaved;

                    queryDef.save(successHandler);

                    return deferred.promise;
                };

                // Handle Pipeline Saved, success contains the updated Pipeline Data
                var pipelineSaved = function(success) {
                    // Update PipelineData with data retrieved from the Server
                    queryDef.data.Pipeline = success.Pipeline;
                    queryDef.data.TestParameters = success.TestParameters;
                    queryDef.id = success.Pipeline.EntityId;
                    $location.search("PipelineId", success.Pipeline.EntityId);
                    queryDef.readOnly = !success.Pipeline.AllowEdit;
                    queryDef.data.DataSources = success.DataSources;
                    pipelineService.postProcessDataSources(queryDef.data);

                    toastr.clear();
                    toastr.success("Pipeline " + success.Pipeline.EntityId + " saved and loaded", "Saved", { autoDismiss: true });

                    // Reset jsPlumb, re-Init Connections
                    plumbGui.instance.reset();
                    $scope.connectionsInitialized = false;
                    refreshWarnings(queryDef.data, vm);
                };

                // #endregion

                // Repaint jsPlumb
                $scope.repaint = function() {
                    plumbGui.instance.repaintEverything();
                };

                // Show/Hide Debug info
                $scope.toogleDebug = function() {
                    $scope.debug = !$scope.debug;
                };

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
                                showEntityCountOnStreams(success);
                            });
                            $log.debug(success);
                        }, function(reason) {
                            toastr.error(reason, "Query failed");
                        });
                    };


                    var showEntityCountOnStreams = function(result) {
                        angular.forEach(result.Streams, function(stream) {
                            // Find jsPlumb Connection for the current Stream
                            var sourceElementId = plumbGui.dataSrcIdPrefix + stream.Source;
                            var targetElementId = plumbGui.dataSrcIdPrefix + stream.Target;
                            if (stream.Target === "00000000-0000-0000-0000-000000000000")
                                targetElementId = plumbGui.dataSrcIdPrefix + "Out";

                            var fromUuid = sourceElementId + "_out_" + stream.SourceOut;
                            var toUuid = targetElementId + "_in_" + stream.TargetIn;

                            var sEndp = plumbGui.instance.getEndpoint(fromUuid);
                            var streamFound = false;
                            if (sEndp) {
                                angular.forEach(sEndp.connections, function(connection) {
                                    if (connection.endpoints[1].getUuid() === toUuid) {
                                        // when connection found, update it's label with the Entities-Count
                                        connection.setLabel({
                                            label: stream.Count.toString(),
                                            cssClass: "streamEntitiesCount"
                                        });
                                        streamFound = true;
                                        return;
                                    }
                                });
                            }

                            if (!streamFound)
                                $log.error("Stream not found", stream, sEndp);
                        });
                    };

                    // Ensure the Pipeline is saved
                    if (saveFirst)
                        $scope.savePipeline().then(query);
                    else
                        query();
                };

        });


    // temp: extracted pure functions

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
})();