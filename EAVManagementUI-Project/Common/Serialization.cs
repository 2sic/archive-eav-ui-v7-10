using System;
using System.Collections.Generic;
using System.Linq;

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
		/// Get EntityModel without circular references in Attribute-Values for JSON Serialization with JavaScriptSerializer
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
			var originalAttributeModels = _entity.Attributes.Where(a => a.Value.Type == "Entity").Select(a => (AttributeModel<EntityRelationshipModel>)a.Value);
			foreach (var originalAttributeModel in originalAttributeModels)
			{
				var newAttributeModel = new
				{
					originalAttributeModel.Name,
					originalAttributeModel.Type,
					originalAttributeModel.IsTitle,
					Values = from ValueModel<EntityRelationshipModel> originalValueTyped in originalAttributeModel.Values
							 select new
							 {
								 originalValueTyped.ValueId,
								 originalValueTyped.Languages,
								 originalValueTyped.ChangeLogIdCreated,
								 TypedContents = originalValueTyped.TypedContents.EntityIds
							 },
					DefaultValue = ((ValueModel<EntityRelationshipModel>)originalAttributeModel.DefaultValue).TypedContents.EntityIds,
					TypedContents = originalAttributeModel.TypedContents.EntityIds,
					originalAttributeModel.Typed
				};
				result.Add(originalAttributeModel.Name, newAttributeModel);
			}

			return result;
		}
	}
}