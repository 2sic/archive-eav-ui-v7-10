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
		private readonly EavContext _ctx;

		/// <summary>
		/// Initializes a new instance of the XmlImport class.
		/// </summary>
		public XmlImport(EavContext ctx)
		{
			_ctx = ctx;
		}

		/// <summary>
		/// Returns an EAV import entity
		/// </summary>
		/// <returns></returns>
		public Import.Entity GetImportEntityUnsafe(XElement xEntity, int assignmentObjectTypeId, int? keyNumber = null)
		{
			var entity = new Import.Entity
			{
				AssignmentObjectTypeId = assignmentObjectTypeId,
				AttributeSetStaticName = xEntity.Attribute("AttributeSetStaticName").Value,
				EntityGuid = Guid.Parse(xEntity.Attribute("EntityGUID").Value),
				KeyNumber = keyNumber,
				Values = new Dictionary<string, List<IValueImportModel>>()
			};

			// Get all Values from flat xEntity as anonymous Model
			var flatValues = from v in xEntity.Elements("Value")
							 select new
							 {
								 Key = v.Attribute("Key").Value,
								 Type = v.Attribute("Type").Value,
								 v.Attribute("Value").Value,
								 Dimensions = from d in v.Elements("Dimension")
											  select new
											  {
												  DimensionId = int.Parse(d.Attribute("DimensionID").Value),
												  ReadOnly = bool.Parse(d.Attribute("ReadOnly").Value)
											  }
							 };

			// Group values by Attribute
			var attributeValues = from v in flatValues
								  group v by v.Key into valuesGrouped
								  select new
								  {
									  AttributeStaticName = valuesGrouped.Key,
									  AttributeType = valuesGrouped.First().Type,
									  Values = from v2 in valuesGrouped
											   select new { StringValue = v2.Value, v2.Dimensions }
								  };

			// Append Attributes Import-Entity
			foreach (var attr in attributeValues)
			{
				var valuesList = new List<IValueImportModel>();

				// Add all Values
				foreach (var value in attr.Values)
				{
					// Create typed ValueModel
					IValueImportModel valueModel;
					switch (attr.AttributeType)
					{
						case "String":
						case "Hyperlink":
							valueModel = new ValueImportModel<string>(entity) { Value = value.StringValue };
							break;
						case "Boolean":
							bool? boolValue = null;
							if (!string.IsNullOrEmpty(value.StringValue))
								boolValue = bool.Parse(value.StringValue);
							valueModel = new ValueImportModel<bool?>(entity) { Value = boolValue };
							break;
						case "DateTime":
							DateTime? dateTimeValue = null;
							if (!string.IsNullOrEmpty(value.StringValue))
								dateTimeValue = DateTime.Parse(value.StringValue);
							valueModel = new ValueImportModel<DateTime?>(entity) { Value = dateTimeValue };
							break;
						case "Number":
							decimal? decimalValue = null;
							if (!string.IsNullOrEmpty(value.StringValue))
								decimalValue = decimal.Parse(value.StringValue);
							valueModel = new ValueImportModel<decimal?>(entity) { Value = decimalValue };
							break;
						case "Entity":
							var entityGuids = !string.IsNullOrEmpty(value.StringValue) ? value.StringValue.Split(',').Select(Guid.Parse).ToList() : new List<Guid>(0);
							valueModel = new ValueImportModel<List<Guid>>(entity) { Value = entityGuids };
							break;
						default:
							throw new NotSupportedException("Import");
					}

					// add all Dimensions
					var dimensionList = new List<Import.ValueDimension>();
					foreach (var dimension in value.Dimensions)
					{
						var dimensionExternalKey = _ctx.GetDimension(dimension.DimensionId).ExternalKey;
						var valueDimension = new Import.ValueDimension { ReadOnly = dimension.ReadOnly, DimensionExternalKey = dimensionExternalKey };
						dimensionList.Add(valueDimension);
					}
					valueModel.ValueDimensions = dimensionList;

					valuesList.Add(valueModel);
				}

				entity.Values.Add(attr.AttributeStaticName, valuesList);
			}


			return entity;
		}

		class ImportValue
		{
			public XElement XmlValue;
			public List<Import.ValueDimension> Dimensions;
		}

		/// <summary>
		/// Returns an EAV import entity
		/// </summary>
		/// <returns></returns>
		public Import.Entity GetImportEntity(XElement xEntity, int assignmentObjectTypeId, List<Dimension> targetDimensions, List<Dimension> sourceDimensions, int sourceDefaultDimensionId, string defaultLanguage, int? keyNumber = null)
		{
			var attributeSetStaticName = xEntity.Attribute("AttributeSetStaticName").Value;

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