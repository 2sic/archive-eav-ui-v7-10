// PipelineService provides an interface to the Server Backend storing Pipelines and their Pipeline Parts

angular.module("EavServices")
    .factory("pipelineService", function ($resource, $q, $filter, eavConfig, $http, contentTypeSvc, metadataSvc, eavAdminDialogs, appId) {
        "use strict";
        var svc = {};
        // Web API Service
        svc.pipelineResource = $resource("eav/PipelineDesigner/:action");
        svc.entitiesResource = $resource("eav/Entities/:action");

        svc.appId = 0;

        console.log("ok");

        // Get the Definition of a DataSource
        svc.getDataSourceDefinitionProperty = function (model, dataSource) {
        	var definition = $filter("filter")(model.InstalledDataSources, function(d) {
	            return d.PartAssemblyAndType === dataSource.PartAssemblyAndType;
	        })[0];
        	if (!definition)
        		throw "DataSource Definition not found: " + dataSource.PartAssemblyAndType;
        	return definition;
        };

        // todo refactor: why do we have 2 methods with same name?
        // Extend Pipeline-Model retrieved from the Server
        var postProcessDataSources = function(model) {
            // Append Out-DataSource for the UI
            model.DataSources.push({
                Name: eavConfig.pipelineDesigner.outDataSource.name,
                Description: eavConfig.pipelineDesigner.outDataSource.description,
                EntityGuid: "Out",
                PartAssemblyAndType: eavConfig.pipelineDesigner.outDataSource.className,
                VisualDesignerData: eavConfig.pipelineDesigner.outDataSource.visualDesignerData, 
                ReadOnly: true
            });

            // Extend each DataSource with Definition-Property and ReadOnly Status
            angular.forEach(model.DataSources, function(dataSource) {
                dataSource.Definition = function() { return svc.getDataSourceDefinitionProperty(model, dataSource); };
                dataSource.ReadOnly = dataSource.ReadOnly || !model.Pipeline.AllowEdit;
                dataSource.VisualDesignerData = dataSource.VisualDesignerData || { Top: 50, Left: 50 }; // in case server returns null, use a default setting
            });
        };

        angular.extend(svc, { 
            
            // get a Pipeline with Pipeline Info with Pipeline Parts and Installed DataSources
            getPipeline: function(pipelineEntityId) {
                var deferred = $q.defer();

                var getPipeline = svc.pipelineResource.get({ action: "GetPipeline", id: pipelineEntityId, appId: svc.appId });
                var getInstalledDataSources = svc.pipelineResource.query({ action: "GetInstalledDataSources" });

                // Join and modify retrieved Data
                $q.all([getPipeline.$promise, getInstalledDataSources.$promise]).then(function(results) {
                    var model = JSON.parse(angular.toJson(results[0])); // workaround to remove AngularJS Promise from the result-Objects
                    model.InstalledDataSources = JSON.parse(angular.toJson(results[1]));

                    // Init new Pipeline Object
                    if (!pipelineEntityId) {
                        model.Pipeline = {
                            AllowEdit: "True"
                        };
                    }

                    // Add Out-DataSource for the UI
                    model.InstalledDataSources.push({
                        PartAssemblyAndType: eavConfig.pipelineDesigner.outDataSource.className,
                        ClassName: eavConfig.pipelineDesigner.outDataSource.className,
                        In: eavConfig.pipelineDesigner.outDataSource.in,
                        Out: null,
                        allowNew: false,
                        PrimaryType: "Target",
                        DynamicOut: false
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
                if ($filter("filter")(model.DataSources, function(d) { return d.EntityGuid === "Out"; })[0])
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
                    return $q.reject("appId must be set to save a Pipeline");

                // Remove some Properties from the DataSource before Saving
                var dataSourcesPrepared = [];
                angular.forEach(dataSources, function(dataSource) {
                    var dataSourceClone = angular.copy(dataSource);
                    delete dataSourceClone.ReadOnly;
                    dataSourcesPrepared.push(dataSourceClone);
                });

                return svc.pipelineResource.save({
                    action: "SavePipeline",
                    appId: svc.appId,
                    Id: pipeline.EntityId /*id later EntityId */
                }, { pipeline: pipeline, dataSources: dataSourcesPrepared }).$promise;
            },
            // clone a whole Pipeline
            clonePipeline: function(pipelineEntityId) {
                return svc.pipelineResource.get({ action: "ClonePipeline", appId: svc.appId, Id: pipelineEntityId }).$promise;
            },


            // Get the URL to configure a DataSource
            editDataSourcePart: function (dataSource, allSources) {
                var sourceDef = $filter("filter")(allSources, { PartAssemblyAndType: dataSource.PartAssemblyAndType }, true)[0];
                console.log(sourceDef);
                var contentTypeName = (sourceDef && sourceDef.ContentType)
                    ? sourceDef.ContentType
                    : "|Config " + $filter("typename")(dataSource.PartAssemblyAndType, "classFullName"); // todo refactor centralize

                var assignmentObjectTypeId = 4; // todo refactor centralize this constant
                var keyGuid = dataSource.EntityGuid;

                // Query for existing Entity
                metadataSvc.getMetadata(assignmentObjectTypeId, keyGuid, contentTypeName).then(function (result) { 
                    var success = result.data;
                    if (success.length) // Edit existing Entity
                        eavAdminDialogs.openItemEditWithEntityId(success[0].Id);
                    else { // Check if the type exists, and if yes, create new Entity

                        contentTypeSvc(appId, "System").getDetails(contentTypeName, { ignoreErrors: true })
                            .then(function() {
                                var items = [
                                    {
                                        ContentTypeName: contentTypeName,
                                        Metadata: {
                                            TargetType: assignmentObjectTypeId,
                                            KeyType: "guid",
                                            Key: keyGuid
                                        }
                                    }
                                ];
                                eavAdminDialogs.openEditItems(items);
                            },
                            function () {
                                alert("Server reports error - this usually means that this data-source doesn't have any configuration");
                            });



                    }
                });
            }

        });

        angular.extend(svc, {
            // Query the Data of a Pipeline
            queryPipeline: function (id) {
                return svc.pipelineResource.get({ action: "QueryPipeline", appId: svc.appId, id: id }).$promise;
            },
            // set appId and init some dynamic configurations
            setAppId: function (newAppId) {
                svc.appId = newAppId;
            },

            // Get all Pipelines of current App
            getPipelines: function () {
                return svc.entitiesResource.query({ action: "GetEntities", appId: svc.appId, contentType: "DataPipeline" });
            },
            // Delete a Pipeline on current App
            deletePipeline: function (id) {
                return svc.pipelineResource.get({ action: "DeletePipeline", appId: svc.appId, id: id }).$promise;
            }
        });

        return svc;
    });