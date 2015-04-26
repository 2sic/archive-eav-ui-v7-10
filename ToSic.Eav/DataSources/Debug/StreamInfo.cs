using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ToSic.Eav.DataSources.Debug
{
    public class StreamInfo
    {
        public Guid Target;
        public Guid Source;
        public string SourceOut;
        public string TargetIn;
        public int Count;
        public bool Error = false;

        public StreamInfo(IDataStream strm, IDataTarget target, string inName)
        {
            try
            {
                Target = (target as IDataSource).DataSourceGuid;
                Source = strm.Source.DataSourceGuid;
                TargetIn = inName;
                foreach (var outStm in strm.Source.Out)
                    if (outStm.Value == strm)
                        SourceOut = outStm.Key;

                //SourceOut = strm.Source.Out.(strm)
                // SourceOut = strm.Name;
                Count = strm.List.Count;
            }
            catch
            {
                Error = true;
            }
        }
    }
}
