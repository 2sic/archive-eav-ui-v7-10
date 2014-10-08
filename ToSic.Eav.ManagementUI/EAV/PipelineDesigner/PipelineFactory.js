// PipelineFactory provides an interface to the Server Backend storing Pipelines and their Pipeline Parts
pipelineDesigner.factory('pipelineFactory', ['$resource', '$q', '$filter', function ($resource, $q, $filter) {
	'use strict';

	var pipelineResource = $resource('/api/EAV/PipelineDesigner/:action');

	var getDataSourceDefinitionProperty = function (model, dataSource) {
		return $filter('filter')(model.InstalledDataSources, function (d) { return d.PartAssemblyAndType == dataSource.PartAssemblyAndType; })[0];
	};

	var postProcessDataSources = function (model) {
		// Append Out-DataSource
		model.DataSources.push({
			Name: '2SexyContent Module',
			Description: 'The module/template which will show this data',
			EntityGuid: 'Out',
			PartAssemblyAndType: 'SexyContentTemplate',
			VisualDesignerData: { Top: 50, Left: 410 }
		});

		// Add Definition to each DataSource
		angular.forEach(model.DataSources, function (dataSource) {
			dataSource.Definition = function () { return getDataSourceDefinitionProperty(model, dataSource); }
		});
	};

	return {
		// get a Pipeline with Pipeline Info with Pipeline Parts and Installed DataSources
		getPipeline: function (pipelineEntityId, appId) {
			var deferred = $q.defer();

			var getPipeline = pipelineResource.get({ action: 'GetPipeline', id: pipelineEntityId, appId: appId });
			var getInstalledDataSources = pipelineResource.query({ action: 'GetInstalledDataSources' });

			// Join and modify retrieved Data
			$q.all([getPipeline.$promise, getInstalledDataSources.$promise]).then(function (results) {
				var model = JSON.parse(angular.toJson(results[0]));	// workaround to remove AngularJS Promise from the result-Objects
				model.InstalledDataSources = JSON.parse(angular.toJson(results[1]));

				// Init new Pipeline Object
				if (!pipelineEntityId) {
					model.Pipeline = {
						AllowEdit: 'True'
					};
				}

				model.InstalledDataSources.push({
					PartAssemblyAndType: 'SexyContentTemplate',
					ClassName: 'SexyContentTemplate',
					In: ['Content', 'Presentation', 'ListContent', 'ListPresentation'],
					Out: null
				});

				postProcessDataSources(model);

				deferred.resolve(model);
			}, function (reason) {
				deferred.reject(reason);
			});

			return deferred.promise;
		},
		postProcessDataSources: function (model) {
			postProcessDataSources(model);
		},
		getNewDataSource: function (model, dataSourceBase) {
			return { Definition: function () { return getDataSourceDefinitionProperty(model, dataSourceBase); } }
		},
		savePipeline: function (appId, pipeline, dataSources) {
			if (!appId)
				return $q.reject('AppId must be set to save a Pipeline');

			return pipelineResource.save({ action: 'SavePipeline', appId: appId, Id: pipeline.EntityId }, { pipeline: pipeline, dataSources: dataSources }).$promise;
		}
	}
}]);