using System;
using System.Collections;
using System.Collections.Generic;
using System.Web.Http;
using System.Web.Script.Serialization;
using ToSic.Eav.DataSources;

namespace ToSic.Eav.ManagementUI.API
{
    public class PipelineDesignerController : ApiController
    {
        private readonly EavContext _context = EavContext.Instance(DataSource.DefaultZoneId, DataSource.MetaDataAppId);

        public PipelineDesignerController()
        {
            _context.UserName = null;	// ToDo: Ensure UserName
        }

        /// <summary>
        /// Get a Pipeline with DataSources
        /// </summary>
        [HttpGet]
        public Dictionary<string, object> GetPipeline(int pipelineEntityId)
        {
            #region Get the Entity descripting the Pipeline
            var source = DataSource.GetInitialDataSource(DataSource.DefaultZoneId, DataSource.MetaDataAppId);
            var metaDataSource = DataSource.GetMetaDataSource(source.ZoneId, source.AppId);

            var entities = source["Default"].List;
            IEntity pipelineEntity;
            try
            {
                pipelineEntity = entities[pipelineEntityId];
                if (pipelineEntity.Type.StaticName != "DataPipeline")
                    throw new Exception("pipelineEntityId is not an DataPipeline Entity");
            }
            catch (Exception)
            {
                throw new ArgumentException(string.Format("Could not load Pipeline-Entity with ID {0}.", pipelineEntityId), "pipelineEntityId");
            }
            #endregion

            // Get DataSources in this Pipeline
            var dataSources = metaDataSource.GetAssignedEntities(DataSource.AssignmentObjectTypeIdDataPipeline, pipelineEntity.EntityGuid);

            #region Deserialize some Entity-Values
            var pipelineJson = Helpers.GetEntityValues(pipelineEntity);
            pipelineJson["StreamWiring"] = DataPipelineWiring.Deserialize((string)pipelineJson["StreamWiring"]);

            var dataSourcesJson = new List<Dictionary<string, object>>();
            var ser = new JavaScriptSerializer();
            foreach (var dataSource in Helpers.GetEntityValues(dataSources))
            {
                dataSource["VisualDesignerData"] = ser.Deserialize<object>((string)dataSource["VisualDesignerData"]);
                dataSourcesJson.Add(dataSource);
            }
            #endregion

            // return consolidated Data
            return new Dictionary<string, object>
            {
                {"Pipeline", pipelineJson},
                {"DataSources", dataSourcesJson},
                {"DataSourcesDefinitions", GetInstalledDataSources()}
            };
        }



        /// <summary>
        /// Get installed DataSources from .NET Runtime as InMemory-Entities
        /// </summary>
        private static IEnumerable<DataSourceInfo> GetInstalledDataSources()
        {
            var result = new List<DataSourceInfo>();
            var installedDataSources = DataSource.GetInstalledDataSources();
            foreach (var dataSource in installedDataSources)
            {
                var dataSourceInstance = (IDataSource)Activator.CreateInstance(dataSource);
                ICollection<string> inStreamNames = null;
                ICollection<string> outStreamNames;
                try
                {
                    outStreamNames = dataSourceInstance.Out.Keys;
                }
                catch (Exception)
                {
                    outStreamNames = null;
                }

                result.Add(new DataSourceInfo
                {
                    PartAssemblyAndType = dataSource.FullName + ", " + dataSource.Assembly.GetName().Name,
                    In = inStreamNames,
                    Out = outStreamNames
                });
            }

            return result;
        }

        public struct DataSourceInfo
        {
            public string PartAssemblyAndType { get; set; }
            public ICollection<string> In { get; set; }
            public ICollection<string> Out { get; set; }
        }

        /// <summary>
        /// Update an Entity with new values. Values not in the list will not change at the moment.
        /// </summary>
        public bool UpdateEntity(int entityId, IDictionary newValues)
        {

            return _context.UpdateEntity(entityId, newValues) != null;
        }

        /// <summary>
        /// Update an Entity with new values. Values not in the list will not change at the moment.
        /// </summary>
        public bool UpdateEntityByGuid(Guid entityGuid, IDictionary newValues)
        {
            return _context.UpdateEntity(entityGuid, newValues) != null;
        }

        /// <summary>
        /// Update an Entity with new values. Values not in the list will not change at the moment.
        /// </summary>
        public bool AddEntity(int attributeSetId, IDictionary values, int assignmentObjectType, Guid keyGuid)
        {
            var newEntity = _context.AddEntity(attributeSetId, values, null, keyGuid, assignmentObjectType);
            return newEntity != null;
        }

        /// <summary>
        /// Delete an Entity
        /// </summary>
        public bool DeleteEntityByGuid(Guid entityGuid)
        {
            return _context.DeleteEntity(entityGuid);
        }
    }
}