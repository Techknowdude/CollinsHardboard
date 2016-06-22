using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace ProductionScheduler.Tests
{
    [TestClass()]
    public class PlateConfigurationTests
    {
        [TestMethod()]
        public void CreateShiftsTest()
        {
            PlateConfiguration configuration = new PlateConfiguration(DateTime.Today, DateTime.Today.AddDays(5));
            //configuration.CreateShifts();
        }
        
    }
}