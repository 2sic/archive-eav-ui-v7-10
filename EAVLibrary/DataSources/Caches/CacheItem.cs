using System;
using System.Collections.Generic;

namespace ToSic.Eav.DataSources.Caches
{
	/// <summary>
	/// Cache Object for a specific App
	/// </summary>
	public class CacheItem
	{
		/// <summary>
		/// Gets all Entities in this App
		/// </summary>
		public IDictionary<int, IEntity> Entities { get; private set; }
		/// <summary>
		/// Gets all ContentTypes in this App
		/// </summary>
		public Dictionary<int, IContentType> ContentTypes { get; private set; }
		/// <summary>
		/// Gets a Dictionary of AssignmentObjectTypes and assigned Entities having a KeyGuid
		/// </summary>
		public Dictionary<int, Dictionary<Guid, IEnumerable<IEntity>>> AssignmentObjectTypesGuid { get; private set; }
		/// <summary>
		/// Gets a Dictionary of AssignmentObjectTypes and assigned Entities having a KeyNumber
		/// </summary>
		public Dictionary<int, Dictionary<int, IEnumerable<IEntity>>> AssignmentObjectTypesNumber { get; private set; }
		/// <summary>
		/// Gets a Dictionary of AssignmentObjectTypes and assigned Entities having a KeyString
		/// </summary>
		public Dictionary<int, Dictionary<string, IEnumerable<IEntity>>> AssignmentObjectTypesString { get; private set; }
		/// <summary>
		/// Get all Relationships between Entities
		/// </summary>
		public IEnumerable<EntityRelationshipItem> Relationships { get; private set; }
		/// <summary>
		/// Gets the DateTime when this CacheItem was populated
		/// </summary>
		public DateTime LastRefresh { get; private set; }

		/// <summary>
		/// Construct a new CacheItem with all required Items
		/// </summary>
		public CacheItem(IDictionary<int, IEntity> entities, Dictionary<int, IContentType> contentTypes,
			Dictionary<int, Dictionary<Guid, IEnumerable<IEntity>>> assignmentObjectTypesGuid, Dictionary<int, Dictionary<int, IEnumerable<IEntity>>> assignmentObjectTypesNumber,
			Dictionary<int, Dictionary<string, IEnumerable<IEntity>>> assignmentObjectTypesString, IEnumerable<EntityRelationshipItem> relationships)
		{
			Entities = entities;
			ContentTypes = contentTypes;
			AssignmentObjectTypesGuid = assignmentObjectTypesGuid;
			AssignmentObjectTypesNumber = assignmentObjectTypesNumber;
			AssignmentObjectTypesString = assignmentObjectTypesString;
			Relationships = relationships;

			LastRefresh = DateTime.Now;
		}
	}
}