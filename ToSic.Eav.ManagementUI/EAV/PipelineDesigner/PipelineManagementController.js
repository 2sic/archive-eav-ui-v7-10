angular.module('pipelineManagement', ['pipelineManagementFactory']).
	config(function ($locationProvider) {
		$locationProvider.html5Mode(true);
	}).
	controller('pipelineManagementController', function ($scope, $location, pipelineManagementFactory) {
		$scope.AppId = $location.search().AppId;
		$scope.pipelines = pipelineManagementFactory.getPipelines($scope.AppId);
	});



angular.module('pipelineManagementFactory', ['ngResource', 'eavGlobalConfigurationProvider']).
	factory('pipelineManagementFactory', function ($resource, $http, eavGlobalConfigurationProvider) {
		'use strict';

		// Web API Service
		var entitiesResource = $resource(eavGlobalConfigurationProvider.api.baseUrl + '/EAV/Entities/:action');
		// Add additional Headers to each http-Request
		angular.extend($http.defaults.headers.common, eavGlobalConfigurationProvider.api.additionalHeaders);

		return {
			getPipelines: function (appId) {
				return entitiesResource.query({ action: 'GetEntities', appId: appId, typeName: 'DataPipeline' });
			}
		}
	})