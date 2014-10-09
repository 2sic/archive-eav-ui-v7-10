// AngularJS Controller for the Pipeline Designer
pipelineDesigner.controller('pipelineDesignerController',
			['$scope', 'pipelineFactory', '$location', '$timeout', '$filter', 'uiNotification', 'eavDialogService',
	function ($scope, pipelineFactory, $location, $timeout, $filter, uiNotification, eavDialogService) {
		'use strict';

		// Init
		uiNotification.note('Loading');
		$scope.dataSourcesCount = 0;
		$scope.dataSourceIdPrefix = 'dataSource_';
		$scope.debug = false;

		// Load Pipeline Data
		var pipelineEntityId = $location.search().PipelineId;
		var appId = $location.search().AppId;
		// Stop if no AppId is set
		if (!appId) {
			$timeout(function () {
				uiNotification.error('Please specify an AppId');
			});
			return;
		}
		// Get Data from PipelineFactory (Web API)
		pipelineFactory.getPipeline(pipelineEntityId, appId).then(function (result) {
			$scope.pipelineData = result;
			$scope.readOnly = !result.Pipeline.AllowEdit;
			uiNotification.note('Ready', $scope.readOnly ? 'This pipeline is read only' : 'You can now desing the Pipeline', true);

			// If a new Pipeline is made, add some Default-DataSources
			if (!$scope.pipelineData.Pipeline.EntityId) {
				$timeout(function () {
					$scope.addDataSource('ToSic.Eav.DataSources.App, ToSic.Eav', { Top: 400, Left: 450 });
				});
			}
		}, function (reason) {
			uiNotification.error('Loading Pipeline failed', reason);
		});

		// init new jsPlumb Instance
		jsPlumb.ready(function () {
			$scope.jsPlumbInstance = jsPlumb.getInstance({
				Connector: ['Bezier', { curviness: 70 }],
				ConnectionOverlays: [['Arrow', { location: 0.7 }]],
				PaintStyle: {
					lineWidth: 4,
					strokeStyle: '#61B7CF',
					joinstyle: 'round',
					outlineColor: 'white',
					outlineWidth: 2
				},
				Container: 'pipelineContainer'
			});
			// If connection on Out-DataSource was removed, remove custom Endpoint
			$scope.jsPlumbInstance.bind('connectionDetached', function (info) {
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

		// #region jsPlumb Endpoint Definitions
		var getEndpointOverlays = function (isSource) {
			return [['Label', {
				id: 'endpointLabel',
				location: [0.5, isSource ? -0.5 : 1.5],
				label: 'Default',
				cssClass: isSource ? 'endpointSourceLabel' : 'endpointTargetLabel',
				events: {
					dblclick: function (labelOverlay) {
						if ($scope.readOnly) return;

						var newLabel = prompt('Rename Stream', labelOverlay.label);
						if (newLabel)
							labelOverlay.setLabel(newLabel);
					}
				}
			}]];
		}

		// the definition of source endpoints (the small blue ones)
		var sourceEndpoint = {
			paintStyle: { strokeStyle: '#7AB02C', fillStyle: 'transparent', radius: 7, lineWidth: 3 },
			maxConnections: -1,
			isSource: true,
			anchor: ['Continuous', { faces: ['top'] }],
			overlays: getEndpointOverlays(true)
		};

		// the definition of target endpoints (will appear when the user drags a connection) 
		var targetEndpoint = {
			paintStyle: { fillStyle: '#7AB02C', radius: 11 },
			maxConnections: 1,
			isTarget: true,
			anchor: ['Continuous', { faces: ['bottom'] }],
			overlays: getEndpointOverlays(false)
		};
		// #endregion

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
					// make the DataSource a Target for new Endpoints (if .In is an Array)
					if (dataSourceDefinition.In) {
						var targetEndpointUnlimited = targetEndpoint;
						targetEndpointUnlimited.maxConnections = -1;
						$scope.jsPlumbInstance.makeTarget(element, targetEndpointUnlimited);
					}

					$scope.jsPlumbInstance.makeSource(element, sourceEndpoint, { filter: '.ep', });
				}

				// make DataSources draggable
				if (!$scope.readOnly) {
					$scope.jsPlumbInstance.draggable(element, {
						grid: [20, 20],
						drag: $scope.dataSourceDrag
					});
				}
			});

			$scope.dataSourcesCount++;
		}

		// Add a jsPlumb Endpoint to an Element
		var addEndpoint = function (element, name, isIn) {
			var dataSource = element.scope().dataSource;
			var uuid = element.attr('id') + (isIn ? '_in_' : '_out_') + name;
			var params = {
				uuid: uuid,
				enabled: !dataSource.ReadOnly || dataSource.EntityGuid == 'Out'	// Endpoints on Out-DataSource must be always enabled
			};
			var endPoint = $scope.jsPlumbInstance.addEndpoint(element, (isIn ? targetEndpoint : sourceEndpoint), params);
			endPoint.getOverlay('endpointLabel').setLabel(name);
		}

		// Initialize jsPlumb Connections once after all DataSources were created in the DOM
		$scope.connectionsInitialized = false;
		$scope.$on('ngRepeatFinished', function () {
			if ($scope.connectionsInitialized) return;

			// suspend drawing and initialise
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

			$scope.connectionsInitialized = true;
		});

		// Add new DataSource
		$scope.addDataSource = function (partAssemblyAndType, visualDesignerData) {
			if (!partAssemblyAndType) {
				partAssemblyAndType = $scope.addDataSourceType.PartAssemblyAndType;
				$scope.addDataSourceType = null;
			}
			if (!visualDesignerData)
				visualDesignerData = { Top: 100, Left: 100 };

			var newDataSource = {
				VisualDesignerData: visualDesignerData,
				Name: $filter('typename')(partAssemblyAndType, 'className'),
				Description: '',
				PartAssemblyAndType: partAssemblyAndType,
				EntityGuid: 'unsaved' + ($scope.dataSourcesCount + 1)
			};
			// Extend it with a Property to it's Definition
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
			if (dataSource.ReadOnly) return;

			var newName = prompt('Rename DataSource', dataSource.Name);
			if (newName != undefined)
				dataSource.Name = newName;
		}

		// Edit Description of a DataSource
		$scope.editDescription = function (dataSource) {
			if (dataSource.ReadOnly) return;

			var newDescription = prompt('Edit Description', dataSource.Description);
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

		// Configure a DataSource
		$scope.configureDataSource = function (dataSource) {
			if (dataSource.ReadOnly) return;

			// Ensure dataSource Entity is saved
			if (!dataSourceIsPersisted(dataSource)) {
				savePipelineInternal();
				return;
			}

			uiNotification.note('Please Wait ..');

			pipelineFactory.getDataSourceConfigurationUrl(appId, dataSource).then(function (success) {
				uiNotification.clear();
				eavDialogService.open({ url: success.Url, title: 'Configure DataSource ' + dataSource.Name });
			}, function (error) {
				uiNotification.error('Open Configuration UI failed', error);
			});
		}

		// Test wether a DataSource is persisted on the Server
		var dataSourceIsPersisted = function (dataSource) {
			return dataSource.EntityGuid.indexOf('unsaved') == -1;
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
			$scope.jsPlumbInstance.selectEndpoints({ target: $scope.dataSourceIdPrefix + 'Out' }).each(function (endpoint) {
				streamsOut.push(endpoint.getOverlay('endpointLabel').label);
			});
			$scope.pipelineData.Pipeline.StreamsOut = streamsOut.join(',');
		}

		// #region Save Pipeline
		// Save Pipeline
		$scope.savePipeline = function () {
			savePipelineInternal(pipelineSaved);
		}

		var savePipelineInternal = function () {
			uiNotification.note('Saving...');

			syncPipelineData();

			pipelineFactory.savePipeline(appId, $scope.pipelineData.Pipeline, $scope.pipelineData.DataSources).then(pipelineSaved, function (reason) {
				uiNotification.error('Save Pipeline failed', reason);
			});
		}

		// Handle Pipeline Saved
		var pipelineSaved = function (success) {
			uiNotification.success('Saved', 'Pipeline ' + success.Pipeline.EntityId, true);

			// Update PipelineData with data retrieved from the Server
			$scope.pipelineData.Pipeline = success.Pipeline;
			$scope.pipelineData.DataSources = success.DataSources;
			pipelineFactory.postProcessDataSources($scope.pipelineData);

			// Reset jsPlumb, re-Init Connections
			$scope.jsPlumbInstance.reset();
			$scope.connectionsInitialized = false;
		};
		// #endregion

		// Repaint jsPlumb
		$scope.repaint = function () {
			$scope.jsPlumbInstance.repaintEverything();
		}

		// Show/Hide Debug info
		$scope.toogleDebug = function () {
			$scope.debug = !$scope.debug;
		}
	}]);