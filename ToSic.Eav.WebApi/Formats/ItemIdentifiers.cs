using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ToSic.Eav.WebApi.Formats
{

    public class ItemIdentifier
    {
        // simple entity identifier (to edit existing)...
        public int EntityId { get; set; }

        // ...or content-type (for new)
        public string ContentTypeName { get; set; }

        #region Additional Assignment information
        public Metadata Metadata { get; set; }
        #endregion
        public GroupAssignment Group { get; set; }

        // this is not needed on the server, but must be passed through so it's still attached to this item if in use
        public dynamic Prefill { get; set; }
        public string Title { get; set; }
    }

    public class EntityWithHeader
    {
        public ItemIdentifier Header { get; set; }
        public EntityWithLanguages Entity { get; set; }
    }

    public class GroupAssignment
    {
        public Guid Guid { get; set; }

        /// <summary>
        /// The Set is either "content" or "listcontent", "presentation" or "listpresentation"
        /// </summary>
        public string Part { get; set; }

        public int Index { get; set; }

        public bool Add { get; set; }
    }
}
