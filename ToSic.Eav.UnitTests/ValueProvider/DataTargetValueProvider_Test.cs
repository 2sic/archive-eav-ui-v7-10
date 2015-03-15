using System;
using System.Runtime.Remoting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.DataSources;
using ToSic.Eav.UnitTests.DataSources;

namespace ToSic.Eav.UnitTests.ValueProvider
{
    [TestClass]
    public class DataTargetValueProvider_Test
    {
        [TestMethod]
        public void DataTargetValueProvider_()
        {
            var pt = new EntityIdFilter();
            // Assemble a simple source-stream with demo data
            const int ItemsToGenerate = 499;
            const string ItemToFilter = "1023";
            var ds = DataTableDataSource_Test.GeneratePersonSourceWithDemoData(ItemsToGenerate, 1001);
            var ds2 = new EntityIdFilter();
            ds2.Attach(ds);
            ds2.EntityIds = ItemToFilter;

            pt.Configuration.Add("SomethingSimple", "Something");
            pt.Configuration.Add("Token1", new ValueCollectionProvider_Test().OriginalSettingDefaultCat);
            pt.Configuration.Add("InTestTitle", "[In:Default:EntityTitle]");
            pt.Configuration.Add("InTestFirstName", "[In:Default:FirstName]");
            pt.Configuration.Add("InTestBadStream", "[In:InvalidStream:Field]");
            pt.Configuration.Add("InTestNoKey", "[In:Default]");
            pt.Configuration.Add("InTestBadKey", "[In:Default:SomeFieldWhichDoesntExist]");
            pt.Configuration.Add("TestMyConfFirstName", "[In:MyConf:FirstName]");
            pt.Attach(ds);
            pt.Attach("MyConf", ds);
            pt.ConfigurationProvider = ds.ConfigurationProvider;
            var x = pt["Default"].List;

            Assert.AreEqual("First Name 1001", pt.Configuration["InTestFirstName"], "Tested in:Default:EntityTitle");
            Assert.AreEqual("", pt.Configuration["InTestBadStream"], "Testing in-token with invalid stream");
            Assert.AreEqual("", pt.Configuration["InTestNoKey"], "Testing in-token with missing field");
            Assert.AreEqual("First Name 27", pt.Configuration["TestMyConfFirstName"], "MyConf stream First Name");
            Assert.AreEqual("", pt.Configuration["InTestBadKey"], "Testing in-token with incorrect field name");
            // var ent = new Entity_Test().TestEntityDaniel();
        }
    }
}
