using System;
using System.Collections.Generic;
using System.Linq;

namespace ToSic.Eav.DataSources
{
	/// <summary>
	/// Helpers to work with Data Pipelines
	/// </summary>
	public class DataPipeline
	{
		private const string PipelineAttributeSetStaticName = "DataPipeline";
		/// <summary>
		/// AttributeHelperTools Name on the Pipeline-Entity describing the Stream-Wiring
		/// </summary>
		public const string StreamWiringAttributeName = "StreamWiring";

		/// <summary>
		/// Copy an existing DataPipeline by copying all Entities and uptdate their GUIDs
		/// </summary>
		public static Entity CopyDataPipeline(int appId, int pipelineEntityId, string userName)
		{
			var ctx = EavContext.Instance(appId: appId);
			ctx.UserName = userName;

			// Clone Pipeline Entity with a new new Guid
			var sourcePipelineEntity = ctx.GetEntity(pipelineEntityId);
			if (sourcePipelineEntity.Set.StaticName != PipelineAttributeSetStaticName)
				throw new ArgumentException("Entity is not an DataPipeline Entity", "pipelineEntityId");
			var pipelineEntityClone = ctx.CloneEntity(sourcePipelineEntity, true);

			// Copy Pipeline Parts with configuration Entity, assign KeyGuid of the new Pipeline Entity
			var pipelineParts = ctx.GetEntities(DataSource.AssignmentObjectTypeIdDataPipeline, sourcePipelineEntity.EntityGUID);
			var pipelinePartClones = new Dictionary<string, Guid>();	// track Guids of originals and their clone
			foreach (var pipelinePart in pipelineParts)
			{
				var pipelinePartClone = ctx.CloneEntity(pipelinePart, true);
				pipelinePartClone.KeyGuid = pipelineEntityClone.EntityGUID;
				pipelinePartClones.Add(pipelinePart.EntityGUID.ToString(), pipelinePartClone.EntityGUID);

				// Copy Configuration Entity, assign KeyGuid of the Clone
				var configurationEntity = ctx.GetEntities(DataSource.AssignmentObjectTypeIdDataPipeline, pipelinePart.EntityGUID).SingleOrDefault();
				if (configurationEntity != null)
				{
					var configurationClone = ctx.CloneEntity(configurationEntity, true);
					configurationClone.KeyGuid = pipelinePartClone.EntityGUID;
				}
			}

			#region Update Stream-Wirings
			var streamWiring = pipelineEntityClone.Values.Single(v => v.Attribute.StaticName == StreamWiringAttributeName);
			var wiringsClone = new List<WireInfo>();
			var wiringsSource = DataPipelineWiring.Deserialize(streamWiring.Value);
			if (wiringsSource != null)
			{
				foreach (var wireInfo in wiringsSource)
				{
					var wireInfoClone = wireInfo; // creates a clone of the Struct
					if (pipelinePartClones.ContainsKey(wireInfo.From))
						wireInfoClone.From = pipelinePartClones[wireInfo.From].ToString();
					if (pipelinePartClones.ContainsKey(wireInfo.To))
						wireInfoClone.To = pipelinePartClones[wireInfo.To].ToString();

					wiringsClone.Add(wireInfoClone);
				}
			}

			streamWiring.Value = DataPipelineWiring.Serialize(wiringsClone);
			#endregion

			ctx.SaveChanges();

			return pipelineEntityClone;
		}


		/// <summary>
		/// Get an Entity Describing a Pipeline
		/// </summary>
		/// <param name="entityId">EntityId</param>
		/// <param name="dataSource">DataSource to load Entity from</param>
		public static IEntity GetPipelineEntity(int entityId, IDataSource dataSource)
		{
			var entities = dataSource[DataSource.DefaultStreamName].List;

			IEntity pipelineEntity;
			try
			{
				pipelineEntity = entities[entityId];
				if (pipelineEntity.Type.StaticName != PipelineAttributeSetStaticName)
					throw new ArgumentException("Entity is not an DataPipeline Entity", "entityId");
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
