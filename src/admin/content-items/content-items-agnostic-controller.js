(function () {
	'use strict';

	angular.module('ContentItemsAppAgnostic', [
		'EavConfiguration',
		'EavAdminUi',
		'EavServices'
		// "agGrid" // needs this, but can't hardwire the dependency as it would cause problems with lazy-loading
	])
		.controller('ContentItemsList', contentItemsListController)
		;

	function contentItemsListController(contentItemsSvc, contentExportService, eavConfig, appId, contentType, eavAdminDialogs, toastr, debugState, $uibModalInstance, $uibModalStack, $q, $translate, entitiesSvc, agGridFilters) {
		/* jshint validthis:true */
		var vm = angular.extend(this, {
			debug: debugState,
			gridOptions: {
				enableSorting: true,
				enableFilter: true,
				rowHeight: 39,
				colWidth: 155,
				headerHeight: 38,
				angularCompileRows: true
			},
			add: add,
			refresh: setRowData,
			openExport: openExport,
			tryToDelete: tryToDelete,
			openDuplicate: openDuplicate,
			close: close,
            debugFilter: showFilter,
            exportJson: exportJson
		});
		var svc;

		var staticColumns = [
			{
				headerName: 'ID',
				field: 'Id',
				width: 50,
				template: '<span tooltip-append-to-body="true" uib-tooltip="Id: {{data.Id}}\nRepoId: {{data._RepositoryId}}\nGuid: {{data.Guid}}" ng-bind="data.Id"></span>',
				cellClass: 'clickable',
				filter: 'number',
				onCellClicked: openEditDialog
			},
			{
				headerName: 'Status',
				field: 'IsPublished',
				width: 75,
				suppressSorting: true,
				template: '<span class="glyphicon" '
				+ 'ng-class="{\'glyphicon-eye-open\': data.IsPublished, \'glyphicon-eye-close\' : !data.IsPublished}" '
				+ 'tooltip-append-to-body="true" uib-tooltip="{{ \'Content.Publish.\' + (data.IsPublished ? \'PnV\': data.IsPublishedEntity ? \'DoP\' : \'D\') | translate }}"></span>'

				+ ' <span icon="{{ data.DraftEntity || data.PublishedEntity ? \'link\' : \'\' }}" '
				+ 'tooltip-append-to-body="true" '
				+ 'uib-tooltip="{{ (data.DraftEntity ? \'Content.Publish.HD\' :\'\') | translate:\'{ id: data.DraftEntity._RepositoryId }\' }} {{ data.DraftEntity._RepositoryId }}\n{{ (data.PublishedEntity ? \'Content.Publish.HP\' :\'\') | translate }} {{ data.PublishedEntity._RepositoryId }}"></span> <span ng-if="data.Metadata" tooltip-append-to-body="true" uib-tooltip="Metadata for type {{ data.Metadata.TargetType}}, id {{ data.Metadata.KeyNumber }}{{ data.Metadata.KeyString }}{{ data.Metadata.KeyGuid }}" icon="tag"></span>',
				valueGetter: valueGetterStatusField
			},
			{
				headerName: 'Title',
				field: '_Title', 
				width: 216,
				cellClass: 'clickable',
				template: '<span tooltip-append-to-body="true" uib-tooltip="{{data._Title}}" ng-bind="data._Title + \' \' + ((!data._Title ? \'Content.Manage.NoTitle\':\'\') | translate)"></span>',
				filter: 'text',
				onCellClicked: openEditDialog
			},
			{
				headerName: '',
				width: 80,
				suppressSorting: true,
				suppressMenu: true,
				template: '<button type="button" class="btn btn-xs btn-square" ng-click="vm.openDuplicate(data)" tooltip-append-to-body="true" uib-tooltip="{{ \'General.Buttons.Copy\' | translate }}">'
				+ '<i icon="duplicate"></i>'
				+ '</button> '
				+ '<button type="button" class="btn btn-xs btn-square" ng-click="vm.tryToDelete(data, false)" tooltip-append-to-body="true" uib-tooltip="{{ \'General.Buttons.Delete\' | translate }}">'
				+ '<i icon="remove"></i> '
                + '</button> '
                + '<button type="button" class="btn btn-xs btn-square btn-warning" ng-click="vm.exportJson(data)" ng-if="vm.debug.on" tooltip-append-to-body="true" uib-tooltip="{{ \'General.Buttons.Export\' | translate }}">'
				+ '<i icon="export"></i> '
				+ '</button>'

			}
		];

		activate();

		function activate() {
			svc = contentItemsSvc(appId, contentType);

			// set RowData an Column Definitions
			$q.all([setRowData(), svc.getColumns()])
				.then(function (success) {
					var columnDefs = getColumnDefs(success[1].data);
					vm.gridOptions.api.setColumnDefs(columnDefs);

					// resize outer modal (if needed)
					var bodyWidth = vm.gridOptions.api.gridPanel.eBodyContainer.clientWidth;
					var viewportWidth = vm.gridOptions.api.gridPanel.eBodyViewport.clientWidth;
					if (bodyWidth < viewportWidth)
						setModalWidth(bodyWidth);

					// try to apply some initial filters...
					var filterModel = agGridFilters.get();//
					vm.gridOptions.api.setFilterModel(filterModel);
				});
		}

		function showFilter() {
			var savedModel = vm.gridOptions.api.getFilterModel();
			console.log('current filter: ', savedModel);
			alert('check console for filter information');
		}

		// set width of outer angular-ui-modal. This is a quick and dirty solution because there's no official way to do this.
		// $uibModalStack.getTop() might get a wrong modal Instance
		// setting the width with inline css in a controller should be avoided
		function setModalWidth(width) {
			var modalDomEl = $uibModalStack.getTop().value.modalDomEl;
			var modalDialog = modalDomEl.children();
			modalDialog.css('width', (width + 47) + 'px');	// add some pixels for padding and scrollbars
		}

		function add() {
			eavAdminDialogs.openItemNew(contentType, setRowData);
		}

		function openExport() {
			// check if there is a filter attached
			var ids = null,
				hasFilters = false,
				mod = vm.gridOptions.api.getFilterModel();

			// check if any filters are applied
			for (var prop in mod)
				if (mod.hasOwnProperty(prop)) {
					hasFilters = true;
					break;
				}

			if (hasFilters) {
				ids = [];
				vm.gridOptions.api.forEachNodeAfterFilterAndSort(function (rowNode) {
					ids.push(rowNode.data.Id);
				});
				if (ids.length === 0)
					ids = null;
			}

			// open export but DONT do a refresh callback, because it delays working with the table even though this is export only
			return eavAdminDialogs.openContentExport(appId, contentType, null, ids);
		}

		function openEditDialog(params) {
			eavAdminDialogs.openItemEditWithEntityId(params.data.Id, setRowData);
		}

		// Get/Update Grid Row-Data
		function setRowData() {
			var sortModel = {};
			var filterModel = {};
			if (vm.gridOptions.api) {
				sortModel = vm.gridOptions.api.getSortModel();
				filterModel = vm.gridOptions.api.getFilterModel();
			}

			return svc.liveListSourceRead().then(function (success) {
				vm.gridOptions.api.setRowData(success.data);
				vm.gridOptions.api.setSortModel(sortModel);
				vm.gridOptions.api.setFilterModel(filterModel);
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
					filterParams: { cellRenderer: cellRendererDefaultFilter }
				};


				switch (eavAttribute.Type) {
					case 'Entity':
						try {
							colDef.allowMultiValue = eavAttribute.Metadata.Entity.AllowMultiValue;
						} catch (e) {
							colDef.allowMultiValue = true;
						}

						colDef.cellRenderer = cellRendererEntity;
						colDef.valueGetter = valueGetterEntityField;
						break;
					case 'DateTime':
						try {
							colDef.useTimePicker = eavAttribute.Metadata.DateTime.UseTimePicker;
						} catch (e) {
							colDef.useTimePicker = false;
						}
						colDef.valueGetter = valueGetterDateTime;
						break;
					case 'Boolean':
						colDef.valueGetter = valueGetterBoolean;
						break;
					case 'Number':
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
				return null;

			return rawValue.map(function (item) {
				return item.Title;
			});
		}

		function valueGetterStatusField(params) {
			return [
				params.data.IsPublished ? 'is published' : 'is not published',
				params.data.Metadata ? 'is metadata' : 'is not metadata'
			];
		}

		function valueGetterDateTime(params) {
			var rawValue = params.data[params.colDef.field];
			if (!rawValue)
				return null;

			// remove 'Z' and replace 'T'
			return params.colDef.useTimePicker ? rawValue.substr(0, 19).replace('T', ' ') : rawValue.substr(0, 10);
		}

		function valueGetterBoolean(params) {
			var rawValue = params.data[params.colDef.field];
			if (typeof rawValue != 'boolean')
				return null;

			return rawValue.toString();
		}

		function cellRendererDefault(params) {
			if (typeof (params.value) != 'string' || params.value === null)
				return params.value;

			var encodedValue = htmlEncode(params.value);
			return '<span ng-non-bindable><span title="' + encodedValue + '">' + encodedValue + '</span></span>';
		}

		function cellRendererDefaultFilter(params) {
			return cellRendererDefault(params) || '(empty)';
		}

		// htmlencode strings (source: http://stackoverflow.com/a/7124052)
		function htmlEncode(text) {
			return text.replace(/&/g, '&amp;').replace(/"/g, '&quot;').replace(/'/g, '&#39;').replace(/</g, '&lt;').replace(/>/g, '&gt;');
		}

		function cellRendererEntity(params) {
			if (!Array.isArray(params.value))
				return null;

			var encodedValue = htmlEncode(params.value.join(', '));
			var result = '<span title="' + encodedValue + '">';
			if (params.colDef.allowMultiValue)
				result += '<span class="badge badge-primary">' + params.value.length + '</span> ';
			result += encodedValue + '</span>';

			return result;
		}
		// #endregion

		function tryToDelete(item) {
			entitiesSvc.tryDeleteAndAskForce(contentType, item._RepositoryId, item._Title).then(setRowData);
		}

		function openDuplicate(item) {
			var items = [{
				ContentTypeName: contentType,
				DuplicateEntity: item.Id
			}];
			eavAdminDialogs.openEditItems(items, svc.liveListReload);
		}

		function close() {
			$uibModalInstance.dismiss('cancel');
        }

        function exportJson(item) {
            return contentExportService.exportEntity(appId, item.Id, contentType, true);
        }
	}
}());