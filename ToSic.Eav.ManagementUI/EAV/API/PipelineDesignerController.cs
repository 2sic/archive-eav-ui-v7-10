using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ToSic.Eav.DataSources;
using ToSic.Eav.DataSources.Caches;

namespace ToSic.Eav.ManagementUI.API
{
	public class PipelineDesignerController : ApiController
	{
		private readonly EavContext _context = EavContext.Instance(DataSource.DefaultZoneId, DataSource.MetaDataAppId);

		public PipelineDesignerController()
		{
			_context.UserName = null;	// ToDo: Ensure UserName
			_context.InitZoneApp();	// ToDo: Ensure AppId
		}

		/// <summary>
		/// Get a Pipeline with DataSources
		/// </summary>
		[HttpGet]
		public Dictionary<string, object> GetPipeline(int? id = null)
		{
			Dictionary<string, object> pipelineJson = null;
			var dataSourcesJson = new List<Dictionary<string, object>>();

			if (id.HasValue)
			{
				// Get the Entity descripting the Pipeline
				var source = DataSource.GetInitialDataSource(DataSource.DefaultZoneId, DataSource.MetaDataAppId);
				var pipelineEntity = GetPipelineEntity(id.Value, source);

				// Get DataSources in this Pipeline
				var metaDataSource = DataSource.GetMetaDataSource(source.ZoneId, source.AppId);
				var dataSources = metaDataSource.GetAssignedEntities(DataSource.AssignmentObjectTypeIdDataPipeline, pipelineEntity.EntityGuid);

				#region Deserialize some Entity-Values
				pipelineJson = Helpers.GetEntityValues(pipelineEntity);
				pipelineJson["StreamWiring"] = DataPipelineWiring.Deserialize((string)pipelineJson["StreamWiring"]);

				foreach (var dataSource in Helpers.GetEntityValues(dataSources))
				{
					dataSource["VisualDesignerData"] = JsonConvert.DeserializeObject((string)dataSource["VisualDesignerData"]);
					dataSourcesJson.Add(dataSource);
				}
				#endregion
			}

			// return consolidated Data
			return new Dictionary<string, object>
			{
				{"Pipeline", pipelineJson},
				{"DataSources", dataSourcesJson}
			};
		}

		/// <summary>
		/// Get an Entity Describing a Pipeline
		/// </summary>
		/// <param name="entityId">EntityId</param>
		/// <param name="dataSource">DataSource to load Entity from</param>
		private static IEntity GetPipelineEntity(int entityId, IDataSource dataSource = null)
		{
			if (dataSource == null)
				dataSource = DataSource.GetInitialDataSource(DataSource.DefaultZoneId, DataSource.MetaDataAppId);

			var entities = dataSource["Default"].List;

			IEntity pipelineEntity;
			try
			{
				pipelineEntity = entities[entityId];
				if (pipelineEntity.Type.StaticName != "DataPipeline")
					throw new Exception("id is not an DataPipeline Entity");
			}
			catch (Exception)
			{
				throw new ArgumentException(string.Format("Could not load Pipeline-Entity with ID {0}.", entityId), "entityId");
			}

			return pipelineEntity;
		}

		/// <summary>
		/// Get installed DataSources from .NET Runtime
		/// </summary>
		[HttpGet]
		public IEnumerable<object> GetInstalledDataSources()
		{
			var result = new List<object>();
			var installedDataSources = DataSource.GetInstalledDataSources();
			foreach (var dataSource in installedDataSources)
			{
				#region Create Instance of DataSource to get In- and Out-Streams
				ICollection<string> outStreamNames = new[] { "Default" };
				ICollection<string> inStreamNames = new[] { "Default" };
				if (!dataSource.IsInterface && !dataSource.IsAbstract)
				{
					var dataSourceInstance = (IDataSource)Activator.CreateInstance(dataSource);
					try
					{
						outStreamNames = dataSourceInstance.Out.Keys;
					}
					catch (Exception)
					{
						outStreamNames = new[] { "(unknown)" };
					}
				}
				else if (dataSource.IsInterface && dataSource == typeof(ICache))
				{
					inStreamNames = null;
				}
				#endregion

				result.Add(new
				{
					PartAssemblyAndType = dataSource.FullName + ", " + dataSource.Assembly.GetName().Name,
					ClassName = dataSource.Name,
					In = inStreamNames,
					Out = outStreamNames
				});
			}

			return result;
		}

		/// <summary>
		/// Save Pipeline
		/// </summary>
		/// <param name="data">JSON object { pipeline: pipeline, dataSources: dataSources }</param>
		/// <param name="appId">AppId this Pipeline belogs to</param>
		/// <param name="id">PipelineEntityId</param>
		[HttpPost]
		public Dictionary<string, object> SavePipeline([FromBody] dynamic data, int appId, int? id = null)
		{
			// Get/Save Pipeline EntityGuid to assign Pipeline Parts to it
			Guid pipelineEntityGuid;
			if (id.HasValue)
			{
				var entity = GetPipelineEntity(id.Value);
				pipelineEntityGuid = entity.EntityGuid;
			}
			else
			{
				Entity entity = SavePipelineEntity(null, data.pipeline);
				pipelineEntityGuid = entity.EntityGUID;
				id = entity.EntityID;
			}

			// ToDo: Resolve correct ID
			var pipelinePartAttributeSetId = 50;
			var newDataSources = SavePipelineParts(data.dataSources, pipelineEntityGuid, pipelinePartAttributeSetId);

			// Update Pipeline Entity with new Wirings etc.
			SavePipelineEntity(id.Value, data.pipeline, newDataSources);

			//return new HttpResponseMessage(HttpStatusCode.OK);
			return GetPipeline(id.Value);
		}

		/// <summary>
		/// Save PipelineParts (DataSources) to EAV
		/// </summary>
		/// <param name="dataSources">JSON describing the DataSources</param>
		/// <param name="pipelineEntityGuid">EngityGuid of the Pipeline-Entity</param>
		/// <param name="pipelinePartAttributeSetId">AttributeSetId of PipelineParts</param>
		private Dictionary<string, Guid> SavePipelineParts(dynamic dataSources, Guid pipelineEntityGuid, int pipelinePartAttributeSetId)
		{
			var newDataSources = new Dictionary<string, Guid>();

			foreach (var dataSource in dataSources)
			{
				if (dataSource.PartAssemblyAndType == "Out") continue; // Don't save Out-DataSource

				// Update existing DataSource or add a new one
				var newValues = GetEntityValues(dataSource);
				if (dataSource.EntityId != null)
					_context.UpdateEntity((int)dataSource.EntityId, newValues);
				else
				{
					Entity entitiy = _context.AddEntity(pipelinePartAttributeSetId, newValues, null, pipelineEntityGuid, DataSource.AssignmentObjectTypeIdDataPipeline);
					newDataSources.Add((string)dataSource.EntityGuid, entitiy.EntityGUID);
				}
			}

			// ToDo: Remove deleted DataSources

			return newDataSources;
		}

		private Entity SavePipelineEntity(int? id, dynamic pipeline, IDictionary<string, Guid> newDataSources = null)
		{
			// Create a clone so it can be modifie before saving but doesn't affect the underlaying JObject.
			// A new Pipeline Entity must be saved twice, this only works
			dynamic pipelineClone = ((JObject)pipeline).DeepClone();

			var wirings = ((JArray)pipeline.StreamWiring).ToObject<IEnumerable<WireInfo>>();

			// Update Wirings of entities just added
			if (newDataSources != null)
			{
				var wiringsNew = new List<WireInfo>();
				foreach (var wireInfo in wirings)
				{
					var newWireInfo = wireInfo;
					if (newDataSources.ContainsKey(wireInfo.From))
						newWireInfo.From = newDataSources[wireInfo.From].ToString();
					if (newDataSources.ContainsKey(wireInfo.To))
						newWireInfo.To = newDataSources[wireInfo.To].ToString();

					wiringsNew.Add(newWireInfo);
				}
				wirings = wiringsNew;
			}
			pipelineClone.StreamWiring = DataPipelineWiring.Serialize(wirings);

			// Add/Update Entity
			Entity result;

			var attriguteSetId = 49; // ToDo: Get correct ID

			IDictionary newValues = GetEntityValues(pipelineClone);
			if (id.HasValue)
				result = _context.UpdateEntity(id.Value, newValues);
			else
				result = _context.AddEntity(attriguteSetId, newValues, null, null);

			return result;
		}

		/// <summary>
		/// Update an Entity with values from a JObject
		/// </summary>
		/// <param name="newValues">JObject with new Values</param>
		/// <param name="excludeKeys">Keys of values to exclude</param>
		private static IDictionary GetEntityValues(JToken newValues, IEnumerable<string> excludeKeys = null)
		{
			var newValuesDict = newValues.ToObject<IDictionary<string, object>>();

			var excludeKeysStatic = new[] { "EntityGuid", "EntityId" };

			return newValuesDict.Where(i => !excludeKeysStatic.Contains(i.Key) && (excludeKeys == null || !excludeKeys.Contains(i.Key))).ToDictionary(k => k.Key, v => v.Value);
		}
	}
}