using System;
using System.Collections.Generic;

namespace ToSic.Eav.DataSources
{
	/// <summary>
	/// Helpers to work with Data Pipelines
	/// </summary>
	public class DataPipeline
	{
		/// <summary>
		/// Copy an existing DataPipeline by copying all Entities and uptdate their GUIDs
		/// </summary>
		public void CopyDataPipeline()
		{
			var entityGuidChanges = new Dictionary<Guid, Guid>();
			// Copy Pipeline


			// Copy Pipeline Parts

			// Copy configurations
		}


		/// <summary>
		/// Get an Entity Describing a Pipeline
		/// </summary>
		/// <param name="entityId">EntityId</param>
		/// <param name="dataSource">DataSource to load Entity from</param>
		public static IEntity GetPipelineEntity(int entityId, IDataSource dataSource)
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

		/// <summary>
		/// Get Entities Describing PipelineParts
		/// </summary>
		/// <param name="zoneId">zoneId of the Pipeline</param>
		/// <param name="appId">appId of the Pipeline</param>
		/// <param name="pipelineEntityGuid">EntityGuid of the Entity describing the Pipeline</param>
		public static IEnumerable<IEntity> GetPipelineParts(int zoneId, int appId, Guid pipelineEntityGuid)
		{
			var metaDataSource = DataSource.GetMetaDataSource(zoneId, appId);
			return metaDataSource.GetAssignedEntities(DataSource.AssignmentObjectTypeIdDataPipeline, pipelineEntityGuid);
		}


	}
}
