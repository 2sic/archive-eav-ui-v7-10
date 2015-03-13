using System.Collections.Generic;

namespace ToSic.Eav.Data
{
    /// <summary>
    /// MetaData of an AttributeHelperTools
    /// </summary>
    public class AttributeWithMetaInfo : Eav.Attribute
    {
        /// <summary>
        /// Indicates whether Attributes is Title of the AttributeSet
        /// </summary>
        public bool IsTitle { get; internal set; }
        /// <summary>
        /// AttributeHelperTools name
        /// </summary>
        public string Name { get; internal set; }
        /// <summary>
        /// AttributeHelperTools Notes
        /// </summary>
        public string Notes { get; internal set; }
        /// <summary>
        /// Indicates whether MetaData has items
        /// </summary>
        public bool HasTypeMetaData { get; internal set; }
        /// <summary>
        /// Dictionary with all MetaData belonging to the AttributeHelperTools
        /// </summary>
        public IDictionary<string, IAttribute> MetaData { get; internal set; }
    }
}