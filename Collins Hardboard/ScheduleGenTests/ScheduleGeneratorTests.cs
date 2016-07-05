using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScheduleGen;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Configuration_windows;
using ImportLib;
using ModelLib;
using StaticHelpers;

namespace ScheduleGen.Tests
{
    [TestClass()]
    public class ScheduleGeneratorTests
    {
        [TestMethod()]
        public void GenerateScheduleTest()
        {
            StaticFactoryValuesManager.CoatingLines = new ObservableCollection<string>() { "Lap", "Panel" };
            StaticFactoryValuesManager.CurrentWaste = 4000;
            StaticFactoryValuesManager.WasteMin = 1000;
            StaticFactoryValuesManager.WasteMax = 10000;


            // Mock masters
            ProductMasterItem mItem1 = new ProductMasterItem(1, "CODE1", "Test master 1", 48, 92, .5, "OM", 20, 100, "D,W", true, "", "T", 2, 4, 3, 5) {MadeIn = "Coating"};
            ProductMasterItem mItem2 = new ProductMasterItem(2, "CODE2", "Test master 2", 50, 92, .5, "OM", 20, 100, "D,W", true, "", "T", 2, 4, 3, 5) { MadeIn = "Coating" };
            ProductMasterItem mItem3 = new ProductMasterItem(3, "CODE3", "Test master 3", 49, 92, .5, "OM", 20, 100, "D,W", true, "", "T", 2, 4, 3, 5) { MadeIn = "Coating" };
            ProductMasterItem mItem4 = new ProductMasterItem(4, "CODE4", "Test master 4", 40, 92, .5, "OM", 20, 100, "D,W", true, "", "U", 20, 100, 60, 5) { MadeIn = "Coating" };
            ProductMasterItem mItemR = new ProductMasterItem(5, "Rough", "Test master 5", 40, 92, .5, "OM", 20, 100, "D,W", true, "", "U", 20, 100, 60, 5) { MadeIn = "Press" };

            StaticInventoryTracker.ProductMasterList.Add(mItem1);
            StaticInventoryTracker.ProductMasterList.Add(mItem2);
            StaticInventoryTracker.ProductMasterList.Add(mItem3);
            StaticInventoryTracker.ProductMasterList.Add(mItem4);
            StaticInventoryTracker.ProductMasterList.Add(mItemR);

            // mock inventory
            InventoryItem iItem1 = new InventoryItem(mItem1, 100, "D");
            InventoryItem iItem2 = new InventoryItem(mItem2, 10, "D");
            InventoryItem iItem3 = new InventoryItem(mItem3, 100, "D");

            StaticInventoryTracker.InventoryItems.Add(iItem1);
            StaticInventoryTracker.InventoryItems.Add(iItem2);
            StaticInventoryTracker.InventoryItems.Add(iItem3);

            // mock sales
            SalesItem sItem1 = new SalesItem(mItem1, "1001", 50, 0, "D", DateTime.Today.AddDays(8));
            SalesItem sItem11 = new SalesItem(mItem1, "1011", 150, 0, "D", DateTime.Today.AddDays(6));
            SalesItem sItem2 = new SalesItem(mItem2, "1002", 50, 0, "D", DateTime.Today.AddDays(8));
            SalesItem sItem4 = new SalesItem(mItem4, "1004", 50, 0, "D", DateTime.Today.AddDays(8));

            StaticInventoryTracker.SalesItems.Add(sItem1);
            StaticInventoryTracker.SalesItems.Add(sItem11);
            StaticInventoryTracker.SalesItems.Add(sItem2);
            StaticInventoryTracker.SalesItems.Add(sItem4);

            // mock sales history
            var highSales = new double[] { 180, 170, 150, 140, 100, 40, 50, 70, 80, 90, 130, 150, 190 };
            var medSales = new double[] { 90, 85, 75, 70, 50, 20, 25, 35, 40, 45, 65, 75, 95 };
            var lowSales = new double[] { 20, 15, 12, 14, 10, 5, 7, 12, 14, 10, 13, 15, 22 };
            ForecastItem fItem1 = new ForecastItem(iItem1.Units, mItem1, highSales);
            ForecastItem fItem2 = new ForecastItem(iItem2.Units, mItem2, medSales);
            ForecastItem fItem3 = new ForecastItem(iItem3.Units, mItem3, medSales);
            ForecastItem fItem4 = new ForecastItem(0, mItem4, lowSales);

            StaticInventoryTracker.ForecastItems.Add(fItem1);
            StaticInventoryTracker.ForecastItems.Add(fItem2);
            StaticInventoryTracker.ForecastItems.Add(fItem3);
            StaticInventoryTracker.ForecastItems.Add(fItem4);

            // factory config
            Configuration cI1 = Configuration.CreateConfiguration("Make item 1", mItemR.MasterID, 1, mItem1.MasterID, 2, 60);
            Configuration cI2 = Configuration.CreateConfiguration("Make item 2", mItemR.MasterID, 1, mItem2.MasterID, 2, 40);
            Configuration cI3 = Configuration.CreateConfiguration("Make item 3", mItemR.MasterID, 1, mItem3.MasterID, 2, 50);
            Configuration cI4 = Configuration.CreateConfiguration("Make item 4", mItemR.MasterID, 1, mItem4.MasterID, 2, 80);

            Machine machineLap = Machine.CreateMachine("LapMachine"); // runs lap, cant run w/ panel. Makes 1 and 2
            machineLap.LinesCanRunOn.Add("Lap");
            machineLap.LineConflicts.Add("Panel");
            machineLap.MachineConflicts.Add("PanelMachine");
            machineLap.AddConfiguration(cI1);
            machineLap.AddConfiguration(cI2);

            Machine machinePanel = Machine.CreateMachine("PanelMachine"); // runs panel, cant run w/lap. Makes 3 and 4
            machinePanel.LinesCanRunOn.Add("Panel");
            machineLap.LineConflicts.Add("Lap");
            machinePanel.MachineConflicts.Add("LapMachine");
            machinePanel.AddConfiguration(cI3);
            machinePanel.AddConfiguration(cI4);

            MachineHandler.Instance.AddMachine(machinePanel);
            MachineHandler.Instance.AddMachine(machineLap);


            // shifts
            var days = Shift.ShiftFactory("Days", DateTime.Today.AddHours(14), TimeSpan.FromHours(8), DateTime.Today.AddDays(-100), DateTime.MaxValue, null, new List<DayOfWeek>() { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday });
            var swing = Shift.ShiftFactory("Swing", DateTime.Today.AddHours(22), TimeSpan.FromHours(8), DateTime.Today.AddDays(-100), DateTime.MaxValue, null, new List<DayOfWeek>() { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday });
            var gyd = Shift.ShiftFactory("Graveyard", DateTime.Today.AddHours(6), TimeSpan.FromHours(8), DateTime.Today.AddDays(-100), DateTime.MaxValue, null, new List<DayOfWeek>() { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday });

            ShiftHandler.CoatingInstance.Shifts.Clear();
            ShiftHandler.CoatingInstance.AddShift(days);
            ShiftHandler.CoatingInstance.AddShift(swing);
            ShiftHandler.CoatingInstance.AddShift(gyd);

            // Priority controls
            ScheduleGenWindow genWindow = new ScheduleGenWindow();
            var saleControl = new SalesNumbersControl(genWindow, 1000);
            var predictionControl = new SalesPrediction(genWindow, 500, SalesPrediction.SalesDurationEnum.Last12Months);
            var wasteControl = new WasteControl(genWindow, 100);
            var widControl = new WidthControl(genWindow, 800);
            genWindow.Show();

            ScheduleGenerator.ControlsList.Add(saleControl);
            ScheduleGenerator.ControlsList.Add(predictionControl);
            ScheduleGenerator.ControlsList.Add(wasteControl);
            ScheduleGenerator.ControlsList.Add(widControl);

            ScheduleGenerator.StartGen = DateTime.Today;
            ScheduleGenerator.EndGen = DateTime.Today.AddDays(1);
            ScheduleGenerator.SalesOutlook = DateTime.Today.AddDays(14);

            ScheduleGenerator.GeneratePredictionSchedule(DateTime.Today.AddDays(14),DateTime.Today, DateTime.Today.AddDays(1));

            Console.ReadLine();
        }

        [TestMethod()]
        public void AddSalesFulfillmentTestSingleSale()
        {
            ProductMasterItem master = new ProductMasterItem(1, "Code1", "", 48, 92, .5, "OM", 40, 100, "", true, "", "", 0, 0, 0, 0);
            StaticInventoryTracker.ProductMasterList.Clear();
            StaticInventoryTracker.ProductMasterList.Add(master);

            var sale = new SalesItem(master, "1001", 100, 0, "D", DateTime.Today);
            StaticInventoryTracker.SalesItems.Add(sale);

            ScheduleGenerator.AddSalesFulfillment(master, 60);

            Assert.AreEqual(60, sale.Fulfilled);
        }

        [TestMethod()]
        public void AddSalesFulfillmentTestMultipleSale()
        {
            ProductMasterItem master = new ProductMasterItem(1, "Code1", "", 48, 92, .5, "OM", 40, 100, "", true, "", "", 0, 0, 0, 0);
            StaticInventoryTracker.ProductMasterList.Clear();
            StaticInventoryTracker.ProductMasterList.Add(master);

            var sale = new SalesItem(master, "1001", 100, 0, "D", DateTime.Today);
            var sale2 = new SalesItem(master, "1002", 300, 0, "D", DateTime.Today.AddDays(1)); // should fill last
            StaticInventoryTracker.SalesItems.Add(sale);
            StaticInventoryTracker.SalesItems.Add(sale2);

            ScheduleGenerator.AddSalesFulfillment(master, 260);

            Assert.AreEqual(100, sale.Fulfilled);
            Assert.AreEqual(160, sale2.Fulfilled);
        }
    }
}