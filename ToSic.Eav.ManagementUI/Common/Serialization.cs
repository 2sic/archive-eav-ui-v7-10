using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Data;

namespace ToSic.Eav.ManagementUI.Serialization
{
	public class EntityJavaScriptModel
	{
		private readonly IEntity _entity;

		#region Properties
		public int EntityId { get { return _entity.EntityId; } }
		public Guid EntityGuid { get { return _entity.EntityGuid; } }
		public IAttribute Title { get { return _entity.Title; } }
		public Dictionary<string, object> Attributes { get { return GetAttributes(); } }
		#endregion

		/// <summary>
		/// Get Entity without circular references in Attribute-Values for JSON Serialization with JavaScriptSerializer
		/// </summary>
		public EntityJavaScriptModel(IEntity entity)
		{
			_entity = entity;
		}

		private Dictionary<string, object> GetAttributes()
		{
			// get all simple attributs
			var result = _entity.Attributes.Where(a => a.Value.Type != "Entity").ToDictionary(k => k.Key, v => (object)v.Value);

			// Append Entity-Attributes
			var originalAttributeModels = _entity.Attributes.Where(a => a.Value.Type == "Entity").Select(a => (Attribute<Data.EntityRelationship>)a.Value);
			foreach (var originalAttributeModel in originalAttributeModels)
			{
				object values = null;
				if (originalAttributeModel.Values != null)
					values = from Value<Data.EntityRelationship> originalValueTyped in originalAttributeModel.Values
							 select new
							 {
								 originalValueTyped.ValueId,
								 originalValueTyped.Languages,
								 originalValueTyped.ChangeLogIdCreated,
								 TypedContents = originalValueTyped.TypedContents.EntityIds
							 };
				IEnumerable<int?> defaultValue = null;
				if (originalAttributeModel.DefaultValue != null)
					defaultValue = ((Value<Data.EntityRelationship>)originalAttributeModel.DefaultValue).TypedContents.EntityIds;

				IEnumerable<int?> typedContents = null;
				if (originalAttributeModel.TypedContents != null)
					typedContents = originalAttributeModel.TypedContents.EntityIds;

				var newAttributeModel = new
				{
					originalAttributeModel.Name,
					originalAttributeModel.Type,
					originalAttributeModel.IsTitle,
					Values = values,
					DefaultValue = defaultValue,
					TypedContents = typedContents,
					originalAttributeModel.Typed
				};
				result.Add(originalAttributeModel.Name, newAttributeModel);
			}

			return result;
		}
	}
}