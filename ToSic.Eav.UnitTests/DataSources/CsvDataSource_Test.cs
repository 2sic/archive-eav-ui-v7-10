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
            var sourceList = source.LightList;

            AssertIsSourceListValid(sourceList, TestFileIdColumnIndex, TestFileTitleColumnIndex);
        }


        [TestMethod]
        public void CsvDataSource_ParseTabDelimitedFile()
        {
            var source = CreateDataSource("Files/CsvDataSource - Test Tab Delimited.csv", "\t", "Anonymous", TestFileIdColumnIndex, TestFileTitleColumnIndex);
            var sourceList = source.LightList;

            AssertIsSourceListValid(sourceList, TestFileIdColumnIndex, TestFileTitleColumnIndex);
        }

        [TestMethod]
        public void CsvDataSource_ParseFileWithQuotedText()
        {
            var source = CreateDataSource("Files/CsvDataSource - Test Quoted Text.csv", ";", "Anonymous", TestFileIdColumnIndex, TestFileTitleColumnIndex);
            var sourceList = source.LightList;

            AssertIsSourceListValid(sourceList, TestFileIdColumnIndex, TestFileTitleColumnIndex);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CsvDataSource_ParseFileWithInvalidIdColumnIndex100()
        {
            try
            {
                var source = CreateDataSource("Files/CsvDataSource - Test Semicolon Delimited.csv", ";", "Anonymous", 100, TestFileTitleColumnIndex);
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
        public void CsvDataSource_ParseFileWithInvalidTextColumnIndex100()
        {
            try
            {
                var source = CreateDataSource("Files/CsvDataSource - Test Semicolon Delimited.csv", ";", "Anonymous", TestFileIdColumnIndex, 100);
                var sourceList = source.LightList;
            }
            catch (Exception ex)
            {       
                // The pipeline does wrap my exception expected
                throw ex.InnerException;
            }
        }

        [TestMethod]
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


        private static void AssertIsSourceListValid(IEnumerable<IEntity> sourceList, int idColumnIndex, int titleColumnIndex)
        {
            // List
            Assert.AreEqual(sourceList.Count(), TestFileRowCount, "Entity list has not the expected length.");

            // Entities
            for (var i = 0; i < sourceList.Count(); i++)
            {
                var entity = sourceList.ElementAt(i);
                
                Assert.AreEqual(TestFileColumnCount, entity.Attributes.Count(), "Entity " + i + ": Attributes do not match the columns in the file.");
                Assert.AreEqual(GetAttributeValueAt(entity, idColumnIndex), entity.EntityId.ToString(), "Entity " + i + ": ID does not match.");
                Assert.IsNotNull(GetAttributeValueAt(entity, titleColumnIndex), "Entity " + i + ": Title should not be null.");
            }
        }

        private static object GetAttributeValueAt(IEntity entity, int index)
        {
            return entity.GetBestValue(entity.Attributes.ElementAt(index).Key);
        }

        public static CsvDataSource CreateDataSource(string filePath, string delimiter = ";", string contentType = "Anonymous", int IdColumnIndex = 0, int TitleColumnIndex = 1)
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
