using System;
using System.Collections.Generic;
using System.Globalization;
using ToSic.Eav.DataSources;

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
        internal static IValue GetValueModel(string attributeType, string value)
        {
            return GetValueModel(attributeType, (object)value, new Dimension[0], -1, -1);
        }
        /// <summary>
        /// Creates a Typed Value Model
        /// </summary>
        internal static IValue GetValueModel(string attributeType, string value, IEnumerable<ILanguage> languages, int valueID, int changeLogIDCreated)
        {
            return GetValueModel(attributeType, (object)value, languages, valueID, changeLogIDCreated);
        }

        /// <summary>
        /// Creates a Typed Value Model for an Entity-Attribute
        /// </summary>
        internal static IValue GetValueModel(string attributeType, IEnumerable<int> entityIds, IDataSource source)
        {
            return GetValueModel(attributeType, entityIds, new Dimension[0], -1, -1, source);
        }

        /// <summary>
        /// Creates a Typed Value Model
        /// </summary>
        private static IValue GetValueModel(string attributeType, object value, IEnumerable<ILanguage> languages, int valueID, int changeLogIDCreated, IDataSource source = null)
        {
            IValueManagement typedModel;
            var stringValue = value as string;
            try
            {
                switch (attributeType)
                {
                    case "Boolean":
                        typedModel = new Value<bool?>(string.IsNullOrEmpty(stringValue) ? (bool?)null : bool.Parse(stringValue));
                        break;
                    case "DateTime":
                        typedModel = new Value<DateTime?>(string.IsNullOrEmpty(stringValue) ? (DateTime?)null : DateTime.Parse(stringValue));
                        break;
                    case "Number":
                        typedModel = new Value<decimal?>(string.IsNullOrEmpty(stringValue) ? (decimal?)null : decimal.Parse(stringValue, CultureInfo.InvariantCulture));
                        break;
                    case "Entity":
                        var entityIds = value as IEnumerable<int>;
                        typedModel = new Value<EntityRelationship>(new EntityRelationship(source) { EntityIds = entityIds });
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