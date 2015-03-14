using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.ValueProvider;

namespace ToSic.Eav.UnitTests.ValueProvider
{
    [TestClass]
    public class ValueCollectionProvider_Test
    {
        #region Important Values which will be checked when resolved
        private string OriginalSettingDefaultCat = "[AppSettings:DefaultCategoryName]";
        private string ResolvedSettingDefaultCat = "All";
        private string OriginalSettingMaxItems = "[AppSettings:MaxItems||100]";
        private string ResolvedSettingMaxItems = "100";
        #endregion

        [TestMethod]
        public void ValueCollection_GeneralFunctionality()
        {
            var vc = ValueCollection();
            var Settings = this.Settings();
            Assert.IsTrue(vc.Sources.Count == 2, "Should have 2 sources");
            Assert.AreEqual("App Settings", vc.Sources["appsettings"].Get("Title") );
            Assert.AreEqual(OriginalSettingDefaultCat, Settings["DefaultCategory"]);
            Assert.AreEqual(OriginalSettingMaxItems, Settings["MaxItems"]);

            vc.LoadConfiguration(Settings);

            Assert.AreEqual(ResolvedSettingDefaultCat, Settings["DefaultCategory"]);
            Assert.AreEqual(ResolvedSettingMaxItems, Settings["MaxItems"]);
        }

        public IValueCollectionProvider ValueCollection()
        {
            var vc = new ValueCollectionProvider();
            var entVc = new EntityValueProvider(AppSettings(), "AppSettings");
            vc.Sources.Add("AppSettings".ToLower(), entVc);
            vc.Sources.Add("AppResources".ToLower(), new EntityValueProvider(AppResources(), "AppResources"));

            return vc;
        }


        public Dictionary<string, string> Settings()
        {
            return new Dictionary<string, string>()
            {
                {"Title", "Settings"},
                {"DefaultCategory", OriginalSettingDefaultCat},
                {"MaxItems", OriginalSettingMaxItems},
                {"PicsPerRow", "3"}
            };
        }

        public Dictionary<string, string> ResolvedSettings()
        {
            var settings = Settings();
            ValueCollection().LoadConfiguration(settings);
            return settings;
        }

        public IEntity AppSettings()
        {
            var vals = new Dictionary<string, object>()
            {
                {"Title", "App Settings"},
                {"DefaultCategoryName", "All"},
                {"MaxPictures", "21"},
                {"PicsPerRow", "3"}
            };

            var ent = new Data.Entity(305200, "AppSettings", vals, "Title");
            return ent;
        }

        public IEntity AppResources()
        {
            var vals = new Dictionary<string, object>()
            {
                {"Title", "Resources"},
                {"Greeting", "Hello there!"},
                {"Introduction", "Welcome to this"}
            };
            var ent = new Data.Entity(305200, "AppResources", vals, "Title");
            return ent;
        }
    }
}
