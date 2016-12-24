using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using CoatingScheduler.Annotations;
using Configuration_windows;
using Microsoft.Office.Interop.Excel;
using ModelLib;
using StaticHelpers;

namespace CoatingScheduler
{
    [Serializable]
    public class CoatingScheduleProduct : CoatingScheduleProductBase, INotifyPropertyChanged
    {

        #region Fields
        private String _thickness;
        private String _productCode;
        private String _grades;
        private bool _hasBarcode;
        private String _units;
        private String _notes;
        private String _placement;
        private String _description;
        private double _unitsPerHour;
        private int _masterID;
        private DurationType _durationType;
        private bool _hasBackbrand;
        private bool _isTrial;
        private Machine _machine;
        private Configuration _config;

        #endregion
        #region Properties

        public string Description
        {
            get { return _description; }
            set
            {
                if (value == _description) return;
                _description = value;
                NotifyPropertyChanged();
            }
        }

        public String Thickness
        {
            get { return _thickness; }
            set
            {
                if (value == _thickness) return;
                _thickness = value;
                NotifyPropertyChanged();
            }
        }
        public String ProductCode
        {
            get { return _productCode; }
            set
            {
                if (value == _productCode) return;
                _productCode = value;
                NotifyPropertyChanged();
            }
        }
        public String Grades
        {
            get { return _grades; }
            set
            {
                if (value == _grades) return;
                _grades = value;
                NotifyPropertyChanged();
            }
        }
        public bool HasBarcode
        {
            get { return _hasBarcode; }
            set
            {
                if (value == _hasBarcode) return;
                _hasBarcode = value;
                NotifyPropertyChanged();
            }
        }

        public String Units
        {
            get { return _units; }
            set
            {
                if (value == _units) return;
                _units = value;
                SpreadUnits();
                NotifyPropertyChanged();
            }
        }
        public String Notes
        {
            get { return _notes; }
            set
            {
                if (value == _notes) return;
                _notes = value;
                NotifyPropertyChanged();
            }
        }

        public String Placement
        {
            get { return _placement; }
            set
            {
                if (value == _placement) return;
                _placement = value;
                NotifyPropertyChanged();
            }
        }

        public double ShiftDuration
        {
            get
            {
                if((ParentLogic.ParentLogic as CoatingScheduleLine)?.Shift != null)
                    return ((CoatingScheduleLine)(ParentLogic).ParentLogic).Shift.Duration.TotalHours;
                return 0;
            }
        }

        public double UnitsPerHour
        {
            get { return _unitsPerHour; }
            set { _unitsPerHour = value; }
        }

        public Int32 MasterID
        {
            get { return _masterID; }
            set { _masterID = value; }
        }

        public DurationType DurationType
        {
            get { return _durationType; }
            set { _durationType = value; }
        }

        public bool HasBackbrand
        {
            get { return _hasBackbrand; }
            set { _hasBackbrand = value; }
        }

        public bool IsTrial
        {
            get { return _isTrial; }
            set { _isTrial = value; }
        }

        public Machine Machine
        {
            get { return _machine; }
            set { _machine = value; }
        }

        public Configuration Config
        {
            get { return _config; }
            set { _config = value; }
        }

        #endregion


        public CoatingScheduleProduct(double thickness, String description, String productCode, String grades, String units, bool hasBarcode = true, String notes = "", String placement = "")
        {
            InitializeMembers(thickness, description, productCode, grades, units, hasBarcode, notes, placement,30);
        }
        public CoatingScheduleProduct(String thickness, String description, String productCode, String grades, String units, bool hasBarcode = true, String notes = "", String placement = "")
        {
            InitializeMembers(thickness, description, productCode, grades, units, hasBarcode, notes, placement,30);
        }

        public CoatingScheduleProduct()
        {
            InitializeMembers(0,"","","","",true,"","",30);
        }

        public CoatingScheduleProduct(ProductMasterItem masterItem)
        {
            InitializeMembers(masterItem.Thickness,masterItem.Description,masterItem.ProductionCode,masterItem.Grades,"",masterItem.HasBarcode,masterItem.Notes, "", masterItem.UnitsPerHour);
            MasterID = masterItem.MasterID;
        }


        public CoatingScheduleProduct(CoatingScheduleProduct other)
        {
            InitializeMembers(other.Thickness,other.Description,other.ProductCode,other.Grades,other.Units,
                other.HasBarcode,other.Notes,other.Placement,other.UnitsPerHour);
            Machine = other.Machine;
            Config = other.Config;
        }

        static CoatingScheduleProduct()
        {
        
        }

        private CoatingScheduleProduct(string thickness, string description, string productCode, string grades, string units, 
            bool hasBarcode, string notes, string placement, bool trial, bool backbrand, int masterId, string coatingLine, double unitsPerHour,
            DurationType durationType,Machine machine, Configuration config)
        {
            InitializeMembers(thickness, description, productCode, grades, units, hasBarcode, notes, placement, unitsPerHour,masterId,backbrand,trial,coatingLine,durationType,machine,config);
        }


        private void InitializeMembers(double thickness, string description, string productCode, string grades, string units, 
            bool hasBarcode, string notes, string placement, double unitsPerHour)
        {
            Thickness = StaticFunctions.ConvertDoubleToStringThickness(thickness);
            InitializeMembers(Thickness,description,productCode,grades,units,hasBarcode,notes,placement,unitsPerHour);
        }


        private void InitializeMembers(string thickness, string description, string productCode, string grades, string units, 
            bool hasBarcode, string notes, string placement, double unitsPerHour, int master = -1, bool backbrand = true, bool trial = false,
            string coatingLine = "",DurationType durationType = DurationType.Units, Machine machine = null, Configuration config = null)
        {
            Thickness = thickness;
            ProductCode = productCode;
            Description = description;
            Grades = grades;
            _units = units;
            HasBarcode = hasBarcode;
            Notes = notes;
            Placement = placement;
            if(Placement == String.Empty)
                Placement = "Placement: ";
            UnitsPerHour = unitsPerHour;
            DurationType = durationType;
            IsTrial = trial;
            HasBackbrand = backbrand;
            MasterID = master;
            CoatingLine = coatingLine;
            Machine = machine;
            Config = config;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public override bool IsFull()
        {
            throw new NotImplementedException();
        }

        public override void SpreadUnits()
        {
            try
            {
                // product updated. call spread on parent
                ((CoatingScheduleShift) ParentLogic).SpreadUnits();
            }
            catch (Exception exception)
            {
                // ignored
            }
        }
        
        public CoatingScheduleProduct SplitProduct(double hours)
        {
            return null;
        }

        public void SetUnits(string units)
        {
            _units = units;
        }

        public static CoatingScheduleProduct Load(Stream stream, IFormatter formatter)
        {
            return (CoatingScheduleProduct) formatter.Deserialize(stream);
        }

        public override void Save(Stream stream, IFormatter formatter)
        {
            formatter.Serialize(stream,this);
        }

        public override Tuple<int, int> ExportToExcel(_Worksheet sheet, Int32 column, Int32 row)
        {
            Int32 nextRow = row;
            Int32 currentCol = column;
            Range range;
            // output thickness
            range = sheet.Range[StaticFunctions.GetRangeIndex(currentCol, nextRow)];
            StaticFunctions.SaveRichTextToCell(range, Thickness);

            // output Description
            ++currentCol;
            range = sheet.Range[StaticFunctions.GetRangeIndex(currentCol, nextRow)];
            StaticFunctions.SaveRichTextToCell(range, Description);

            // output grades
            ++currentCol;
            range = sheet.Range[StaticFunctions.GetRangeIndex(currentCol, nextRow)];
            StaticFunctions.SaveRichTextToCell(range, Grades);

            // output barcode
            ++currentCol;
            range = sheet.Range[StaticFunctions.GetRangeIndex(currentCol, nextRow)];
            StaticFunctions.SaveRichTextToCell(range, HasBarcode ? "Yes" : "No");

            ++nextRow;
            currentCol = column + 1;

            // output units
            String unitType = this.DurationType.ToString() + ": ";

            range = sheet.Range[StaticFunctions.GetRangeIndex(currentCol, nextRow)];
            StaticFunctions.SaveRichTextToCell(range, unitType + Units);
            ++nextRow;

            // output placement
            range = sheet.Range[StaticFunctions.GetRangeIndex(currentCol, nextRow)];
            StaticFunctions.SaveRichTextToCell(range, Placement);

            ++nextRow;

            return new Tuple<int, int>(nextRow, column + ExcelWidth);
        }


    }
}
