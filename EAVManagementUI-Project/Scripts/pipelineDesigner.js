; (function () {
	// todo 2dm
	// enable dynamic add in-connections (drop on box) + corresponding delete-connections
	// improve labels for connections (mouseover?)
	// improve label on arrow (Default>Default?)
	// provide save-method posting state (positions, connections)

	window.pipelineDesigner = {
		dataSourceIdPrefix: "DataSource",
		container: null,
		pipelineEntityId: -1,
		pipelinePartAttributeSetId: -1,
		pipelinePartAssignmentObjectType: 3,
		streamWidth: 55,	// total with of an endpoint including padding
		ddlAddPipelinePartType: null,
		allSourceEndpoints: [],
		allTargetEndpoints: [],
		readOnly: true,
		protectedOutConnections: new Array("Content", "Presentation", "ListContent", "ListPresentation"),
		webServicesUrl: null,
		// hover style.
		connectorHoverStyle: {
			lineWidth: 7,
			strokeStyle: "#2e2aF8"
		},
		// the definition of target endpoints (will appear when the user drags a connection) 
		targetEndpoint: function () {
			return {
				endpoint: "Dot",
				paintStyle: { fillStyle: "#558822", radius: 11 },
				hoverPaintStyle: pipelineDesigner.connectorHoverStyle,
				maxConnections: 1,
				dropOptions: { hoverClass: "hover", activeClass: "active" },
				isTarget: true,
				//                overlays:[
				//                  [ "Label", dynEndpointOverlay("to", true) ] //{ location:[0.5, -0.5], label:"Drop", cssClass:"endpointTargetLabel" } ]
				//                ]
			};
		},

		// this is the paint style for the connecting lines..
		connectorPaintStyle: {
			lineWidth: 5,
			strokeStyle: "#deea18",
			joinstyle: "round",
			outlineColor: "#EAEDEF",
			outlineWidth: 7
		},
		// the label
		sourceEndpointOverlay: {
			location: [0.5, 1.5],
			label: "Dragx",
			cssClass: "endpointSourceLabel"
		},
		dynEndpointOverlay: function (label, isTarget) {
			return {
				location: [0.5, (isTarget ? -0.5 : 1.5)],
				label: label,
				cssClass: "endpoint" + (isTarget ? "Target" : "Source") + "Label"
			};
		},
		pipelineParts: {
			"ToSic.Eav.DataSources.EntityTypeFilter, ToSic.Eav": { shortName: "EntityTypeFilter", in: ["Default"], out: ["Default"] },
			"ToSic.Eav.DataSources.EntityIdFilter, ToSic.Eav": { shortName: "EntityIdFilter", in: ["Default"], out: ["Default"] },
			"ToSic.Eav.DataSources.AttributeFilter, ToSic.Eav": { shortName: "AttributeFilter", in: ["Default"], out: ["Default"] },
			"ToSic.Eav.DataSources.Caches.ICache, ToSic.Eav": { shortName: "ICache", in: [], out: ["Default"] }
		},
		// the definition of source endpoints (the small blue ones)
		sourceEndpoint: function () {
			return {
				endpoint: "Dot",
				paintStyle: { fillStyle: "#225588", radius: 7 },
				isSource: true,
				connector: ["Bezier", { curviness: 0 }],//["Flowchart", { stub: [30, 30], gap: 10 }], // "Straight", // [ "Flowchart", { stub:[40, 60], gap:10 } ],                                                                     
				connectorStyle: pipelineDesigner.connectorPaintStyle,
				hoverPaintStyle: pipelineDesigner.connectorHoverStyle,
				connectorHoverStyle: pipelineDesigner.connectorHoverStyle,
				dragOptions: {},
				maxConnections: -1
				//                overlays:[
				//                  [ "Label", dynEndpointOverlay("from", false)
				////                    { 
				////                       location:[0.5, 1.5], 
				////                       label:"Drag",
				////                       cssClass:"endpointSourceLabel" 
				////                   } 
				//                    ]
				//                ]
			};
		},

		init: function () {

			jsPlumb.importDefaults({
				// default drag options
				DragOptions: { cursor: 'pointer', zIndex: 2000 },
				// default to blue at one end and green at the other
				EndpointStyles: [{ fillStyle: '#225588' }, { fillStyle: '#558822' }],
				// blue endpoints 7 px; green endpoints 11.
				Endpoints: [["Dot", { radius: 7 }], ["Dot", { radius: 11 }]],
				// the overlays to decorate each connection with.  note that the label overlay uses a function to generate the label text; in this
				// case it returns the 'labelText' member that we set on each connection in the 'init' method below.
				ConnectionOverlays: [
					["Arrow", { location: 0.9 }]
					/*["Label", {
						location: 0.5,
						id: "label",
						cssClass: "aLabel"
					}]*/
				]
			});


			//// a source endpoint that sits at BottomCenter
			////     bottomSource = jsPlumb.extend( { anchor:"BottomCenter" }, sourceEndpoint),
			//var init = function (connection) {
			//	/*connection.getOverlay("label").setLabel(connection.sourceId.substring(6) + "-" + connection.targetId.substring(6));*/
			//	connection.bind("editCompleted", function (o) {
			//		if (typeof console != "undefined")
			//			console.log("connection edited. path is now ", o.path);
			//	});
			//};



			//// listen for new connections; initialise them the same way we initialise the connections at startup.
			//jsPlumb.bind("jsPlumbConnection", function (connInfo, originalEvent) {
			//	init(connInfo.connection);
			//});

			// make all the window divs draggable
			//if (!pipelineDesigner.readOnly)
			//	jsPlumb.draggable(jsPlumb.getSelector(".window"), { grid: [20, 20] });
			// THIS DEMO ONLY USES getSelector FOR CONVENIENCE. Use your library's appropriate selector method!
			//jsPlumb.draggable(jsPlumb.getSelector(".window"));


			var maxX = 100, maxY = 100, minWidth = 112;	// px unit
			//var screenX = 60, screenY = 40;
			var dataSources;
			var connectionList;
			pipelineDesigner.container = $("div#main");

			function initDataSources() {
				// get Pipeline containing DataPipelineParts and DataPipeline
				$.ajax({
					type: "POST",
					contentType: "application/json; charset=utf-8",
					url: pipelineDesigner.webServicesUrl + "/GetPipeline",
					data: JSON.stringify({ pipelineEntityId: pipelineDesigner.pipelineEntityId }),
					async: false,
					error: function (data) { alert(data.responseText); },
					success: function (data) {
						// init dataSources (from DataPipelineParts)
						dataSources = [];
						//	Add Default-Out
						dataSources.push({ sourceid: "Out", name: "2SexyContent Module", description: "The module/template which will show this data", type: "SexyContentTemplate", out: [], in: [], posx: 147, posy: 10 });
						$.each(data.d.DataPipelineParts, function (j, dataPipelinePart) {
							var typeInfo = pipelineDesigner.pipelineParts[dataPipelinePart.PartAssemblyAndType];
							if (typeInfo == undefined) {
								alert("Type " + dataPipelinePart.PartAssemblyAndType + " is unknown");
								return;
							}
							var item = { sourceid: dataPipelinePart.EntityGuid, name: dataPipelinePart.Name, description: dataPipelinePart.Description, type: dataPipelinePart.PartAssemblyAndType, out: typeInfo.out, in: typeInfo.in, configurationEntityId: -1 };
							try {
								var visualDesignerData = $.parseJSON("{" + dataPipelinePart.VisualDesignerData + "}");
								item.posx = visualDesignerData.PosX;
								item.posy = visualDesignerData.PosY;
							} catch (e) {
								//alert("couldn't parse VisualDesignerData\r\n" + dataPipelinePart.VisualDesignerData);	// might happen when a new Part was added
							}
							dataSources.push(item);
						});

						// init connectionList (from DataPipeline StreamWiring)
						connectionList = [];
						var streamWirings = data.d.DataPipeline[0].StreamWiring.split("\r\n");
						var wiringRegex = /(.*):(.*)>(.*):(.*)/;
						$.each(streamWirings, function (j, wiring) {
							var match = wiring.match(wiringRegex);
							var connection = { from: match[1] + ":" + match[2], to: match[3] + ":" + match[4] };
							connectionList.push(connection);
						});

						pipelineDesigner.pipelineEntityGuid = data.d.DataPipeline[0].EntityGuid;

						// init Out's In-Connections (from DataPipeline StreamsOut)
						dataSources[0].in = data.d.DataPipeline[0].StreamsOut.split(",");
					}
				});
			}

			function showDataSources(sourceList) {
				for (var ds = 0; ds < sourceList.length; ds++) {
					var src = sourceList[ds];
					if (src.posx == undefined) src.posx = Math.floor((Math.random() * maxX) + 1);
					if (src.posy == undefined) src.posy = Math.floor((Math.random() * maxY) + 1);
					//src.posx = src.posx / maxX * screenX;
					//src.posy = src.posy / maxY * screenY;
					// set ideal with based on amount of streams
					src.boxWidth = src.out.length * pipelineDesigner.streamWidth;
					if (src.in.length > src.out.length) src.boxWidth = src.in.length * pipelineDesigner.streamWidth;
					if (src.boxWidth < minWidth) src.boxWidth = minWidth;

					// set some constants
					src.id = pipelineDesigner.dataSourceIdPrefix + src.sourceid;
					// Build PipelinePart with it's context menu
					var friendlyName = pipelineDesigner.pipelineParts.hasOwnProperty(src.type) ? pipelineDesigner.pipelineParts[src.type].shortName : src.type;
					var containerContent = '<div class="window" id="' + src.id + '" style="top:' + src.posy + 'px; left:' + src.posx + 'px; width:' + src.boxWidth + 'px">';
					containerContent += '<span class="DataSourceTitle">' + src.name + '</span>';
					containerContent += '<div class="DataSourceDescription">' + src.description + '</div>';
					containerContent += '<div class="DataSourceContextMenu">';
					containerContent += '<div><span class="Label">Type:</span> <span class="DataSourceType">' + friendlyName + '</span></div><ul>';
					// "Out" has special Context Menu
					if (src.sourceid == "Out") {
						containerContent += '<li><a id="AddOutConnection" href="javascript:pipelineDesigner.addOutConnection()">Add In-Connection</div></li>';
					} else {
						containerContent += '<li><a href="javascript:pipelineDesigner.editDataSourceTitleDescription(\'' + src.id + '\', \'Title\')">Edit Title</a></li>';
						containerContent += '<li><a href="javascript:pipelineDesigner.editDataSourceTitleDescription(\'' + src.id + '\', \'Description\')">Edit Description</a></li>';
						containerContent += '<li><a href="javascript:pipelineDesigner.deleteDataSource(\'' + src.id + '\')">Delete</a></li>';
					}

					containerContent += '</ul></div></div>';
					pipelineDesigner.container.append(containerContent);

					// Create all out-points
					for (var o = 0; o < src.out.length; o++) {
						var pLeftMargin = (src.boxWidth - src.out.length * pipelineDesigner.streamWidth) / 2;
						var pOffset = o * pipelineDesigner.streamWidth + pipelineDesigner.streamWidth / 2;
						var posLblX = (pLeftMargin + pOffset) / src.boxWidth;
						var sourceUUID = src.id + '_out_' + src.out[o];
						pipelineDesigner.allSourceEndpoints.push(jsPlumb.addEndpoint(src.id, pipelineDesigner.sourceEndpoint(), { anchor: [posLblX, 0, 0, 0], uuid: sourceUUID, overlays: [["Label", pipelineDesigner.dynEndpointOverlay(src.out[o], false)]], enabled: !pipelineDesigner.readOnly }));
					}

					// Create all in-points
					for (var i = 0; i < src.in.length; i++) {
						var pLeftMargin = (src.boxWidth - src.in.length * pipelineDesigner.streamWidth) / 2;
						var pOffset = i * pipelineDesigner.streamWidth + pipelineDesigner.streamWidth / 2;
						var posLblX = (pLeftMargin + pOffset) / src.boxWidth;
						var sourceUUID = src.id + '_in_' + src.in[i];

						var endPoint = jsPlumb.addEndpoint(src.id, pipelineDesigner.targetEndpoint(), { anchor: [posLblX, 1, 0, 0], uuid: sourceUUID, overlays: [["Label", pipelineDesigner.dynEndpointOverlay(src.in[i], true)]], enabled: !pipelineDesigner.readOnly });

						// Add Dobuleclick handler to Out-Connections
						if (src.id == pipelineDesigner.dataSourceIdPrefix + "Out" && $.inArray(src.in[i], pipelineDesigner.protectedOutConnections) == -1) {
							endPoint.bind("dblclick", function (a) {
								if (confirm('Delete "' + $(a.overlays[0].canvas).text() + '"?'))
									jsPlumb.deleteEndpoint(a);
							});
						}

						pipelineDesigner.allTargetEndpoints.push(endPoint);
					}
				}
			}

			initDataSources();
			showDataSources(dataSources);
			// make all the window divs draggable                                      (again)
			if (!pipelineDesigner.readOnly)
				jsPlumb.draggable(jsPlumb.getSelector(".window"), { grid: [20, 20] });	//[id!='DataSourceOut']

			function setInitialConnections(conList) {
				for (var c = 0; c < conList.length; c++) {
					var from = 'DataSource' + conList[c].from.replace(":", "_out_");
					var to = 'DataSource' + conList[c].to.replace(":", "_in_");
					jsPlumb.connect({ uuids: [from, to], editable: true });
				}
			}

			setInitialConnections(connectionList);

			if (pipelineDesigner.readOnly)
				return;

			//
			// listen for clicks on connections, and offer to delete connections on click.
			//
			jsPlumb.bind("click", function (conn, originalEvent) {
				if (confirm("Delete connection from " + conn.sourceId + " to " + conn.targetId + "?"))
					jsPlumb.detach(conn);
			});

			//jsPlumb.bind("connectionDrag", function (connection) {
			//	console.log("connection " + connection.id + " is being dragged");
			//});

			//jsPlumb.bind("connectionDragStop", function (connection) {
			//	console.log("connection " + connection.id + " was dragged");
			//});

			// show Context Menu on mouse hover
			$(".window").hover(function () {
				$(this).find(".DataSourceContextMenu").show();
			}, function () {
				$(this).find(".DataSourceContextMenu").hide();
			});


			// init Add Pipeline Part
			pipelineDesigner.ddlAddPipelinePartType = $("#ddlAddPipelinePartType");
			$.each(pipelineDesigner.pipelineParts, function (key, pipelinePart) {
				pipelineDesigner.ddlAddPipelinePartType.append('<option value="' + key + '">' + pipelinePart.shortName + "</option>");
			});

			// Save new PipelinePart
			$("#btnAddPipelinePart").click(pipelineDesigner.addPipelinePart);
		},
		addPipelinePart: function () {
			/// <summary>Add new Pipeline Part</summary>
			// Add/Save new PipelinePart-Entity
			$.ajax({
				type: "POST",
				contentType: "application/json; charset=utf-8",
				url: pipelineDesigner.webServicesUrl + "/AddEntity",
				data: JSON.stringify({ attributeSetId: pipelineDesigner.pipelinePartAttributeSetId, values: { Name: name, PartAssemblyAndType: pipelineDesigner.ddlAddPipelinePartType.val() }, assignmentObjectType: pipelineDesigner.pipelinePartAssignmentObjectType, keyGuid: pipelineDesigner.pipelineEntityGuid }),
				async: false,
				success: function (data) {
					if (!data.d) alert("Add DataPipelinePart failed");
				},
				error: function (data) { alert(data.responseText); }
			});

			pipelineDesigner.reset();
			pipelineDesigner.init();
		},
		addOutConnection: function () {
			var newName = prompt("Name");
			if (!newName) return;
			var srcId = pipelineDesigner.dataSourceIdPrefix + "Out";
			var posLblX = 0;	// ToDo: Calculate position, might have to move existings closer
			var sourceUuid = "Out_in_" + newName;

			var endPoint = jsPlumb.addEndpoint(srcId, pipelineDesigner.targetEndpoint(), { anchor: [posLblX, 1, 0, 0], uuid: sourceUuid, overlays: [["Label", pipelineDesigner.dynEndpointOverlay(newName, true)]] });
			pipelineDesigner.allTargetEndpoints.push(endPoint);

			// Save & reload - widths, positions etc. get recalculated automatically
			pipelineDesigner.save();
			pipelineDesigner.reset();
			pipelineDesigner.init();
		},
		arrangeEndpoints: function (srcId, amount) {
			srcId
		},
		save: function () {
			if (pipelineDesigner.readOnly) {
				alert("Save not allowed in ReadOnly mode");
				return;
			}

			//#region save all Pipeline Parts
			$(".window", pipelineDesigner.container).each(function () {
				var $this = $(this);
				var dataSourceId = $this.attr("id").replace(pipelineDesigner.dataSourceIdPrefix, "");
				var name = $this.find(".DataSourceTitle").text();
				var description = $this.find(".DataSourceDescription").text();
				var visualDesignerData = '"PosX":' + Math.round($this.position().left) + ', "PosY":' + Math.round($this.position().top);

				if (dataSourceId != "Out") {
					$.ajax({
						type: "POST",
						contentType: "application/json; charset=utf-8",
						url: pipelineDesigner.webServicesUrl + "/UpdateEntityByGuid",
						data: JSON.stringify({ entityGuid: dataSourceId, newValues: { VisualDesignerData: visualDesignerData, Name: name, Description: description } }),
						async: false,
						success: function (data) {
							if (!data.d) alert("Updated DataPipelinePart failed");
						},
						error: function (data) { alert(data.responseText); }
					});
				}
			});
			//#endregion

			//#region save DataPipeline
			// pepare StreamWirings
			var streamWirings = [];
			$.each(jsPlumb.getConnections(), function (i, connection) {
				var sourceId = connection.sourceId.replace(pipelineDesigner.dataSourceIdPrefix, "");
				var sourceName = $(connection.endpoints[0].overlays[0].canvas).text();
				var targetId = connection.targetId.replace(pipelineDesigner.dataSourceIdPrefix, "");
				var targetName = $(connection.endpoints[1].overlays[0].canvas).text();
				streamWirings.push(sourceId + ":" + sourceName + ">" + targetId + ":" + targetName);
			});
			// prepare StreamsOut
			var streamsOut = [];
			var endPoints = jsPlumb.getEndpoints(pipelineDesigner.dataSourceIdPrefix + "Out");
			$.each(endPoints, function () {
				streamsOut.push($(this.overlays[0].canvas).text());
			});

			// Update DataPipeline
			$.ajax({
				type: "POST",
				contentType: "application/json; charset=utf-8",
				url: pipelineDesigner.webServicesUrl + "/UpdateEntity",
				data: JSON.stringify({ entityId: pipelineDesigner.pipelineEntityId, newValues: { StreamWiring: streamWirings.join("\r\n"), StreamsOut: streamsOut.join(",") } }),
				async: false,
				success: function (data) {
					if (!data.d) alert("Updated DataPipeline failed");
				},
				error: function (data) { alert(data.responseText); }
			});
			//#endregion

			alert("Saved");
		},
		editDataSourceTitleDescription: function (id, label) {
			var ds = $("#" + id);
			var $title = ds.find(".DataSource" + label);
			var oldText = $title.text();
			var newText = prompt("New " + label + ":", oldText);
			if (newText == undefined) return;
			$title.text(newText);
		},
		deleteDataSource: function (id) {
			var ds = $("#" + id);
			if (!confirm("Realy delete \"" + ds.find(".DataSourceTitle").text() + "\"?")) return;
			jsPlumb.removeAllEndpoints(ds);
			ds.remove();

			$.ajax({
				type: "POST",
				contentType: "application/json; charset=utf-8",
				url: pipelineDesigner.webServicesUrl + "/DeleteEntityByGuid",
				data: JSON.stringify({ entityGuid: id.replace(pipelineDesigner.dataSourceIdPrefix, "") }),
				async: false,
				success: function (data) {
					if (!data.d) alert("Delete DataPipelinePart failed");
				},
				error: function (data) { alert(data.responseText); }
			});

			$("#lbtnSavePipeline").click();
		},
		reset: function () {
			jsPlumb.reset();
			$(".window").remove();
		}
	};
})();