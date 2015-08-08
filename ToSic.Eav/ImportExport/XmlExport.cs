using System;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.Persistence;

namespace ToSic.Eav.ImportExport
{
	/// <summary>
	/// Export EAV Data in XML Format
	/// </summary>
	public class XmlExport
	{
		private readonly EavContext _ctx;
	    private readonly DbShortcuts DbS;

		/// <summary>
		/// Initializes a new instance of the XmlExport class.
		/// </summary>
		public XmlExport(EavContext ctx)
		{
			_ctx = ctx;
            DbS = new DbShortcuts(ctx);
		}

		/// <summary>
		/// Returns an Entity XElement
		/// </summary>
		public XElement GetEntityXElement(int entityId)
		{
			var iEntity = new DbLoadAsEav(_ctx).GetEavEntity(entityId);
			return GetEntityXElement(iEntity);
		}

		/// <summary>
		/// Returns an Entity XElement
		/// </summary>
		public XElement GetEntityXElement(IEntity entity)
		{
			var eavEntity = DbS.GetEntity(entity.EntityId);
			//var attributeSet = _ctx.GetAttributeSet(eavEntity.AttributeSetID);

			// Prepare Values
			var values = (from e in entity.Attributes
						  where e.Value.Values != null
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
			var valueSerialized = SerializeValue(value);
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

        /// <summary>
        /// Serialize Value to a String for SQL Server or XML Export
        /// </summary>
        internal string SerializeValue(IValue value)
        {
            var stringValue = value as Value<string>;
            if (stringValue != null)
                return stringValue.TypedContents;

            var relationshipValue = value as Value<Data.EntityRelationship>;
            if (relationshipValue != null)
            {
                var entityGuids = relationshipValue.TypedContents.EntityIds.Select(entityId => entityId.HasValue ? DbS.GetEntity(entityId.Value).EntityGUID : Guid.Empty);

                return string.Join(",", entityGuids);
            }

            var boolValue = value as Value<bool?>;
            if (boolValue != null)
                return boolValue.TypedContents.ToString();

            var dateTimeValue = value as Value<DateTime?>;
            if (dateTimeValue != null)
                return dateTimeValue.TypedContents.HasValue ? dateTimeValue.TypedContents.Value.ToString("s") : "";

            var decimalValue = value as Value<decimal?>;
            if (decimalValue != null)
                return decimalValue.TypedContents.HasValue ? decimalValue.TypedContents.Value.ToString(CultureInfo.InvariantCulture) : "";

            throw new NotSupportedException("Can't serialize Value");
        }
	}
}
