using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Configuration_windows;
using ImportLib;
using ModelLib;

namespace ScheduleGen
{

    /// <summary>
    /// This class holds information on the production requirements for a particular item
    /// </summary>
    public class ProductRequirements
    {

        #region Fields
        private ProductMasterItem _masterItem;
        private Dictionary<DateTime, RequirementsDay> _requiredPieces;
        private List<ConfigItem> _input;
        private int _outPieces;
        #endregion

        #region Properties
        public ProductMasterItem MasterItem
        {
            get
            {
                return _masterItem;
            }

            set
            {
                _masterItem = value;
            }
        }

        public Dictionary<DateTime, RequirementsDay> RequiredPieces
        {
            get { return _requiredPieces; }
            set { _requiredPieces = value; }
        }

        /// <summary>
        /// Item IDs and number required to make some number of pieces of this product.
        /// [ID][Number to make]
        /// </summary>
        public List<ConfigItem> Input
        {
            get
            {
                return _input;
            }

            set
            {
                _input = value;
            }
        }

        /// <summary>
        /// Number of pieces created when the items in the input list are used.
        /// </summary>
        public int OutPieces
        {
            get
            {
                return _outPieces;
            }

            set
            {
                _outPieces = value;
            }
        }

        public bool HasConfig { get; set; }

        #endregion

        /// <summary>
        /// Factory for product requirements
        /// </summary>
        /// <returns></returns>
        public static ProductRequirements CreateProductRequirements(ProductMasterItem master, Configuration usedConfig)
        {
            var req = new ProductRequirements(master);
            if(usedConfig != null && usedConfig.OutputItems.Any(c => c.MasterID == master.MasterID))
            {
                req.OutPieces = (int) (usedConfig.OutputItems.FirstOrDefault(o => o.MasterID == master.MasterID).Pieces > 0 ? usedConfig.OutputItems.FirstOrDefault(o => o.MasterID == master.MasterID).Pieces : 1);
                req.Input = usedConfig.InputItems.ToList();
                req.HasConfig = true;
            }
            else
            {
                req.HasConfig = false;
            }
            RequirementsHandler.Register(req);
            return req;
        }

        private ProductRequirements(ProductMasterItem masterItem)
        {
            _requiredPieces = new Dictionary<DateTime, RequirementsDay>();
            _masterItem = masterItem;
        }

        #region Instance Functions
        
        /// <summary>
        /// Adds a listing to the gross requirements of the item on the date. If another sale is already on that date, the required items are added.
        /// </summary>
        /// <param name="day"></param>
        /// <param name="pieces"></param>
        public void AddSale(DateTime day, double pieces)
        {
            // Validate day
            var validDate = RequirementsHandler.ValidateDay(day);

            // Safe access to dictionary. Creates new entry if needed.
            RequirementsDay requirement = null;
            if (RequiredPieces.ContainsKey(validDate))
            {
                requirement = RequiredPieces[validDate];
                requirement.AddGrossRequirement(pieces);
            }
            else
            {
                requirement = new RequirementsDay(this, validDate);
                RequiredPieces[validDate] = requirement;
                requirement.AddGrossRequirement(pieces);
            }

        }

        /// <summary>
        /// Adds the number of pieces to on hand inventory
        /// </summary>
        /// <param name="day"></param>
        /// <param name="pieces"></param>
        public void AddOnHandInventory(int pieces)
        {
            // Validate day
            var validDate = RequirementsHandler.GetEarliestDay();

            RequirementsDay reqDay = null;
            RequirementsDay prevDay = null;

            // Safe Access.
            if (RequiredPieces.ContainsKey(validDate))
            {
                reqDay = RequiredPieces[validDate];
                reqDay.AddOnHand(pieces);
            }
            else
            {
                reqDay = new RequirementsDay(this, validDate);
                RequiredPieces[validDate] = reqDay;
                reqDay.Inventory = pieces;
            }
        }

        public void UpdateInventory(DateTime startDay)
        {
            // Validate day
            var validDate = RequirementsHandler.ValidateDay(startDay);

            RequirementsDay reqDay = null;
            RequirementsDay prevDay = null;

            // add inventory numbers for each day
            reqDay = GetRequirementDay(validDate);
            validDate = validDate.AddDays(1);

            while (validDate <= RequirementsHandler.GetLatestDay())
            {
                prevDay = reqDay;
                reqDay = GetRequirementDay(validDate);
                reqDay.Inventory = prevDay.NextInventoryPieces;
                validDate = validDate.AddDays(1);
            }
        }


        /// <summary>
        /// Get the requirements for a day. Creates a new day if there is not one
        /// </summary>
        /// <param name="day"></param>
        /// <returns></returns>
        public RequirementsDay GetRequirementDay(DateTime day)
        {
            var validDay = RequirementsHandler.ValidateDay(day);

            RequirementsDay reqDay = null;

            if (RequiredPieces.ContainsKey(validDay))
            {
                reqDay = RequiredPieces[validDay];
            }
            else
            {
                reqDay = new RequirementsDay(this,validDay);
                RequiredPieces[validDay] = reqDay;
            }

            return reqDay;
        }

        #endregion

        public void OutputString(StringBuilder outputString)
        {
            var product = StaticInventoryTracker.ProductMasterList.FirstOrDefault(x => x.MasterID == MasterItem.MasterID);
            string name = product?.ProductionCode;

            outputString.Append($"{name}");
            for (int i = 0; i < RequirementsHandler.GetNumDays(); ++i)
            {
                outputString.Append($",{" "}"); // keep columns
            }
            outputString.AppendLine(); // end product line

            StringBuilder grossBuilder = new StringBuilder();
            StringBuilder onHandBuilder = new StringBuilder();
            StringBuilder netBuilder = new StringBuilder();
            StringBuilder purchaseBuilder = new StringBuilder();

            grossBuilder.Append($"{"Gross"},");
            onHandBuilder.Append($"{"OnHand"},");
            netBuilder.Append($"{"Net"},");
            purchaseBuilder.Append($"{"POS"},");

            DateTime current = RequirementsHandler.GetEarliestDay();
            while (current <= RequirementsHandler.GetLatestDay())
            {
                RequirementsDay day = null;
                if (RequiredPieces.ContainsKey(current))
                    day = RequiredPieces[current];
                if (day != null)
                {
                    grossBuilder.Append($"{day.GrossPieces},");
                    onHandBuilder.Append($"{day.Inventory},");
                    netBuilder.Append($"{day.NetRequiredPieces},");
                    purchaseBuilder.Append($"{day.PurchaseOrderPieces},");
                }
                else
                {
                    grossBuilder.Append($"{"0"},");
                    onHandBuilder.Append($"{"0"},");
                    netBuilder.Append($"{"0"},");
                    purchaseBuilder.Append($"{"0"},");
                }
                current = current.AddDays(1);
            }

            outputString.AppendLine(grossBuilder.ToString());
            outputString.AppendLine(onHandBuilder.ToString());
            outputString.AppendLine(netBuilder.ToString());
            outputString.AppendLine(purchaseBuilder.ToString());
        }

        public void UpdateNextInventory(DateTime day, double change)
        {
            RequirementsDay nextDay;
            if (day.AddDays(1) <= RequirementsHandler.GetLatestDay())
            {
                nextDay = GetRequirementDay(day.AddDays(1));
                nextDay.Inventory += change;
            }

        }

        public void SetInventory(double pieces)
        {
            var day = GetRequirementDay(RequirementsHandler.GetEarliestDay());
            day.Inventory = pieces;
        }
    }
}
