using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ToSic.Eav.DataSources.Exceptions
{
    class DataSourceException: System.Exception
    {
        private string p;
        private Exception ex;

        public DataSourceException(string p)
        {
            // TODO: Complete member initialization
            this.p = p;
        }

        public DataSourceException(string p, Exception ex)
        {
            // TODO: Complete member initialization
            this.p = p;
            this.ex = ex;
        }
    }
}
