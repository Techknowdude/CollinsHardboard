using System;

namespace CoatingScheduleMomentos
{
    public class ProductMomento : ProductMomentoBase
    {
        public ProductMomento(String thick, String code, String grades, bool barcode,
            String units, String notes, String placement, String desc)
        {
            Thickness = thick;
            ProductCode = code;
            Grades = grades;
            HasBarcode = barcode;
            Units = units;
            Notes = notes;
            Placement = placement;
            Description = desc;
        }

        public ProductMomento()
        {
            
        }

        #region Fields
        private String _thickness;
        private String _productCode;
        private String _grades;
        private bool _hasBarcode;
        private String _units;
        private String _notes;
        private String _placement;
        private String _description;

        #endregion

        #region Properties
        public string Thickness
        {
            get { return _thickness; }
            set { _thickness = value; }
        }

        public string ProductCode
        {
            get { return _productCode; }
            set { _productCode = value; }
        }

        public string Grades
        {
            get { return _grades; }
            set { _grades = value; }
        }

        public bool HasBarcode
        {
            get { return _hasBarcode; }
            set { _hasBarcode = value; }
        }

        public string Units
        {
            get { return _units; }
            set { _units = value; }
        }

        public string Notes
        {
            get { return _notes; }
            set { _notes = value; }
        }

        public string Placement
        {
            get { return _placement; }
            set { _placement = value; }
        }

        public string Description
        {
            get { return _description; }
            set { _description = value; }
        }

        #endregion
    }
}
