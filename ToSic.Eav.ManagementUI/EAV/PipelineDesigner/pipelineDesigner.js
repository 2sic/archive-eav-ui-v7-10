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
		'nodes': [
		  {
		  	'id': 0,
		  	'title': 'Published',
		  	'text': '',
		  	'top': 200,
		  	'left': 20,
		  },
		  {
		  	'id': 1,
		  	'title': 'Private',
		  	'text': '',
		  	'top': 20,
		  	'left': 500,
		  },
		  {
		  	'id': 2,
		  	'title': 'Pending',
		  	'text': 'Pending review',
		  	'top': 340,
		  	'left': 420,
		  },
		],
		'connections': [
		  { 'from': 'node0', 'to': 'node1', 'label': 'retract' },
		  { 'from': 'node1', 'to': 'node2', 'label': 'submit for publication' },
		  { 'from': 'node1', 'to': 'node0', 'label': 'publish' },
		  { 'from': 'node2', 'to': 'node0', 'label': 'publish' },
		  { 'from': 'node2', 'to': 'node1', 'label': 'retract' },
		],
	});


});

pipelineDesigner.controller('designerController', function ($scope, pipeline) {
	'use strict';
	$scope.pipeline = pipeline.get();


	jsPlumb.ready(function () {
		var instance = jsPlumb.getInstance({
			Endpoint: ['Dot', { radius: 2 }],
			HoverPaintStyle: { strokeStyle: '#1e8151', lineWidth: 2 },
			ConnectionOverlays: [
			  [
				'Arrow', {
					location: 1,
					id: 'arrow',
					length: 14,
					foldback: 0.8
				}
			  ],
			  [
				'Label',
				{
					label: 'Click to edit',
					id: 'label',
					cssClass: 'aLabel connectionLabel',
					location: 0.62,
				}
			  ]
			],
			Container: 'pipeline'
		});
		//instance.doWhileSuspended(function () {
		//	instance.connect({ source: 'node0', target: 'node1' });
		//});

		var windows = jsPlumb.getSelector('#pipeline .node');

		// initialise draggable elements.
		instance.draggable(windows);

		// bind a click listener to each connection; the connection is deleted. you could of course
		// just do this: jsPlumb.bind('click', jsPlumb.detach), but I wanted to make it clear what was
		// happening.
		//instance.bind('click', function(c) {
		//  instance.detach(c);
		//});

		// bind a connection listener. note that the parameter passed to this function contains more than
		// just the new connection - see the documentation for a full list of what is included in 'info'.
		// this listener sets the connection's internal
		// id as the label overlay's text.
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

		instance.doWhileSuspended(function () {

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
					source: value.from,
					target: value.to
				});//.getOverlay('label').setLabel(value.label);
			});
			// make all nodes draggable
			instance.draggable($('.node'), { grid: [20, 20] });
		});
	});
});