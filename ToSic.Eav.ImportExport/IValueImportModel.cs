using System;
using System.Collections.Generic;

namespace ToSic.Eav.Import
{
    public interface IValueImportModel
    {
        IEnumerable<ValueDimension> ValueDimensions { get; set; }
        ImportEntity ParentEntity { get; }

        String StringValueForTesting { get; }
    }
}