using System;
using System.Collections;
using System.Collections.Generic;
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
		public Dictionary<string, object> GetPipeline(int pipelineEntityId = 0)
		{
			Dictionary<string, object> pipelineJson = null;
			var dataSourcesJson = new List<Dictionary<string, object>>();

			if (pipelineEntityId > 0)
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

		[HttpPost]
		//public int SavePipeline(int? pipelineEntityId, [FromBody] dynamic pipeline, [FromBody] dynamic dataSources)
		public int SavePipeline(int? pipelineEntityId)
		{
			return -1;
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