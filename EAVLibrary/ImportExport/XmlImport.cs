using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using ToSic.Eav.Import;

namespace ToSic.Eav.ImportExport
{
	/// <summary>
	/// Import EAV Data from XML Format
	/// </summary>
	public class XmlImport
	{
		private class ImportValue
		{
			public XElement XmlValue;
			public List<Import.ValueDimension> Dimensions;
		}

		/// <summary>
		/// Returns an EAV import entity
		/// </summary>
		/// <param name="xEntity">xEntity to parse</param>
		/// <param name="assignmentObjectTypeId">assignmentObjectTypeId of the Entity</param>
		/// <param name="targetDimensions">all Dimensions that exist in the Target-App/Zone</param>
		/// <param name="sourceDimensions">all Dimensions that exist in the Source-App/Zone</param>
		/// <param name="sourceDefaultDimensionId">Default Dimension ID of the Surce-App/Zone</param>
		/// <param name="defaultLanguage">Default Language of the Target-App/Zone</param>
		/// <param name="keyNumber">KeyNumber of the Entity</param>
		public Import.Entity GetImportEntity(XElement xEntity, int assignmentObjectTypeId, List<Dimension> targetDimensions, List<Dimension> sourceDimensions, int sourceDefaultDimensionId, string defaultLanguage, int? keyNumber = null)
		{
			//var attributeSetStaticName = xEntity.Attribute("AttributeSetStaticName").Value;

			// ToDo: Review!
			//// Special case: App AttributeSets must be assigned to the current app
			//if (xEntity.Attribute("AssignmentObjectType").Value == "App")
			//{
			//	keyNumber = _appId;
			//	assignmentObjectTypeId = SexyContent.AssignmentObjectTypeIDSexyContentApp;
			//}

			var targetEntity = new Import.Entity
			{
				AssignmentObjectTypeId = assignmentObjectTypeId,
				AttributeSetStaticName = xEntity.Attribute("AttributeSetStaticName").Value,
				EntityGuid = Guid.Parse(xEntity.Attribute("EntityGUID").Value),
				KeyNumber = keyNumber
			};

			var targetValues = new Dictionary<string, List<IValueImportModel>>();

			// Group values by StaticName
			var valuesGroupedByStaticName = xEntity.Elements("Value")
				.GroupBy(v => v.Attribute("Key").Value, e => e, (key, e) => new { StaticName = key, Values = e.ToList() });

			// Process each attribute (values grouped by StaticName)
			foreach (var sourceAttribute in valuesGroupedByStaticName)
			{
				var sourceValues = sourceAttribute.Values;
				var tempTargetValues = new List<ImportValue>();

				// Process each target's language
				foreach (var targetDimension in targetDimensions.OrderByDescending(p => p.ExternalKey == defaultLanguage).ThenBy(p => p.ExternalKey))
				{
					// This list will contain all source dimensions
					var sourceLanguages = new List<Dimension>();

					// Add exact match source language, if exists
					var exactMatchSourceDimension = sourceDimensions.FirstOrDefault(p => p.ExternalKey == targetDimension.ExternalKey);
					if (exactMatchSourceDimension != null)
						sourceLanguages.Add(exactMatchSourceDimension);

					// Add un-exact match language
					var unExactMatchSourceDimensions = sourceDimensions.Where(p => p.ExternalKey != targetDimension.ExternalKey && p.ExternalKey.StartsWith(targetDimension.ExternalKey.Substring(0, 3)))
						.OrderByDescending(p => p.ExternalKey == defaultLanguage)
						.ThenByDescending(p => p.ExternalKey.Substring(0, 2) == p.ExternalKey.Substring(3, 2))
						.ThenBy(p => p.ExternalKey);
					sourceLanguages.AddRange(unExactMatchSourceDimensions);

					// Add primary language, if current target is primary
					if (targetDimension.ExternalKey == defaultLanguage)
					{
						var sourcePrimaryLanguage = sourceDimensions.FirstOrDefault(p => p.DimensionID == sourceDefaultDimensionId);
						if (sourcePrimaryLanguage != null && !sourceLanguages.Contains(sourcePrimaryLanguage))
							sourceLanguages.Add(sourcePrimaryLanguage);
					}

					XElement sourceValue = null;
					var readOnly = false;

					foreach (var sourceLanguage in sourceLanguages)
					{
						sourceValue = sourceValues.FirstOrDefault(p => p.Elements("Dimension").Any(d => d.Attribute("DimensionID").Value == sourceLanguage.DimensionID.ToString()));

						if (sourceValue == null)
							continue;

						readOnly = Boolean.Parse(sourceValue.Elements("Dimension").FirstOrDefault(p => p.Attribute("DimensionID").Value == sourceLanguage.DimensionID.ToString()).Attribute("ReadOnly").Value);

						// Override ReadOnly for primary target language
						if (targetDimension.ExternalKey == defaultLanguage)
							readOnly = false;

						break;
					}

					// Take first value if there is only one value wihtout a dimension (default / fallback value), but only in primary language
					if (sourceValue == null && sourceValues.Count == 1 && !sourceValues.Elements("Dimension").Any() && targetDimension.ExternalKey == defaultLanguage)
						sourceValue = sourceValues.First();

					// Process found value
					if (sourceValue != null)
					{
						// Special cases for template-describing values
						//if (attributeSetStaticName == SexyContent.AttributeSetStaticNameTemplateContentTypes)
						//{
						//	var sourceValueString = sourceValue.Attribute("Value").Value;
						//	if (!String.IsNullOrEmpty(sourceValueString))
						//	{
						//		switch (sourceAttribute.StaticName)
						//		{
						//			case "ContentTypeID":
						//				var attributeSet = _sexy.ContentContext.AttributeSetExists(sourceValueString, _sexy.ContentContext.AppId) ? _sexy.ContentContext.GetAttributeSet(sourceValueString) : null;
						//				sourceValue.Attribute("Value").SetValue(attributeSet != null ? attributeSet.AttributeSetID.ToString() : "0");
						//				break;
						//			case "DemoEntityID":
						//				var entityGuid = new Guid(sourceValue.Attribute("Value").Value);
						//				var demoEntity = _sexy.ContentContext.EntityExists(entityGuid) ? _sexy.ContentContext.GetEntity(entityGuid) : null;
						//				sourceValue.Attribute("Value").SetValue(demoEntity != null ? demoEntity.EntityID.ToString() : "0");
						//				break;
						//		}
						//	}
						//}


						//// Correct FileId in Hyperlink fields (takes XML data that lists files)
						//if (sourceValue.Attribute("Type").Value == "Hyperlink")
						//{
						//	var sourceValueString = sourceValue.Attribute("Value").Value;
						//	var fileRegex = new Regex("^File:(?<FileId>[0-9]+)", RegexOptions.IgnoreCase);
						//	var a = fileRegex.Match(sourceValueString);
						//	if (a.Success && a.Groups["FileId"].Length > 0)
						//	{
						//		var originalId = int.Parse(a.Groups["FileId"].Value);

						//		if (_fileIdCorrectionList.ContainsKey(originalId))
						//		{
						//			var newValue = fileRegex.Replace(sourceValueString, "File:" + _fileIdCorrectionList[originalId].ToString());
						//			sourceValue.Attribute("Value").SetValue(newValue);
						//		}

						//	}
						//}

						var dimensionsToAdd = new List<Import.ValueDimension>();
						if (targetDimensions.Single(p => p.ExternalKey == targetDimension.ExternalKey).DimensionID >= 1)
							dimensionsToAdd.Add(new Import.ValueDimension { DimensionExternalKey = targetDimension.ExternalKey, ReadOnly = readOnly });

						// If value has already been added to the list, add just dimension with original ReadOnly state
						var existingImportValue = tempTargetValues.FirstOrDefault(p => p.XmlValue == sourceValue);
						if (existingImportValue != null)
							existingImportValue.Dimensions.AddRange(dimensionsToAdd);
						else
						{
							tempTargetValues.Add(new ImportValue
							{
								Dimensions = dimensionsToAdd,
								XmlValue = sourceValue
							});
						}

					}

				}

				var currentAttributesImportValues = tempTargetValues.Select(tempImportValue => GetImportValue(tempImportValue.XmlValue, tempImportValue.Dimensions, targetEntity)).ToList();
				targetValues.Add(sourceAttribute.StaticName, currentAttributesImportValues);
			}

			targetEntity.Values = targetValues;

			return targetEntity;
		}

		private static IValueImportModel GetImportValue(XElement xValue, IEnumerable<Import.ValueDimension> valueDimensions, Import.Entity referencingEntity)
		{
			var stringValue = xValue.Attribute("Value").Value;
			var type = xValue.Attribute("Type").Value;

			IValueImportModel valueModel;

			switch (type)
			{
				case "String":
				case "Hyperlink":
					valueModel = new ValueImportModel<string>(referencingEntity) { Value = stringValue };
					break;
				case "Number":
					decimal typedDecimal;
					var isDecimal = Decimal.TryParse(stringValue, NumberStyles.Any, CultureInfo.InvariantCulture, out typedDecimal);
					decimal? typedDecimalNullable = null;
					if (isDecimal)
						typedDecimalNullable = typedDecimal;
					valueModel = new ValueImportModel<decimal?>(referencingEntity)
					{
						Value = typedDecimalNullable
					};
					break;
				case "Entity":
					var entityGuids = !string.IsNullOrEmpty(stringValue) ? stringValue.Split(',').Select(Guid.Parse).ToList() : new List<Guid>(0);
					valueModel = new ValueImportModel<List<Guid>>(referencingEntity) { Value = entityGuids };
					break;
				case "DateTime":
					DateTime typedDateTime;
					valueModel = new ValueImportModel<DateTime?>(referencingEntity)
					{
						Value = DateTime.TryParse(stringValue, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out typedDateTime) ? typedDateTime : new DateTime?()
					};
					break;
				case "Boolean":
					bool typedBoolean;
					valueModel = new ValueImportModel<bool?>(referencingEntity)
					{
						Value = Boolean.TryParse(stringValue, out typedBoolean) ? typedBoolean : new bool?()
					};
					break;
				default:
					throw new ArgumentOutOfRangeException(type, stringValue, "Unknown type argument found in import XML.");
			}

			valueModel.ValueDimensions = valueDimensions;

			return valueModel;
		}
	}
}