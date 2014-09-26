using System;
using System.Collections;
using System.Collections.Generic;
using System.Web.Http;
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

		[HttpGet]
		public Dictionary<string, IEnumerable<Dictionary<string, object>>> GetPipeline(int pipelineEntityId)
		{
			var source = DataSource.GetInitialDataSource(DataSource.DefaultZoneId, DataSource.MetaDataAppId);
			var metaDataSource = DataSource.GetMetaDataSource(source.ZoneId, source.AppId);

			var dataPipeline = source["Default"].List[pipelineEntityId];	// ToDo: Ensure it's of Type "PipelineEntity" so users cannot graze all Entities
			var dataPipelineParts = metaDataSource.GetAssignedEntities(DataSource.AssignmentObjectTypeIdDataPipeline, dataPipeline.EntityGuid);

			var streams = new Dictionary<string, object>
				{
					{ "DataPipeline", dataPipeline },
					{ "DataPipelineParts", dataPipelineParts }
				};

			var result = Helpers.GetJsonStreams(streams);
			return result;
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