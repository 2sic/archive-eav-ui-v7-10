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

	function contentItemsListController(contentItemsSvc, eavConfig, appId, contentType, eavAdminDialogs, debugState, $modalInstance, $timeout, $q) {
		/* jshint validthis:true */
		var vm = this;
		vm.debug = debugState;
		var svc;

		var staticColumns = [
	{ headerName: "ID", field: "Id" },
	{ headerName: "Status", field: "IsPublished" },
	{ headerName: "Title", field: "Title" },
	{ headerName: "Actions", field: "IsPublished" }
		];


		vm.gridOptions = {
			columnDefs: staticColumns,
			//rowData: createRowData(),
			//rowData: staticRowData,
			//rowSelection: 'multiple',
			//enableColResize: true,
			enableSorting: true,
			enableFilter: true,
			//groupHeaders: true,
			rowHeight: 22,
			//pinnedColumnCount: 3,
			//onModelUpdated: onModelUpdated,
			//suppressRowClickSelection: true,
		};

		activate();

		function activate() {
			svc = contentItemsSvc(appId, contentType);

			svc.getColumns().then(function (success) {
				var columnDefs = getColumnDefs(success.data);
				vm.gridOptions.api.setColumnDefs(columnDefs);
			});

			svc.liveListSourceRead().then(function (success) {
				vm.gridOptions.api.setRowData(success.data);
			});

		}

		vm.add = function add() {
			eavAdminDialogs.openItemNew(contentType, svc.liveListReload);
		};

		vm.edit = function (item) {
			eavAdminDialogs.openItemEditWithEntityId(item.Id, svc.liveListReload);
		};


		function getColumnDefs(eavAttributes) {
			var columnDefs = staticColumns;

			angular.forEach(eavAttributes, function (eavAttribute) {
				if (eavAttribute.IsTitle) return;	// don't add Title-Field twice

				columnDefs.push({
					headerName: eavAttribute.StaticName,
					field: eavAttribute.StaticName,
					cellRenderer: function (params) {
						// htmlencode strings (source: http://stackoverflow.com/a/7124052)
						if (typeof (params.value) == "string")
							return params.value.replace(/&/g, '&amp;').replace(/"/g, '&quot;').replace(/'/g, '&#39;').replace(/</g, '&lt;').replace(/>/g, '&gt;');
						return params.value;
					}
				});
			});

			return columnDefs;
		}

		//vm.refresh = svc.liveListReload;

		//vm.items = svc.liveList();

		//vm.dynamicColumns = [];
		//svc.getColumns().then(function (result) {
		//    var cols = result.data;
		//    for (var c = 0; c < cols.length && c < vm.maxDynamicColumns; c++) {
		//        if (!cols[c].IsTitle)
		//            vm.dynamicColumns.push(cols[c]);
		//    }
		//});

		//vm.tryToDelete = function tryToDelete(item) {
		//    if (confirm("Delete '" + "title-unknown-yet" + "' (" + item.RepositoryId + ") ?"))
		//        svc.delete(item.RepositoryId);
		//};

		//vm.openDuplicate = function openDuplicate(item) {
		//    var items = [
		//        {
		//            ContentTypeName: contentType,
		//            DuplicateEntity: item.Id
		//        }
		//    ];
		//    eavAdminDialogs.openEditItems(items, svc.liveListReload);

		//};

		vm.close = function () {
			$modalInstance.dismiss("cancel");
		};

	}

}());