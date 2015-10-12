using System;
using System.Collections.Generic;
using System.Globalization;
// using ToSic.Eav.DataSources;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav.Data
{
    /// <summary>
    /// Represents a Value
    /// </summary>
    public class Value
    {
        public int ValueId { get; set; }
        public IEnumerable<ILanguage> Languages { get; set; }
        public int ChangeLogIdCreated { get; set; }


        /// <summary>
        /// Creates a Typed Value Model
        /// </summary>
        public static IValue GetValueModel(string attributeType, string value)
        {
            return GetValueModel(attributeType, (object)value, new Dimension[0], -1, -1);
        }
        /// <summary>
        /// Creates a Typed Value Model
        /// </summary>
        public static IValue GetValueModel(string attributeType, string value, IEnumerable<ILanguage> languages, int valueID, int changeLogIDCreated)
        {
            return GetValueModel(attributeType, (object)value, languages, valueID, changeLogIDCreated);
        }

        /// <summary>
        /// Creates a Typed Value Model for an Entity-Attribute
        /// </summary>
        public static IValue GetValueModel(string attributeType, IEnumerable<int?> entityIds, IDeferredEntitiesList fullEntityListForLookup = null)
        {
            return GetValueModel(attributeType, entityIds, new Dimension[0], -1, -1, fullEntityListForLookup);
        }

        /// <summary>
        /// Creates a Typed Value Model
        /// </summary>
        private static IValue GetValueModel(string attributeType, object value, IEnumerable<ILanguage> languages, int valueID, int changeLogIDCreated, IDeferredEntitiesList fullEntityListForLookup = null)
        {
            IValueManagement typedModel;
            var stringValue = value as string;
            try
            {
                switch (attributeType)
                {
                    case "Boolean": // todo: replace with AttributeTypeEnum.Boolean.ToString() and test
                        typedModel = new Value<bool?>(string.IsNullOrEmpty(stringValue) ? (bool?)null : bool.Parse(stringValue));
                        break;
                    case "DateTime": // todo: replace with AttributeTypeEnum...
                        typedModel = new Value<DateTime?>(string.IsNullOrEmpty(stringValue) ? (DateTime?)null : DateTime.Parse(stringValue));
                        break;
                    case "Number": // todo: replace with AttributeTypeEnum...
                        typedModel = new Value<decimal?>(string.IsNullOrEmpty(stringValue) ? (decimal?)null : decimal.Parse(stringValue, CultureInfo.InvariantCulture));
                        break;
                    case "Entity": // todo: replace with AttributeTypeEnum...
                        var entityIds = value as IEnumerable<int?>;
                        typedModel = new Value<EntityRelationship>(new EntityRelationship(fullEntityListForLookup, entityIds));
                        break;
                    default:
                        typedModel = new Value<string>(stringValue);
                        break;
                }
            }
            catch
            {
                return new Value<string>(stringValue);
            }

            typedModel.Languages = languages;
            typedModel.ValueId = valueID;
            typedModel.ChangeLogIdCreated = changeLogIDCreated;

            return (IValue)typedModel;
        }
    }
}