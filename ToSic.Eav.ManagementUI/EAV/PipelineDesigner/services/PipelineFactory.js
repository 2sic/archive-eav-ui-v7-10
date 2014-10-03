// PipelineFactory provides an interface to the Server Backend storing Pipelines and their Pipeline Parts
pipelineDesigner.factory('pipelineFactory', ['$resource', '$q', function ($resource, $q) {
	'use strict';

	var pipelineResource = $resource('/api/EAV/PipelineDesigner/:action');

	return {
		// get a Pipeline with Pipeline Info and Pipeline Parts
		getPipeline: function (pipelineEntityId) {

			//console.log("Begin getPipeline");

			var deferred = $q.defer();

			var getPipeline = pipelineResource.get({ action: 'GetPipeline', pipelineEntityId: pipelineEntityId });
			var getInstalledDataSources = pipelineResource.query({ action: 'GetInstalledDataSources' });


			var promises = $q.all([getPipeline, getInstalledDataSources]).then(function (results) {
				var model = {
					Pipeline: results[0].Pipeline,
					DataSources: results[0].DataSources,
					//InstalledDataSources: results[1],
				};
				//var getPipelineResult = results[0];
				//getPipelineResult.test = "TEST123";
				//console.log(getPipelineResult);
				//getPipelineResult.installedDataSources = results[1];
				//var installedDataSources = results[1];
				console.log(results);
				console.log(results.length);
				console.log(results[0]);
				console.log(results[0].DataSources);

				//deferred.resolve(results[0]);
				deferred.resolve({ "Pipeline": "test" });
				//console.log("Values:");
				//console.log(values[0]);
				//console.log(values[1]);
			});

			//console.log(getPipeline);
			//console.log(promises);

			//.then(function (res1) {
			//	//res = { "Test": "Test" };
			//	console.log("all resolved");

			//	deferred.resolve(['Hello', 'world!']);


			//	console.log(res1);
			//});

			//console.log("End getPipeline");
			return deferred.promise;

			//// modify retrieved Data
			//getPipeline.$promise.then(function (data) {
			//	// Append Out-DataSource
			//	data.DataSources.push({
			//		Name: "Out",
			//		EntityGuid: "Out",
			//		PartAssemblyAndType: "Out",
			//		VisualDesignerData: { Top: 40, Left: 410 }
			//	});
			//});

			//console.log($q.all(getPipeline));

			//return $q.all(getPipeline, dataSources);
		},
		savePipeline: function (pipeline) {

		}
	}
}]);