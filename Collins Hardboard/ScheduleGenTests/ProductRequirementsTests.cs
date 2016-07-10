using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScheduleGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Configuration_windows;
using ImportLib;

namespace ScheduleGen.Tests
{
    [TestClass()]
    public class ProductRequirementsTests
    {
        [TestMethod()]
        public void TestSaleCalculations()
        {
            InternalImport.GetInstance().ImportAll();
            ConfigurationsHandler.GetInstance().Load();

            foreach (var masterItem in StaticInventoryTracker.ProductMasterList)
            {
                var config =
                    ConfigurationsHandler.GetInstance()
                        .Configurations.FirstOrDefault(x => x.ItemOutID == masterItem.MasterID);
                if (config == null) // no config found.
                    config = Configuration.CreateConfiguration("Blank", -1, 1, masterItem.MasterID, 1);
                ProductRequirements.CreateProductRequirements(masterItem.MasterID, config);
            }

            foreach (var salesItem in StaticInventoryTracker.SalesItems)
            {
                ProductRequirements.GetRequirements(salesItem.MasterID)?.AddSale(salesItem.Date,(int) salesItem.Pieces);
            }

            ProductRequirements.AddCurrentInventory();

            ProductRequirements.OutputStringToFile();

            Assert.Inconclusive();
        }

        [TestMethod()]
        public void AddOnHandCheckFutureOnHand()
        {
            var config = Configuration.CreateConfiguration("Blank Config");
            var req = ProductRequirements.CreateProductRequirements(0, config);
            int onHand = 1337;

            // make sure there is another day ahead of it.
            var nextDay = req.GetRequirementDay(DateTime.Today.AddDays(10));
            req.AddOnHandInventory(onHand);

            Assert.AreEqual(nextDay.OnHandPieces, onHand);
        }

        [TestMethod()]
        public void AddOnHandCheckNextDayOnHand()
        {
            var config = Configuration.CreateConfiguration("Blank Config");
            var req = ProductRequirements.CreateProductRequirements(0, config);
            int onHand = 1337;

            // make sure there is another day ahead of it.
            var nextDay = req.GetRequirementDay(DateTime.Today.AddDays(1));
            req.AddOnHandInventory(onHand);

            Assert.AreEqual(nextDay.OnHandPieces, onHand);
        }


        [TestMethod()]
        public void AddSaleAndCheckSubitemGross()
        {
            Configuration config = Configuration.CreateConfiguration("Main", 1, 1, 0, 1);
            Configuration subconfig = Configuration.CreateConfiguration("Sub", 2, 1, 1, 1);
            var requirement = ProductRequirements.CreateProductRequirements(0, config);
            var subitemReq = ProductRequirements.CreateProductRequirements(1, subconfig);
            var day = new DateTime(2000, 1, 1);
            int neededPieces = 100;

            requirement.AddSale(day, neededPieces);

            var checkReq = requirement.GetRequirementDay(day.AddDays(-ProductRequirements.LeadTimeDays));
            var checkSub = subitemReq.GetRequirementDay(day.AddDays(-ProductRequirements.LeadTimeDays));

            Assert.AreEqual(neededPieces, checkReq.PurchaseOrderPieces);
            Assert.AreEqual(neededPieces, checkSub.GrossPieces);
        }

        [TestMethod()]
        public void AddSaleAndCheckSubitemWithOnHand()
        {
            Configuration config = Configuration.CreateConfiguration("Main", 1, 1, 0, 1);
            Configuration subconfig = Configuration.CreateConfiguration("Sub", 2, 1, 1, 1);
            var requirement = ProductRequirements.CreateProductRequirements(0, config);
            var subitemReq = ProductRequirements.CreateProductRequirements(1, subconfig);
            var day = new DateTime(2000, 1, 1);
            int neededPieces = 100;
            int onHand = 35;

            requirement.AddSale(day, neededPieces);

            var checkReq = requirement.GetRequirementDay(day.AddDays(-ProductRequirements.LeadTimeDays));
            var checkSub = subitemReq.GetRequirementDay(day.AddDays(-ProductRequirements.LeadTimeDays));
            checkSub.AddOnHand(onHand);

            Assert.AreEqual(neededPieces, checkReq.PurchaseOrderPieces);
            Assert.AreEqual(neededPieces, checkSub.GrossPieces);
            Assert.AreEqual(neededPieces - onHand, checkSub.NetRequiredPieces);
        }

        [TestMethod()]
        public void AddSaleTest()
        {
            Configuration config = Configuration.CreateConfiguration("",1,1,0,1,1);
            var requirement = ProductRequirements.CreateProductRequirements(0,config);
            var day = new DateTime(2000, 1, 1);
            int pieces = 100;

            requirement.AddSale(day, pieces);

            var checkReq = requirement.GetRequirementDay(day);

            Assert.AreEqual(pieces, checkReq.GrossPieces);
            Assert.AreEqual(pieces, checkReq.NetRequiredPieces);
            Assert.AreEqual(0, checkReq.OnHandPieces);
        }

        [TestMethod()]
        public void AddSaleOnHandTest()
        {
            Configuration config = Configuration.CreateConfiguration("",1,1,0,1,1);
            var requirement = ProductRequirements.CreateProductRequirements(0,config);
            var day = new DateTime(2000, 1, 1);
            int pieces = 100;
            int onHand = 50;

            requirement.AddOnHandInventory(onHand);
            requirement.AddSale(day, pieces);

            var checkReq = requirement.GetRequirementDay(day);

            Assert.AreEqual(pieces, checkReq.GrossPieces);
            Assert.AreEqual(pieces - onHand, checkReq.NetRequiredPieces);
            Assert.AreEqual(onHand, checkReq.OnHandPieces);
        }

        [TestMethod()]
        public void AddSaleTestBadTime()
        {
            Configuration config = Configuration.CreateConfiguration("",1,1,0,1,1);
            var requirement = ProductRequirements.CreateProductRequirements(0,config);
            var day = new DateTime(2000, 1, 1);
            int pieces = 100;

            requirement.AddSale(day, pieces);

            var checkReq = requirement.GetRequirementDay(day.AddSeconds(1));

            Assert.AreEqual(pieces, checkReq.GrossPieces);
            Assert.AreEqual(pieces, checkReq.NetRequiredPieces);
            Assert.AreEqual(0, checkReq.OnHandPieces);

            checkReq = requirement.GetRequirementDay(day.AddHours(23).AddMinutes(59).AddSeconds(59));

            Assert.AreEqual(pieces, checkReq.GrossPieces);
            Assert.AreEqual(pieces, checkReq.NetRequiredPieces);
            Assert.AreEqual(0, checkReq.OnHandPieces);
        }

        [TestMethod()]
        public void CalcPurchasOrdersTest()
        {

        }
    }
}