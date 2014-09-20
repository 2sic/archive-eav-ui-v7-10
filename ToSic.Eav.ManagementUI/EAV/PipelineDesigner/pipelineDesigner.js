var pipelineDesigner = angular.module('pipelineDesinger', []);

pipelineDesigner.factory('pipeline', function () {
	'use strict';
	var pipeline = null;

	return {
		get: function () { return pipeline; },
		set: function (fc) { pipeline = fc; }
	};
});

pipelineDesigner.run(function (pipeline) {
	'use strict';


	pipeline.set({
		dataSources: {
			guid1: {
				fqn: 'ToSic.ToSexy.DataSources.ModuleDataSource, ToSic.ToSexy',
				name: 'Module Data Source',
				description: 'Provides data to the module',
				top: 47,
				left: 700
			},
			guid2: {
				fqn: 'ToSic.Eav.DataSources.Caches.ICache, ToSic.Eav',
				name: 'Cached DB',
				description: '',
				top: 287,
				left: 390
			}
		},
		connections: [
		  { from: 'guid1', out: 'DefaultOut', to: 'guid2', in: 'DefaultIn' },
		  { from: 'guid2', out: 'DefaultOut', to: 'guid1', in: 'DefaultIn' },
		  //{ 'from': 'dataSource1', 'to': 'dataSource2', 'label': 'submit for publication' },
		  //{ 'from': 'dataSource1', 'to': 'dataSource0', 'label': 'publish' },
		  //{ 'from': 'dataSource2', 'to': 'dataSource0', 'label': 'publish' },
		  //{ 'from': 'dataSource2', 'to': 'dataSource1', 'label': 'retract' },
		],
	});


});

pipelineDesigner.controller('designerController', function ($scope, pipeline) {
	'use strict';
	$scope.pipeline = pipeline.get();

	$scope.dataSourceIdPrefix = 'dataSource_';


	jsPlumb.ready(function () {

		var instance = jsPlumb.getInstance({
			Anchor: 'Continuous',
			DragOptions: { cursor: 'pointer', zIndex: 2000, hoverClass: 'dragHover' },	// default drag options
			Connector: ['StateMachine', { curviness: 30 }],
			ConnectionOverlays: [['Arrow', { location: 0.7 }]],
			Endpoint: ['Dot', { radius: 8 }],
			EndpointOverlays: [
				['Label', {
					//location: 0.5,
					id: 'endpointLabel',
					label: 'Default',
					cssClass: 'endpointLabel'
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

		//instance.bind('connection', function (info) {
		//	info.connection.getOverlay('label').setLabel(info.connection.id);
		//});

		// Inline Editing Input
		//function inline_edit_input(object) {
		//	var label = object;
		//	var value = label.text();
		//	label.empty();
		//	label.append('<input type="text" value="' + value + '" data-value-orig="' + value + '" />')
		//	  .append('<div class="actions"><button class="btn btn-xs btn-default">Abbrechen</button><button class="btn btn-xs btn-primary">Speichern</button></div>');
		//}

		//function inline_edit_input_save(object) {
		//	var label = object.parent().parent();
		//	var value = label.find('input').val();
		//	label.empty();
		//	label.append(value);
		//}

		//function inline_edit_input_cancel(object) {
		//	var label = object.parent().parent();
		//	var value = label.find('input').attr('data-value-orig');
		//	label.empty();
		//	label.append(value);
		//}

		//// Connection Label Inline Editing
		//$(document).on("dblclick", ".connectionLabel", function (event) {
		//	inline_edit_input($(this));
		//});
		//// Save
		//$(document).on("click", ".connectionLabel .btn-primary", function (event) {
		//	inline_edit_input_save($(this));
		//});
		//// Cancel
		//$(document).on("click", ".connectionLabel .btn-default", function (event) {
		//	inline_edit_input_cancel($(this));
		//});


		// suspend drawing and initialise.
		instance.doWhileSuspended(function () {
			var dataSources = jsPlumb.getSelector('#pipeline .dataSource');

			// make each '.ep' div a source
			instance.makeSource(dataSources, { filter: '.ep', });

			// initialise all DataSource elements as connection targets
			instance.makeTarget(dataSources);

			// read connections from Pipeline and connect them
			$.each($scope.pipeline.connections, function (index, value) {
				var connection = instance.connect({
					source: $scope.dataSourceIdPrefix + value.from,
					target: $scope.dataSourceIdPrefix + value.to
				});
				connection.endpoints[0].getOverlay('endpointLabel').setLabel(value.out);
				connection.endpoints[1].getOverlay('endpointLabel').setLabel(value.in);
			});
			// make all DataSources draggable
			instance.draggable($('.dataSource'), {
				grid: [20, 20],
				drag: $scope.dataSourceDrag
			});

		});
	});

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

	$scope.savePipeline = function () {
		//alert("save");
	}

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
});