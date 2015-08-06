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

			var eavContext = EavContext.Instance(zoneId, appId);

			var attributeSet = eavContext.GetAttributeSet(result.AttributeSetId);

			// Resolve ZoneId & AppId of the MetaData. If this AttributeSet uses configuration of another AttributeSet, use MetaData-ZoneId & -AppId
			var metaDataAppId = attributeSet.UsesConfigurationOfAttributeSet.HasValue ? DataSource.MetaDataAppId : eavContext.AppId;
			var metaDataZoneId = attributeSet.UsesConfigurationOfAttributeSet.HasValue ? DataSource.DefaultZoneId : eavContext.ZoneId;

			var config = eavContext.GetAttributes(attributeSet.AttributeSetID).ToList().Select(a => new
			{
				a.Type,
				a.StaticName,
				MetaData = eavContext.GetAttributeMetaData(a.AttributeID, metaDataZoneId, metaDataAppId, null).ToDictionary(v => v.Key, e => e.Value[0])
			});

			return config;
		}

	}
}