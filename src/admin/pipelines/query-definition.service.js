
(function() {

    var guiTypes = buildGuiTypes();

    /*
        shared data state across various components
    */
    angular.module("PipelineDesigner").factory("queryDef",
        function (pipelineId, pipelineService, $q, $location, toastr, $filter, eavConfig) {

            var queryDef = {
                id: pipelineId, // injected from URL
                dsCount: 0,
                readOnly: true,
                data: null,


                // Test wether a DataSource is persisted on the Server
                dataSourceIsPersisted: function(dataSource) {
                    return dataSource.EntityGuid.indexOf("unsaved") === -1;
                },

                addDataSource: function(partAssemblyAndType, visualDesignerData, entityGuid) {
                    if (!visualDesignerData)
                        visualDesignerData = { Top: 100, Left: 100 };

                    var newDataSource = {
                        VisualDesignerData: visualDesignerData,
                        Name: $filter("typename")(partAssemblyAndType, "className"),
                        Description: "",
                        PartAssemblyAndType: partAssemblyAndType,
                        EntityGuid: entityGuid || "unsaved" + (queryDef.dsCount + 1)
                    };
                    // Extend it with a Property to it's Definition
                    newDataSource = angular.extend(newDataSource,
                        pipelineService.getNewDataSource(queryDef.data, newDataSource));

                    queryDef.data.DataSources.push(newDataSource);
                },

                loadQuery: function() {
                    return pipelineService.getPipeline(queryDef.id)
                        .then(function (success) {
                            queryDef.data = success;

                            // If a new (empty) Pipeline is made, init new Pipeline
                            if (!queryDef.id || queryDef.data.DataSources.length === 1) {
                                queryDef.readOnly = false;
                                queryDef.loadQueryFromDefaultTemplate();
                            } else {
                                // if read only, show message
                                queryDef.readOnly = !success.Pipeline.AllowEdit;
                                toastr.info(queryDef.readOnly
                                    ? "This pipeline is read only"
                                    : "You can now design the Pipeline. \nVisit 2sxc.org/help for more.",
                                    "Ready",
                                    { autoDismiss: true });
                            }
                        });
                },


                // Init a new Pipeline with DataSources and Wirings from Configuration
                loadQueryFromDefaultTemplate: function() {
                    var templateForNew = eavConfig.pipelineDesigner.defaultPipeline.dataSources;
                    angular.forEach(templateForNew, function (dataSource) {
                        queryDef.addDataSource(dataSource.partAssemblyAndType, dataSource.visualDesignerData, dataSource.entityGuid);
                    });

                    // testing...
                    queryDef.data.Pipeline = { StreamWiring: eavConfig.pipelineDesigner.defaultPipeline.streamWiring };
                },

                // save the current query and reload entire definition as returned from server
                save: function() {
                    queryDef.readOnly = true;

                    return pipelineService.savePipeline(queryDef.data.Pipeline, queryDef.data.DataSources)
                        .then(function(success) {
                                // Update PipelineData with data retrieved from the Server
                                queryDef.data.Pipeline = success.Pipeline;
                                queryDef.data.TestParameters = success.TestParameters;
                                queryDef.id = success.Pipeline.EntityId;
                                $location.search("PipelineId", success.Pipeline.EntityId);
                                queryDef.readOnly = !success.Pipeline.AllowEdit;
                                queryDef.data.DataSources = success.DataSources;
                                pipelineService.postProcessDataSources(queryDef.data);

                                // communicate to the user...
                                toastr.clear();
                                toastr.success("Pipeline " + success.Pipeline.EntityId + " saved and loaded",
                                    "Saved", { autoDismiss: true });

                            },
                            function(reason) {
                                toastr.error(reason, "Save Pipeline failed");
                                queryDef.readOnly = false;
                            });
                },

                dsTypeInfo: function (dataSource) {
                    // maybe we already retrieved it before...
                    if (dataSource.guiTypeInfo) return dataSource.guiTypeInfo;

                    var typeInfo = null;
                    // try to find the type on the source
                    var found = $filter("filter")(queryDef.data.InstalledDataSources,
                        { PartAssemblyAndType: dataSource.PartAssemblyAndType });
                    if (found && found.length) {
                        var primType = found[0].PrimaryType;
                        typeInfo = Object.assign({}, primType ? guiTypes[primType] : guiTypes.Unknown);
                        if (found[0].Icon) typeInfo.icon = guiTypes.iconPrefix + found[0].Icon;
                        if (found[0].DynamicOut) typeInfo.dynamicOut = true;
                    }
                    if (!typeInfo) typeInfo = guiTypes.Unknown;

                    dataSource.guiTypeInfo = typeInfo;
                    return typeInfo;
                }
            };


            return queryDef;
        });

function buildGuiTypes() {
    var guiTypes = {},
        iconPref = "eav-icon-";

    function addGuiType(name, icon, notes) { guiTypes[name] = { name: name, icon: iconPref + icon, notes: notes }; }

    addGuiType("Unknown", "circle", "unknown type");
    addGuiType("Cache", "history", "caching of data");
    addGuiType("Filter", "filter", "filter data - usually returning less items than came in");
    addGuiType("Logic", "shuffle", "logic operations - usually choosing between different streams");
    addGuiType("Lookup", "search", "lookup operation - usually looking for other data based on a criteria");
    addGuiType("Modify", "star-half-alt", "modify data - usually changing, adding or removing values"); // tod  o
    addGuiType("Security", "user", "security - usually limit what the user sees based on his identity");
    addGuiType("Sort", "sort-alt-up", "sort the items");
    addGuiType("Source", "export", "source of new data - usually SQL, CSV or similar");
    addGuiType("Target", "target", "target - usually just a destination of data");

    guiTypes.iconPrefix = iconPref;
    return guiTypes;
}


})();