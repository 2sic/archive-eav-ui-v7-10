using System;
using System.Collections.Generic;
using System.Linq;

namespace ToSic.Eav.DataSources.Caches
{
	/// <summary>
	/// Cache Object for a specific App
	/// </summary>
	public class CacheItem
	{
		#region Private Fields
		private IDictionary<int, IEntity> _publishedEntities;
		private IDictionary<int, IEntity> _draftEntities;
		#endregion

		#region Properties
		/// <summary>
		/// Gets all Entities in this App
		/// </summary>
		public IDictionary<int, IEntity> Entities { get; private set; }

		/// <summary>
		/// Get all Published Entities in this App (excluding Drafts)
		/// </summary>
		public IDictionary<int, IEntity> PublishedEntities
		{
			get { return _publishedEntities ?? (_publishedEntities = Entities.Where(e => e.Value.IsPublished).ToDictionary(k => k.Key, v => v.Value)); }
		}
		/// <summary>
		/// Get all Entities not having a Draft (Entities that are Published (not having a draft) or draft itself)
		/// </summary>
		public IDictionary<int, IEntity> DraftEntities
		{
			get { return _draftEntities ?? (_draftEntities = Entities.Where(e => e.Value.GetDraft() == null).ToDictionary(k => k.Value.EntityId, v => v.Value)); }
		}
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
		#endregion

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