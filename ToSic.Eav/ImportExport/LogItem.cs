using System;
using System.Diagnostics;

namespace ToSic.Eav.Import
{
    public class LogItem
    {
        public EventLogEntryType EntryType { get; private set; }
        public ImportEntity ImportEntity { get; set; }
        public ImportAttributeSet ImportAttributeSet { get; set; }
        public ImportAttribute ImportAttribute { get; set; }
        public IValueImportModel Value { get; set; }
        public Exception Exception { get; set; }
        public string Message { get; private set; }

        public LogItem(EventLogEntryType entryType, string message)
        {
            EntryType = entryType;
            Message = message;
        }
    }
}
