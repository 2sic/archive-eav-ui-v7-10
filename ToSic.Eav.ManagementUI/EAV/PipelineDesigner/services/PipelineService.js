pipelineDesigner.factory('pipelineFactory', ['$resource', '$location', function ($resource, $location) {
	'use strict';

	var pipelineResource = $resource('/api/pipeline/:pipelineEntityId');

	var pipelineId = $location.search().PipelineId;
	var pipeline = pipelineResource.get({ pipelineEntityId: $location.search().PipelineId });

	return {
		getPipeline: function () {
			return {
				dataSources: [
					{
						guid: 'guid1',
						typeName: 'ToSic.ToSexy.DataSources.ModuleDataSource, ToSic.ToSexy',
						name: 'Module Data Source',
						description: 'Provides data to the module',
						top: 47,
						left: 700
					},
					{
						guid: 'guid2',
						typeName: 'ToSic.Eav.DataSources.Caches.ICache, ToSic.Eav',
						name: 'Cached DB',
						description: '',
						top: 287,
						left: 390
					}
				],
				connections:
				[
					{ from: 'guid1', out: 'DefaultOut', to: 'guid2', in: 'DefaultIn' },
					{ from: 'guid2', out: 'DefaultOut', to: 'guid1', in: 'DefaultIn' }
				]
			}
		},
		savePipeline: function (pipeline) {

		}
	}
}]);