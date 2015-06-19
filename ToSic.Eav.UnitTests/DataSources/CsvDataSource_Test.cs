using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ToSic.Eav.DataSources;
using ToSic.Eav.UnitTests.ValueProvider;

namespace ToSic.Eav.UnitTests.DataSources
{
    [TestClass]
    public class CsvDataSource_Test
    {
        private const int TestFileRowCount = 40;

        private const int TestFileColumnCount = 5;

        private const int TestFileIdColumnIndex = 0;

        private const int TestFileTitleColumnIndex = 1;


        [TestMethod]
        public void CsvDataSource_ParseSemicolonDelimitedFile()
        {
            var source = CreateDataSource("Files/CsvDataSource - Test Semicolon Delimited.csv", ";", "Anonymous", TestFileIdColumnIndex, TestFileTitleColumnIndex);
            AssertIsSourceListValid(source);
        }

        [TestMethod]
        public void CsvDataSource_ParseTabDelimitedFile()
        {
            var source = CreateDataSource("Files/CsvDataSource - Test Tab Delimited.csv", "\t", "Anonymous", TestFileIdColumnIndex, TestFileTitleColumnIndex);
            AssertIsSourceListValid(source);
        }

        [TestMethod]
        [Description("Parses a file where texts are enquoted, for example 'Hello 2sic'.")]
        public void CsvDataSource_ParseFileWithQuotedText()
        {
            var source = CreateDataSource("Files/CsvDataSource - Test Quoted Text.csv", ";", "Anonymous", TestFileIdColumnIndex, TestFileTitleColumnIndex);
            AssertIsSourceListValid(source);
        }

        [TestMethod]
        [Description("Parses a file and the index of the ID column is not defined - IDs should be taken from line numbers.")]
        public void CsvDataSource_ParseFileWithUndefinedIdColumnIndex()
        {
            var source = CreateDataSource("Files/CsvDataSource - Test Semicolon Delimited.csv", ";", "Anonymous", null, TestFileTitleColumnIndex);
            AssertIsSourceListValid(source);
        }

        [TestMethod]
        [Description("Parses a file, but the index of the ID column is out of range (Index = Columns count = 40) - Test should fail with exception.")]
        [ExpectedException(typeof(ArgumentException))]
        public void CsvDataSource_ParseFileWithIdColumnIndexOutOfRange()
        {
            try
            {
                var source = CreateDataSource("Files/CsvDataSource - Test Semicolon Delimited.csv", ";", "Anonymous", TestFileRowCount /* 40 */, TestFileTitleColumnIndex);
                var sourceList = source.LightList;
            }
            catch (Exception ex)
            {       
                // The pipeline does wrap my exception expected
                throw ex.InnerException;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CsvDataSource_ParseFileWithTextColumnIndexOutOfRange()
        {
            try
            {
                var source = CreateDataSource("Files/CsvDataSource - Test Semicolon Delimited.csv", ";", "Anonymous", TestFileIdColumnIndex, TestFileRowCount /* 40 */);
                var sourceList = source.LightList;
            }
            catch (Exception ex)
            {       
                // The pipeline does wrap my exception expected
                throw ex.InnerException;
            }
        }

        [TestMethod]
        [Description("Parses a file where one row has not values for all columns - Test should fail with exception.")]
        [ExpectedException(typeof(FormatException))]
        public void CsvDataSource_ParseFileWithInvalidRow()
        {
            try
            {
                var source = CreateDataSource("Files/CsvDataSource - Test Invalid Row.csv", ";", "Anonymous", TestFileIdColumnIndex, TestFileTitleColumnIndex);
                var sourceList = source.LightList;
            }
            catch (Exception ex)
            {
                // The pipeline does wrap my exception expected
                throw ex.InnerException;
            }
        }




        private void AssertIsSourceListValid(CsvDataSource source)
        {
            var sourceList = source.LightList.OrderBy(item => item.EntityId).ToList();

            // List
            Assert.AreEqual(sourceList.Count(), TestFileRowCount, "Entity list has not the expected length.");

            // Entities
            for (var i = 0; i < sourceList.Count(); i++)
            {
                var entity = sourceList.ElementAt(i);

                Assert.AreEqual(TestFileColumnCount, entity.Attributes.Count(), "Entity " + i + ": Attributes do not match the columns in the file.");
                if (!source.IdColumnIndex.HasValue)
                {
                    Assert.AreEqual(i + 1, entity.EntityId, "Entity " + i + ": ID does not match.");
                }
                else
                {
                    Assert.AreEqual(GetAttributeValueAt(entity, source.IdColumnIndex.Value), entity.EntityId.ToString(), "Entity " + i + ": ID does not match.");
                }
                Assert.IsNotNull(GetAttributeValueAt(entity, source.TitleColumnIndex), "Entity " + i + ": Title should not be null.");
            }
        }

        private static object GetAttributeValueAt(IEntity entity, int index)
        {
            return entity.GetBestValue(entity.Attributes.ElementAt(index).Key);
        }

        public static CsvDataSource CreateDataSource(string filePath, string delimiter = ";", string contentType = "Anonymous", int? IdColumnIndex = null, int TitleColumnIndex = 1)
        {
            var source = new CsvDataSource() 
            {
                FilePath = filePath,
                Delimiter = delimiter,
                ContentType = contentType,
                IdColumnIndex = IdColumnIndex,
                TitleColumnIndex = TitleColumnIndex

            };
            source.ConfigurationProvider = new ValueCollectionProvider_Test().ValueCollection();
            return source;
        }
    }
}
