using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.DataSources;

namespace ToSic.Eav.UnitTests.DataSources
{
    [TestClass]
    public class EntityIdFilter_Test
    {
        [TestMethod]
        public void EntityIdFilter_SingleItem()
        {
            const string ItemToFilter = "1023";
            var filtered = CreateFilterForTesting(100, ItemToFilter);
            var list = filtered.List;

            Assert.AreEqual(ItemToFilter, list.First().Value.EntityId.ToString());
        }


        [TestMethod]
        public void EntityIdFilter_NoItems()
        {
            const string ItemToFilter = "";
            var filtered = CreateFilterForTesting(100, ItemToFilter);
            var list = filtered.List;

            Assert.AreEqual(0, list.Count, "Should return 0 items");
        }

        [TestMethod]
        public void EntityIdFilter_MultipleItems()
        {
            const string ItemToFilter = "1011,1023,1050,1003";
            var filtered = CreateFilterForTesting(100, ItemToFilter);
            var list = filtered.List;

            Assert.AreEqual("1011", list.First().Value.EntityId.ToString(), "Test that sorting IS affeted");
            Assert.AreEqual(4, list.Count, "Count after filtering");
        }

        [TestMethod]
        public void EntityIdFilter_FilterWithSpaces()
        {
            const string ItemToFilter = "1011, 1023 ,1050   ,1003";
            var filtered = CreateFilterForTesting(100, ItemToFilter);
            var list = filtered.List;

            Assert.AreEqual("1011", list.First().Value.EntityId.ToString(), "Test that sorting IS affeted");
            Assert.AreEqual(4, list.Count, "Count after filtering");
        }

        public static EntityIdFilter CreateFilterForTesting(int testItemsInRootSource, string entityIdsValue)
        {
            var ds = DataTableDataSource_Test.GeneratePersonSourceWithDemoData(testItemsInRootSource, 1001);
            var filtered = new EntityIdFilter();
            filtered.ConfigurationProvider = ds.ConfigurationProvider;
            filtered.Attach(ds);
            filtered.EntityIds = entityIdsValue;
            return filtered;
        }
    }
}
