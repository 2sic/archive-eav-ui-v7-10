
(function () {
    var jsPlumb;    // needed, as we'll fill it later with the window value

    var instanceTemplate = {
        Connector: ["Bezier", { curviness: 70 }],
        HoverPaintStyle: {
            lineWidth: 4,
            strokeStyle: "#216477",
            outlineWidth: 2,
            outlineColor: "white"
        },
        PaintStyle: {
            lineWidth: 4,
            strokeStyle: "#61B7CF",
            joinstyle: "round",
            outlineColor: "white",
            outlineWidth: 2
        },
        Container: "pipelineContainer"
    };


    angular.module("PipelineDesigner").factory("plumbGui",
        function(queryDef, $filter, $log, $timeout) {

            var plumbGui = {
                dataSrcIdPrefix: "dataSource_",
                connectionsInitialized: false
                //setReadOnly: function(newRo) { readOnly = newRo; }
            };


            // the definition of source endpoints (the small blue ones)
            plumbGui.sourceEndpoint = {
                paintStyle: { fillStyle: "transparent", radius: 10, lineWidth: 0 },
                cssClass: "sourceEndpoint",
                maxConnections: -1,
                isSource: true,
                anchor: ["Continuous", { faces: ["top"] }],
                overlays: getEndpointOverlays(true, queryDef.readOnly)
            };

            // the definition of target endpoints (will appear when the user drags a connection) 
            plumbGui.targetEndpoint = {
                paintStyle: { fillStyle: "transparent", radius: 10, lineWidth: 0 },
                cssClass: "targetEndpoint",
                maxConnections: 1,
                isTarget: true,
                anchor: ["Continuous", { faces: ["bottom"] }],
                overlays: getEndpointOverlays(false, queryDef.readOnly),
                dropOptions: { hoverClass: "hover", activeClass: "active" }
            };

            // this will retrieve the dataSource info-object for a DOM element
            plumbGui.findDataSourceOfElement = function fdsog(element) {
                var guid = element.attributes.guid.value;
                var list = queryDef.data.DataSources;
                var found = $filter("filter")(list, { EntityGuid: guid })[0];
                return found;
            };

            // Sync jsPlumb Connections and StreamsOut to the pipelineData-Object
            plumbGui.pushPlumbConfigToQueryDef = function() {
                var connectionInfos = [];
                angular.forEach(plumbGui.instance.getAllConnections(),
                    function(connection) {
                        connectionInfos.push({
                            From: connection.sourceId.substr(plumbGui.dataSrcIdPrefix.length),
                            Out: connection.endpoints[0].getOverlay("endpointLabel").label,
                            To: connection.targetId.substr(plumbGui.dataSrcIdPrefix.length),
                            In: connection.endpoints[1].getOverlay("endpointLabel").label
                        });
                    });
                queryDef.data.Pipeline.StreamWiring = connectionInfos;

                var streamsOut = [];
                plumbGui.instance.selectEndpoints({ target: plumbGui.dataSrcIdPrefix + "Out" }).each(
                    function(endpoint) {
                        streamsOut.push(endpoint.getOverlay("endpointLabel").label);
                    });
                queryDef.data.Pipeline.StreamsOut = streamsOut.join(",");
            };


            // Add a jsPlumb Endpoint to an Element
            plumbGui.addEndpoint = function(element, name, isIn) {
                if (!element.length) {
                    $log.error({ message: "Element not found", selector: element.selector });
                    return;
                }
                console.log(element);

                var dataSource = plumbGui.findDataSourceOfElement(element[0]);

                var uuid = element[0].id + (isIn ? "_in_" : "_out_") + name;
                // old - using jQuery - var uuid = element.attr("id") + (isIn ? "_in_" : "_out_") + name;
                var params = {
                    uuid: uuid,
                    enabled:
                        !dataSource.ReadOnly ||
                            dataSource.EntityGuid === "Out" // Endpoints on Out-DataSource must be always enabled
                };
                var endPoint = plumbGui.instance.addEndpoint(element,
                    (isIn ? plumbGui.targetEndpoint : plumbGui.sourceEndpoint),
                    params);
                endPoint.getOverlay("endpointLabel").setLabel(name);
            };

            plumbGui.initWirings = function(streamWiring) {
                angular.forEach(streamWiring,
                    function(wire) {
                        // read connections from Pipeline
                        var sourceElementId = plumbGui.dataSrcIdPrefix + wire.From;
                        var fromUuid = sourceElementId + "_out_" + wire.Out;
                        var targetElementId = plumbGui.dataSrcIdPrefix + wire.To;
                        var toUuid = targetElementId + "_in_" + wire.In;

                        // Ensure In- and Out-Endpoint exist
                        if (!plumbGui.instance.getEndpoint(fromUuid))
                            plumbGui.addEndpoint(jsPlumb.getSelector("#" + sourceElementId), wire.Out, false);
                        if (!plumbGui.instance.getEndpoint(toUuid))
                            plumbGui.addEndpoint(jsPlumb.getSelector("#" + targetElementId), wire.In, true);

                        try {
                            plumbGui.instance.connect({ uuids: [fromUuid, toUuid] });
                        } catch (e) {
                            $log.error({ message: "Connection failed", from: fromUuid, to: toUuid });
                        }
                    });
            };

            plumbGui.putEntityCountOnConnection = function (result) {
                angular.forEach(result.Streams, function (stream) {
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
                        angular.forEach(sEndp.connections, function (connection) {
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



            plumbGui.buildInstance = function() {
                jsPlumb = window.jsPlumb; // re-set global variable, as now it's initialized & ready
                plumbGui.instance = jsPlumb.getInstance(instanceTemplate);

                // If connection on Out-DataSource was removed, remove custom Endpoint
                plumbGui.instance.bind("connectionDetached",
                    function(info) {
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
                plumbGui.instance.bind("connection", function (info) {
                    if (!plumbGui.connectionsInitialized) return;

                    // Repeat until a valid Stream-Name is provided by the user
                    var repeatCount = 0;
                    var labelPrompt,
                        targetEndpointHavingSameLabel;

                    var endpointHandling = function (endpoint) {
                        var label = endpoint.getOverlay("endpointLabel").getLabel();
                        if (label === labelPrompt &&
                            info.targetEndpoint.id !== endpoint.id &&
                            angular.element(endpoint.canvas).hasClass("targetEndpoint"))
                            targetEndpointHavingSameLabel = endpoint;
                    };

                    while (true) {
                        repeatCount++;

                        var promptMessage = "Please name the Stream";
                        if (repeatCount > 1)
                            promptMessage += ". Ensure the name is not used by any other Stream on this DataSource.";

                        var endpointLabel = info.targetEndpoint.getOverlay("endpointLabel");
                        labelPrompt = prompt(promptMessage, endpointLabel.getLabel());
                        if (labelPrompt)
                            endpointLabel.setLabel(labelPrompt);
                        else
                            continue;

                        // Check if any other Target-Endpoint has the same Stream-Name (Label)
                        var endpoints = plumbGui.instance.getEndpoints(info.target.id);
                        targetEndpointHavingSameLabel = null; // reset...

                        angular.forEach(endpoints, endpointHandling);
                        if (targetEndpointHavingSameLabel)
                            continue;

                        break;
                    }
                });
            };

            return plumbGui;

        });


    // #region jsPlumb Endpoint Definitions
    function getEndpointOverlays(isSource, readOnlyMode) {
        return [
            [
                "Label", {
                    id: "endpointLabel",
                    location: [0.5, isSource ? -0.5 : 1.5],
                    label: "Default",
                    cssClass: "noselect " + (isSource ? "endpointSourceLabel" : "endpointTargetLabel"),
                    events: {
                        dblclick: function (labelOverlay) {
                            if (readOnlyMode) return;

                            var newLabel = prompt("Rename Stream", labelOverlay.label);
                            if (newLabel)
                                labelOverlay.setLabel(newLabel);
                        }
                    }
                }
            ]
        ];
    }


})();