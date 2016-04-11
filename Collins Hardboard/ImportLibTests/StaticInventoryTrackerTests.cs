using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using ModelLib;

namespace ImportLib.Tests
{
    [TestClass()]
    public class StaticInventoryTrackerTests
    {
        [TestMethod()]
        public void AddPastSaleTestDontAddCurrentMonth()
        {
            ProductMasterItem master = new ProductMasterItem(1, "TestMaster01", "Testing master item", 48, 92, .5, "OM", 40, 50, "", true, "", "", 0, 0, 0, 0);
            StaticInventoryTracker.ProductMasterList.Add(master);

            StaticInventoryTracker.AddPastSale(1, DateTime.Today, 100);

            // item should not be added
            Assert.AreEqual(StaticInventoryTracker.ForecastItems.Count, 0);
        }
        [TestMethod()]
        public void AddPastSaleTestAddLastMonth()
        {
            ProductMasterItem master = new ProductMasterItem(1, "TestMaster01", "Testing master item", 48, 92, .5, "OM", 40, 50, "", true, "", "", 0, 0, 0, 0);
            StaticInventoryTracker.ProductMasterList.Add(master);

            StaticInventoryTracker.AddPastSale(1, DateTime.Today.AddDays(-DateTime.Today.Day), 100);

            // item should be added
            Assert.AreEqual(StaticInventoryTracker.ForecastItems.Count, 1);
        }
        [TestMethod()]
        public void AddPastSaleTestAdd12MonthsAgo()
        {
            ProductMasterItem master = new ProductMasterItem(1, "TestMaster01", "Testing master item", 48, 92, .5, "OM", 40, 50, "", true, "", "", 0, 0, 0, 0);
            StaticInventoryTracker.ProductMasterList.Add(master);

            StaticInventoryTracker.AddPastSale(1, DateTime.Today.AddDays(-DateTime.Today.Day).AddMonths(-12), 100);

            // item should be added
            Assert.AreEqual(StaticInventoryTracker.ForecastItems.Count, 1);
        }
    }
}