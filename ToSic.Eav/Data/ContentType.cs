using System.Collections.Generic;

namespace ToSic.Eav.Data
{
    /// <summary>
    /// Represents a Content Type
    /// </summary>
    public class ContentType : IContentType
    {
        #region simple properties
        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Static name - can be a GUID or a system-term for special types
        /// </summary>
        public string StaticName { get; set; }
        /// <summary>
        /// Internal Id of the attribute-set of this content-type. Don't worry about this one, you probably won't understand it and that's ok. 
        /// </summary>
        public int AttributeSetId { get; private set; }
        /// <summary>
        /// What this content-types if for, if it's a system type or something
        /// </summary>
        public string Scope { get; private set; }
        /// <summary>
        /// todo
        /// </summary>
        public int? UsesConfigurationOfAttributeSet { get; private set; }

        /// <summary>
        /// Dictionary with all AttributeHelperTools Definitions
        /// </summary>
        internal IDictionary<int, AttributeBase> AttributeDefinitions { get; set; }
        #endregion

        /// <summary>
        /// Initializes a new instance of the ContentType class.
        /// </summary>
        public ContentType(string name, string staticName, int attributeSetId, string scope, int? usesConfigurationOfAttributeSet)
        {
            Name = name;
            StaticName = staticName;
            AttributeSetId = attributeSetId;
            Scope = scope;
            UsesConfigurationOfAttributeSet = usesConfigurationOfAttributeSet;
        }

        /// <summary>
        /// Overload for in-memory entities
        /// </summary>
        /// <param name="name"></param>
        public ContentType(string name)
        {
            Name = name;
        }
    }
}