using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data;
using ToSic.Eav.DataSources;

namespace ToSic.Eav.UnitTests.DataSources
{
    [TestClass]
    public class DataTableDataSource_Test
    {
        [TestMethod]
        public void DataSource_Create_GeneralTest()
        {
            const int ItemsToGenerate = 499;
            var ds = DataTableDataSource_Test.GeneratePersonSourceWithDemoData(ItemsToGenerate);
            Assert.IsTrue(ds.In.Count == 0, "In count should be 0");
            Assert.IsTrue(ds.Out.Count == 1, "Out cound should be 1");
            var defaultOut = ds["Default"];
            Assert.IsTrue(defaultOut != null);
            try
            {
                var x = ds["Something"];
                Assert.Fail("Access to another out should fail");
            }
            catch { }
            Assert.IsTrue(defaultOut.List.Count == ItemsToGenerate);
        }

        public static DataTableDataSource GeneratePersonSourceWithDemoData(int itemsToGenerate = 10, int firstId = 1001)
        {
            var dataTable = new DataTable();
            dataTable.Columns.AddRange(new[]
            {
                new DataColumn(DataTableDataSource.EntityIdDefaultColumnName, typeof (int)),
                new DataColumn("FullName"),
                new DataColumn("FirstName"),
                new DataColumn("LastName"),
                new DataColumn("City"),
                new DataColumn("Male", typeof (bool)),
                new DataColumn("Birthdate", typeof (DateTime))
            });
            AddSemirandomPersons(dataTable, itemsToGenerate, firstId);

            var source = new DataTableDataSource(dataTable, "Person", titleField: "FullName");
            source.ConfigurationProvider = new ValueProvider.ValueCollectionProvider_Test().ValueCollection();

            return source;
        }

        private static void AddSemirandomPersons(DataTable dataTable, int itemsToGenerate = 10, int firstId = 1000)
        {
            for (var i = firstId; i < firstId + itemsToGenerate; i++)
            {
                var firstName = "First Name " + i;
                var lastName = "Last Name " + i;
                var fullName = firstName + " " + lastName;
                dataTable.Rows.Add(i, fullName, firstName, lastName, "City " + i, i % 3 == 0, DateTime.Now.AddYears(-27));
            }
        }


    }
}
