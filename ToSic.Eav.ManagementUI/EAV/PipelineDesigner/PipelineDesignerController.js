pipelineDesigner.controller('pipelineDesignerController', ['$scope', 'pipelineFactory', '$location', function ($scope, pipelineFactory, $location) {
	'use strict';

	// Load Pipeline Data
	var pipelineEntityId = $location.search().PipelineId;
	pipelineFactory.getPipeline(pipelineEntityId).then(function (result) {
		$scope.pipelineData = result;
	});

	$scope.dataSourcesCount = 0;
	$scope.dataSourceIdPrefix = 'dataSource_';

	jsPlumb.ready(function () {
		// init new jsPlumb Instance
		var instance = jsPlumb.getInstance({
			Anchor: 'Continuous',
			DragOptions: { cursor: 'pointer', zIndex: 2000, hoverClass: 'dragHover' },
			Connector: ['StateMachine', { curviness: 30 }],
			ConnectionOverlays: [['Arrow', { location: 0.7 }]],
			Endpoint: ['Dot', { radius: 8 }],
			EndpointOverlays: [
				['Label', {
					id: 'endpointLabel',
					label: 'Default',
					cssClass: 'endpointLabel',
					events: {
						dblclick: function (labelOverlay) {
							var newLabel = prompt("Rename Stream", labelOverlay.label);
							if (newLabel)
								labelOverlay.setLabel(newLabel);
						}
					}
				}]
			],
			PaintStyle: {
				strokeStyle: '#5c96bc',
				lineWidth: 2,
				outlineColor: 'transparent',
				outlineWidth: 4
			},
			Container: 'pipeline'
		});
		//jsPlumb.Defaults.MaxConnections = 5;
		$scope.jsPlumbInstance = instance;
	});

	$scope.makeDataSource = function (dataSource, element) {
		//console.log('makeDataSource');
		// suspend drawing and initialise
		$scope.jsPlumbInstance.doWhileSuspended(function () {

			$scope.jsPlumbInstance.makeSource(element, { filter: '.ep', });
			$scope.jsPlumbInstance.makeTarget(element);

			//var dynEndpointOverlay = function (label, isTarget) {
			//    return {
			//        location: [0.5, (isTarget ? -0.5 : 1.5)],
			//        label: label,
			//        cssClass: "endpoint" + (isTarget ? "Target" : "Source") + "Label"
			//    };
			//}

			//var sourceEndpoint = {
			//	endpoint: "Dot",
			//paintStyle: { fillStyle: "#225588", radius: 7 },
			//isSource: true,
			//connector: ["Bezier", { curviness: 0 }],//["Flowchart", { stub: [30, 30], gap: 10 }], // "Straight", // [ "Flowchart", { stub:[40, 60], gap:10 } ],                                                                     
			//connectorStyle: pipelineDesigner.connectorPaintStyle,
			//hoverPaintStyle: pipelineDesigner.connectorHoverStyle,
			//connectorHoverStyle: pipelineDesigner.connectorHoverStyle,
			//dragOptions: {},
			//maxConnections: -1
			//                overlays:[
			//                  [ "Label", dynEndpointOverlay("from", false)
			////                    { 
			////                       location:[0.5, 1.5], 
			////                       label:"Drag",
			////                       cssClass:"endpointSourceLabel" 
			////                   } 
			//                    ]
			//                ]
			//}


			//$scope.jsPlumbInstance.addEndpoint(element);

			//$scope.jsPlumbInstance.addEndpoint(element, sourceEndpoint, {
			//    anchor: [1, 1, 0, 0],
			//    //overlays: [["Label", dynEndpointOverlay("Label", false)]],
			//    //enabled: true
			//});

			// make DataSources draggable
			$scope.jsPlumbInstance.draggable(element, {
				// grid: [20, 20],
				drag: $scope.dataSourceDrag
			});
		});

		$scope.dataSourcesCount++;
	}

	// Initialize jsPlumb Connections once after all DataSources were created in the DOM
	$scope.connectionsInitialized = false;
	$scope.$on('ngRepeatFinished', function () {
		if ($scope.connectionsInitialized) return;

		$scope.jsPlumbInstance.doWhileSuspended(function () {
			angular.forEach($scope.pipelineData.Pipeline.StreamWiring, function (wire) {
				// read connections from Pipeline and connect DataSources
				var connection = $scope.jsPlumbInstance.connect({
					source: $scope.dataSourceIdPrefix + wire.From,
					target: $scope.dataSourceIdPrefix + wire.To
				});
				connection.endpoints[0].getOverlay('endpointLabel').setLabel(wire.Out);
				connection.endpoints[1].getOverlay('endpointLabel').setLabel(wire.In);
			});
		});

		$scope.connectionsInitialized = true;
	});

	// Add new DataSource
	$scope.addDataSource = function () {
		$scope.pipelineData.DataSources.push({
			VisualDesignerData: { Top: 100, Left: 100 },
			Name: "",
			Description: "",
			PartAssemblyAndType: $scope.addDataSourceType.PartAssemblyAndType,
			EntityGuid: 'unsaved' + ($scope.dataSourcesCount + 1)
		});
	}

	// Delete a DataSource
	$scope.remove = function (index) {
		var dataSource = $scope.pipelineData.DataSources[index];
		if (!confirm('Delete DataSource "' + (dataSource.Name || '(unnamed)') + '"?')) return;
		var element = $('#' + $scope.dataSourceIdPrefix + dataSource.EntityGuid);
		$scope.jsPlumbInstance.detachAllConnections(element);
		$scope.pipelineData.DataSources.splice(index, 1);
	}

	// Edit name of a DataSource
	$scope.editName = function (dataSource) {
		var newName = prompt("Rename DataSource", dataSource.Name);
		if (newName != undefined)
			dataSource.Name = newName;
	}

	// Edit Description of a DataSource
	$scope.editDescription = function (dataSource) {
		var newDescription = prompt("Edit Description", dataSource.Description);
		if (newDescription != undefined)
			dataSource.Description = newDescription;
	}

	// Update DataSource Position on Drag
	$scope.dataSourceDrag = function () {
		var $this = $(this);
		var offset = $this.offset();
		var dataSource = $this.scope().dataSource;
		$scope.$apply(function () {
			dataSource.VisualDesignerData.Top = Math.round(offset.top);
			dataSource.VisualDesignerData.Left = Math.round(offset.left);
		});
	}

	// Show/Hide Endpoint Overlays
	$scope.showEndpointOverlays = true;
	$scope.toggleEndpointOverlays = function () {
		$scope.showEndpointOverlays = !$scope.showEndpointOverlays;

		angular.forEach($scope.jsPlumbInstance.getAllConnections(), function (connection) {
			angular.forEach(connection.endpoints, function (endpoint) {
				if ($scope.showEndpointOverlays)
					endpoint.showOverlays();
				else
					endpoint.hideOverlays();
			});
		});
	}

	// Sync jsPlumb Connections to the pipelineData-Object
	$scope.syncConnections = function () {
		var connectionInfos = [];
		angular.forEach($scope.jsPlumbInstance.getAllConnections(), function (connection) {
			connectionInfos.push({
				From: connection.sourceId.substr($scope.dataSourceIdPrefix.length),
				Out: connection.endpoints[0].getOverlay('endpointLabel').label,
				To: connection.targetId.substr($scope.dataSourceIdPrefix.length),
				In: connection.endpoints[1].getOverlay('endpointLabel').label
			});
		});

		$scope.pipelineData.Pipeline.StreamWiring = connectionInfos;
	}

	// Save Pipeline
	$scope.savePipeline = function () {
		$scope.syncConnections();

		console.log("save...");
	}

	// Repaint jsPlumb
	$scope.repaint = function () {
		$scope.jsPlumbInstance.repaintEverything();
	}
}]);