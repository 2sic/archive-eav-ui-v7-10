pipelineDesigner.controller('pipelineDesignerController', ['$scope', 'pipeline', function ($scope, pipeline) {
	'use strict';
	$scope.pipeline = pipeline.get();
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
						dblclick: function (labelOverlay, originalEvent) {
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
		$scope.jsPlumbInstance = instance;


		// suspend drawing and initialise
		instance.doWhileSuspended(function () {
			var dataSources = jsPlumb.getSelector('#pipeline .dataSource');
			$scope.dataSourcesCount = dataSources.length;

			$scope.makeSource(dataSources);

			// initialise all DataSource elements as connection targets
			$scope.makeTarget(dataSources);

			// read connections from Pipeline and connect DataSources
			$.each($scope.pipeline.connections, function (index, value) {
				var connection = instance.connect({
					source: $scope.dataSourceIdPrefix + value.from,
					target: $scope.dataSourceIdPrefix + value.to
				});
				connection.endpoints[0].getOverlay('endpointLabel').setLabel(value.out);
				connection.endpoints[1].getOverlay('endpointLabel').setLabel(value.in);
			});
		});
	});

	$scope.makeSource = function (dataSources) {
		if (typeof $scope.jsPlumbInstance == "undefined") return; // prevents duplicate makeSource() on first initialization
		$scope.jsPlumbInstance.makeSource(dataSources, { filter: '.ep', });

		// make DataSources draggable
		$scope.jsPlumbInstance.draggable(dataSources, {
			grid: [20, 20],
			drag: $scope.dataSourceDrag
		});
	}

	$scope.makeTarget = function (dataSources) {
		if (typeof $scope.jsPlumbInstance == "undefined") return; // prevents duplicate makeSource() on first initialization

		$scope.jsPlumbInstance.makeTarget(dataSources);
	}

	$scope.editName = function (dataSource) {
		var newName = prompt("Rename DataSource", dataSource.name);
		if (newName != undefined)
			dataSource.name = newName;
	}
	$scope.editDescription = function (dataSource) {
		var newDescription = prompt("Edit Description", dataSource.description);
		if (newDescription != undefined)
			dataSource.description = newDescription;
	}

	// Update DataSoruce Position on Drag
	$scope.dataSourceDrag = function () {
		var $this = $(this);
		var offset = $this.offset();
		var dataSource = $this.scope().dataSource;
		$scope.$apply(function () {
			dataSource.top = Math.round(offset.top);
			dataSource.left = Math.round(offset.left);
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


	// Sync UI-Connections to Pipeline-Object
	$scope.syncConnections = function () {
		var connectionInfos = [];
		angular.forEach($scope.jsPlumbInstance.getAllConnections(), function (connection) {
			connectionInfos.push({
				from: connection.sourceId.substr($scope.dataSourceIdPrefix.length),
				out: connection.endpoints[0].getOverlay('endpointLabel').label,
				to: connection.targetId.substr($scope.dataSourceIdPrefix.length),
				in: connection.endpoints[1].getOverlay('endpointLabel').label
			});
		});

		$scope.pipeline.connections = connectionInfos;
	}

	// Add new DataSource
	$scope.addDataSource = function (typeName) {
		$scope.dataSourcesCount++;
		$scope.pipeline.dataSources.push({ typeName: typeName, guid: 'unsaved' + $scope.dataSourcesCount });

	}
	// Ensure new DataSources are jsPlumb-Sources/Targets when DOM is ready
	$scope.$on('ngRepeatFinished', function () {
		$scope.makeSource($(".dataSource:last"));
		$scope.makeTarget($(".dataSource:last"));
	});


	// Delete DataSource
	$scope.remove = function (array, index) {
		var dataSource = array[index];
		if (!confirm('Delete DataSource "' + (dataSource.name || '(unnamed)') + '"?')) return;
		var element = $('#' + $scope.dataSourceIdPrefix + dataSource.guid);
		$scope.jsPlumbInstance.detachAllConnections(element);
		array.splice(index, 1);
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