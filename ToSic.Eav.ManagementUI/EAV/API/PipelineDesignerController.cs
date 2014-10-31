using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ToSic.Eav.DataSources;
using ToSic.Eav.DataSources.Caches;
using ToSic.Eav.DataSources.Tokens;
using Microsoft.Practices.Unity;

namespace ToSic.Eav.ManagementUI.API
{
	/// <summary>
	/// Web API Controller for the Pipeline Designer UI
	/// </summary>
	public class PipelineDesignerController : ApiController
	{
		private EavContext _context;
		private readonly string _userName;

		/// <summary>
		/// Default Constructor
		/// </summary>
		public PipelineDesignerController() : this("EAV Pipeline Designer") { }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="userName">current UserName</param>
		/// <param name="eavConnectionString">optional EAV Connection String</param>
		public PipelineDesignerController(string userName, string eavConnectionString = null)
		{
			// Preserving circular reference
			GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Serialize;
			GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings.PreserveReferencesHandling = PreserveReferencesHandling.Objects;

			_userName = userName;

			if (eavConnectionString != null)
				Eav.Configuration.SetConnectionString(eavConnectionString);
		}

		/// <summary>
		/// Get URL to configure a DataSource
		/// </summary>
		/// <param name="appId">AppId of the Pipeline/DataSource</param>
		/// <param name="dataSourceEntityGuid">EntityGuid of the DataSource</param>
		/// <param name="dataSourceFullName">FullName of the DataSource Type</param>
		/// <param name="newItemUrl">URL Schema to NewItem-Form</param>
		/// <param name="editItemUrl">URL Schema to EditItem-Form</param>
		[HttpGet]
		public object GetDataSourceConfigurationUrl(int appId, Guid dataSourceEntityGuid, string dataSourceFullName, string newItemUrl, string editItemUrl)
		{
			_context = EavContext.Instance(appId: appId);
			var cache = DataSource.GetCache(_context.ZoneId, _context.AppId);

			var attributeSetName = "|Config " + dataSourceFullName;
			var contentType = cache.GetContentType(attributeSetName);
			if (contentType == null)
				throw new ArgumentException("No Configuration found for Name " + attributeSetName, "dataSourceFullName");
			var attributeSetId = contentType.AttributeSetId;

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
				// Get the Entity describing the Pipeline and Pipeline Parts (DataSources)
				var source = DataSource.GetInitialDataSource(appId: appId);
				var pipelineEntity = DataPipeline.GetPipelineEntity(id.Value, source);
				var dataSources = DataPipeline.GetPipelineParts(source.ZoneId, source.AppId, pipelineEntity.EntityGuid);

				#region Deserialize some Entity-Values
				pipelineJson = Helpers.GetEntityValues(pipelineEntity);
				pipelineJson[DataPipeline.StreamWiringAttributeName] = DataPipelineWiring.Deserialize((string)pipelineJson[DataPipeline.StreamWiringAttributeName]);

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
		/// Get installed DataSources from .NET Runtime but only those with [PipelineDesigner Attribute]
		/// </summary>
		[HttpGet]
		public IEnumerable<object> GetInstalledDataSources()
		{
			var result = new List<object>();
			var installedDataSources = DataSource.GetInstalledDataSources();
			foreach (var dataSource in installedDataSources.Where(d => d.GetCustomAttributes(typeof(PipelineDesignerAttribute), false).Any()))
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
				// Handle Interfaces (currently only ICache) with Unity
				else if (dataSource.IsInterface)
				{
					var dataSourceInstance = (IDataSource)Factory.Container.Resolve(dataSource);
					outStreamNames = dataSourceInstance.Out.Keys;
					if (dataSourceInstance is ICache)
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
		public Dictionary<string, object> SavePipeline([FromBody] dynamic data, int appId, int? id = null)
		{
			_context = EavContext.Instance(appId: appId);
			_context.UserName = _userName;
			var source = DataSource.GetInitialDataSource(appId: appId);

			// Get/Save Pipeline EntityGuid. Its required to assign Pipeline Parts to it.
			Guid pipelineEntityGuid;
			if (id.HasValue)
			{
				var entity = DataPipeline.GetPipelineEntity(id.Value, source);
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
			var existingEntityGuids = DataPipeline.GetPipelineParts(zoneId, appId, pipelineEntityGuid).Select(e => e.EntityGuid);

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
		/// Query the Result of a Pipline using Test-Parameters
		/// </summary>
		[HttpGet]
		public Dictionary<string, IEnumerable<IEntity>> QueryPipeline(int appId, int id)
		{
			var configurationPropertyAccesses = GetPipelineTestParameters(appId, id);

			var outStreams = DataPipelineFactory.GetDataSource(appId, id, configurationPropertyAccesses, new PassThrough()).Out;
			return outStreams.ToDictionary(k => k.Key, v => v.Value.List.Select(l => l.Value));
		}

		/// <summary>
		/// Get Test Parameters for a Pipeline from the Pipeline-Entity
		/// </summary>
		private static IEnumerable<IPropertyAccess> GetPipelineTestParameters(int appId, int pipelineEntityId)
		{
			// Get the Entity describing the Pipeline
			var source = DataSource.GetInitialDataSource(appId: appId);
			var pipelineEntity = DataPipeline.GetPipelineEntity(pipelineEntityId, source);

			// Parse Test-Parameters in Format [Token:Property]=Value
			var testParameters = ((IAttribute<string>)pipelineEntity["TestParameters"]).TypedContents;
			if (testParameters == null)
				return null;
			var paramMatches = Regex.Matches(testParameters, @"(?:\[(?<Token>\w+):(?<Property>\w+)\])=(?<Value>[^\r]*)");

			// Create a list of static Property Accessors
			var result = new List<IPropertyAccess>();
			foreach (Match testParam in paramMatches)
			{
				var token = testParam.Groups["Token"].Value.ToLower();

				// Ensure a PropertyAccess exists
				var propertyAccess = result.FirstOrDefault(i => i.Name == token) as StaticPropertyAccess;
				if (propertyAccess == null)
				{
					propertyAccess = new StaticPropertyAccess(token);
					result.Add(propertyAccess);
				}

				// Add the static value
				propertyAccess.Properties.Add(testParam.Groups["Property"].Value, testParam.Groups["Value"].Value);
			}

			return result;
		}

		/// <summary>
		/// Clone a Pipeline with all DataSources and their configurations
		/// </summary>
		[HttpGet]
		public object ClonePipeline(int appId, int id)
		{
			var clonePipelineEntity = DataPipeline.CopyDataPipeline(appId, id, _userName);
			return new { EntityId = clonePipelineEntity.EntityID };
		}
	}
}