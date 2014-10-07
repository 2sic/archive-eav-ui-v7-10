pipelineDesigner.controller('pipelineDesignerController', ['$scope', 'pipelineFactory', '$location', '$timeout', function ($scope, pipelineFactory, $location, $timeout) {
	'use strict';

	// Load Pipeline Data
	var pipelineEntityId = $location.search().PipelineId;
	pipelineFactory.getPipeline(pipelineEntityId).then(function (result) {
		$scope.pipelineData = result;
	});

	$scope.dataSourcesCount = 0;
	$scope.dataSourceIdPrefix = 'dataSource_';

	// init new jsPlumb Instance
	jsPlumb.ready(function () {
		$scope.jsPlumbInstance = jsPlumb.getInstance({
			Connector: ["Bezier", { curviness: 70 }],
			ConnectionOverlays: [['Arrow', { location: 0.7 }]],
			PaintStyle: {
				lineWidth: 4,
				strokeStyle: "#61B7CF",
				joinstyle: "round",
				outlineColor: "white",
				outlineWidth: 2
			},
			Container: 'pipeline'
		});
		// If connection on Out-DataSource was removed, remove custom Endpoint
		$scope.jsPlumbInstance.bind("connectionDetached", function (info) {
			if (info.targetId == $scope.dataSourceIdPrefix + 'Out') {
				var fixedEndpoints = angular.element(info.target).scope().dataSource.Definition().In;
				var label = info.targetEndpoint.getOverlay('endpointLabel').label;
				if (fixedEndpoints.indexOf(label) == -1) {
					$timeout(function () {
						$scope.jsPlumbInstance.deleteEndpoint(info.targetEndpoint);
					});
				}
			}
		});
	});

	//#region jsPlumb Endpoint Definitions
	var getEndpointOverlays = function (isSource) {
		return [['Label', {
			id: 'endpointLabel',
			location: [0.5, isSource ? -0.5 : 1.5],
			label: 'Unnamed',
			cssClass: isSource ? 'endpointSourceLabel' : 'endpointTargetLabel',
			events: {
				dblclick: function (labelOverlay) {
					var newLabel = prompt("Rename Stream", labelOverlay.label);
					if (newLabel)
						labelOverlay.setLabel(newLabel);
				}
			}
		}]];
	}

	// the definition of source endpoints (the small blue ones)
	var sourceEndpoint = {
		paintStyle: { strokeStyle: "#7AB02C", fillStyle: "transparent", radius: 7, lineWidth: 3 },
		maxConnections: -1,
		isSource: true,
		anchor: ["Continuous", { faces: ["top"] }],
		overlays: getEndpointOverlays(true)
	};

	// the definition of target endpoints (will appear when the user drags a connection) 
	var targetEndpoint = {
		paintStyle: { fillStyle: "#7AB02C", radius: 11 },
		maxConnections: -1,
		isTarget: true,
		anchor: ["Continuous", { faces: ["bottom"] }],
		overlays: getEndpointOverlays(false)
	};
	//#endregion

	// make a DataSource with Endpoints, called by the datasource-Directive
	$scope.makeDataSource = function (dataSource, element) {
		// suspend drawing and initialise
		$scope.jsPlumbInstance.doWhileSuspended(function () {
			// Add Out- and In-Endpoints from Definition
			var dataSourceDefinition = dataSource.Definition();
			if (dataSourceDefinition != null) {
				// Add Out-Endpoints
				angular.forEach(dataSourceDefinition.Out, function (name) {
					addEndpoint(element, name, false);
				});
				// Add In-Endpoints
				angular.forEach(dataSourceDefinition.In, function (name) {
					addEndpoint(element, name, true);
				});
				// make the DataSource a Target for new Endpoints
				if (dataSourceDefinition.In)
					$scope.jsPlumbInstance.makeTarget(element, targetEndpoint);
			}

			// make DataSources draggable
			$scope.jsPlumbInstance.draggable(element, {
				// grid: [20, 20],
				drag: $scope.dataSourceDrag
			});
		});

		$scope.dataSourcesCount++;
	}

	// Add a jsPlumb Endpoint to an Element
	var addEndpoint = function (element, name, isIn) {
		var uuid = element.attr('id') + (isIn ? '_in_' : '_out_') + name;
		var endPoint = $scope.jsPlumbInstance.addEndpoint(element, (isIn ? targetEndpoint : sourceEndpoint), { uuid: uuid });
		endPoint.getOverlay('endpointLabel').setLabel(name);
	}

	// Initialize jsPlumb Connections once after all DataSources were created in the DOM
	$scope.connectionsInitialized = false;
	$scope.$on('ngRepeatFinished', function () {
		if ($scope.connectionsInitialized) return;

		initWirings();

		$scope.connectionsInitialized = true;
	});

	var initWirings = function () {
		$scope.jsPlumbInstance.doWhileSuspended(function () {
			angular.forEach($scope.pipelineData.Pipeline.StreamWiring, function (wire) {
				// read connections from Pipeline
				var sourceElementId = $scope.dataSourceIdPrefix + wire.From;
				var fromUuid = sourceElementId + '_out_' + wire.Out;
				var targetElementId = $scope.dataSourceIdPrefix + wire.To;
				var toUuid = targetElementId + '_in_' + wire.In;

				// Ensure In- and Out-Endpoint exist
				if (!$scope.jsPlumbInstance.getEndpoint(fromUuid))
					addEndpoint(jsPlumb.getSelector('#' + sourceElementId), wire.Out, false);
				if (!$scope.jsPlumbInstance.getEndpoint(toUuid))
					addEndpoint(jsPlumb.getSelector('#' + targetElementId), wire.In, true);

				$scope.jsPlumbInstance.connect({ uuids: [fromUuid, toUuid] });
			});
		});
		$scope.repaint();	// repaint to continuous connections are aligned correctly
	}

	// Add new DataSource
	$scope.addDataSource = function () {
		var newDataSource = {
			VisualDesignerData: { Top: 100, Left: 100 },
			Name: "",
			Description: "",
			PartAssemblyAndType: $scope.addDataSourceType.PartAssemblyAndType,
			EntityGuid: 'unsaved' + ($scope.dataSourcesCount + 1)
		};
		// Extend it with Definition-Property
		newDataSource = angular.extend(newDataSource, pipelineFactory.getNewDataSource($scope.pipelineData, newDataSource));

		$scope.pipelineData.DataSources.push(newDataSource);
	}

	// Delete a DataSource
	$scope.remove = function (index) {
		var dataSource = $scope.pipelineData.DataSources[index];
		if (!confirm('Delete DataSource "' + (dataSource.Name || '(unnamed)') + '"?')) return;
		var elementId = $scope.dataSourceIdPrefix + dataSource.EntityGuid;
		$scope.jsPlumbInstance.selectEndpoints({ element: elementId }).remove();
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

		var endpoints = $scope.jsPlumbInstance.selectEndpoints();
		if ($scope.showEndpointOverlays)
			endpoints.showOverlays();
		else
			endpoints.hideOverlays();
	}

	// Sync jsPlumb Connections and StreamsOut to the pipelineData-Object
	var syncPipelineData = function () {
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

		var streamsOut = [];
		$scope.jsPlumbInstance.selectEndpoints({ target: $scope.dataSourceIdPrefix + "Out" }).each(function (endpoint) {
			streamsOut.push(endpoint.getOverlay('endpointLabel').label);
		});
		$scope.pipelineData.Pipeline.StreamsOut = streamsOut.join(",");
	}

	// Save Pipeline
	$scope.savePipeline = function () {
		syncPipelineData();

		pipelineFactory.savePipeline($location.search().AppId, $scope.pipelineData.Pipeline, $scope.pipelineData.DataSources).then(function (success) {
			alert('Pipeline saved successfully');

			$scope.pipelineData.Pipeline = success.Pipeline;
			$scope.pipelineData.DataSources = success.DataSources;
			pipelineFactory.postProcessDataSources($scope.pipelineData);

			$scope.jsPlumbInstance.reset();

			$timeout(function () {	// init wirings when DOM is ready
				initWirings();
			});

		}, function (reason) {
			alert('Failed: ' + reason);
		});
	}

	// Repaint jsPlumb
	$scope.repaint = function () {
		$scope.jsPlumbInstance.repaintEverything();
	}
}]);