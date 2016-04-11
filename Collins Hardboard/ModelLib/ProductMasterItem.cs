using System;
using System.IO;
using StaticHelpers;

namespace ModelLib
{
    public class ProductMasterItem
    {

        #region DataMembers/Properties
        private static Int32 _idCounter = 0;
        public static Int32 IDCounter { get { return _idCounter; } set { _idCounter = value; } }

        private Int32 _uniqueId;
        public Int32 UniqueID { get { return _uniqueId; } set { _uniqueId = value; } }

        public Int32 MasterID
        {
            get { return _masterId; }
            set { _masterId = value; }
        }

        private String _madeIn;
        public String MadeIn { get { return _madeIn; } set { _madeIn = value; } }

        private String _productionCode = String.Empty;
        public String ProductionCode
        {
            get { return _productionCode; }
            set { _productionCode = value; }
        }

        private String _description = String.Empty;
        public String Description
        {
            get { return _description; }
            set { _description = value; }
        }

        private double _width;
        public double Width
        {
            get { return _width; }
            set { _width = value; }
        }

        private double _length;
        public double Length
        {
            get { return _length; }
            set { _length = value; }
        }

        private double _thickness;
        public double Thickness
        {
            get { return _thickness; }
            set { _thickness = value; }
        }

        private String _texture = String.Empty;
        public String Texture
        {
            get { return _texture; }
            set { _texture = value; }
        }


        private double _waste;
        public double Waste { get { return _waste; } set { _waste = value; } }


        private Int32 _pcsPerUnit;
        private int _masterId;
        public Int32 PiecesPerUnit { get { return _pcsPerUnit; } set { _pcsPerUnit = value; } }
        public string Grades { get; set; }
        public bool HasBarcode { get; set; }

        public double MaxSupply { get; set; }

        public double MinSupply { get; set; }

        public string TurnType { get; set; }

        public string Notes { get; set; }

        public double TargetSupply { get; set; }

        public double UnitsPerHour { get; set; }
        #endregion

        public ProductMasterItem(int idNumber, string itemCode, string description, double width, double length, string thickness, string texture, double waste, int pcsPerUnit, string grades, bool hasBarcode, string notes, string turnType, double minSupply, double maxSupply, double targetSupply, double unitsPerHour)
        {
            UniqueID = IDCounter++;
            PiecesPerUnit = pcsPerUnit;
            Grades = grades;
            HasBarcode = hasBarcode;
            ProductionCode = itemCode;
            Description = description;
            Width = width;
            Length = length;
            Thickness = StaticFunctions.StringToDouble(thickness);
            Texture = texture;
            Waste = waste;
            TargetSupply = targetSupply;
            Notes = notes;
            TurnType = turnType;
            MinSupply = minSupply;
            MaxSupply = maxSupply;
            UnitsPerHour = unitsPerHour;
            MasterID = idNumber;
            MadeIn = "";
        }

        public ProductMasterItem(int idNumber, string itemCode, string description, double width, double length, double thickness, string texture, double waste, int pcsPerUnit, string grades, bool hasBarcode, string notes, string turnType, double minSupply, double maxSupply, double targetSupply, double unitsPerHour)
        {
            UniqueID = IDCounter++;
            PiecesPerUnit = pcsPerUnit;
            Grades = grades;
            HasBarcode = hasBarcode;
            ProductionCode = itemCode;
            Description = description;
            Width = width;
            Length = length;
            Thickness = thickness;
            Texture = texture;
            Waste = waste;
            TargetSupply = targetSupply;
            Notes = notes;
            TurnType = turnType;
            MinSupply = minSupply;
            MaxSupply = maxSupply;
            UnitsPerHour = unitsPerHour;
            MasterID = idNumber;
            MadeIn = "";
        }

        public static ProductMasterItem CreateDefault()
        {
            return new ProductMasterItem(-1,"Testcode","No description",38,64,"1/2","OM",10,34,"DWE",true,"","Units",0,0,0,0);
        }

        public void SetValues(ProductMasterItem item)
        {
            Description = item.Description;
            Width = item.Width;
            Length = item.Length;
            Thickness = item.Thickness;
            Texture = item.Texture;
            Waste = item.Waste;
            Grades = item.Grades;
            HasBarcode = item.HasBarcode;
            Notes = item.Notes;
            TurnType = item.TurnType;
            MinSupply = item.MinSupply;
            MaxSupply = item.MaxSupply;
            TargetSupply = item.TargetSupply;
            UnitsPerHour = item.UnitsPerHour;
            MasterID = item.MasterID;
            MadeIn = item.MadeIn;
        }

        public override string ToString()
        {
            return Description;
        }

        public void Save(BinaryWriter writer)
        {
            writer.Write(ProductionCode);
            writer.Write(Description);
            writer.Write(Width);
            writer.Write(Length);
            writer.Write(Thickness);
            writer.Write(Texture);
            writer.Write(Waste);
            writer.Write(Grades);
            writer.Write(HasBarcode);
            writer.Write(Notes);
            writer.Write(TurnType);
            writer.Write(MinSupply);
            writer.Write(MaxSupply);
            writer.Write(TargetSupply);
            writer.Write(UnitsPerHour);
            writer.Write(PiecesPerUnit);
            writer.Write(MasterID);
            writer.Write(MadeIn);
        }

        public static ProductMasterItem Load(BinaryReader reader)
        {
            String code = reader.ReadString();
            String desc = reader.ReadString();
            double width = reader.ReadDouble();
            double len = reader.ReadDouble();
            double thick = reader.ReadDouble();
            String tex = reader.ReadString();
            double waste = reader.ReadDouble();
            String grades = reader.ReadString();
            bool bar = reader.ReadBoolean();
            String notes = reader.ReadString();
            String turns = reader.ReadString();
            double min = reader.ReadDouble();
            double max = reader.ReadDouble();
            double target = reader.ReadDouble();
            double units = reader.ReadDouble();
            Int32 pcs = reader.ReadInt32();
            Int32 id = reader.ReadInt32();
            String made = reader.ReadString();

            return new ProductMasterItem(id,code,desc,width,len,StaticFunctions.ConvertDoubleToStringThickness(thick),tex,waste,pcs,grades,bar,notes,turns,min,max,target,units) {MadeIn = made};
        }

        public override bool Equals(object obj)
        {
            ProductMasterItem other = obj as ProductMasterItem;
            if (other != null)
            {
                return MasterID == other.MasterID;
            }
            else
            {
                return false;
            }
        }
    }
}
