// PipelineFactory provides an interface to the Server Backend storing Pipelines and their Pipeline Parts
pipelineDesigner.factory('pipelineFactory', ['$resource', '$q', '$filter', 'eavGlobalConfigurationProvider', function ($resource, $q, $filter, eavGlobalConfigurationProvider) {
	'use strict';

	// Web API Service
	var pipelineResource = $resource(eavGlobalConfigurationProvider.apiBaseUrl + '/EAV/PipelineDesigner/:action');

	// Get the Definition of a DataSource
	var getDataSourceDefinitionProperty = function (model, dataSource) {
		return $filter('filter')(model.InstalledDataSources, function (d) { return d.PartAssemblyAndType == dataSource.PartAssemblyAndType; })[0];
	};

	// Extend Pipeline-Model retrieved from the Server
	var postProcessDataSources = function (model) {
		// Append Out-DataSource for the UI
		model.DataSources.push({
			Name: '2SexyContent Module',
			Description: 'The module/template which will show this data',
			EntityGuid: 'Out',
			PartAssemblyAndType: 'SexyContentTemplate',
			VisualDesignerData: { Top: 50, Left: 410 },
			ReadOnly: true
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

				// Add Out-DataSource for the UI
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
		// Ensure Model has all DataSources and they're linked to their Definition-Object
		postProcessDataSources: function (model) {
			postProcessDataSources(model);
		},
		// Get a JSON for a DataSource with Definition-Property
		getNewDataSource: function (model, dataSourceBase) {
			return { Definition: function () { return getDataSourceDefinitionProperty(model, dataSourceBase); } }
		},
		// Save whole Pipline
		savePipeline: function (appId, pipeline, dataSources) {
			if (!appId)
				return $q.reject('AppId must be set to save a Pipeline');

			return pipelineResource.save({ action: 'SavePipeline', appId: appId, Id: pipeline.EntityId }, { pipeline: pipeline, dataSources: dataSources }).$promise;
		},
		// Get the URL
		getDataSourceConfigurationUrl: function (appId, dataSource) {
			return pipelineResource.get({
				action: 'GetDataSourceConfigurationUrl',
				appId: appId,
				dataSourceEntityGuid: dataSource.EntityGuid,
				partAssemblyAndType: dataSource.PartAssemblyAndType,
				newItemUrl: eavGlobalConfigurationProvider.newItemUrl + '&PreventRedirect=true',
				editItemUrl: eavGlobalConfigurationProvider.editItemUrl + '&PreventRedirect=true'
			}).$promise;
		}
	}
}]);