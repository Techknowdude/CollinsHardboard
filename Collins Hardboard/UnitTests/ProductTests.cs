using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModelLib;
using StaticHelpers;

namespace UnitTests
{
    [TestClass]
    public class ProductTests
    {
        private TestContext testContextInstance;

        public TestContext TestContext
        {
            get { return testContextInstance; }
            set { testContextInstance = value; }
        }


        [TestMethod]
        [DeploymentItem("TestingData.xls")]
        [DataSource("System.Data.OleDb","Provider=Microsoft.jet.OLEDB.4.0;Data Source='TestingData.xls';Persist Security Info=False;Extended Properties='Excel 8.0'", "WorkItemData$",      // The table name, in this case, the sheet name with a '$' appended.				
         DataAccessMethod.Sequential)]
        public void MembersMatchPassedValues()
        {
            String prodCode = (String)TestContext.DataRow["ProductCode"];
            String description = (String)TestContext.DataRow["Description"];
            double width = (double)TestContext.DataRow["Width"];
            double length = (double)TestContext.DataRow["Length"];
            String thicknessString = TestContext.DataRow["Thickness"].ToString();
            double thickness = (double)TestContext.DataRow["Thickness"];
            String texture =  (String)TestContext.DataRow["Texture"];
            Int32 quantity =  (int)(double)TestContext.DataRow["Quantity"];
            String grade = (String)TestContext.DataRow["Grade"];
            double waste = (double)TestContext.DataRow["Waste"];

            Product testItem = new Product(prodCode, description, width,length, thicknessString, texture, quantity, grade, waste );

            Assert.AreEqual(testItem.BoardGrade,grade,"Grade not equal to passed var.");
            Assert.AreEqual(testItem.Description,description,"Item description does not match: " + description);
            Assert.AreEqual(testItem.Width, width, "Width not equal to " + width);
            Assert.AreEqual(testItem.Length, length, "Length not equal to " + length);
            Assert.AreEqual(testItem.Thickness, thickness, "Thickness string not equal to " + thicknessString);
            Assert.AreEqual(testItem.Waste, waste, "Waste not equal to " + thicknessString);

            Product testItem2 = new Product(prodCode, description, width, length, thickness, texture, quantity, grade, waste);

            Assert.AreEqual(testItem2.Thickness, thickness, "Thickness double not equal to " + thicknessString);
        }
    }
}
