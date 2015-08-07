using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Http;
using System.Web.UI.WebControls;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ToSic.Eav.DataSources;
using ToSic.Eav.DataSources.Caches;
using Microsoft.Practices.Unity;
using ToSic.Eav.Serializers;
using ToSic.Eav.ValueProvider;

namespace ToSic.Eav.ManagementUI.FormlyEditUI
{
	/// <summary>
	/// Web API Controller for the Pipeline Designer UI
	/// </summary>
	public class EntityController : ApiController
    {

		// I must keep the serializer so it can be configured from outside if necessary
		private Serializer _serializer;
		public Serializer Serializer
		{
			get
			{
				if (_serializer == null)
				{
					_serializer = Factory.Container.Resolve<Serializer>();
					_serializer.IncludeGuid = true;
				}
				return _serializer;
			}
		}

		/// <summary>
		/// Returns the configuration for a content type
		/// </summary>
		[HttpGet]
		public object GetEntity(int zoneId, int appId, int entityId)
		{
			var cache = DataSource.GetCache(zoneId, appId);
			var result = cache.List[entityId];
			return Serializer.Prepare(result);
		}

	}
}