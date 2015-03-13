using System.Collections.Generic;

namespace ToSic.Eav.Data
{
    /// <summary>
    /// Represents a Zone
    /// </summary>
    public class Zone
    {
        /// <summary>
        /// ZoneId
        /// </summary>
        public int ZoneId { get; internal set; }
        /// <summary>
        /// AppId of the default App in this Zone
        /// </summary>
        public int DefaultAppId { get; internal set; }
        /// <summary>
        /// All Apps in this Zone with Id and Name
        /// </summary>
        public Dictionary<int, string> Apps { get; internal set; }
    }
}