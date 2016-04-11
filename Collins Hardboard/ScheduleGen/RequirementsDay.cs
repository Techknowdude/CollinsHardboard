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

        private int _grossPieces;
        private int _onHandPieces;
        private int _netRequiredPieces;
        private int _purchaseOrderPieces;

        #endregion

        #region Properties

        /// <summary>
        /// Setter will adjust the net requirements.
        /// </summary>
        public int GrossPieces
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
        public int OnHandPieces
        {
            get { return _onHandPieces; }

            set
            {
                _onHandPieces = value;
                CalcNetRequired();
            }

        }

        /// <summary>
        /// Need for additional pieces
        /// </summary>
        public int NetRequiredPieces
        {
            get { return _netRequiredPieces; }

            set { _netRequiredPieces = value; }
        }

        /// <summary>
        /// How many pieces are required to be produced on this day
        /// </summary>
        public int PurchaseOrderPieces
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
        public void AddGrossRequirement(int pieces)
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
            return _calculated == other._calculated && _grossPieces == other._grossPieces && _onHandPieces == other._onHandPieces && _netRequiredPieces == other._netRequiredPieces && _purchaseOrderPieces == other._purchaseOrderPieces;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = _calculated.GetHashCode();
                hashCode = (hashCode * 397) ^ _grossPieces;
                hashCode = (hashCode * 397) ^ _onHandPieces;
                hashCode = (hashCode * 397) ^ _netRequiredPieces;
                hashCode = (hashCode * 397) ^ _purchaseOrderPieces;
                return hashCode;
            }
        }

        public void AddOnHand(int pieces)
        {
            // this should recalc net
            OnHandPieces += pieces;
        }
    }
}
