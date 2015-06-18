using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.DataSources;
using ToSic.Eav.DataSources.Caches;

namespace ToSic.Eav.UnitTests.DataSources.Caches
{
    [TestClass]
    public class QuickCaches_Test
    {
        [TestMethod]
        public void QuickCache_AddListAndCheckIfIn()
        {
            const string ItemToFilter = "1023";
            var ds = CreateFilterForTesting(100, ItemToFilter);

            var cache = ds.Cache;
            var listCache = cache as IListCache;
            Assert.IsFalse(listCache.HasList(ds.CacheFullKey), "Should not have it in cache yet");

            listCache.SetList(ds.CacheFullKey, ds.LightList, ds.CacheLastRefresh);
            Assert.IsTrue(listCache.HasList(ds.CacheFullKey), "Should have it in cache now");
            Assert.IsTrue(listCache.HasList(ds), "Should also have the DS default");
            
            Assert.IsTrue(listCache.HasList(ds[DataSource.DefaultStreamName], false), "should have it by stream as well");
            Assert.IsFalse(listCache.HasList(ds[DataSource.DefaultStreamName], true), "should not have it named ATM");
            Assert.IsFalse(listCache.HasList(ds[DataSource.DefaultStreamName]), "should not have it named ATM with default parameter");
            

            // Try to auto-retrieve 
            IEnumerable<IEntity> cached = listCache.GetList(ds.CacheFullKey).LightList;

            Assert.AreEqual(1, cached.Count());

            cached = listCache.GetList(ds).LightList;
            Assert.AreEqual(1, cached.Count());

            var lci = listCache.GetList(ds[DataSource.DefaultStreamName]);
            Assert.AreEqual(null, lci, "Cached should be null because the name isn't correct");

            lci = listCache.GetList(ds[DataSource.DefaultStreamName], true);
            Assert.AreEqual(null, lci, "Cached should be null because the name isn't correct");


            cached = listCache.GetList(ds[DataSource.DefaultStreamName], false).LightList;
            Assert.AreEqual(1, cached.Count());

        }

        [TestMethod]
        public void QuickCache_AddAndWaitForReTimeout()
        {
            const string ItemToFilter = "1027";
            var ds = CreateFilterForTesting(100, ItemToFilter);

            var listCache = ds.Cache as IListCache;
            listCache.DefaultListRetentionTimeInSeconds = 1;
            Assert.IsFalse(listCache.HasList(ds.CacheFullKey), "Should not have it in cache yet");

            listCache.SetList(ds.CacheFullKey, ds.LightList, ds.CacheLastRefresh);
            Assert.IsTrue(listCache.HasList(ds.CacheFullKey), "Should have it in cache now");

            Thread.Sleep(400);
            Assert.IsTrue(listCache.HasList(ds.CacheFullKey), "Should STILL be in cache");

            Thread.Sleep(601);
            Assert.IsFalse(listCache.HasList(ds.CacheFullKey), "Should NOT be in cache ANY MORE");
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
