using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ToSic.Eav.DataSources;
using ToSic.Eav.DataSources.Caches;
using ToSic.Eav.DataSources.Tokens;

namespace ToSic.Eav.ManagementUI.API
{
	public class PipelineDesignerController : ApiController
	{
		private EavContext _context;

		public PipelineDesignerController()
		{
			// Preserving circular reference
			GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Serialize;
			GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
		}

		/// <summary>
		/// Get URL to configure a DataSource
		/// </summary>
		/// <param name="appId">AppId of the Pipeline/DataSource</param>
		/// <param name="dataSourceEntityGuid">EntityGuid of the DataSource</param>
		/// <param name="partAssemblyAndType">Type</param>
		/// <param name="newItemUrl">URL Schema to NewItem-Form</param>
		/// <param name="editItemUrl">URL Schema to EditItem-Form</param>
		[HttpGet]
		public object GetDataSourceConfigurationUrl(int appId, Guid dataSourceEntityGuid, string partAssemblyAndType, string newItemUrl, string editItemUrl)
		{
			_context = EavContext.Instance(appId: appId);
			var cache = DataSource.GetCache(_context.ZoneId, _context.AppId);

			// ToDo: Refactor
			string attributeSetName;
			switch (partAssemblyAndType)
			{
				case "ToSic.Eav.DataSources.EntityTypeFilter, ToSic.Eav":
					attributeSetName = "Configuration of an EntityTypeFilter DataSource";
					break;
				case "ToSic.Eav.DataSources.EntityIdFilter, ToSic.Eav":
					attributeSetName = "Configuration of an EntityIdFilter Data Source";
					break;
				default:
					throw new ArgumentException("No Configuration AttributeSet assigned for this DataSource Type", "partAssemblyAndType");
			}
			var attributeSetId = cache.GetContentType(attributeSetName).AttributeSetId;

			var url = Forms.GetItemFormUrl(dataSourceEntityGuid, attributeSetId, DataSource.AssignmentObjectTypeIdDataPipeline, newItemUrl, editItemUrl);

			return new { Url = url };
		}

		/// <summary>
		/// Get a Pipeline with DataSources
		/// </summary>
		[HttpGet]
		public Dictionary<string, object> GetPipeline(int appId, int? id = null)
		{
			Dictionary<string, object> pipelineJson = null;
			var dataSourcesJson = new List<Dictionary<string, object>>();

			if (id.HasValue)
			{
				// Get the Entity describing the Pipeline
				var source = DataSource.GetInitialDataSource(appId: appId);
				var pipelineEntity = GetPipelineEntity(id.Value, source);

				// Get DataSources in this Pipeline
				var dataSources = GetPipelineParts(source.ZoneId, source.AppId, pipelineEntity.EntityGuid);

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
		private static IEntity GetPipelineEntity(int entityId, IDataSource dataSource)
		{
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

		private static IEnumerable<IEntity> GetPipelineParts(int zoneId, int appId, Guid pipelineEntityGuid)
		{
			var metaDataSource = DataSource.GetMetaDataSource(zoneId, appId);
			return metaDataSource.GetAssignedEntities(DataSource.AssignmentObjectTypeIdDataPipeline, pipelineEntityGuid);
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
				ICollection<string> outStreamNames = new string[0];
				ICollection<string> inStreamNames = new string[0];
				if (!dataSource.IsInterface && !dataSource.IsAbstract)
				{
					var dataSourceInstance = (IDataSource)Activator.CreateInstance(dataSource);
					try
					{
						outStreamNames = dataSourceInstance.Out.Keys;
					}
					catch
					{
						outStreamNames = null;
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
			const string userName = "EAV Pipeline Designer";
			return SavePipeline(data, appId, id, userName);
		}

		/// <summary>
		/// Save Pipeline
		/// </summary>
		/// <param name="data">JSON object { pipeline: pipeline, dataSources: dataSources }</param>
		/// <param name="appId">AppId this Pipeline belogs to</param>
		/// <param name="id">PipelineEntityId</param>
		/// <param name="userName">Username performing this Save-Action</param>
		protected Dictionary<string, object> SavePipeline(dynamic data, int appId, int? id = null, string userName = null)
		{
			_context = EavContext.Instance(appId: appId);
			_context.UserName = userName;
			var source = DataSource.GetInitialDataSource(appId: appId);

			// Get/Save Pipeline EntityGuid. Its required to assign Pipeline Parts to it.
			Guid pipelineEntityGuid;
			if (id.HasValue)
			{
				var entity = GetPipelineEntity(id.Value, source);
				pipelineEntityGuid = entity.EntityGuid;

				if (((IAttribute<bool?>)entity["AllowEdit"]).TypedContents == false)
					throw new InvalidOperationException("Pipeline has AllowEdit set to false");
			}
			else
			{
				Entity entity = SavePipelineEntity(null, data.pipeline);
				pipelineEntityGuid = entity.EntityGUID;
				id = entity.EntityID;
			}

			var pipelinePartAttributeSetId = _context.GetAttributeSet(DataSource.DataPipelinePartStaticName).AttributeSetID;
			var newDataSources = SavePipelineParts(data.dataSources, pipelineEntityGuid, pipelinePartAttributeSetId);
			DeletedRemovedPipelineParts(data.dataSources, newDataSources, pipelineEntityGuid, source.ZoneId, source.AppId);

			// Update Pipeline Entity with new Wirings etc.
			SavePipelineEntity(id.Value, data.pipeline, newDataSources);

			return GetPipeline(appId, id.Value);
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
				// Skip Out-DataSource
				if (dataSource.EntityGuid == "Out") continue;

				// Update existing DataSource
				var newValues = GetEntityValues(dataSource);
				if (dataSource.EntityId != null)
					_context.UpdateEntity((int)dataSource.EntityId, newValues);
				// Add new DataSource
				else
				{
					var entitiy = _context.AddEntity(pipelinePartAttributeSetId, newValues, null, pipelineEntityGuid, DataSource.AssignmentObjectTypeIdDataPipeline);
					newDataSources.Add((string)dataSource.EntityGuid, entitiy.EntityGUID);
				}
			}

			return newDataSources;
		}

		/// <summary>
		/// Delete Pipeline Parts (DataSources) that are not present
		/// </summary>
		private void DeletedRemovedPipelineParts(IEnumerable<JToken> dataSources, Dictionary<string, Guid> newDataSources, Guid pipelineEntityGuid, int zoneId, int appId)
		{
			// Get EntityGuids currently stored in EAV
			var existingEntityGuids = GetPipelineParts(zoneId, appId, pipelineEntityGuid).Select(e => e.EntityGuid);

			// Get EntityGuids from the UI (except Out and unsaved)
			var newEntityGuids = dataSources.Select(d => (string)((JObject)d).Property("EntityGuid").Value).Where(g => g != "Out" && !g.StartsWith("unsaved")).Select(Guid.Parse).ToList();
			newEntityGuids.AddRange(newDataSources.Values);

			foreach (var entityToDelet in existingEntityGuids.Where(existingGuid => !newEntityGuids.Contains(existingGuid)))
				_context.DeleteEntity(entityToDelet);
		}

		/// <summary>
		/// Save a Pipeline Entity to EAV
		/// </summary>
		/// <param name="id">EntityId of the Entity describing the Pipeline</param>
		/// <param name="pipeline">JSON with the new Entity-Values</param>
		/// <param name="newDataSources">Array with new DataSources and the unsavedName and final EntityGuid</param>
		private Entity SavePipelineEntity(int? id, dynamic pipeline, IDictionary<string, Guid> newDataSources = null)
		{
			// Create a clone so it can be modifie before saving but doesn't affect the underlaying JObject.
			// A new Pipeline Entity must be saved twice, but some Field-Values are changed before saving it
			dynamic pipelineClone = pipeline.DeepClone();

			// Update Wirings of Entities just added
			var wirings = pipeline.StreamWiring.ToObject<IEnumerable<WireInfo>>();
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
			var attributeSetId = _context.GetAttributeSet(DataSource.DataPipelineStaticName).AttributeSetID;
			IDictionary newValues = GetEntityValues(pipelineClone);
			return id.HasValue ? _context.UpdateEntity(id.Value, newValues) : _context.AddEntity(attributeSetId, newValues, null, null);
		}

		/// <summary>
		/// Update an Entity with values from a JObject
		/// </summary>
		/// <param name="newValues">JObject with new Values</param>
		private static IDictionary GetEntityValues(JToken newValues)
		{
			var newValuesDict = newValues.ToObject<IDictionary<string, object>>();

			var excludeKeysStatic = new[] { "EntityGuid", "EntityId" };

			return newValuesDict.Where(i => !excludeKeysStatic.Contains(i.Key)).ToDictionary(k => k.Key, v => v.Value);
		}

		/// <summary>
		/// Query the Result of a Pipline
		/// </summary>
		[HttpGet]
		public object QueryPipeline(int appId, int id)
		{
			return QueryPipeline(appId, id, null);
		}

		/// <summary>
		/// Query the Result of a Pipline
		/// </summary>
		protected object QueryPipeline(int appId, int id, IPropertyAccess[] configurationPropertyAccesses)
		{
			var outStreams = DataPipelineFactory.GetDataSource(appId, id, configurationPropertyAccesses).Out;
			return outStreams.ToDictionary(k => k.Key, v => v.Value.List.Select(l => l.Value));
		}
	}
}