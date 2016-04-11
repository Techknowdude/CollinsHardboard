using System;
using ImportLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class ImportManagerTests
    {
        private TestContext testContextInstance;
        public TestContext TestContext
        {
            get { return testContextInstance; }
            set { testContextInstance = value; }
        }

        [TestMethod]
        [DeploymentItem("TestingData.xls")]
        [DataSource("System.Data.OleDb",
            "Provider=Microsoft.Jet.OLEDB.4.0;Data Source='TestingData.xls';Extended Properties='Excel 8.0'",
            "ImportManagerTestData$", // The table name, in this case, the sheet name with a '$' appended.				
            DataAccessMethod.Sequential)]
        public void CheckValidInventoryImportNumbers()
        {
            InternalImport import = InternalImport.GetInstance();

            import.InventoryFileName = "InventoryImportExample.csv";
            PrivateObject importPO = new PrivateObject(import);
            Int32 completedRows = 0;
            object[] parameters = new object[] {completedRows};
            int[] failedRows = (int[])importPO.Invoke("ImportInventoryFile",parameters);
        }

    }
}
