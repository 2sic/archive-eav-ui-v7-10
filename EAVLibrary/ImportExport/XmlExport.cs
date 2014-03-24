using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace ToSic.Eav.ImportExport
{
	/// <summary>
	/// Export EAV Data in XML Format
	/// </summary>
	public class XmlExport
	{
		private readonly EavContext _ctx;
		private readonly List<int> _referencedFileIds = new List<int>();

		/// <summary>
		/// Delegate for additional modifications to the created Value-XElement
		/// </summary>
		public delegate void ExtendValueDelegate(string attributeStaticname, string attributeSetStaticName, string value, XElement valueXElement);

		/// <summary>
		/// Gets a List of all FileIds in any exported Entity-Values
		/// </summary>
		public List<int> ReferencedFileIds
		{
			get { return _referencedFileIds; }
		}

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
		public XElement GetEntityXElement(Entity e, ExtendValueDelegate extendValueDelegate = null)
		{
			var attributeSet = _ctx.GetAttributeSet(e.AttributeSetID);

			// create Entity-Element, get values from GetAttributeValueXElement()
			return new XElement("Entity",
				new XAttribute("AssignmentObjectType", e.AssignmentObjectType.Name),
				new XAttribute("AttributeSetStaticName", attributeSet.StaticName),
				new XAttribute("AttributeSetName", attributeSet.Name),
				new XAttribute("EntityGUID", e.EntityGUID),
				from v in _ctx.GetValues(e.EntityID)
				where v.ChangeLogDeleted == null
				select GetAttributeValueXElement(v.Attribute.StaticName, v, v.Attribute.Type, attributeSet.StaticName, extendValueDelegate));
		}

		/// <summary>
		/// Gets an Entity Value XElement
		/// </summary>
		private XElement GetAttributeValueXElement(string attributeStaticname, EavValue value, string attributeType, string attributeSetStaticName, ExtendValueDelegate extendValueDelegate)
		{
			var valueSerialized = EavContext.SerializeValue(value.Value);
			// create Value-Child-Element with Dimensions as Children
			var valueXElement = new XElement("Value",
				new XAttribute("Key", attributeStaticname),
				new XAttribute("Value", valueSerialized),
				!String.IsNullOrEmpty(attributeType) ? new XAttribute("Type", attributeType) : null,
				value.ValuesDimensions.Select(p => new XElement("Dimension",
						new XAttribute("DimensionID", p.DimensionID),
						new XAttribute("ReadOnly", p.ReadOnly)
					))
				);

			if (extendValueDelegate != null)
				extendValueDelegate(attributeStaticname, attributeSetStaticName, valueSerialized, valueXElement);

			// Collect all FileIds
			if (attributeType == "Hyperlink")
			{
				var fileMatch = Regex.Match(valueSerialized, "^File:(?<FileId>[0-9]+)", RegexOptions.IgnoreCase);
				if (fileMatch.Success && fileMatch.Groups["FileId"].Length > 0)
					_referencedFileIds.Add(int.Parse(fileMatch.Groups["FileId"].Value));
			}

			return valueXElement;
		}
	}
}
