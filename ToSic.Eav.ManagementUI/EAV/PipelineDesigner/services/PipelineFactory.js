// PipelineFactory provides an interface to the Server Backend storing Pipelines and their Pipeline Parts
pipelineDesigner.factory('pipelineFactory', ['$resource', function ($resource) {
    'use strict';

    var pipelineResource = $resource('/api/EAV/PipelineDesigner/:action');

    return {
        // get a Pipeline with Pipeline Info and Pipeline Parts
        getPipeline: function (pipelineEntityId) {
            var getPipeline = pipelineResource.get({ action: 'GetPipeline', pipelineEntityId: pipelineEntityId });

            // modify retrieved Data
            getPipeline.$promise.then(function (data) {
                // modify Pipeline
                // split StreamWiring-String with Regex
                var connectionList = [];
                var streamWirings = data.Pipeline.StreamWiring.split("\r\n");
                var wiringRegex = /(.*):(.*)>(.*):(.*)/;
                angular.forEach(streamWirings, function (wiring) {
                    var match = wiring.match(wiringRegex);
                    var connection = { from: match[1], out: match[2], to: match[3], in: match[4] };
                    connectionList.push(connection);
                });
                data.Pipeline.ConnectionList = connectionList;


                // modify DataSources
                angular.forEach(data.DataSources, function (dataSource) {
                    // convert VisualDesignerData to a JSON Object
                    dataSource.VisualDesignerData = angular.fromJson(dataSource.VisualDesignerData);
                });
            });

            return getPipeline;
        },
        savePipeline: function (pipeline) {

        }
    }
}]);