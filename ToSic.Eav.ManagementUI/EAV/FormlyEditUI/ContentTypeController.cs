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
using ToSic.Eav.BLL;
using ToSic.Eav.DataSources;
using ToSic.Eav.DataSources.Caches;
using Microsoft.Practices.Unity;
using ToSic.Eav.Persistence;
using ToSic.Eav.Serializers;
using ToSic.Eav.ValueProvider;

namespace ToSic.Eav.ManagementUI.FormlyEditUI
{
	/// <summary>
	/// Web API Controller for the Pipeline Designer UI
	/// </summary>
	public class ContentTypeController : ApiController
    {

		public ContentTypeController()
		{
		}

		/// <summary>
		/// Returns the configuration for a content type
		/// </summary>
		[HttpGet]
		public IEnumerable<dynamic> GetContentTypeConfiguration(int zoneId, int appId, string contentTypeName)
		{
			var cache = DataSource.GetCache(zoneId, appId);
			var result = cache.GetContentType(contentTypeName);

			if(result == null)
				throw new Exception("Content type " + contentTypeName + " not found.");

			var eavContext = EavDataController.Instance(zoneId, appId);
			var metaData = new Metadata();

			// Resolve ZoneId & AppId of the MetaData. If this AttributeSet uses configuration of another AttributeSet, use MetaData-ZoneId & -AppId
			var metaDataAppId = result.UsesConfigurationOfAttributeSet.HasValue ? Constants.MetaDataAppId : eavContext.AppId;
			var metaDataZoneId = result.UsesConfigurationOfAttributeSet.HasValue ? Constants.DefaultZoneId : eavContext.ZoneId;

			var config = result.AttributeDefinitions.Select(a => new
			{
				a.Value.Type,
				StaticName = a.Value.Name,
				MetaData = metaData.GetAttributeMetaData(a.Value.AttributeId, metaDataZoneId, metaDataAppId).ToDictionary(v => v.Key, e => e.Value[0])
			});

			return config;
		}

	}
}