// Config and Controller for the Pipeline Management UI
angular.module("PipelineManagement", ["PipelineService", "eavGlobalConfigurationProvider", "eavNgSvcs", "EavAdminUi", "Eavi18n"]).
	controller("PipelineManagementController", function ($modalInstance, appId, pipelineService, eavAdminDialogs) {
	    var vm = this;
        vm.appId = appId;

	    pipelineService.setAppId(appId);
	    pipelineService.initContentTypes();
	    // Make URL-Provider available to the scope
	    vm.getPipelineUrl = pipelineService.getPipelineUrl;

	    // Refresh List of Pipelines
	    vm.refresh = function () {
	        vm.pipelines = pipelineService.getPipelines(appId);
	    };
	    vm.refresh();

	    // Delete a Pipeline
        vm.delete = function(pipeline) {
            if (!confirm("Delete Pipeline \"" + pipeline.Name + "\" (" + pipeline.Id + ")?"))
                return;

            pipelineService.deletePipeline(pipeline.Id).then(function() {
                vm.refresh();
            }, function(reason) {
                alert(reason);
            });
        };

	    // Clone a Pipeline
        vm.clone = function(pipeline) {
            pipelineService.clonePipeline(pipeline.Id).then(function() {
                vm.refresh();
            }, function(reason) {
                alert(reason);
            });
        };

        vm.permissions = function (item) {
            return eavAdminDialogs.openPermissionsForGuid(appId, item.Guid);
        };

        vm.add = function add() {
            eavAdminDialogs.openItemNew(pipelineService.dataPipelineAttributeSetId, vm.refresh);
        };

        vm.edit = function edit(item) {
            eavAdminDialogs.openItemEditWithEntityId(item.Id, vm.refresh);
        };

        vm.design = function design(item) {
            return eavAdminDialogs.editPipeline(vm.appId, item.Id, vm.refresh);
        };
        vm.liveEval = function admin() {
            var inp = prompt("This is for very advanced operations. Only use this if you know what you're doing. \n\n Enter admin commands:");
            if (inp)
                eval(inp); // jshint ignore:line
        };
        vm.close = function () { $modalInstance.dismiss("cancel"); };
    });