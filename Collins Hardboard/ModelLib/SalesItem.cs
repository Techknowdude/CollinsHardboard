using System;
using System.IO;
using System.Windows.Controls;
using StaticHelpers;

namespace ModelLib
{
    public class SalesItem : ObservableObject
    {
        #region DataMembers

        private static Int32 _idCounter = 0;
        public static Int32 IDCounter { get { return _idCounter; } set { _idCounter = value; }}

        private Int32 _invItemID;
        public Int32 InventoryItemID { get { return _invItemID; } set { _invItemID = value; } }

        private String _productionCode = String.Empty;
        public String ProductionCode
        {
            get { return _productionCode; }
            set { _productionCode = value; }
        }

        public String InvoiceNumber { get; set; }

        /// <summary>
        /// Total order expressed as Units.
        /// </summary>
        public double Units { get; set; }

        private double _pcs;
        /// <summary>
        /// Pieces per whole unit
        /// </summary>
        public double Pieces
        {
            get { return _pcs; }
            set { _pcs = value; }
        }

        private String _grade;

        public String Grade
        {
            get { return _grade; }
            set { _grade = value; }
        }

        public DateTime Date { get; set; }
        public int MasterID { get; set; }

        public double Fulfilled { get; set; }
        public double ScheduledToFill { get; set; }

        #endregion

        public SalesItem()
        {
            InventoryItemID = IDCounter++;
        }

        public SalesItem(string code, string invoiceNum, double unit, double pcs, string grade, DateTime date, int itemId = -1, int master = -1)
        {
            InvoiceNumber = invoiceNum;
            InventoryItemID = itemId != -1 ? itemId : IDCounter++;
            MasterID = master;

            ProductionCode = code;
            Units = unit;
            Pieces = pcs;
            Grade = grade;
            Date = date;
        }

        public SalesItem(ProductMasterItem mItem1, string invoiceNum, int unit, int pcs, string grade, DateTime date)
        {
            InvoiceNumber = invoiceNum;
            InventoryItemID = IDCounter++;
            MasterID = mItem1.MasterID;

            ProductionCode = mItem1.ProductionCode;
            Units = unit;
            Pieces = pcs;
            Grade = grade;
            Date = date;
        }

        /// <summary>
        /// Used to check if all data members are the same. Does not check unique ID.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool IsEqual(SalesItem other)
        {
            return Date == other.Date && InvoiceNumber == other.InvoiceNumber && Grade == other.Grade &&
                   ProductionCode == other.ProductionCode && Math.Abs(Units - other.Units) < 0.0001 && Math.Abs(Pieces - other.Pieces) < 0.0001;
        }

        public void SaveItem(BinaryWriter writer)
        {
            writer.Write(InvoiceNumber);
            writer.Write(InventoryItemID);
            writer.Write(ProductionCode);
            writer.Write(Units);
            writer.Write(Pieces);
            writer.Write(Grade);
            writer.Write(Date.ToLongDateString());
            writer.Write(MasterID);
            writer.Write(Fulfilled);
            writer.Write(ScheduledToFill);
        }

        public static SalesItem Load(BinaryReader reader)
        {
            string salesNum = reader.ReadString();
            int itemID = reader.ReadInt32();
            string code = reader.ReadString();
            double units = reader.ReadDouble();
            double pieces = reader.ReadDouble();
            string grade = reader.ReadString();
            DateTime date;
            DateTime.TryParse(reader.ReadString(), out date);
            int master = reader.ReadInt32();
            double filled = reader.ReadDouble();
            double scheduled = reader.ReadDouble();

            var newSales = new SalesItem(code, salesNum, units, pieces, grade, date, itemID) {MasterID = master,ScheduledToFill = scheduled,Fulfilled = filled};

            return newSales;
        }

        public override string ToString()
        {
            return $"Invoice #{InvoiceNumber}, product: {ProductionCode}";
        }

        public double FulfillOrder(double unitsToMake)
        {
            double remainder = unitsToMake > Units ? unitsToMake - Units : 0;

            Fulfilled = unitsToMake - remainder;

            return remainder;
        }

        public double ScheduleToRun(double units)
        {
            double remainder = units > Units ? units - Units : 0;

            ScheduledToFill = units - remainder;

            return remainder;
        }
    }
}
