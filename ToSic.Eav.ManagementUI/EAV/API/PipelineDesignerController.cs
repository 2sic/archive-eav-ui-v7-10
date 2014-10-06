using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Script.Serialization;
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

				var ser = new JavaScriptSerializer();
				foreach (var dataSource in Helpers.GetEntityValues(dataSources))
				{
					try
					{
						dataSource["VisualDesignerData"] = ser.Deserialize<object>((string)dataSource["VisualDesignerData"]);
					}
					catch (ArgumentException) { }
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
		public string SavePipeline([FromBody] dynamic data, int id = 0)
		{
			// Serialize some Entity-Values
			var pipeline = data.pipeline;
			var wirings = ((Newtonsoft.Json.Linq.JArray)pipeline.StreamWiring).ToObject<IEnumerable<WireInfo>>();
			pipeline.StreamWiring = DataPipelineWiring.Serialize(wirings);

			UpdateEntity(id, pipeline);

			// ToDo: Make dataSource.Definition a function so it's not in the JSON

			var dataSources = data.dataSources;
			// ToDo: Remove deleted DataSources
			// ToDo: Save new DataSources
			// ToDo: Update existing DataSources

			return pipeline.Name;
		}

		private void UpdateEntity(int entityId, dynamic newValues, IList<string> excludeKeys = null)
		{
			var newValuesDict = ((Newtonsoft.Json.Linq.JObject)newValues).ToObject<IDictionary<string, object>>();
			if (excludeKeys == null)
				excludeKeys = new List<string>();

			excludeKeys.Add("EntityGuid");
			excludeKeys.Add("EntityId");

			newValuesDict.Where(i => !excludeKeys.Contains(i.Key));

			//_context.UpdateEntity(entityId, newValuesDict);
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