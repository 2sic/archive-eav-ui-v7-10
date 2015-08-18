using System;
using System.Collections.Generic;

namespace ToSic.Eav.WebApi.Formats
{
    public class EntityWithLanguages
    {
        public int Id;
        public Guid Guid;
        public Type Type;
        public string TitleAttributeName;
        public Attribute[] Attributes;
    }

    public class Attribute
    {
        public string Key;
        public ValueSet[] Values;
    }

    public class ValueSet
    {
        public object Value;
        public Dictionary<string, bool> Dimensions;
    }

    public class Type
    {
        public string Name;
        public string StaticName;
    }
}
