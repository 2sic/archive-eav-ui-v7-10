using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Practices.Unity;
using ToSic.Eav.Implementations.ValueConverter;
using ToSic.Eav.Import;
using System.Collections;

namespace ToSic.Eav.ImportExport.Refactoring.Extensions
{
    public static class EntityImportExtension
    {
        /// <summary>
        /// Get values of an attribute in all languages, for example Tobi (German) and Toby (English) of 
        /// the attribute Name.
        /// </summary>
        public static IEnumerable<IValueImportModel> GetAttributeValues(this Import.ImportEntity importEntity, string valueName)
        {
            return importEntity.Values.Where(item => item.Key == valueName).Select(item => item.Value).FirstOrDefault();
        }

        /// <summary>
        /// Get the value of an attribute in the language specified.
        /// </summary>
        public static IValueImportModel GetAttributeValue(this Import.ImportEntity importEntity, string valueName, string valueLanguage)
        {
            var values = importEntity.GetAttributeValues(valueName);
            if (values == null)
            {
                return null;
            }
            return values.Where(value => value.ValueDimensions.Any(dimension => dimension.DimensionExternalKey == valueLanguage)).FirstOrDefault();
        }

        /// <summary>
        /// Add a value to the attribute specified. To do so, set the name, type and string of the value, as 
        /// well as some language properties.
        /// </summary>
        public static IValueImportModel AppendAttributeValue(this Import.ImportEntity importEntity, string valueName, string valueString, string valueType, string valueLanguage = null, bool valueReadOnly = false, bool resolveHyperlink = false)
        {
            var valueModel = GetValueModel(importEntity, valueString, valueType, valueLanguage, valueReadOnly, resolveHyperlink);          
            var entityValue = importEntity.Values.Where(item => item.Key == valueName).Select(item => item.Value).FirstOrDefault();
            if (entityValue == null)
            {
                importEntity.Values.Add(valueName, valueModel.ToList());
            }
            else
            {
                importEntity.Values[valueName].Add(valueModel);
            }
            return valueModel;
        }

        public static IValueImportModel AppendAttributeValue(this Import.ImportEntity importEntity, string valueName, object value, string valueType, string valueLanguage = null, bool valueReadOnly = false, bool resolveHyperlink = false)
        {
            var valueString = string.Empty;

            var enumerable = value as IEnumerable;
            if (enumerable != null)
            {
                valueString = string.Join(",", enumerable);
            }
            else
            {
                valueString = value.ToString();
            }

            return importEntity.AppendAttributeValue(valueName, valueString, valueType, valueLanguage, valueReadOnly, resolveHyperlink);

        }


        public static void AppendAttributeValues(this Import.ImportEntity importEntity, AttributeSet attributeSet, Dictionary<string, object> values, string valuesLanguage, bool valuesReadOnly, bool resolveHyperlink)
        {
            foreach (var value in values)
            {
                // Handle special attributes (for example of the system)
                if (value.Key == "IsPublished")
                {
                    importEntity.IsPublished = value.Value is bool ? (bool)value.Value : true;
                    continue;
                }        
                // Handle content-type attributes
                var attribute = attributeSet.GetAttribute(value.Key);
                if (attribute == null)
                { 
                    throw new ArgumentException("Attribute '" + value.Key + "' does not exist.");
                }
                importEntity.AppendAttributeValue(value.Key, value.Value.ToString(), attribute.Type, valuesLanguage, valuesReadOnly, resolveHyperlink);
            }
        }


        private static IValueImportModel GetValueModel(Import.ImportEntity importEntity, string valueString, string valueType, string valueLanguage = null, bool valueRedOnly = false, bool resolveHyperlink = false)
        {
            IValueImportModel valueModel;
            var valueConverter = Factory.Container.Resolve<IEavValueConverter>();
            switch (valueType)
            {
                case "Boolean":
                    {
                        valueModel = new ValueImportModel<bool?>(importEntity)
                        {
                            Value = string.IsNullOrEmpty(valueString) ? null : new Boolean?(Boolean.Parse(valueString))
                        };
                    }
                    break;

                case "Number":
                    {
                        valueModel = new ValueImportModel<decimal?>(importEntity)
                        {
                            Value = string.IsNullOrEmpty(valueString) ? null : new Decimal?(Decimal.Parse(valueString))
                        };
                    }
                    break;

                case "DateTime":
                    {
                        valueModel = new ValueImportModel<DateTime?>(importEntity)
                        {
                            Value = string.IsNullOrEmpty(valueString) ? null : new DateTime?(DateTime.Parse(valueString))
                        };
                    }
                    break;

                case "Hyperlink":
                    {
                        string valueReference;
                        if (string.IsNullOrEmpty(valueString) || !resolveHyperlink)
                            valueReference = valueString;
                        else
                        {
                            valueReference = valueConverter.Convert(ConversionScenario.ConvertFriendlyToData, valueType, valueString);
                        }
                        valueModel = new ValueImportModel<string>(importEntity) { Value = valueReference };
                    }
                    break;

                case "Entity":
                    {
                        valueModel = new ValueImportModel<List<Guid>>(importEntity) 
                        { 
                            Value = string.IsNullOrEmpty(valueString) ? new List<Guid>() : valueString.Split(',').Select(Guid.Parse).ToList()
                        };
                    }
                    break;

                default:
                    {   // String
                        valueModel = new ValueImportModel<string>(importEntity) { Value = HttpUtility.HtmlDecode(valueString) };
                    }
                    break;
            }
            if (valueLanguage != null)
            {
                valueModel.AppendLanguageReference(valueLanguage, valueRedOnly);
            }
            return valueModel;
        }


        public static void Import(this Import.ImportEntity importEntity, int zoneId, int appId, string userName)
        {
            var import = new Eav.Import.Import(zoneId, appId, userName, false);
            import.RunImport(null, new[] { importEntity }, true, true);
        }
    }
}