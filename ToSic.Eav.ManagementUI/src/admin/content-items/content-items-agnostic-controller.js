(function () {
	'use strict';

	angular.module("ContentItemsAppAgnostic", [
        "EavConfiguration",
        "EavAdminUi",
        "EavServices",
		"agGrid"
	])
        .controller("ContentItemsList", contentItemsListController)
	;

	function contentItemsListController(contentItemsSvc, eavConfig, appId, contentType, eavAdminDialogs, debugState, $modalInstance, $q, $window) {
		/* jshint validthis:true */
		var vm = this;
		vm.debug = debugState;
		var svc;

		var staticColumns = [
			{
				headerName: "ID",
				field: "Id",
				width: 50,
				template: '<span tooltip-append-to-body="true" tooltip="Id: {{data.Id}}\nRepoId: {{data.RepositoryId}}\nGuid: {{data.Guid}}" ng-bind="data.Id"></span>',
				cellClass: "clickable",
				filter: 'number',
				onCellClicked: openEditDialog
			},
			{
				headerName: "Status",
				field: "IsPublished",
				width: 70,
				suppressSorting: true,
				suppressMenu: true,
				template: '<span class="glyphicon" ng-class="{\'glyphicon-eye-open\': data.IsPublished, \'glyphicon-eye-close\' : !data.IsPublished}" tooltip-append-to-body="true" tooltip="{{ \'Content.Publish.\' + (data.IsPublished ? \'PnV\': data.Published ? \'DoP\' : \'D\') | translate }}"></span> <span icon="{{ data.Draft ? \'link\' : data.Published ? \'link\' : \'\' }}" tooltip-append-to-body="true" tooltip="{{ (data.Draft ? \'Content.Publish.HD\' :\'\') | translate:\'{ id: data.Draft.RepositoryId}\' }}\n{{ (data.Published ? \'Content.Publish.HP\' :\'\') | translate }} #{{ data.Published.RepositoryId }}"></span> <span ng-if="data.Metadata" tooltip-append-to-body="true" tooltip="Metadata for type {{ data.Metadata.TargetType}}, id {{ data.Metadata.KeyNumber }}{{ data.Metadata.KeyString }}{{ data.Metadata.KeyGuid }}" icon="tag"></span>'
			},
			{
				headerName: "Title",
				field: "Title",
				width: 216,
				cellClass: "clickable",
				template: '<span tooltip-append-to-body="true" tooltip="{{data.Title}}" ng-bind="data.Title + \' \' + ((!data.Title ? \'Content.Manage.NoTitle\':\'\') | translate)"></span>',
				filter: 'text',
				onCellClicked: openEditDialog
			},
			{
				headerName: "",
				field: "",
				width: 70,
				suppressSorting: true,
				suppressMenu: true,
				template: '<button type="button" class="btn btn-xs btn-square" ng-click="vm.openDuplicate(data)" tooltip-append-to-body="true" tooltip="{{ \'General.Buttons.Copy\' | translate }}"><i icon="duplicate"></i></button> <button type="button" class="btn btn-xs btn-square" ng-click="vm.tryToDelete(data)" tooltip-append-to-body="true" tooltip="{{ \'General.Buttons.Delete\' | translate }}"><i icon="remove"></i> </button>'
			}
		];


		vm.gridOptions = {
			enableSorting: true,
			enableFilter: true,
			rowHeight: 39,
			colWidth: 155,
			headerHeight: 38,
			angularCompileRows: true
		};

		activate();

		function activate() {
			svc = contentItemsSvc(appId, contentType);

			$q.all([setRowData(), svc.getColumns()])
				.then(function (success) {
					var columnDefs = getColumnDefs(success[1].data);
					vm.gridOptions.api.setColumnDefs(columnDefs);
				});
		}

		vm.add = function add() {
			eavAdminDialogs.openItemNew(contentType, setRowData);
		};

		function openEditDialog(params) {
			eavAdminDialogs.openItemEditWithEntityId(params.data.Id, setRowData);
		}

		function setRowData() {
			if (vm.gridOptions.api)
				vm.gridOptions.api.setRowData(null);

			return svc.liveListSourceRead().then(function (success) {
				vm.gridOptions.api.setRowData(success.data);
			});
		}

		function getColumnDefs(eavAttributes) {
			var columnDefs = staticColumns;

			angular.forEach(eavAttributes, function (eavAttribute) {
				if (eavAttribute.IsTitle) {
					staticColumns[2].eavAttribute = eavAttribute;
					return;	// don't add Title-Field twice
				}

				var colDef = {
					eavAttribute: eavAttribute,
					headerName: eavAttribute.StaticName,
					field: eavAttribute.StaticName
				};


				switch (eavAttribute.Type) {
					case "Entity":
						var allowMultiValue;
						try {
							allowMultiValue = eavAttribute.Metadata.Entity.AllowMultiValue;
						} catch (e) {
							allowMultiValue = true;
						}

						if (!allowMultiValue) {
							colDef.valueGetter = function (params) {
								var rawValue = params.data[params.colDef.field];
								if (typeof (rawValue) == "undefined") return null;
								return rawValue[0].Title;
							};
						}

						colDef.cellRenderer = allowMultiValue ? cellRendererEntityMulti : cellRendererDefault;
						colDef.filterParams = { values: getUniqueEntityValues(eavAttribute.StaticName) };

						break;
						//case "DateTime":
						//	colDef.valueGetter = function (params) {
						//		var rawValue = params.data[params.colDef.field];
						//		if (typeof (rawValue) == "undefined") return null;
						//		return new Date(rawValue);
						//	};
						//	break;
					case "Number":
						colDef.cellRenderer = cellRendererDefault;
						colDef.filter = 'number';
						break;
					default:
						colDef.cellRenderer = cellRendererDefault;
						colDef.filterParams = { cellRenderer: cellRendererDefault };
						break;
				}

				columnDefs.push(colDef);
			});

			return columnDefs;
		}

		function getUniqueEntityValues(property) {
			// loop all rows and specified entity-field. Create an object with all EntityIds and Titles. This way no Array.Contains() is needed.
			var distinct = {};
			var rows = vm.gridOptions.rowData;
			rows.forEach(function (row) {
				row[property].forEach(function (entity) {
					distinct[entity.Id] = entity.Title;
				});
			});

			return $window.Object.keys(distinct).map(function (key) { return distinct[key]; });	// source: http://stackoverflow.com/a/26166303

		}

		function cellRendererDefault(params) {
			if (typeof (params.value) == "undefined")
				return null;

			// htmlencode strings (source: http://stackoverflow.com/a/7124052)
			if (typeof (params.value) == "string")
				return params.value.replace(/&/g, '&amp;').replace(/"/g, '&quot;').replace(/'/g, '&#39;').replace(/</g, '&lt;').replace(/>/g, '&gt;');
			return params.value;
		}

		function cellRendererEntityMulti(params) {
			if (typeof (params.value) == "undefined")
				return null;

			var result = '<span class="badge badge-primary">' + params.value.length + '</span> ';

			for (var i = 0; i < params.value.length; i++)
				result += params.value[i].Title + (i < params.value.length - 1 ? ", " : "");

			return result;
		}

		vm.refresh = setRowData;

		vm.tryToDelete = function (item) {
			if (confirm("Delete '" + "title-unknown-yet" + "' (" + item.RepositoryId + ") ?"))
				svc.delete(item.RepositoryId);
		};

		vm.openDuplicate = function (item) {
			var items = [
		        {
		        	ContentTypeName: contentType,
		        	DuplicateEntity: item.Id
		        }
			];
			eavAdminDialogs.openEditItems(items, svc.liveListReload);
		};

		vm.close = function () {
			$modalInstance.dismiss("cancel");
		};

	}

}());