using System;
using System.Linq;
using System.Xml.Linq;

namespace ToSic.Eav.ImportExport
{
	/// <summary>
	/// Export EAV Data in XML Format
	/// </summary>
	public class XmlExport
	{
		private readonly EavContext _ctx;

		/// <summary>
		/// Initializes a new instance of the XmlExport class.
		/// </summary>
		public XmlExport(EavContext ctx = null)
		{
			_ctx = ctx;
		}

		/// <summary>
		/// Returns an Entity XElement
		/// </summary>
		public XElement GetEntityXElement(int entityId)
		{
			var iEntity = _ctx.GetEntityModel(entityId);
			return GetEntityXElement(iEntity);
		}

		/// <summary>
		/// Returns an Entity XElement
		/// </summary>
		public XElement GetEntityXElement(IEntity entity)
		{
			var eavEntity = _ctx.GetEntity(entity.EntityId);
			//var attributeSet = _ctx.GetAttributeSet(eavEntity.AttributeSetID);

			// Prepare Values
			var values = (from e in entity.Attributes
						  select new
						  {
							  allValues = from v in e.Value.Values
										  select new
										  {
											  e.Key,
											  e.Value.Type,
											  ValueModel = v
										  }
						  }).SelectMany(e => e.allValues);

			var valuesXElement = from v in values
								 select GetValueXElement(v.Key, v.ValueModel, v.Type);

			// create Entity-XElement
			return new XElement("Entity",
				new XAttribute("AssignmentObjectType", eavEntity.AssignmentObjectType.Name),
				new XAttribute("AttributeSetStaticName", entity.Type.StaticName),
				new XAttribute("AttributeSetName", entity.Type.Name),
				new XAttribute("EntityGUID", entity.EntityGuid),
				valuesXElement);
		}

		/// <summary>
		/// Gets a Value XElement
		/// </summary>
		private XElement GetValueXElement(string attributeStaticname, IValue value, string attributeType)
		{
			var valueSerialized = _ctx.SerializeValue(value);
			// create Value-Child-Element with Dimensions as Children
			var valueXElement = new XElement("Value",
				new XAttribute("Key", attributeStaticname),
				new XAttribute("Value", valueSerialized),
				!String.IsNullOrEmpty(attributeType) ? new XAttribute("Type", attributeType) : null,
				value.Languages.Select(p => new XElement("Dimension",
						new XAttribute("DimensionID", p.DimensionId),
						new XAttribute("ReadOnly", p.ReadOnly)
					))
				);

			return valueXElement;
		}
	}
}
