using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModelLib;

namespace ProductionScheduler
{
    public class PressItem
    {
        #region Fields

        //private DateTime _startDate;
        private string _thickness;
        private Int32 _numShifts;
        private DateTime _endTime;
        private ProductMasterItem _product;
        private String _desctiption;
        #endregion

        #region Properties

        //public DateTime StartDate
        //{
        //    get { return _startDate; }
        //    set { _startDate = value; }
        //}

        public string Thickness
        {
            get { return _thickness; }
            set { _thickness = value; }
        }

        public Int32 NumShifts
        {
            get { return _numShifts; }
            set { _numShifts = value; }
        }

        public DateTime EndTime
        {
            get { return _endTime; }
            set { _endTime = value; }
        }

        public ProductMasterItem Product1
        {
            get { return _product; }
            set { _product = value; }
        }

        public string Desctiption
        {
            get { return _desctiption; }
            set { _desctiption = value; }
        }

        public bool IsTrial { get; set; }

        #endregion

        public static PressItem CreatePressItem(string thickness, Int32 numShifts, DateTime endTime, //DateTime startDate,
            ProductMasterItem product)
        {
            return new PressItem(thickness, numShifts, endTime, product); //startDate,
        }

        public PressItem(string thickness, Int32 numShifts, DateTime endTime, ProductMasterItem product) //DateTime startDate, 
        {
           // _startDate = startDate;
            _thickness = thickness;
            _numShifts = numShifts;
            _endTime = endTime;
            _product = product;
        }
    }
}
