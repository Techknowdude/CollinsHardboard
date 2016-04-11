using System;
using System.Collections.Generic;
using System.IO;
using CoatingScheduler;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StaticHelpers;

namespace UnitTests
{
    [TestClass]
    public class CoatingMomentoTests
    {

        private TestContext _testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return _testContextInstance;
            }
            set
            {
                _testContextInstance = value;
            }
        }

        [TestMethod]
        [DeploymentItem("TestingData.xls")]
        [DataSource("System.Data.OleDb",
            "Provider=Microsoft.Jet.OLEDB.4.0;Data Source='TestingData.xls';Extended Properties='Excel 8.0'",
            "ProductMomento$", // The table name, in this case, the sheet name with a '$' appended.				
            DataAccessMethod.Sequential)]
        public void TestProductSaveAndLoad()
        {
            string thickness = StaticFunctions.ConvertDoubleToStringThickness((double)TestContext.DataRow["Thickness"]);
            string productCode = (string)TestContext.DataRow["Product Code"];
            string grades = (string)TestContext.DataRow["Grades"];
            string units = StaticFunctions.ConvertDoubleToStringThickness((double)TestContext.DataRow["Units"]);
            string notes = (string)TestContext.DataRow["Notes"];
            string placement = (string)TestContext.DataRow["Placement"];
            string desc = (string)TestContext.DataRow["Description"];
            bool hasBarcode = (bool)TestContext.DataRow["Has Barcode"];

            String incorrectValue = "Wrong";

            // create test product
            CoatingScheduleProduct product = new CoatingScheduleProduct(thickness, desc, productCode, grades, units, hasBarcode, notes, placement);

            // ensure the check values are the actual value of the product
            thickness = product.Thickness;
            productCode = product.ProductCode;
            grades = product.Grades;
            units = product.Units;
            notes = product.Notes;
            placement = product.Placement;
            desc = product.Description;
            hasBarcode = product.HasBarcode;

            // create caretaker for product
            ProductCaretaker pCaretaker = new ProductCaretaker();

            // store data
            pCaretaker.Momento = product.CreateMomento() as ProductMomento;

            // change values to new values
            product.Thickness = incorrectValue;
            product.Description = incorrectValue;
            product.ProductCode = incorrectValue;
            product.Grades = incorrectValue;
            product.Units = incorrectValue;
            product.Notes = incorrectValue;
            product.Placement = incorrectValue;
            product.Description = incorrectValue;
            product.HasBarcode = !hasBarcode;

            // ensure new values for all properties
            Assert.AreNotEqual(thickness, product.Thickness);
            Assert.AreNotEqual(productCode, product.ProductCode);
            Assert.AreNotEqual(grades, product.Grades);
            Assert.AreNotEqual(units, product.Units);
            Assert.AreNotEqual(notes, product.Notes);
            Assert.AreNotEqual(placement, product.Placement);
            Assert.AreNotEqual(desc, product.Description);
            Assert.AreNotEqual(hasBarcode, product.HasBarcode);

            // load old data
            pCaretaker.LoadLogic(product);

            // check if old data matches new data
            Assert.AreEqual(thickness, product.Thickness);
            Assert.AreEqual(productCode, product.ProductCode);
            Assert.AreEqual(grades, product.Grades);
            Assert.AreEqual(units, product.Units);
            Assert.AreEqual(notes, product.Notes);
            Assert.AreEqual(placement, product.Placement);
            Assert.AreEqual(desc, product.Description);
            Assert.AreEqual(hasBarcode, product.HasBarcode);
        }

        [TestMethod]
        [DeploymentItem("TestingData.xls")]
        [DataSource("System.Data.OleDb",
            "Provider=Microsoft.Jet.OLEDB.4.0;Data Source='TestingData.xls';Extended Properties='Excel 8.0'",
            "ProductMomento$", // The table name, in this case, the sheet name with a '$' appended.				
            DataAccessMethod.Sequential)]
        public void TestShiftSaveAndLoad()
        {
            string thickness = StaticFunctions.ConvertDoubleToStringThickness((double)TestContext.DataRow["Thickness"]);
            string productCode = (string)TestContext.DataRow["Product Code"];
            string grades = (string)TestContext.DataRow["Grades"];
            string units = StaticFunctions.ConvertDoubleToStringThickness((double)TestContext.DataRow["Units"]);
            string notes = (string)TestContext.DataRow["Notes"];
            string placement = (string)TestContext.DataRow["Placement"];
            string desc = (string)TestContext.DataRow["Description"];
            bool hasBarcode = (bool)TestContext.DataRow["Has Barcode"];

            String incorrectValue = "Wrong";

            // create test product
            CoatingScheduleProduct product = new CoatingScheduleProduct(thickness, desc, productCode, grades, units, hasBarcode, notes, placement);
            CoatingScheduleProduct product2 = new CoatingScheduleProduct(thickness, desc, productCode, grades, units, hasBarcode, notes, placement);
            CoatingScheduleProduct product3 = new CoatingScheduleProduct(thickness, desc, productCode, grades, units, hasBarcode, notes, placement);
            CoatingScheduleShift shift = new CoatingScheduleShift();

            shift.ChildrenLogic.Add(product);
            shift.ChildrenLogic.Add(product2);
            shift.ChildrenLogic.Add(product3);

            // ensure the check values are the actual value of the product
            GetProductValues(product, out thickness, out desc, out productCode, out grades, out units, out hasBarcode, out notes, out placement);
            GetProductValues(product2, out thickness, out desc, out productCode, out grades, out units, out hasBarcode, out notes, out placement);
            GetProductValues(product3, out thickness, out desc, out productCode, out grades, out units, out hasBarcode, out notes, out placement);

            // create caretaker for product
            //ProductCaretaker pCaretaker = new ProductCaretaker();
            //ProductCaretaker pCaretaker2 = new ProductCaretaker();
            //ProductCaretaker pCaretaker3 = new ProductCaretaker();

            ShiftCaretaker sCaretaker = new ShiftCaretaker();

            // store data
            //pCaretaker.Momento = product.CreateMomento() as ProductMomento;
            //pCaretaker2.Momento = product2.CreateMomento() as ProductMomento;
            //pCaretaker3.Momento = product3.CreateMomento() as ProductMomento;
            sCaretaker.Momento = shift.CreateMomento() as ShiftMomento;

            // change values to new values
            SetProductValues(product, incorrectValue, incorrectValue, incorrectValue, incorrectValue, incorrectValue,
                !hasBarcode, incorrectValue, incorrectValue);
            SetProductValues(product2, incorrectValue, incorrectValue, incorrectValue, incorrectValue, incorrectValue,
                !hasBarcode, incorrectValue, incorrectValue);
            SetProductValues(product3, incorrectValue, incorrectValue, incorrectValue, incorrectValue, incorrectValue,
                !hasBarcode, incorrectValue, incorrectValue);
            shift.ChildrenLogic.Clear();

            // ensure new values for all properties
            CheckProductValues(product, incorrectValue, incorrectValue, incorrectValue, incorrectValue, incorrectValue,
                !hasBarcode, incorrectValue, incorrectValue);

            CheckProductValues(product2, incorrectValue, incorrectValue, incorrectValue, incorrectValue, incorrectValue,
                !hasBarcode, incorrectValue, incorrectValue);

            CheckProductValues(product3, incorrectValue, incorrectValue, incorrectValue, incorrectValue, incorrectValue,
                !hasBarcode, incorrectValue, incorrectValue);
            Assert.AreEqual(shift.ChildrenLogic.Count, 0);

            // load old data
            sCaretaker.LoadLogic(shift);

            //product.LoadMomento(pCaretaker.Momento);
            //product2.LoadMomento(pCaretaker2.Momento);
            //product3.LoadMomento(pCaretaker3.Momento);


            // check if old data matches new data
            foreach (CoatingScheduleProduct logic in shift.ChildrenLogic)
            {
                CheckProductValues(logic, thickness, desc, productCode, grades, units, hasBarcode, notes, placement);
            }
        }
        [TestMethod]
        [DeploymentItem("TestingData.xls")]
        [DataSource("System.Data.OleDb",
            "Provider=Microsoft.Jet.OLEDB.4.0;Data Source='TestingData.xls';Extended Properties='Excel 8.0'",
            "ProductMomento$", // The table name, in this case, the sheet name with a '$' appended.				
            DataAccessMethod.Sequential)]
        public void TestShiftSaveAndLoadFromBinFile()
        {
            string thickness = StaticFunctions.ConvertDoubleToStringThickness((double)TestContext.DataRow["Thickness"]);
            string productCode = (string)TestContext.DataRow["Product Code"];
            string grades = (string)TestContext.DataRow["Grades"];
            string units = StaticFunctions.ConvertDoubleToStringThickness((double)TestContext.DataRow["Units"]);
            string notes = (string)TestContext.DataRow["Notes"];
            string placement = (string)TestContext.DataRow["Placement"];
            string desc = (string)TestContext.DataRow["Description"];
            bool hasBarcode = (bool)TestContext.DataRow["Has Barcode"];

            String incorrectValue = "Wrong";

            // create test product
            CoatingScheduleProduct product = new CoatingScheduleProduct(thickness, desc, productCode, grades, units, hasBarcode, notes, placement);
            CoatingScheduleProduct product2 = new CoatingScheduleProduct(thickness, desc, productCode, grades, units, hasBarcode, notes, placement);
            CoatingScheduleProduct product3 = new CoatingScheduleProduct(thickness, desc, productCode, grades, units, hasBarcode, notes, placement);
            CoatingScheduleShift shift = new CoatingScheduleShift();

            shift.ChildrenLogic.Add(product);
            shift.ChildrenLogic.Add(product2);
            shift.ChildrenLogic.Add(product3);

            // ensure the check values are the actual value of the product
            GetProductValues(product, out thickness, out desc, out productCode, out grades, out units, out hasBarcode, out notes, out placement);
            GetProductValues(product2, out thickness, out desc, out productCode, out grades, out units, out hasBarcode, out notes, out placement);
            GetProductValues(product3, out thickness, out desc, out productCode, out grades, out units, out hasBarcode, out notes, out placement);


            ShiftCaretaker sCaretaker = new ShiftCaretaker();

            sCaretaker.Momento = shift.CreateMomento() as ShiftMomento;

            using (BinaryWriter fout = new BinaryWriter(File.Open("test.sch", FileMode.Create)))
            {
                sCaretaker.SaveToBin(fout);
            }
            // change values to new values
            SetProductValues(product, incorrectValue, incorrectValue, incorrectValue, incorrectValue, incorrectValue,
                !hasBarcode, incorrectValue, incorrectValue);
            SetProductValues(product2, incorrectValue, incorrectValue, incorrectValue, incorrectValue, incorrectValue,
                !hasBarcode, incorrectValue, incorrectValue);
            SetProductValues(product3, incorrectValue, incorrectValue, incorrectValue, incorrectValue, incorrectValue,
                !hasBarcode, incorrectValue, incorrectValue);
            shift.ChildrenLogic.Clear();

            // ensure new values for all properties
            CheckProductValues(product, incorrectValue, incorrectValue, incorrectValue, incorrectValue, incorrectValue,
                !hasBarcode, incorrectValue, incorrectValue);

            CheckProductValues(product2, incorrectValue, incorrectValue, incorrectValue, incorrectValue, incorrectValue,
                !hasBarcode, incorrectValue, incorrectValue);

            CheckProductValues(product3, incorrectValue, incorrectValue, incorrectValue, incorrectValue, incorrectValue,
                !hasBarcode, incorrectValue, incorrectValue);
            Assert.AreEqual(shift.ChildrenLogic.Count, 0);

            sCaretaker.Momento = null;

            // load old data

            using (BinaryReader fin = new BinaryReader(File.Open("test.sch", FileMode.OpenOrCreate)))
            {
                sCaretaker.LoadFromBin(fin);
            }

            sCaretaker.LoadLogic(shift);

            //product.LoadMomento(pCaretaker.Momento);
            //product2.LoadMomento(pCaretaker2.Momento);
            //product3.LoadMomento(pCaretaker3.Momento);


            // check if old data matches new data
            foreach (CoatingScheduleProduct logic in shift.ChildrenLogic)
            {
                CheckProductValues(logic, thickness, desc, productCode, grades, units, hasBarcode, notes, placement);
            }
        }


        void GetProductValues(CoatingScheduleProduct product, out String thickness, out String  desc, out String productCode, out String grades, 
            out String units, out bool hasBarcode,out String notes, out String placement )
        {
            thickness = product.Thickness;
            productCode = product.ProductCode;
            grades = product.Grades;
            units = product.Units;
            notes = product.Notes;
            placement = product.Placement;
            desc = product.Description;
            hasBarcode = product.HasBarcode;
        }

        private void SetProductValues(CoatingScheduleProduct product, String thickness, String desc,
            String productCode, String grades, String units, bool hasBarcode, String notes, String placement)
        {
            product.Thickness = thickness;
            product.Description = desc;
            product.ProductCode = productCode;
            product.Grades = grades;
            product.Units = units;
            product.HasBarcode = hasBarcode;
            product.Notes = notes;
            product.Placement = placement;
        }

        void CheckProductValues(CoatingScheduleProduct product, String thickness, String desc,
            String productCode, String grades, String units, bool hasBarcode, String notes, String placement)
        {
            Assert.AreEqual(thickness, product.Thickness);
            Assert.AreEqual(productCode, product.ProductCode);
            Assert.AreEqual(grades, product.Grades);
            Assert.AreEqual(units, product.Units);
            Assert.AreEqual(notes, product.Notes);
            Assert.AreEqual(placement, product.Placement);
            Assert.AreEqual(desc, product.Description);
            Assert.AreEqual(hasBarcode, product.HasBarcode);
        }


        [TestMethod]
        [DeploymentItem("TestingData.xls")]
        [DataSource("System.Data.OleDb",
            "Provider=Microsoft.Jet.OLEDB.4.0;Data Source='TestingData.xls';Extended Properties='Excel 8.0'",
            "ProductMomento$", // The table name, in this case, the sheet name with a '$' appended.				
            DataAccessMethod.Sequential)]
        public void TestProductBinarySaveAndLoad()
        {

            string thickness = StaticFunctions.ConvertDoubleToStringThickness((double)TestContext.DataRow["Thickness"]);
            string productCode = (string)TestContext.DataRow["Product Code"];
            string grades = (string)TestContext.DataRow["Grades"];
            string units = StaticFunctions.ConvertDoubleToStringThickness((double)TestContext.DataRow["Units"]);
            string notes = (string)TestContext.DataRow["Notes"];
            string placement = (string)TestContext.DataRow["Placement"];
            string desc = (string)TestContext.DataRow["Description"];
            bool hasBarcode = (bool)TestContext.DataRow["Has Barcode"];

            String incorrectValue = "Wrong";

            // create test product
            CoatingScheduleProduct product = new CoatingScheduleProduct(thickness, desc, productCode, grades, units, hasBarcode, notes, placement);

            // ensure the check values are the actual value of the product
            thickness = product.Thickness;
            productCode = product.ProductCode;
            grades = product.Grades;
            units = product.Units;
            notes = product.Notes;
            placement = product.Placement;
            desc = product.Description;
            hasBarcode = product.HasBarcode;

            // create caretaker for product
            ProductCaretaker pCaretaker = new ProductCaretaker();

            // store data
            pCaretaker.Momento = product.CreateMomento() as ProductMomento;
            using (BinaryWriter fout = new BinaryWriter(File.Open("test.sch", FileMode.Create)))
            {
                pCaretaker.SaveToBin(fout);
            }

            // change values to new values
            product.Thickness = incorrectValue;
            product.Description = incorrectValue;
            product.ProductCode = incorrectValue;
            product.Grades = incorrectValue;
            product.Units = incorrectValue;
            product.Notes = incorrectValue;
            product.Placement = incorrectValue;
            product.Description = incorrectValue;
            product.HasBarcode = !hasBarcode;

            // Delete momento
            pCaretaker.Momento = null;

            // load old data
            using (BinaryReader fin = new BinaryReader(File.Open("test.sch", FileMode.Open)))
            {
                pCaretaker.LoadFromBin(fin);
            }

            pCaretaker.LoadLogic(product);

            // check if old data matches new data
            Assert.AreEqual(thickness, product.Thickness);
            Assert.AreEqual(productCode, product.ProductCode);
            Assert.AreEqual(grades, product.Grades);
            Assert.AreEqual(units, product.Units);
            Assert.AreEqual(notes, product.Notes);
            Assert.AreEqual(placement, product.Placement);
            Assert.AreEqual(desc, product.Description);
            Assert.AreEqual(hasBarcode, product.HasBarcode);
        }

        [TestMethod]
        [DeploymentItem("TestingData.xls")]
        [DataSource("System.Data.OleDb",
            "Provider=Microsoft.Jet.OLEDB.4.0;Data Source='TestingData.xls';Extended Properties='Excel 8.0'",
            "ProductMomento$", // The table name, in this case, the sheet name with a '$' appended.				
            DataAccessMethod.Sequential)]
        public void TestLineSaveAndLoad()
        {
            string thickness = StaticFunctions.ConvertDoubleToStringThickness((double)TestContext.DataRow["Thickness"]);
            string productCode = (string)TestContext.DataRow["Product Code"];
            string grades = (string)TestContext.DataRow["Grades"];
            string units = StaticFunctions.ConvertDoubleToStringThickness((double)TestContext.DataRow["Units"]);
            string notes = (string)TestContext.DataRow["Notes"];
            string placement = (string)TestContext.DataRow["Placement"];
            string desc = (string)TestContext.DataRow["Description"];
            bool hasBarcode = (bool)TestContext.DataRow["Has Barcode"];
            string coatingLine = "Lap";

            String incorrectValue = "Wrong";

            // create products
            List<CoatingScheduleProduct> products = new List<CoatingScheduleProduct>
            {
                new CoatingScheduleProduct(thickness, desc + "1", productCode, grades, units, hasBarcode, notes, placement),
                new CoatingScheduleProduct(thickness , desc+ "2", productCode, grades, units, hasBarcode, notes, placement),
                new CoatingScheduleProduct(thickness, desc + "3", productCode, grades, units, hasBarcode, notes, placement)
            };

            // ensure the check values are the actual value of the product
            
            thickness = products[0].Thickness;
            productCode = products[0].ProductCode;
            grades = products[0].Grades;
            units = products[0].Units;
            notes = products[0].Notes;
            placement = products[0].Placement;
            hasBarcode = products[0].HasBarcode;

            CoatingScheduleShift shift = new CoatingScheduleShift();
            shift.CoatingLine = coatingLine;

            foreach (CoatingScheduleProduct product in products)
            {
                product.CoatingLine = coatingLine;
                shift.AddLogic(product);                
            }

            CoatingScheduleLine line = new CoatingScheduleLine();
            line.CoatingLine = coatingLine;

            // add shift to line
            line.CoatingLine = coatingLine;
            line.ChildrenLogic.Add(shift);
            shift.Connect(line);

            // create caretaker for product
            LineCaretaker lCaretaker = new LineCaretaker();

            // store data
            lCaretaker.Momento = line.CreateMomento() as LineMomento;

            // change values to new values
            foreach (CoatingScheduleProduct product in shift.ChildrenLogic)
            {
                product.Thickness = incorrectValue;
                product.Description = incorrectValue;
                product.ProductCode = incorrectValue;
                product.Grades = incorrectValue;
                product.Units = incorrectValue;
                product.Notes = incorrectValue;
                product.Placement = incorrectValue;
                product.Description = incorrectValue;
                product.HasBarcode = !hasBarcode;
                product.CoatingLine = incorrectValue;
            }
            shift.CoatingLine = incorrectValue;
            line.CoatingLine = incorrectValue;

            // ensure new values for all properties
            foreach (CoatingScheduleProduct product in shift.ChildrenLogic)
            {
                Assert.AreNotEqual(thickness, product.Thickness);
                Assert.AreNotEqual(productCode, product.ProductCode);
                Assert.AreNotEqual(grades, product.Grades);
                Assert.AreNotEqual(units, product.Units);
                Assert.AreNotEqual(notes, product.Notes);
                Assert.AreNotEqual(placement, product.Placement);
                Assert.AreNotEqual(desc, product.Description);
                Assert.AreNotEqual(hasBarcode, product.HasBarcode);
                Assert.AreNotEqual(coatingLine, product.CoatingLine);
            }
            Assert.AreNotEqual(shift.CoatingLine, coatingLine);
            Assert.AreNotEqual(line.CoatingLine, coatingLine);

            // load old data
            lCaretaker.LoadLogic(line);

            // check if old data matches new data
            for (Int32 index = 0; index < line.ChildrenLogic[0].ChildrenLogic.Count; index++)
            {
                CoatingScheduleProduct product = (CoatingScheduleProduct) line.ChildrenLogic[0].ChildrenLogic[index];
                Assert.AreEqual(thickness, product.Thickness);
                Assert.AreEqual(productCode, product.ProductCode);
                Assert.AreEqual(grades, product.Grades);
                Assert.AreEqual(units, product.Units);
                Assert.AreEqual(notes, product.Notes);
                Assert.AreEqual(placement, product.Placement);
                Assert.AreEqual(desc + (index + 1), product.Description);
                Assert.AreEqual(hasBarcode, product.HasBarcode);
                Assert.AreEqual(coatingLine, product.CoatingLine);
            }
        }
    }
}
