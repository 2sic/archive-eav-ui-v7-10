using System;
using System.Collections;
using System.Collections.Generic;
using System.Web.Script.Services;
using System.Web.Services;
using ToSic.Eav.DataSources;

namespace ToSic.Eav.ManagementUI
{
	/// <summary>
	/// Webservice to Get and modify Data Pipelines
	/// </summary>
	[WebService(Namespace = "http://schemas.2sic.com/2013/ToSexyContent/PipelineDesignerServices/01.00")]
	[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
	[System.ComponentModel.ToolboxItem(false)]
	[ScriptService]
	public class PipelineDesignerServicesBase : WebService
	{
		private readonly EavContext _context = EavContext.Instance(DataSource.DefaultZoneId, DataSource.MetaDataAppId);

		public PipelineDesignerServicesBase()
		{
			_context.UserName = User.Identity.Name;
		}

		[WebMethod]
		[ScriptMethod(ResponseFormat = ResponseFormat.Json)]
		public Dictionary<string, IEnumerable<Dictionary<string, object>>> GetPipeline(int pipelineEntityId)
		{
			var source = DataSource.GetInitialDataSource(DataSource.DefaultZoneId, DataSource.MetaDataAppId);
			var metaDataSource = DataSource.GetMetaDataSource(source.ZoneId, source.AppId);

			//var pipelineEntityId = int.Parse(HttpContext.Current.Request.QueryString["PipelineEntityId"]);
			var dataPipeline = source["Default"].List[pipelineEntityId];	// ToDo: Ensure it's of Type "PipelineEntity" so users cannot graze all Entities
			var dataPipelineParts = metaDataSource.GetAssignedEntities(DataSource.AssignmentObjectTypeIdDataPipeline, dataPipeline.EntityGuid);

			var streams = new Dictionary<string, object>
				{
					{ "DataPipeline", dataPipeline },
					{ "DataPipelineParts", dataPipelineParts }
				};

			var result = Helpers.GetJsonStreams(streams);
			//HttpContext.Current.Response.ContentType = "application/json";
			return result;
		}

		/// <summary>
		/// Update an Entity with new values. Values not in the list will not change at the moment.
		/// </summary>
		[WebMethod]
		[ScriptMethod(ResponseFormat = ResponseFormat.Json)]
		public bool UpdateEntity(int entityId, IDictionary newValues)
		{

			return _context.UpdateEntity(entityId, newValues) != null;
		}

		/// <summary>
		/// Update an Entity with new values. Values not in the list will not change at the moment.
		/// </summary>
		[WebMethod]
		[ScriptMethod(ResponseFormat = ResponseFormat.Json)]
		public bool UpdateEntityByGuid(Guid entityGuid, IDictionary newValues)
		{
			return _context.UpdateEntity(entityGuid, newValues) != null;
		}

		/// <summary>
		/// Update an Entity with new values. Values not in the list will not change at the moment.
		/// </summary>
		[WebMethod]
		[ScriptMethod(ResponseFormat = ResponseFormat.Json)]
		public bool AddEntity(int attributeSetId, IDictionary values, int assignmentObjectType, Guid keyGuid)
		{
			var newEntity = _context.AddEntity(attributeSetId, values, null, keyGuid, assignmentObjectType);
			return newEntity != null;
		}

		/// <summary>
		/// Delete an Entity
		/// </summary>
		[WebMethod]
		[ScriptMethod(ResponseFormat = ResponseFormat.Json)]
		public bool DeleteEntityByGuid(Guid entityGuid)
		{
			return _context.DeleteEntity(entityGuid);
		}
	}
}
