// PipelineFactory provides an interface to the Server Backend storing Pipelines and their Pipeline Parts
pipelineDesigner.factory('pipelineFactory', ['$resource', function ($resource) {
    'use strict';

    var pipelineResource = $resource('/api/EAV/PipelineDesigner/:action');

    return {
        // get a Pipeline with Pipeline Info and Pipeline Parts
        getPipeline: function (pipelineEntityId) {
            var getPipeline = pipelineResource.get({ action: 'GetPipeline', pipelineEntityId: pipelineEntityId });

            return getPipeline;
        },
        savePipeline: function (pipeline) {

        }
    }
}]);