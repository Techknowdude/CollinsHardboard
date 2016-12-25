using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScheduleGen
{
    /// <summary>
    /// Class that holds the information of one product on a particular day.
    /// </summary>
    public class RequirementsDay
    {
        #region Fields

        private bool _calculated;

        private double _grossPieces;
        private double _inventory;
        private double _netRequiredPieces;
        private double _purchaseOrderPieces;
        private double _nextInventoryPieces;

        #endregion

        public RequirementsDay(ProductRequirements parentRequirements, DateTime day)
        {
            ParentRequirements = parentRequirements;
            Day = day;
        }

        public ProductRequirements ParentRequirements { get; set; }
        public DateTime Day { get; set; }
        #region Properties

        /// <summary>
        /// Setter will adjust the net requirements.
        /// </summary>
        public double GrossPieces
        {
            get { return _grossPieces; }

            set
            {
                _grossPieces = value;
                CalcNetRequired();
            }
        }

        /// <summary>
        /// Setter will adjust the net requirements.
        /// </summary>
        public double Inventory
        {
            get { return _inventory; }

            set
            {
                _inventory = value;
                CalcNetRequired();
            }

        }

        public double NextInventoryPieces
        {
            get { return _nextInventoryPieces; }
            set
            {
                    _nextInventoryPieces = value;
            }
        }

        public double PieceCountTolerance = 0.00000;

        /// <summary>
        /// Need for additional pieces. Updates other requirements if changed.
        /// </summary>
        public double NetRequiredPieces
        {
            get { return _netRequiredPieces; }

            set
            {
                    _netRequiredPieces = value;
            }
        }

        /// <summary>
        /// How many pieces are required to be produced on this day
        /// </summary>
        public double PurchaseOrderPieces
        {
            get { return _purchaseOrderPieces; }

            set { _purchaseOrderPieces = value; }
        }

        /// <summary>
        /// True if the requirement has already been calculated. Set to false by default and also reset to false after the calculation is finished.
        /// </summary>
        public bool Calculated
        {
            get { return _calculated; }
            set { _calculated = value; }
        }

        #endregion

        public void CalcNetRequired()
        {
            NetRequiredPieces = Math.Max(GrossPieces - Inventory,0); // Don't show negative NetRequired, use 0.
            NextInventoryPieces = Math.Max(Inventory - GrossPieces, 0);
        }

        /// <summary>
        /// Adds the pieces to the current gross requirement
        /// </summary>
        /// <param name="pieces"></param>
        public void AddGrossRequirement(double pieces)
        {
            // This recalculates everything and changes any dependent items as well.
            GrossPieces += pieces;
        }

        public override bool Equals(object obj)
        {
            var other = obj as RequirementsDay;
            if (other == null)
                return base.Equals(obj);
            else
            {
                return Math.Abs(GrossPieces - other.GrossPieces) < PieceCountTolerance
                       && Math.Abs(other.Inventory - Inventory) < PieceCountTolerance
                       && Math.Abs(PurchaseOrderPieces - other.PurchaseOrderPieces) < PieceCountTolerance;
            }
        }

        protected bool Equals(RequirementsDay other)
        {
            return _calculated == other._calculated && _grossPieces == other._grossPieces && _inventory == other._inventory && _netRequiredPieces == other._netRequiredPieces && _purchaseOrderPieces == other._purchaseOrderPieces;
        }

        public void AddOnHand(double pieces)
        {
            // this should recalc net
            Inventory += pieces;
        }
    }
}
