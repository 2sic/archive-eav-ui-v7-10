
(function () {
    var jsPlumb;    // needed, as we'll fill it later with the window value

    var linePaintDefault = {
        lineWidth: 4,
        strokeStyle: '#61B7CF',
        joinstyle: 'round',
        outlineColor: 'white',
        outlineWidth: 2
    };
    var lineCount = 0,
        lineColors = [
            '#009688', '#00bcd4', '#3f51b5', '#9c27b0', '#e91e63',
            '#db4437', '#ff9800', '#60a917', '#60a917', '#008a00',
            '#00aba9', '#1ba1e2', '#0050ef', '#6a00ff', '#aa00ff',
            '#f472d0', '#d80073', '#a20025', '#e51400', '#fa6800',
            '#f0a30a', '#e3c800', '#825a2c', '#6d8764', '#647687',
            '#76608a', '#a0522d'
        ],
        uuidColorMap = {},
        maxCols = lineColors.length - 1;
    function resetLineCount() { lineCount = 0; }
    function nextLinePaintStyle(uuid) {
        return uuidColorMap[uuid]
            || (uuidColorMap[uuid] = Object.assign({}, linePaintDefault, { strokeStyle: lineColors[lineCount++ % maxCols] }));
    }

    console.log(nextLinePaintStyle());
    
    var instanceTemplate = {
        Connector: ['Bezier', { curviness: 70 }],
        HoverPaintStyle: {
            lineWidth: 4,
            strokeStyle: '#216477',
            outlineWidth: 2,
            outlineColor: 'white'
        },
        PaintStyle: nextLinePaintStyle("dummy"),// linePaintDefault,
        Container: 'pipelineContainer'
    };



    angular.module('PipelineDesigner').factory('plumbGui',
        function(queryDef, $filter, $log, $timeout) {

            var plumbGui = {
                dataSrcIdPrefix: 'dataSource_',
                connectionsInitialized: false
            };


            // the definition of source endpoints (the small blue ones)
            plumbGui.buildSourceEndpoint = function() {
                return {
                    paintStyle: { fillStyle: 'transparent', radius: 10, lineWidth: 0 },
                    cssClass: 'sourceEndpoint',
                    maxConnections: -1,
                    isSource: true,
                    anchor: ['Continuous', { faces: ['top'] }],
                    overlays: getEndpointOverlays(true, queryDef.readOnly)
                };
            };

            // the definition of target endpoints (will appear when the user drags a connection) 
            plumbGui.buildTargetEndpoint = function() {
                return {
                    paintStyle: { fillStyle: 'transparent', radius: 10, lineWidth: 0 },
                    cssClass: 'targetEndpoint',
                    maxConnections: 1,
                    isTarget: true,
                    anchor: ['Continuous', { faces: ['bottom'] }],
                    overlays: getEndpointOverlays(false, queryDef.readOnly),
                    dropOptions: { hoverClass: 'hover', activeClass: 'active' }
                };
            };

            // this will retrieve the dataSource info-object for a DOM element
            plumbGui.findDataSourceOfElement = function fdsog(element) {
                var guid = element.attributes.guid.value;
                var list = queryDef.data.DataSources;
                var found = $filter('filter')(list, { EntityGuid: guid })[0];
                return found;
            };

            // Sync jsPlumb Connections and StreamsOut to the pipelineData-Object
            plumbGui.pushPlumbConfigToQueryDef = function() {
                var connectionInfos = [];
                angular.forEach(plumbGui.instance.getAllConnections(),
                    function(connection) {
                        connectionInfos.push({
                            From: connection.sourceId.substr(plumbGui.dataSrcIdPrefix.length),
                            Out: connection.endpoints[0].getOverlay('endpointLabel').label,
                            To: connection.targetId.substr(plumbGui.dataSrcIdPrefix.length),
                            In: connection.endpoints[1].getOverlay('endpointLabel').label
                        });
                    });
                queryDef.data.Pipeline.StreamWiring = connectionInfos;

                var streamsOut = [];
                plumbGui.instance.selectEndpoints({ target: plumbGui.dataSrcIdPrefix + 'Out' }).each(
                    function(endpoint) {
                        streamsOut.push(endpoint.getOverlay('endpointLabel').label);
                    });
                queryDef.data.Pipeline.StreamsOut = streamsOut.join(',');
            };


            // Add a jsPlumb Endpoint to an Element
            plumbGui.addEndpoint = function(element, name, isIn) {
                if (!element.length) {
                    $log.error({ message: 'Element not found', selector: element.selector });
                    return;
                }
                //console.log(element);

                var dataSource = plumbGui.findDataSourceOfElement(element[0]);

                var uuid = element[0].id + (isIn ? '_in_' : '_out_') + name;
                var params = {
                    uuid: uuid,
                    enabled:
                        !dataSource.ReadOnly ||
                            dataSource.EntityGuid === 'Out' // Endpoints on Out-DataSource must be always enabled
                };
                var endPoint = plumbGui.instance.addEndpoint(element,
                    (isIn ? plumbGui.buildTargetEndpoint() : plumbGui.buildSourceEndpoint()),
                    params);
                endPoint.getOverlay('endpointLabel').setLabel(name);
            };

            plumbGui.initWirings = function () {
                angular.forEach(queryDef.data.Pipeline.StreamWiring,
                    function(wire) {
                        // read connections from Pipeline
                        var sourceElementId = plumbGui.dataSrcIdPrefix + wire.From;
                        var fromUuid = sourceElementId + '_out_' + wire.Out;
                        var targetElementId = plumbGui.dataSrcIdPrefix + wire.To;
                        var toUuid = targetElementId + '_in_' + wire.In;

                        // Ensure In- and Out-Endpoint exist
                        if (!plumbGui.instance.getEndpoint(fromUuid))
                            plumbGui.addEndpoint(jsPlumb.getSelector('#' + sourceElementId), wire.Out, false);
                        if (!plumbGui.instance.getEndpoint(toUuid))
                            plumbGui.addEndpoint(jsPlumb.getSelector('#' + targetElementId), wire.In, true);

                        try {
                            plumbGui.instance.connect({
                                uuids: [fromUuid, toUuid],
                                //PaintStyle: nextLinePaintStyle(),// { strokeWidth: 15, stroke: 'rgba(0, 243,230,18)' }
                                paintStyle: nextLinePaintStyle(fromUuid)
                            });
                        } catch (e) {
                            $log.error({ message: 'Connection failed', from: fromUuid, to: toUuid });
                        }
                    });
            };

            plumbGui.putEntityCountOnConnection = function (result) {
                angular.forEach(result.Streams, function (stream) {
                    // Find jsPlumb Connection for the current Stream
                    var sourceElementId = plumbGui.dataSrcIdPrefix + stream.Source;
                    var targetElementId = plumbGui.dataSrcIdPrefix + stream.Target;
                    if (stream.Target === '00000000-0000-0000-0000-000000000000'
                        || stream.Target === queryDef.data.Pipeline.EntityGuid)
                        targetElementId = plumbGui.dataSrcIdPrefix + 'Out';

                    var fromUuid = sourceElementId + '_out_' + stream.SourceOut;
                    var toUuid = targetElementId + '_in_' + stream.TargetIn;

                    var sEndp = plumbGui.instance.getEndpoint(fromUuid);
                    var streamFound = false;
                    if (sEndp) {
                        angular.forEach(sEndp.connections, function (connection) {
                            if (connection.endpoints[1].getUuid() === toUuid) {
                                // when connection found, update it's label with the Entities-Count
                                connection.setLabel({
                                    label: stream.Count.toString(),
                                    cssClass: 'streamEntitiesCount'
                                });
                                streamFound = true;
                                return;
                            }
                        });
                    }

                    // only when debugging
                    //if (!streamFound)
                    //    $log.error('Stream not found', stream, sEndp);
                });
            };


            plumbGui.makeSource = function(dataSource, element, dragHandler) {
                // suspend drawing and initialise
                plumbGui.instance.batch(function() {

                    // make DataSources draggable. Must happen before makeSource()!
                    if (!queryDef.readOnly)
                        plumbGui.instance.draggable(element,
                            {
                                grid: [20, 20],
                                drag: dragHandler
                            });

                    // Add Out- and In-Endpoints from Definition
                    var dataSourceDefinition = dataSource.Definition();
                    if (dataSourceDefinition) {
                        // Add Out-Endpoints
                        angular.forEach(dataSourceDefinition.Out,
                            function(name) {
                                plumbGui.addEndpoint(element, name, false);
                            });
                        // Add In-Endpoints
                        angular.forEach(dataSourceDefinition.In,
                            function(name) {
                                plumbGui.addEndpoint(element, name, true);
                            });
                        // make the DataSource a Target for new Endpoints (if .In is an Array)
                        if (dataSourceDefinition.In) {
                            var targetEndpointUnlimited = plumbGui.buildTargetEndpoint();
                            targetEndpointUnlimited.maxConnections = -1;
                            plumbGui.instance.makeTarget(element, targetEndpointUnlimited);
                        }

                        if (dataSourceDefinition.DynamicOut)
                            plumbGui.instance.makeSource(element,
                                plumbGui.buildSourceEndpoint(),
                                { filter: '.add-endpoint .new-connection' });
                    }
                });
            };



            plumbGui.buildInstance = function() {
                plumbGui.instance = jsPlumb.getInstance(instanceTemplate);

                // If connection on Out-DataSource was removed, remove custom Endpoint
                plumbGui.instance.bind('connectionDetached',
                    function(info) {
                        if (info.targetId === plumbGui.dataSrcIdPrefix + 'Out') {
                            var element = angular.element(info.target);
                            var fixedEndpoints = plumbGui.findDataSourceOfElement(element).dataSource.Definition().In;
                            var label = info.targetEndpoint.getOverlay('endpointLabel').label;
                            if (fixedEndpoints.indexOf(label) === -1) {
                                $timeout(function() {
                                    plumbGui.instance.deleteEndpoint(info.targetEndpoint);
                                });
                            }
                        }
                    });


                // If a new connection is created, ask for a name of the In-Stream
                plumbGui.instance.bind('connection', function (info) {
                    if (!plumbGui.connectionsInitialized) return;

                    // Repeat until a valid Stream-Name is provided by the user
                    var repeatCount = 0;
                    var labelPrompt,
                        targetEndpointHavingSameLabel;

                    var endpointHandling = function (endpoint) {
                        var label = endpoint.getOverlay('endpointLabel').getLabel();
                        if (label === labelPrompt &&
                            info.targetEndpoint.id !== endpoint.id &&
                            angular.element(endpoint.canvas).hasClass('targetEndpoint'))
                            targetEndpointHavingSameLabel = endpoint;
                    };

                    while (true) {
                        repeatCount++;

                        var promptMessage = 'Please name the Stream';
                        if (repeatCount > 1)
                            promptMessage += '. Ensure the name is not used by any other Stream on this DataSource.';

                        var endpointLabel = info.targetEndpoint.getOverlay('endpointLabel');
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

            // init new jsPlumb Instance
            window.jsPlumb.ready(function () {
                jsPlumb = window.jsPlumb; // re-set local short-name, as now it's initialized & ready
                plumbGui.buildInstance();// can't do this before jsplumb is ready...
            });

            return plumbGui;

        });


    // #region jsPlumb Endpoint Definitions
    function getEndpointOverlays(isSource, readOnlyMode) {
        return [
            [
                'Label', {
                    id: 'endpointLabel',
                    //location: [0.5, isSource ? -0.5 : 1.5],
                    location: [0.5, isSource ? 0 : 1],
                    label: 'Default',
                    cssClass: 'noselect ' + (isSource ? 'endpointSourceLabel' : 'endpointTargetLabel'),
                    events: {
                        dblclick: function (labelOverlay) {
                            if (readOnlyMode) return;

                            var newLabel = prompt('Rename Stream', labelOverlay.label);
                            if (newLabel)
                                labelOverlay.setLabel(newLabel);
                        }
                    }
                }
            ]
        ];
    }


})();