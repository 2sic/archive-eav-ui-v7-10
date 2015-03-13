using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav;
using System.Collections.Generic;

namespace ToSic.Eav.UnitTests
{
    [TestClass]
    public class Entity_Test
    {
        [TestMethod]
        public void CreateSimpleUnpersistedEntity()
        {
            var valDaniel = new Dictionary<string, object>()
            {
                {"FirstName", "Daniel"},
                {"LastName", "Mettler"},
                {"Phone", "+41 81 750 67 70"},
                {"Age", 37}
            };
            var entDaniel = new Data.Entity(1, "TestType", valDaniel, "FirstName");

            var notFound = false;
            Assert.AreEqual(1, entDaniel.EntityId);
            Assert.AreEqual(Guid.Empty, entDaniel.EntityGuid);
            Assert.AreEqual("Daniel", entDaniel.Title[0].ToString());
            Assert.AreEqual("Daniel", entDaniel.GetBestValue("EntityTitle"));
            Assert.AreEqual("Daniel", entDaniel.GetBestValue("FirstName", out notFound));
            Assert.AreEqual("Mettler", entDaniel.GetBestValue("LastName", new string[] {"EN"}, out notFound));
        }
    }
}
