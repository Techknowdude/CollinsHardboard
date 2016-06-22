using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using ModelLib;

namespace ProductionScheduler.Tests
{
    [TestClass()]
    public class PressShiftTests
    {
        [TestMethod()]
        public void AddTest()
        {
            PressShift shift = new PressShift(DateTime.Today, TimeSpan.FromHours(8));
            ProductMasterItem item = ProductMasterItem.CreateDefault();
            item.UnitsPerHour = 20;
            double unitsToMake = 100;
            double unitsLeftover = unitsToMake;

            shift.Add(item, ref unitsLeftover);

            Assert.AreEqual(0,unitsLeftover);
            unitsLeftover = unitsToMake;
            shift.Add(item, ref unitsLeftover);

            Assert.AreEqual(40,unitsLeftover);
        }
    }
}