﻿// PipelineFactory provides an interface to the Server Backend storing Pipelines and their Pipeline Parts
pipelineDesigner.factory('pipelineFactory', ['$resource', function ($resource) {
    'use strict';

    var pipelineResource = $resource('/api/EAV/PipelineDesigner/:action');

    return {
        // get a Pipeline with Pipeline Info and Pipeline Parts
        getPipeline: function (pipelineEntityId) {
            var getPipeline = pipelineResource.get({ action: 'GetPipeline', pipelineEntityId: pipelineEntityId });

            // modify retrieved Data
            getPipeline.$promise.then(function (data) {
                // Add Out-DataSource
                data.DataSources.push({
                    Name: "Out",
                    EntityGuid: "Out",
                    PartAssemblyAndType: "Out",
                    VisualDesignerData: { Top: 100, Left: 100 }
                });
            });


            return getPipeline;
        },
        savePipeline: function (pipeline) {

        }
    }
}]);