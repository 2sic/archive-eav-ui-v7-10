using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace ToSic.Eav.Import
{
    public class ValueImportModel<T> : IValueImportModel
    {
        public T Value { get; set; }
        public IEnumerable<ValueDimension> ValueDimensions { get; set; }
        public ImportEntity ParentEntity { get; private set; }

        public ValueImportModel(ImportEntity parentEntity)
        {
            ParentEntity = parentEntity;
        }

        public string StringValueForTesting 
        {
            get { return Value.ToString(); }
        }
    }

    public static class ValueImportModel
    {   
        public static IValueImportModel GetModel(string value, string type, IEnumerable<ValueDimension> dimensions, ImportEntity importEntity)
        {
            IValueImportModel valueModel;

            switch (type)
            {
                case "String":
                case "Hyperlink":
                    valueModel = new ValueImportModel<string>(importEntity) { Value = value };
                    break;
                case "Number":
                    decimal typedDecimal;
                    var isDecimal = Decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out typedDecimal);
                    decimal? typedDecimalNullable = null;
                    if (isDecimal)
                        typedDecimalNullable = typedDecimal;
                    valueModel = new ValueImportModel<decimal?>(importEntity)
                    {
                        Value = typedDecimalNullable
                    };
                    break;
                case "Entity":
                    var entityGuids = !String.IsNullOrEmpty(value) ? value.Split(',').Select(v =>
                    {
                        var guid = Guid.Parse(v);
                        return guid == Guid.Empty ? new Guid?() : guid;
                    }).ToList() : new List<Guid?>(0);
                    valueModel = new ValueImportModel<List<Guid?>>(importEntity) { Value = entityGuids };
                    break;
                case "DateTime":
                    DateTime typedDateTime;
                    valueModel = new ValueImportModel<DateTime?>(importEntity)
                    {
                        Value = DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out typedDateTime) ? typedDateTime : new DateTime?()
                    };
                    break;
                case "Boolean":
                    bool typedBoolean;
                    valueModel = new ValueImportModel<bool?>(importEntity)
                    {
                        Value = Boolean.TryParse(value, out typedBoolean) ? typedBoolean : new bool?()
                    };
                    break;
                default:
                    throw new ArgumentOutOfRangeException(type, value, "Unknown type argument found in import XML.");
            }

            valueModel.ValueDimensions = dimensions;

            return valueModel;
        }
    }
}