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
			//Anchor: 'Continuous',
			//DragOptions: { cursor: 'pointer', zIndex: 2000, hoverClass: 'dragHover' },
			//Connector: ['StateMachine', { curviness: 30 }],
			ConnectionOverlays: [['Arrow', { location: 0.7 }]],
			//EndpointOverlays: [
			//	['Label', {
			//		id: 'endpointLabel',
			//		location: [0.5, -0.5],
			//		label: 'Default',
			//		cssClass: 'endpointLabel',
			//		events: {
			//			dblclick: function (labelOverlay) {
			//				var newLabel = prompt("Rename Stream", labelOverlay.label);
			//				if (newLabel)
			//					labelOverlay.setLabel(newLabel);
			//			}
			//		}
			//	}]
			//],
			//PaintStyle: {
			//	strokeStyle: '#5c96bc',
			//	lineWidth: 2,
			//	outlineColor: 'transparent',
			//	outlineWidth: 4
			//},
			Container: 'pipeline'
		});
		$scope.jsPlumbInstance = instance;
	});

	// this is the paint style for the connecting lines..
	var connectorPaintStyle = {
		lineWidth: 4,
		strokeStyle: "#61B7CF",
		joinstyle: "round",
		outlineColor: "white",
		outlineWidth: 2
	};

	// the definition of source endpoints (the small blue ones)
	$scope.sourceEndpoint = {
		endpoint: "Dot",
		paintStyle: {
			strokeStyle: "#7AB02C",
			fillStyle: "transparent",
			radius: 7,
			lineWidth: 3
		},
		isSource: true,
		connector: ["Bezier", { curviness: 70 }],
		//connector: ["StateMachine", { stub: [40, 60], gap: 10, cornerRadius: 5, alwaysRespectStubs: true }],
		connectorStyle: connectorPaintStyle,
		//hoverPaintStyle: endpointHoverStyle,
		//connectorHoverStyle: connectorHoverStyle,
		//dragOptions: {},
		anchor: "Top",
		overlays: [
			['Label', {
				id: 'endpointLabel',
				location: [0.5, -0.5],
				label: 'Default',
				cssClass: 'endpointSourceLabel',
				events: {
					dblclick: function (labelOverlay) {
						var newLabel = prompt("Rename Stream", labelOverlay.label);
						if (newLabel)
							labelOverlay.setLabel(newLabel);
					}
				}
			}]
		]
	};

	// the definition of target endpoints (will appear when the user drags a connection) 
	$scope.targetEndpoint = {
		endpoint: "Dot",
		paintStyle: { fillStyle: "#7AB02C", radius: 11 },
		//hoverPaintStyle: endpointHoverStyle,
		maxConnections: -1,
		dropOptions: { hoverClass: "hover", activeClass: "active" },
		isTarget: true,
		anchor: "Bottom",
		overlays: [
			['Label', {
				//id: 'endpointLabel',
				location: [0.5, 1.5],
				label: 'Default',
				cssClass: 'endpointTargetLabel',
				events: {
					dblclick: function (labelOverlay) {
						var newLabel = prompt("Rename Stream", labelOverlay.label);
						if (newLabel)
							labelOverlay.setLabel(newLabel);
					}
				}
			}]
		]
	};

	$scope.makeDataSource = function (dataSource, element) {
		// suspend drawing and initialise
		$scope.jsPlumbInstance.doWhileSuspended(function () {
			if (dataSource.Definition != null) {
				// Add Out-Endpoints
				angular.forEach(dataSource.Definition.Out, function (name) {
					$scope.addOutConnection(element, name);
				});
				// Add In-Endpoints
				angular.forEach(dataSource.Definition.In, function (name) {
					$scope.addInConnection(element, name);
				});
			}

			// make DataSources draggable
			$scope.jsPlumbInstance.draggable(element, {
				// grid: [20, 20],
				drag: $scope.dataSourceDrag
			});
		});

		$scope.dataSourcesCount++;
	}

	$scope.addOutConnection = function (element, name) {
		$scope.jsPlumbInstance.addEndpoint(element, $scope.targetEndpoint);
	}
	$scope.addInConnection = function (element, name) {
		$scope.jsPlumbInstance.addEndpoint(element, $scope.sourceEndpoint);
	}

	// Initialize jsPlumb Connections once after all DataSources were created in the DOM
	$scope.connectionsInitialized = false;
	$scope.$on('ngRepeatFinished', function () {
		if ($scope.connectionsInitialized) return;

		return;

		$scope.jsPlumbInstance.doWhileSuspended(function () {
			angular.forEach($scope.pipelineData.Pipeline.StreamWiring, function (wire) {
				// read connections from Pipeline and connect DataSources
				var connection = $scope.jsPlumbInstance.connect({
					source: $scope.dataSourceIdPrefix + wire.From,
					target: $scope.dataSourceIdPrefix + wire.To
				});
				//connection.endpoints[0].getOverlay('endpointLabel').setLabel(wire.Out);
				//connection.endpoints[1].getOverlay('endpointLabel').setLabel(wire.In);
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