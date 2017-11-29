// Config and Controller for the Pipeline Management UI
angular.module('PipelineManagement', [
    'EavServices',
    'EavConfiguration',
    'eavNgSvcs',
    'EavAdminUi'
]).
    controller('PipelineManagement', function ($uibModalInstance, appId, pipelineService, debugState, eavAdminDialogs, eavConfig, contentExportService) {
        var vm = this;
        vm.debug = debugState;
        vm.appId = appId;

        pipelineService.setAppId(appId);

        // Refresh List of Pipelines
        vm.refresh = function () {
            vm.pipelines = pipelineService.getPipelines(appId);
        };
        vm.refresh();

        // Delete a Pipeline
        vm.delete = function (pipeline) {
            if (!confirm('Delete Pipeline "' + pipeline.Name + '" (' + pipeline.Id + ')?'))
                return;

            pipelineService.deletePipeline(pipeline.Id).then(function () {
                vm.refresh();
            }, function (reason) {
                alert(reason);
            });
        };

        // Clone a Pipeline
        vm.clone = function (pipeline) {
            pipelineService.clonePipeline(pipeline.Id).then(function () {
                vm.refresh();
            }, function (reason) {
                alert(reason);
            });
        };

        vm.permissions = function (item) {
            return eavAdminDialogs.openPermissionsForGuid(appId, item.Guid);
        };

        vm.export = function (item) {
            return contentExportService.exportEntity(appId, item.Id, 'Query', true);
        };

        vm.add = function add() {
            var items = [{
                ContentTypeName: 'DataPipeline',
                Prefill: { TestParameters: eavConfig.pipelineDesigner.testParameters }
            }];
            eavAdminDialogs.openEditItems(items, vm.refresh);
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
        vm.close = function () { $uibModalInstance.dismiss('cancel'); };
    });