﻿using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using ToSic.Eav.DataSources;

namespace ToSic.Eav.ManagementUI.API
{
	/// <summary>
	/// Web API Controller for the Pipeline Designer UI
	/// </summary>
	public class EntitiesController : ApiController
	{
		/// <summary>
		/// Get all Entities of specified Type
		/// </summary>
		public IEnumerable<Dictionary<string, object>> GetEntities(int appId, string typeName, string cultureCode = null)
		{
			var source = DataSource.GetInitialDataSource(appId: appId);
			var typeFilter = DataSource.GetDataSource<EntityTypeFilter>(appId: appId, upstream: source);
			typeFilter.TypeName = typeName;

			return typeFilter.List.Select(t => Helpers.GetEntityValues(t.Value, cultureCode: cultureCode));
		}

		/// <summary>
		/// Get a ContentType by Name
		/// </summary>
		public IContentType GetContentType(int appId, string name)
		{
			var source = DataSource.GetInitialDataSource(appId: appId);
			var cache = DataSource.GetCache(source.ZoneId, appId);
			return cache.GetContentType(name);
		}
	}
}