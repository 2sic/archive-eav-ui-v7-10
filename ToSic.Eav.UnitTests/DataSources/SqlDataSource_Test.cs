using System;
using System.Collections;
using System.Data;
using System.Runtime.Remoting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Data;
using ToSic.Eav.DataSources;

namespace ToSic.Eav.UnitTests
{
    [TestClass]
    public class SqlDataSource_Test
    {
        private const string Connection = "";
        private const string ContentTypeName = "SqlData";
        

        [TestMethod]
        public void SqlDataSource_NoConfigChangesIfNotNecessary()
        {
            var initQuery = "Select * From Products";
            var sql = GenerateSqlDataSource(Connection, initQuery, ContentTypeName);
            var ConfigCountBefore = sql.Configuration.Count;
            sql.EnsureConfigurationIsLoaded();

            Assert.AreEqual(ConfigCountBefore, sql.Configuration.Count);
            Assert.AreEqual(initQuery, sql.SelectCommand);
        }

        [TestMethod]
        public void SqlDataSource_SqlInjectionProtection()
        {
            var initQuery = "Select * From Products Where ProductId = [QueryString:Id]";
            var expectedQuery = "Select * From Products Where ProductId = @" + SqlDataSource.ExtractedParamPrefix + "1";
            var sql = GenerateSqlDataSource(Connection, initQuery, ContentTypeName);
            var ConfigCountBefore = sql.Configuration.Count;
            sql.EnsureConfigurationIsLoaded();

            Assert.AreEqual(ConfigCountBefore + 1, sql.Configuration.Count);
            Assert.AreEqual(expectedQuery, sql.SelectCommand);
            Assert.AreEqual("", sql.Configuration["@" + SqlDataSource.ExtractedParamPrefix + "1"]);
        }

        [TestMethod]
        public void SqlDataSource_SqlInjectionProtectionComplex()
        {
            var initQuery = @"Select Top [AppSettings:MaxPictures] * 
From Products 
Where CatName = [AppSettings:DefaultCategoryName||NotFound] 
And ProductSort = [AppSettings:DoesntExist||CorrectlyDefaulted]";
            var expectedQuery = @"Select Top @" + SqlDataSource.ExtractedParamPrefix + @"1 * 
From Products 
Where CatName = @" + SqlDataSource.ExtractedParamPrefix + @"2 
And ProductSort = @" + SqlDataSource.ExtractedParamPrefix + @"3";

            var sql = GenerateSqlDataSource(Connection, initQuery, ContentTypeName);
            var ConfigCountBefore = sql.Configuration.Count;
            sql.EnsureConfigurationIsLoaded();

            Assert.AreEqual(ConfigCountBefore + 3, sql.Configuration.Count);
            Assert.AreEqual(expectedQuery, sql.SelectCommand);
            Assert.AreEqual(ValueProvider.ValueCollectionProvider_Test.MaxPictures, sql.Configuration["@" + SqlDataSource.ExtractedParamPrefix + "1"]);
            Assert.AreEqual(ValueProvider.ValueCollectionProvider_Test.DefaultCategory, sql.Configuration["@" + SqlDataSource.ExtractedParamPrefix + "2"]);
            Assert.AreEqual("CorrectlyDefaulted", sql.Configuration["@" + SqlDataSource.ExtractedParamPrefix + "3"]);
        }

        public static SqlDataSource GenerateSqlDataSource(string connection, string query, string typeName)
        {
            var source = new SqlDataSource(connection, query, typeName);
            source.ConfigurationProvider = new ValueProvider.ValueCollectionProvider_Test().ValueCollection();

            return source;
        }
    }
}
