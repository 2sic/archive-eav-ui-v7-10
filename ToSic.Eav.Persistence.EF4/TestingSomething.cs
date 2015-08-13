using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ToSic.Eav
{
    class TestingSomething
    {

        public TestingSomething()
        {
            var x = new EavContext();
            var y = x.AlternateSaveHandler;
        }
    }
}
