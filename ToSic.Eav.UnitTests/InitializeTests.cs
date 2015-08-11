using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ToSic.Eav.UnitTests
{
    [TestClass]
    class InitializeTests
    {
        [AssemblyInitialize()]
        public static void AssemblyInit(TestContext context)
        {
            new Eav.DataSources.Configuration().ConfigureDefaultMappings(Eav.Factory.Container);
        }
    }
}
