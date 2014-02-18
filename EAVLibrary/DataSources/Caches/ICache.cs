using System;

namespace ToSic.Eav.DataSources.Caches
{
	/// <summary>
	/// Caching interface for standard Eav Cache
	/// </summary>
	public interface ICache
	{
		/// <summary>
		/// Clean cache
		/// </summary>
		void PurgeCache(int zoneId, int appId);

		/// <summary>
		/// Gets the DateTime when this CacheItem was populated
		/// </summary>
		DateTime LastRefresh { get; }

		/// <summary>
		/// Gets a GeontentType by Name
		/// </summary>
		IContentType GetContentType(string name);
		/// <summary>
		/// Gets a GeontentType by Id
		/// </summary>
		IContentType GetContentType(int contentTypeId);

		/// <summary>
		/// Get/Resolve ZoneId and AppId for specified ZoneId and/or AppId. If both are null, default ZoneId with it's default App is returned.
		/// </summary>
		/// <returns>Item1 = ZoneId, Item2 = AppId</returns>
		Tuple<int, int> GetZoneAppId(int? zoneId = null, int? appId = null);

		/// <summary>
		/// Get AssignmentObjectTypeId by Name
		/// </summary>
		int GetAssignmentObjectTypeId(string assignmentObjectTypeName);
	}
}
