// PipelineService provides an interface to the Server Backend storing Pipelines and their Pipeline Parts
angular.module('PipelineService', ['ContentTypeServices', 'ngResource'])
    .factory('pipelineService', function($resource, $q, $filter, eavGlobalConfigurationProvider, $http, contentTypeSvc) {
        'use strict';
        var svc = {};
        // Web API Service
        svc.pipelineResource = $resource(eavGlobalConfigurationProvider.api.baseUrl + '/EAV/PipelineDesigner/:action');
        svc.entitiesResource = $resource(eavGlobalConfigurationProvider.api.baseUrl + '/EAV/Entities/:action');

        svc.dataPipelineAttributeSetId = 0;
        svc.appId = 0;

        // Get the Definition of a DataSource
        svc.getDataSourceDefinitionProperty = function(model, dataSource) {
            return $filter('filter')(model.InstalledDataSources, function(d) { return d.PartAssemblyAndType == dataSource.PartAssemblyAndType; })[0];
        };

        // todo refactor: why do we have 2 methods with same name?
        // Extend Pipeline-Model retrieved from the Server
        var postProcessDataSources = function(model) {
            // Append Out-DataSource for the UI
            model.DataSources.push({
                Name: eavGlobalConfigurationProvider.pipelineDesigner.outDataSource.name,
                Description: eavGlobalConfigurationProvider.pipelineDesigner.outDataSource.description,
                EntityGuid: 'Out',
                PartAssemblyAndType: eavGlobalConfigurationProvider.pipelineDesigner.outDataSource.className,
                VisualDesignerData: eavGlobalConfigurationProvider.pipelineDesigner.outDataSource.visualDesignerData,
                ReadOnly: true
            });

            // Extend each DataSource with Definition-Property and ReadOnly Status
            angular.forEach(model.DataSources, function(dataSource) {
                dataSource.Definition = function() { return svc.getDataSourceDefinitionProperty(model, dataSource); };
                dataSource.ReadOnly = dataSource.ReadOnly || !model.Pipeline.AllowEdit;
            });
        };

        angular.extend(svc, { 
            
            // get a Pipeline with Pipeline Info with Pipeline Parts and Installed DataSources
            getPipeline: function(pipelineEntityId) {
                var deferred = $q.defer();

                var getPipeline = svc.pipelineResource.get({ action: 'GetPipeline', id: pipelineEntityId, appId: svc.appId });
                var getInstalledDataSources = svc.pipelineResource.query({ action: 'GetInstalledDataSources' });

                // Join and modify retrieved Data
                $q.all([getPipeline.$promise, getInstalledDataSources.$promise]).then(function(results) {
                    var model = JSON.parse(angular.toJson(results[0])); // workaround to remove AngularJS Promise from the result-Objects
                    model.InstalledDataSources = JSON.parse(angular.toJson(results[1]));

                    // Init new Pipeline Object
                    if (!pipelineEntityId) {
                        model.Pipeline = {
                            AllowEdit: 'True'
                        };
                    }

                    // Add Out-DataSource for the UI
                    model.InstalledDataSources.push({
                        PartAssemblyAndType: eavGlobalConfigurationProvider.pipelineDesigner.outDataSource.className,
                        ClassName: eavGlobalConfigurationProvider.pipelineDesigner.outDataSource.className,
                        In: eavGlobalConfigurationProvider.pipelineDesigner.outDataSource.in,
                        Out: null,
                        allowNew: false
                    });

                    postProcessDataSources(model);

                    deferred.resolve(model);
                }, function(reason) {
                    deferred.reject(reason);
                });

                return deferred.promise;
            },
            // Ensure Model has all DataSources and they're linked to their Definition-Object
            postProcessDataSources: function(model) {
                // stop Post-Process if the model already contains the Out-DataSource
                if ($filter('filter')(model.DataSources, function(d) { return d.EntityGuid == 'Out'; })[0])
                    return;

                postProcessDataSources(model);
            },
            // Get a JSON for a DataSource with Definition-Property
            getNewDataSource: function(model, dataSourceBase) {
                return {
                    Definition: function() { return svc.getDataSourceDefinitionProperty(model, dataSourceBase); }
                };
            },
            // Save whole Pipline
            savePipeline: function(pipeline, dataSources) {
                if (!svc.appId)
                    return $q.reject('appId must be set to save a Pipeline');

                // Remove some Properties from the DataSource before Saving
                var dataSourcesPrepared = [];
                angular.forEach(dataSources, function(dataSource) {
                    var dataSourceClone = angular.copy(dataSource);
                    delete dataSourceClone.ReadOnly;
                    dataSourcesPrepared.push(dataSourceClone);
                });

                return svc.pipelineResource.save({
                    action: 'SavePipeline',
                    appId: svc.appId,
                    Id: pipeline.EntityId /*id later EntityId */
                }, { pipeline: pipeline, dataSources: dataSourcesPrepared }).$promise;
            },
            // clone a whole Pipeline
            clonePipeline: function(pipelineEntityId) {
                return svc.pipelineResource.get({ action: 'ClonePipeline', appId: svc.appId, Id: pipelineEntityId }).$promise;
            },
            // Get the URL to configure a DataSource
            getDataSourceConfigurationUrl: function(dataSource) {
                var dataSourceFullName = $filter('typename')(dataSource.PartAssemblyAndType, 'classFullName');
                var contentTypeName = '|Config ' + dataSourceFullName; // todo refactor centralize
                var assignmentObjectTypeId = 4; // todo refactor centralize
                var keyGuid = dataSource.EntityGuid;
                var preventRedirect = true;

                var deferred = $q.defer();

                // Query for existing Entity
                svc.entitiesResource.query({ action: 'GetAssignedEntities', appId: svc.appId, assignmentObjectTypeId: assignmentObjectTypeId, keyGuid: keyGuid, contentType: contentTypeName }, function (success) {
                    if (success.length) // Edit existing Entity
                        deferred.resolve(eavGlobalConfigurationProvider.itemForm.getEditItemUrl(success[0].Id /*EntityId*/, null, preventRedirect));
                    else { // Create new Entity
                        // todo: this is a get-content-type, it shouldn't be using the entitiesResource
                        // todo: but I'm not sure when it is being used
                        svc.entitiesResource.get({ action: 'GetContentType', appId: svc.appId, contentType: contentTypeName }, function (contentType) {
                            // test for "null"-response
                            if (contentType[0] == 'n' && contentType[1] == 'u' && contentType[2] == 'l' && contentType[3] == 'l')
                                deferred.reject('Content Type ' + contentTypeName + ' not found.');
                            else
                                deferred.resolve(eavGlobalConfigurationProvider.itemForm.getNewItemUrl(contentType.AttributeSetId, assignmentObjectTypeId, { KeyGuid: keyGuid, ReturnUrl: null }, preventRedirect));
                        }, function(reason) {
                            deferred.reject(reason);
                        });
                    }
                }, function(reason) {
                    deferred.reject(reason);
                });

                return deferred.promise;
            },

        });

        angular.extend(svc, {
            // Query the Data of a Pipeline
            queryPipeline: function (id) {
                return svc.pipelineResource.get({ action: 'QueryPipeline', appId: svc.appId, id: id }).$promise;
            },
            // set appId and init some dynamic configurations
            setAppId: function (newAppId) {
                svc.appId = newAppId;
            },
            // Init some Content Types, currently only used for getPipelineUrl('new', ...)
            initContentTypes: function initContentTypes() {
                return contentTypeSvc(svc.appId).getDetails('DataPipeline').then(function (result) {
                    svc.dataPipelineAttributeSetId = result.data.AttributeSetId;
                });
            },
            // Get all Pipelines of current App
            getPipelines: function () {
                return svc.entitiesResource.query({ action: 'GetEntities', appId: svc.appId, contentType: 'DataPipeline' });
            },
            // Delete a Pipeline on current App
            deletePipeline: function (id) {
                return svc.pipelineResource.get({ action: 'DeletePipeline', appId: svc.appId, id: id }).$promise;
            }
        });

        return svc;
    });