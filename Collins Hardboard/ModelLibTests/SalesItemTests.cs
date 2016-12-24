using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModelLib;

namespace ModelLibTests
{
    [TestClass()]
    public class SalesItemTests
    {
        [TestMethod()]
        public void SaveItemTest()
        {
            byte[] data = new byte[200];


            SalesItem item = new SalesItem("Code","0001",100,15,"ECON",DateTime.Today,34,2);
            SalesItem readItem = null;

            using (BinaryWriter writer = new BinaryWriter(new MemoryStream(data)))
            {
                item.SaveItem(writer);
            }

            using (BinaryReader reader = new BinaryReader(new MemoryStream(data)))
            {
                readItem = SalesItem.Load(reader);
            }

            Assert.AreEqual(item.ProductionCode,readItem.ProductionCode);
            Assert.AreEqual(item.InvoiceNumber,readItem.InvoiceNumber);
            Assert.AreEqual(item.Units,readItem.Units);
            Assert.AreEqual(item.TotalPieces,readItem.TotalPieces);
            Assert.AreEqual(item.Grade,readItem.Grade);
            Assert.AreEqual(item.Date,readItem.Date);
            Assert.AreEqual(item.MasterID,readItem.MasterID);
        }
    }
}