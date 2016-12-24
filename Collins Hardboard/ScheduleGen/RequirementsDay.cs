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
        private double _onHandUnits;
        private double _netRequiredUnits;
        private double _purchaseOrderPieces;

        #endregion

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
        public double OnHandPieces
        {
            get { return _onHandUnits; }

            set
            {
                _onHandUnits = value;
                CalcNetRequired();
            }

        }

        /// <summary>
        /// Need for additional pieces
        /// </summary>
        public double NetRequiredPieces
        {
            get { return _netRequiredUnits; }

            set { _netRequiredUnits = value; }
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
            NetRequiredPieces = GrossPieces - OnHandPieces;
            if (NetRequiredPieces < 0) // onHand covers requirement. Don't show negative NetRequired, use 0.
            {
                NetRequiredPieces = 0;
            }
        }

        /// <summary>
        /// Adds the pieces to the current gross requirement
        /// </summary>
        /// <param name="pieces"></param>
        public void AddGrossRequirement(double pieces)
        {
            // This recalculates the 
            GrossPieces += pieces;
        }

        public override bool Equals(object obj)
        {
            var other = obj as RequirementsDay;
            if (other == null)
                return base.Equals(obj);
            else
            {
                return GrossPieces == other.GrossPieces
                       && other.OnHandPieces == OnHandPieces
                       && PurchaseOrderPieces == other.PurchaseOrderPieces;
            }
        }

        protected bool Equals(RequirementsDay other)
        {
            return _calculated == other._calculated && _grossPieces == other._grossPieces && _onHandUnits == other._onHandUnits && _netRequiredUnits == other._netRequiredUnits && _purchaseOrderPieces == other._purchaseOrderPieces;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = _calculated.GetHashCode();
                hashCode = (hashCode * 397) ^ (int) _grossPieces;
                hashCode = (hashCode * 397) ^ (int) _onHandUnits;
                hashCode = (hashCode * 397) ^ (int) _netRequiredUnits;
                hashCode = (hashCode * 397) ^ (int) _purchaseOrderPieces;
                return hashCode;
            }
        }

        public void AddOnHand(double pieces)
        {
            // this should recalc net
            OnHandPieces += pieces;
        }
    }
}
