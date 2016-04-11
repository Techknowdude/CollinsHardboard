using System;
using System.IO;
using System.Windows;
using StaticHelpers;

namespace ModelLib
{
    public class InventoryItem : DependencyObject
    {
        #region DataMembers

        private static Int32 _idCounter = 0;
        public static Int32 IDCounter { get { return _idCounter; } set { _idCounter = value; }}

        public bool IsPurged
        {
            get { return _isPurged; }
            set { _isPurged = value; }
        }

        private Int32 _invItemID;
        private bool _isPurged = false;
        private double _genUnits;
        public Int32 InventoryItemID { get { return _invItemID; } set { _invItemID = value; } }


        public static readonly DependencyProperty UnitProperty =
            DependencyProperty.Register("Units", typeof(double),
            typeof(InventoryItem), new UIPropertyMetadata(null));

        public static readonly DependencyProperty UnitPiecesProperty =
            DependencyProperty.Register("UnitPieces", typeof(double),
            typeof(InventoryItem), new UIPropertyMetadata(null));

        public static readonly DependencyProperty ProductCodeProperty =
            DependencyProperty.Register("ProductCode", typeof(string),
            typeof(InventoryItem), new UIPropertyMetadata(null));

        public static readonly DependencyProperty GradeProperty =
            DependencyProperty.Register("Grade", typeof(String),
            typeof(InventoryItem), new UIPropertyMetadata(null));

        public double Units
        {
            get { return (double)GetValue(UnitProperty); }
            set { SetValue(UnitProperty, value); }
        }

        public double PiecesPerUnit
        {
            get { return (double)GetValue(UnitPiecesProperty); }
            set { SetValue(UnitPiecesProperty, value); }
        }

        public String ProductCode
        {
            get { return (String)GetValue(ProductCodeProperty); }
            set { SetValue(ProductCodeProperty, value); }
        }

        public String Grade
        {
            get { return (String)GetValue(GradeProperty); }
            set { SetValue(GradeProperty, value); }
        }

        public int MasterID { get; set; }

        // change in units
        public double GenUnits
        {
            get { return Units + _genUnits; }
            set { _genUnits = value - Units; }
        }

        #endregion

        public InventoryItem(string code, double unit = 0, double pcsPer = 0, string grade = "", int master = -1, int id = -1, bool purge = false)
        {
            InventoryItemID = id == -1 ? IDCounter++ : -1;
            MasterID = master != -1 ? master : -1;
            ProductCode = code;
            Units = unit;
            PiecesPerUnit = pcsPer;
            Grade = grade;
            IsPurged = purge;
        }

        public InventoryItem(ProductMasterItem item1, double units, string grade, bool purge = false)
        {
            InventoryItemID = IDCounter++;
            MasterID = item1.MasterID;
            ProductCode = item1.ProductionCode;
            Units = units;
            PiecesPerUnit = item1.PiecesPerUnit;
            Grade = grade;
            IsPurged = purge;
        }

        public void AddUnits(double units)
        {
            Units += units;
        }

        public void SetValues(InventoryItem item)
        {
            ProductCode = item.ProductCode;
            Units = item.Units;
            PiecesPerUnit = item.PiecesPerUnit;
            Grade = Grade;
            InventoryItemID = item.InventoryItemID;
            MasterID = item.MasterID;
        }

        public static InventoryItem LoadItem(BinaryReader reader)
        {
            string grade = reader.ReadString();
            string code = reader.ReadString();
            double units = reader.ReadDouble();
            double pcsPer = reader.ReadDouble();
            int id = reader.ReadInt32();
            int master = reader.ReadInt32();
            bool purge = reader.ReadBoolean();
            return new InventoryItem(code,units,pcsPer,grade,master,id,purge);
        }

        public void SaveItem(BinaryWriter writer)
        {
            writer.Write(Grade);
            writer.Write(ProductCode);
            writer.Write(Units);
            writer.Write(PiecesPerUnit);
            writer.Write(InventoryItemID);
            writer.Write(MasterID);
            writer.Write(IsPurged);
        }
    }
}
