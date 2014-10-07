// PipelineFactory provides an interface to the Server Backend storing Pipelines and their Pipeline Parts
pipelineDesigner.factory('pipelineFactory', ['$resource', '$q', '$filter', function ($resource, $q, $filter) {
	'use strict';

	var pipelineResource = $resource('/api/EAV/PipelineDesigner/:action');

	var getDataSourceDefinitionProperty = function (model, dataSource) {
		return $filter('filter')(model.InstalledDataSources, function (d) { return d.PartAssemblyAndType == dataSource.PartAssemblyAndType; })[0];
	};

	return {
		// get a Pipeline with Pipeline Info with Pipeline Parts and Installed DataSources
		getPipeline: function (pipelineEntityId) {
			var deferred = $q.defer();

			var getPipeline = pipelineResource.get({ action: 'GetPipeline', id: pipelineEntityId });
			var getInstalledDataSources = pipelineResource.query({ action: 'GetInstalledDataSources' });

			// Join and modify retrieved Data
			$q.all([getPipeline.$promise, getInstalledDataSources.$promise]).then(function (results) {
				var model = JSON.parse(angular.toJson(results[0]));	// workaround to remove AngularJS Promise from the result-Objects
				model.InstalledDataSources = JSON.parse(angular.toJson(results[1]));

				// Append Out-DataSource
				model.DataSources.push({
					Name: "Out",
					EntityGuid: "Out",
					PartAssemblyAndType: "Out",
					VisualDesignerData: { Top: 50, Left: 410 },
					AllowDelete: false
				});
				model.InstalledDataSources.push({
					PartAssemblyAndType: "Out",
					ClassName: "Out",
					In: ["Content", "Presentation", "ListContent", "ListPresentation"],
					Out: null
				});

				// Add Definition to each DataSource
				angular.forEach(model.DataSources, function (dataSource) {
					dataSource.Definition = function () { return getDataSourceDefinitionProperty(model, dataSource); }
				});

				deferred.resolve(model);
			});

			return deferred.promise;
		},
		getNewDataSource: function (model, dataSourceBase) {
			return { Definition: function () { return getDataSourceDefinitionProperty(model, dataSourceBase); } }
		},
		savePipeline: function (appId, pipeline, dataSources) {
			pipelineResource.save({ action: 'SavePipeline', appId: appId, Id: pipeline.EntityId }, { pipeline: pipeline, dataSources: dataSources });
		}
	}
}]);