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
		'dataSources': {
			guid1: {
				fqn: 'ToSic.ToSexy.DataSources.ModuleDataSource, ToSic.ToSexy',
				name: 'Module Data Source',
				description: 'Provides data to the module',
				top: 27,
				left: 0
			},
			guid2: {
				fqn: 'ToSic.Eav.DataSources.Caches.ICache, ToSic.Eav',
				name: 'Cached DB',
				description: '',
				top: 287,
				left: 390
			}
		}
		,
		'connections': [
		  { 'from': 'guid1', 'to': 'guid2', 'label': 'Default-Default' },
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

	jsPlumb.ready(function () {
		var instance = jsPlumb.getInstance({
			// default drag options
			DragOptions: { cursor: 'pointer', zIndex: 2000 },
			ConnectionOverlays: [
			  ['Arrow', { location: 1 }],
			  ['Label', {
			  	location: 0.5,
			  	id: 'label',
			  	cssClass: 'aLabel connectionLabel'
			  }]
			],
			Container: 'pipeline'
		});

		instance.bind('connection', function (info) {
			info.connection.getOverlay('label').setLabel(info.connection.id);
		});

		// Inline Editing Input
		function inline_edit_input(object) {
			var label = object;
			var value = label.text();
			label.empty();
			label.append('<input type="text" value="' + value + '" data-value-orig="' + value + '" />')
			  .append('<div class="actions"><button class="btn btn-xs btn-default">Abbrechen</button><button class="btn btn-xs btn-primary">Speichern</button></div>');
		}

		function inline_edit_input_save(object) {
			var label = object.parent().parent();
			var value = label.find('input').val();
			label.empty();
			label.append(value);
		}

		function inline_edit_input_cancel(object) {
			var label = object.parent().parent();
			var value = label.find('input').attr('data-value-orig');
			label.empty();
			label.append(value);
		}

		// Connection Label Inline Editing
		$(document).on("dblclick", ".connectionLabel", function (event) {
			inline_edit_input($(this));
		});
		// Save
		$(document).on("click", ".connectionLabel .btn-primary", function (event) {
			inline_edit_input_save($(this));
		});
		// Cancel
		$(document).on("click", ".connectionLabel .btn-default", function (event) {
			inline_edit_input_cancel($(this));
		});

		var dataSourceIdPrefix = 'dataSource_';

		// suspend drawing and initialise.
		instance.doWhileSuspended(function () {
			var windows = jsPlumb.getSelector('#pipeline .dataSource');

			// make each '.ep' div a source and give it some parameters to work with.  here we tell it
			// to use a Continuous anchor and the StateMachine connectors, and also we give it the
			// connector's paint style.  note that in this demo the strokeStyle is dynamically generated,
			// which prevents us from just setting a jsPlumb.Defaults.PaintStyle.  but that is what i
			// would recommend you do. Note also here that we use the 'filter' option to tell jsPlumb
			// which parts of the element should actually respond to a drag start.
			instance.makeSource(windows, {
				filter: '.ep',        // only supported by jquery
				anchor: 'Continuous',
				connector: ['StateMachine', { curviness: 30 }],
				connectorStyle: {
					strokeStyle: '#5c96bc',
					lineWidth: 2,
					outlineColor: 'transparent',
					outlineWidth: 4
				},
				maxConnections: 5,
				onMaxConnections: function (info, e) {
					alert('Maximum connections (' + info.maxConnections + ') reached');
				}
			});

			// initialise all '.w' elements as connection targets.
			instance.makeTarget(windows, {
				dropOptions: { hoverClass: 'dragHover' },
				anchor: 'Continuous'
			});

			// read connections from flowchart and connect them
			$.each($scope.pipeline.connections, function (index, value) {
				instance.connect({
					source: dataSourceIdPrefix + value.from,
					target: dataSourceIdPrefix + value.to
				}).getOverlay('label').setLabel(value.label);
			});
			// make all DataSources draggable
			instance.draggable($('.dataSource'), {
				grid: [20, 20],
				// Update $scope on drag
				drag: function () {
					var offset = $(this).offset();
					var entityGuid = $(this).attr('id').substr(dataSourceIdPrefix.length);
					$scope.$apply(function () {
						var dataSource = $scope.pipeline.dataSources[entityGuid];
						dataSource.top = offset.top;
						dataSource.left = offset.left;
					});
				}
			});
		});
	});

	$scope.savePipeline = function () {
		//alert("save");
	}
});