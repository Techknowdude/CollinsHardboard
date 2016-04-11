using System;
using StaticHelpers;

namespace ModelLib
{
    public class Product
    {

        #region DataMembers/Properties
        private static Int32 _idCounter = 0;
        public static Int32 IDCounter { get { return _idCounter; } set { _idCounter = value; } }

        private Int32 _productID;
        public Int32 ProductID { get { return _productID; } set { _productID = value; } }

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

        private Int32 _qty;
        public Int32 Quantity
        {
            get { return _qty; }
            set { _qty = value; }
        }

        private String _grade;
        public String BoardGrade
        {
            get { return _grade; }
            set { _grade = value; }
        }

        private double _waste;
        public double Waste {get { return _waste; } set { _waste = value; }}

        #endregion

        public Product(String itemCode, String description, double width, double length, double thickness,
            String texture, Int32 qty, String grade, double waste)
        {
            ProductID = IDCounter++;

            ProductionCode = itemCode;
            Description = description;
            Width = width;
            Length = length;
            Thickness = thickness;
            Texture = texture;
            Quantity = qty;
            BoardGrade = grade;
            Waste = waste;
        }
        public Product(String itemCode, String description, double width, double length, String thickness,
            String texture, Int32 qty, String grade, double waste)
        {
            ProductID = IDCounter++;

            ProductionCode = itemCode;
            Description = description;
            Width = width;
            Length = length;
            Thickness = StaticFunctions.StringToDouble(thickness);
            Texture = texture;
            Quantity = qty;
            BoardGrade = grade;
            Waste = waste;
        }
    }
}
