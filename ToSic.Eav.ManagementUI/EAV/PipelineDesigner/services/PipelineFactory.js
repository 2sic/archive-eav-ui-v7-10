// PipelineFactory provides an interface to the Server Backend storing Pipelines and their Pipeline Parts
pipelineDesigner.factory('pipelineFactory', ['$resource', '$q', '$filter', function ($resource, $q, $filter) {
	'use strict';

	var pipelineResource = $resource('/api/EAV/PipelineDesigner/:action');

	return {
		// get a Pipeline with Pipeline Info with Pipeline Parts and Installed DataSources
		getPipeline: function (pipelineEntityId) {
			var deferred = $q.defer();

			var getPipeline = pipelineResource.get({ action: 'GetPipeline', pipelineEntityId: pipelineEntityId });
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
					VisualDesignerData: { Top: 40, Left: 410 },
					AllowDelete: false
				});
				model.InstalledDataSources.push({
					PartAssemblyAndType: "Out",
					ClassName: "Out",
					In: ["Content", "Presentation", "ListContent", "ListPresentation"],
					Out: null
				});

				// Add Navigation-Property from each DataSource to its Definition
				angular.forEach(model.DataSources, function (dataSource) {
					dataSource.Definition = $filter('filter')(model.InstalledDataSources, function (d) { return d.PartAssemblyAndType == dataSource.PartAssemblyAndType; })[0];
				});

				deferred.resolve(model);
			});

			return deferred.promise;
		},
		savePipeline: function (pipeline) {

		}
	}
}]);