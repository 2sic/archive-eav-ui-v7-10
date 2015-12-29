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
				width: 70,
				suppressSorting: true,
				suppressMenu: true,
				template: '<button type="button" class="btn btn-xs btn-square" ng-click="vm.openDuplicate(data)" tooltip-append-to-body="true" tooltip="{{ \'General.Buttons.Copy\' | translate }}"><i icon="duplicate"></i></button> <button type="button" class="btn btn-xs btn-square" ng-click="vm.tryToDelete(data)" tooltip-append-to-body="true" tooltip="{{ \'General.Buttons.Delete\' | translate }}"><i icon="remove"></i> </button>'
			}
		];
		var emptyText = "(empty)";


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

		// Get/Update Grid Row-Data
		function setRowData() {
			if (vm.gridOptions.api)
				vm.gridOptions.api.setRowData(null);

			return svc.liveListSourceRead().then(function (success) {
				vm.gridOptions.api.setRowData(success.data);
			});
		}

		// get Grid Column-Definitions from an Array of EAV-Attributes
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
					field: eavAttribute.StaticName,
					cellRenderer: cellRendererDefault,
					filterParams: { cellRenderer: cellRendererDefault }
				};


				switch (eavAttribute.Type) {
					case "Entity":
						var allowMultiValue;
						try {
							allowMultiValue = eavAttribute.Metadata.Entity.AllowMultiValue;
						} catch (e) {
							allowMultiValue = true;
						}

						colDef.cellRenderer = function (params) { return cellRendererEntityBase(params, allowMultiValue); };
						colDef.valueGetter = valueGetterEntityField;
						break;
					case "DateTime":
						colDef.valueGetter = valueGetterDateTime;
						break;
					case "Boolean":
						colDef.valueGetter = valueGetterBoolean;
						break;
					case "Number":
						colDef.filter = 'number';
						break;
				}

				columnDefs.push(colDef);
			});

			return columnDefs;
		}

		//#region Column Value-Getter and Cell Renderer
		function valueGetterEntityField(params) {
			var rawValue = params.data[params.colDef.field];
			if (rawValue.length === 0)
				return emptyText;

			return rawValue.map(function (item) {
				return item.Title;
			});
		}

		function valueGetterDateTime(params) {
			var rawValue = params.data[params.colDef.field];
			if (!rawValue)
				return emptyText;
			return rawValue.substr(0, 19).replace('T', ' ');	// remove 'Z' and replace 'T'
		}

		function valueGetterBoolean(params) {
			var rawValue = params.data[params.colDef.field];
			if (typeof rawValue != "boolean")
				return null;

			return rawValue.toString();
		}

		function cellRendererDefault(params) {
			if (typeof (params.value) != "string")
				return params.value;

			var encodedValue = htmlEncode(params.value);
			return '<span title="' + encodedValue + '">' + encodedValue + '</span>';
		}

		// htmlencode strings (source: http://stackoverflow.com/a/7124052)
		function htmlEncode(text) {
			return text.replace(/&/g, '&amp;').replace(/"/g, '&quot;').replace(/'/g, '&#39;').replace(/</g, '&lt;').replace(/>/g, '&gt;');
		}

		function cellRendererEntityBase(params, showBadge) {
			if (!Array.isArray(params.value))
				return null;

			var encodedValue = htmlEncode(params.value.join(", "));
			var result = '<span title="' + encodedValue + '">';
			if (showBadge)
				result += '<span class="badge badge-primary">' + params.value.length + '</span> ';
			result += encodedValue + '</span>';

			return result;
		}
		// #endregion

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