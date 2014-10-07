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
		public Dictionary<string, object> GetPipeline(int id = 0)
		{
			Dictionary<string, object> pipelineJson = null;
			var dataSourcesJson = new List<Dictionary<string, object>>();

			if (id > 0)
			{
				#region Get the Entity descripting the Pipeline
				var source = DataSource.GetInitialDataSource(DataSource.DefaultZoneId, DataSource.MetaDataAppId);
				var metaDataSource = DataSource.GetMetaDataSource(source.ZoneId, source.AppId);

				var entities = source["Default"].List;
				IEntity pipelineEntity;
				try
				{
					pipelineEntity = entities[id];
					if (pipelineEntity.Type.StaticName != "DataPipeline")
						throw new Exception("id is not an DataPipeline Entity");
				}
				catch (Exception)
				{
					throw new ArgumentException(string.Format("Could not load Pipeline-Entity with ID {0}.", id), "id");
				}
				#endregion

				// Get DataSources in this Pipeline
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
		/// <param name="id">PipelineEntityId</param>
		[HttpPost]
		public string SavePipeline([FromBody] dynamic data, int appId, int id = 0)
		{
			#region Save Pipeline Parts
			var dataSources = data.dataSources;

			// ToDo: Resolve correct IDs
			var pipelinePartAttributeSetId = 50;
			var pipelineEntityGuid = Guid.Parse("0a049dc5-5d7e-48b7-9168-1c41a585811c");

			var newDataSources = new Dictionary<string, Guid>();

			foreach (var dataSource in dataSources)
			{
				if (dataSource.PartAssemblyAndType == "Out") continue;	// Don't save Out-DataSource

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
			#endregion

			#region Save Pipeline
			var pipeline = data.pipeline;

			// Update Wirings of entities just added
			var wiringsOriginal = ((JArray)pipeline.StreamWiring).ToObject<IEnumerable<WireInfo>>();
			var wirings = new List<WireInfo>();
			foreach (var wireInfo in wiringsOriginal)
			{
				var newWireInfo = wireInfo;
				if (newDataSources.ContainsKey(wireInfo.From))
					newWireInfo.From = newDataSources[wireInfo.From].ToString();
				if (newDataSources.ContainsKey(wireInfo.To))
					newWireInfo.To = newDataSources[wireInfo.To].ToString();

				wirings.Add(newWireInfo);
			}
			pipeline.StreamWiring = DataPipelineWiring.Serialize(wirings);

			_context.UpdateEntity(id, GetEntityValues(pipeline));

			#endregion
			return pipeline.Name;
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

		///// <summary>
		///// Update an Entity with new values. Values not in the list will not change at the moment.
		///// </summary>
		//public bool UpdateEntity(int entityId, IDictionary newValues)
		//{
		//	return _context.UpdateEntity(entityId, newValues) != null;
		//}

		///// <summary>
		///// Update an Entity with new values. Values not in the list will not change at the moment.
		///// </summary>
		//public bool UpdateEntityByGuid(Guid entityGuid, IDictionary newValues)
		//{
		//	return _context.UpdateEntity(entityGuid, newValues) != null;
		//}

		///// <summary>
		///// Update an Entity with new values. Values not in the list will not change at the moment.
		///// </summary>
		//public bool AddEntity(int attributeSetId, IDictionary values, int assignmentObjectType, Guid keyGuid)
		//{
		//	var newEntity = _context.AddEntity(attributeSetId, values, null, keyGuid, assignmentObjectType);
		//	return newEntity != null;
		//}

		///// <summary>
		///// Delete an Entity
		///// </summary>
		//public bool DeleteEntityByGuid(Guid entityGuid)
		//{
		//	return _context.DeleteEntity(entityGuid);
		//}
	}
}