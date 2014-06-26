using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.Unity;
using ToSic.Eav.DataSources.RootSources;
using ToSic.Eav.DataSources.SqlSources;

namespace ToSic.Eav.DataSources.Caches
{
	/// <summary>
	/// Represents an abstract Cache DataSource
	/// </summary>
	public abstract class BaseCache : BaseDataSource, IMetaDataSource, ICache
	{
		/// <summary>PublishedEntities Stream Name</summary>
		public const string PublishedStreamName = "Published";
		/// <summary>Draft-Entities Stream Name</summary>
		public const string DraftsStreamName = "Drafts";

		protected IDataSource Cache { get; set; }

		protected BaseCache()
		{
			Out.Add(DataSource.DefaultStreamName, new DataStream(this, DataSource.DefaultStreamName, GetEntities));
			Out.Add(PublishedStreamName, new DataStream(this, PublishedStreamName, GetPublishedEntities));
			Out.Add(DraftsStreamName, new DataStream(this, DraftsStreamName, GetDraftEntities));
		}

		private IDictionary<int, IEntity> GetEntities()
		{
			return EnsureCache().Entities;
		}

		private IDictionary<int, IEntity> GetPublishedEntities()
		{
			return EnsureCache().PublishedEntities;
		}

		private IDictionary<int, IEntity> GetDraftEntities()
		{
			return EnsureCache().DraftEntities;
		}

		/// <summary>
		/// The root DataSource
		/// </summary>
		/// <remarks>Unity sets this automatically</remarks>
		[Dependency]
		public IRootSource Backend { get; set; }

		/// <summary>
		/// Gets or sets the Dictionary of all Zones an Apps
		/// </summary>
		public abstract Dictionary<int, ZoneModel> ZoneApps { get; protected set; }
		/// <summary>
		/// Gets or sets the Dictionary of all AssignmentObjectTypes
		/// </summary>
		public abstract Dictionary<int, string> AssignmentObjectTypes { get; protected set; }
		/// <summary>
		/// Gets the KeySchema used to store values for a specific Zone and App. Must contain {0} for ZoneId and {1} for AppId
		/// </summary>
		public abstract string CacheKeySchema { get; }

		/// <summary>
		/// Gets the DateTime when this Cache was populated
		/// </summary>
		public DateTime LastRefresh { get { return EnsureCache().LastRefresh; } }

		/// <summary>
		/// Test whether CacheKey exists in Cache
		/// </summary>
		protected abstract bool HasCacheItem(string cacheKey);
		/// <summary>
		/// Sets the CacheItem with specified CacheKey
		/// </summary>
		protected abstract void SetCacheItem(string cacheKey, CacheItem item);
		/// <summary>
		/// Get CacheItem with specified CacheKey
		/// </summary>
		protected abstract CacheItem GetCacheItem(string cacheKey);
		/// <summary>
		/// Remove the CacheItem with specified CacheKey
		/// </summary>
		protected abstract void RemoveCacheItem(string cacheKey);

		/// <summary>
		/// Ensure cache for current AppId
		/// </summary>
		protected CacheItem EnsureCache()
		{
			if (ZoneApps == null || AssignmentObjectTypes == null)
			{
				ZoneApps = Backend.GetAllZones();

				AssignmentObjectTypes = Backend.GetAssignmentObjectTypes();
			}

			if (ZoneId == 0 || AppId == 0)
				return null;

			var cacheKey = string.Format(CacheKeySchema, ZoneId, AppId);

			if (!HasCacheItem(cacheKey))
			{
				// Init EavSqlStore once
				var zone = GetZoneAppInternal(ZoneId, AppId);
				((EavSqlStore)Backend).InitZoneApp(zone.Item1, zone.Item2);

				SetCacheItem(cacheKey, Backend.GetDataForCache(Cache));
			}

			return GetCacheItem(cacheKey);
		}

		/// <summary>
		/// Clear Cache for specific Zone/App
		/// </summary>
		public void PurgeCache(int zoneId, int appId)
		{
			var cacheKey = string.Format(CacheKeySchema, zoneId, appId);

			RemoveCacheItem(cacheKey);
		}

		/// <summary>
		/// Clear Zones/Apps List
		/// </summary>
		public void PurgeGlobalCache()
		{
			ZoneApps = null;
		}

		/// <summary>
		/// Get a ContentType by StaticName if found of DisplayName if not
		/// </summary>
		/// <param name="name">Either StaticName or DisplayName</param>
		public IContentType GetContentType(string name)
		{
			var cache = EnsureCache();
			// Lookup StaticName first
			var matchByStaticName = cache.ContentTypes.FirstOrDefault(c => c.Value.StaticName.Equals(name));
			if (matchByStaticName.Value != null)
				return matchByStaticName.Value;

			// Lookup Name afterward
			var matchByName = cache.ContentTypes.FirstOrDefault(c => c.Value.Name.Equals(name));
			if (matchByName.Value != null)
				return matchByName.Value;

			return null;
		}

		/// <summary>
		/// Get a ContentType by Id
		/// </summary>
		public IContentType GetContentType(int contentTypeId)
		{
			var cache = EnsureCache();
			return cache.ContentTypes.FirstOrDefault(c => c.Key == contentTypeId).Value;
		}

		/// <summary>
		/// Get all Content Types
		/// </summary>
		public IDictionary<int, IContentType> GetContentTypes()
		{
			return EnsureCache().ContentTypes;
		}

		/// <summary>
		/// Get/Resolve ZoneId and AppId for specified ZoneId and/or AppId. If both are null, default ZoneId with it's default App is returned.
		/// </summary>
		/// <returns>Item1 = ZoneId, Item2 = AppId</returns>
		public Tuple<int, int> GetZoneAppId(int? zoneId = null, int? appId = null)
		{
			EnsureCache();

			return GetZoneAppInternal(zoneId, appId);
		}

		private Tuple<int, int> GetZoneAppInternal(int? zoneId, int? appId)
		{
			var resultZoneId = zoneId.HasValue
								   ? zoneId.Value
								   : (appId.HasValue
										  ? ZoneApps.Single(z => z.Value.Apps.Any(a => a.Key == appId.Value)).Key
										  : DataSource.DefaultZoneId);

			var resultAppId = appId.HasValue
								  ? ZoneApps[resultZoneId].Apps.Single(a => a.Key == appId.Value).Key
								  : ZoneApps[resultZoneId].DefaultAppId;

			return Tuple.Create(resultZoneId, resultAppId);
		}

		/// <summary>
		/// Get AssignmentObjectTypeId by Name
		/// </summary>
		public int GetAssignmentObjectTypeId(string assignmentObjectTypeName)
		{
			EnsureCache();

			return AssignmentObjectTypes.SingleOrDefault(a => a.Value == assignmentObjectTypeName).Key;
		}

		/// <summary>
		/// Get Entities with specified AssignmentObjectTypeId and Key
		/// </summary>
		public IEnumerable<IEntity> GetAssignedEntities(int assignmentObjectTypeId, Guid key, string contentTypeName = null)
		{
			var cache = EnsureCache();

			Dictionary<Guid, IEnumerable<IEntity>> keyGuidDictionary;
			if (cache.AssignmentObjectTypesGuid.TryGetValue(assignmentObjectTypeId, out keyGuidDictionary))
			{
				IEnumerable<IEntity> entities;
				if (keyGuidDictionary.TryGetValue(key, out entities))
					return entities.Where(e => contentTypeName == null || e.Type.StaticName == contentTypeName);
			}

			return new List<IEntity>();
		}

		/// <summary>
		/// Get Entities with specified AssignmentObjectTypeId and Key
		/// </summary>
		public IEnumerable<IEntity> GetAssignedEntities(int assignmentObjectTypeId, string key, string contentTypeName = null)
		{
			var cache = EnsureCache();

			Dictionary<string, IEnumerable<IEntity>> keyStringDictionary;
			if (cache.AssignmentObjectTypesString.TryGetValue(assignmentObjectTypeId, out keyStringDictionary))
			{
				IEnumerable<IEntity> entities;
				if (keyStringDictionary.TryGetValue(key, out entities))
					return entities.Where(e => contentTypeName == null || e.Type.StaticName == contentTypeName);
			}

			return new List<IEntity>();
		}

		/// <summary>
		/// Get Entities with specified AssignmentObjectTypeId and Key
		/// </summary>
		public IEnumerable<IEntity> GetAssignedEntities(int assignmentObjectTypeId, int key, string contentTypeName = null)
		{
			var cache = EnsureCache();

			Dictionary<int, IEnumerable<IEntity>> keyNumberDictionary;
			if (cache.AssignmentObjectTypesNumber.TryGetValue(assignmentObjectTypeId, out keyNumberDictionary))
			{
				IEnumerable<IEntity> entities;
				if (keyNumberDictionary.TryGetValue(key, out entities))
					return entities.Where(e => contentTypeName == null || e.Type.StaticName == contentTypeName);
			}

			return new List<IEntity>();
		}
	}
}
