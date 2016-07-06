using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup.Localizer;
using System.Xml;
using Configuration_windows;
using ImportLib;

namespace ScheduleGen
{

    /// <summary>
    /// This class holds information on the production requirements for a perticular item
    /// </summary>
    public class ProductRequirements
    {
        public static int LeadTimeDays = 1;

        #region Fields

        private static DateTime _earliestDay = DateTime.MaxValue;
        private static DateTime _latestDay = DateTime.MinValue;
        private static List<ProductRequirements> _allRequirements = new List<ProductRequirements>();
        private static int _numDays;

        private int _masterID;
        private Dictionary<DateTime, RequirementsDay> _requiredPieces;
        private Tuple<int, int> _input;
        private int _outPieces;
        #endregion

        #region Properties
        public int MasterID
        {
            get
            {
                return _masterID;
            }

            set
            {
                _masterID = value;
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
        public Tuple<int, int> Input
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

        #endregion

        /// <summary>
        /// Factory for product requirements
        /// </summary>
        /// <returns></returns>
        public static ProductRequirements CreateProductRequirements(int masterID, Configuration usedConfig)
        {
            var req = new ProductRequirements(masterID);
            req.OutPieces = usedConfig.ItemsOut > 0 ? usedConfig.ItemsOut : 1;
            req.Input = new Tuple<int, int>(usedConfig.ItemInID,usedConfig.ItemsIn);
            _allRequirements.Add(req);
            return req;
        }

        private ProductRequirements(int masterID)
        {
            _requiredPieces = new Dictionary<DateTime, RequirementsDay>();
            _masterID = masterID;
        }

        #region Instance Functions
        
        /// <summary>
        /// Adds a listing to the gross requirements of the item on the date. If another sale is already on that date, the required items are added.
        /// </summary>
        /// <param name="day"></param>
        /// <param name="pieces"></param>
        public void AddSale(DateTime day, int pieces)
        {
            // Validate day
            var validDate = ValidateDay(day);

            // Update earliest and latest days
            if (validDate > _latestDay)
                _latestDay = validDate;
            if (validDate < _earliestDay)
                _earliestDay = validDate;

            // Safe access to dictionary. Creates new entry if needed.
            RequirementsDay requirement = null;
            if (RequiredPieces.ContainsKey(validDate))
            {
                requirement = RequiredPieces[validDate];
                requirement.AddGrossRequirement(pieces);
            }
            else
            {
                requirement = new RequirementsDay {GrossPieces = pieces};
                RequiredPieces[validDate] = requirement;
            }

            // add items to purchase order number.
            var posDay = GetRequirementDay(day.AddDays(-LeadTimeDays));
            posDay.PurchaseOrderPieces += pieces;

            // set required number of items to make this product.
            int numToMake = pieces*(Input.Item2/OutPieces);
            if (Input.Item2%OutPieces != 0)
                numToMake++;

            // add required number to gross for required item.
            var reqItem = GetRequirements(Input.Item1);
            reqItem?.AddSale(day.AddDays(-LeadTimeDays),numToMake);
        }

        /// <summary>
        /// Adds the number of pieces to on hand inventory
        /// </summary>
        /// <param name="day"></param>
        /// <param name="pieces"></param>
        public void AddOnHandInventory(DateTime day, int pieces)
        {
            // Validate day
            var validDate = ValidateDay(day);


            // Update earliest and latest days
            if (validDate > _latestDay)
                _latestDay = validDate;
            if (validDate < _earliestDay)
                _earliestDay = validDate;

            RequirementsDay reqDay = null;

            // Safe Access.
            if (RequiredPieces.ContainsKey(validDate))
            {
                reqDay = RequiredPieces[validDate];
                reqDay.AddOnHand(pieces);
            }
            else
            {
                reqDay = new RequirementsDay { OnHandPieces = pieces };
                RequiredPieces[validDate] = reqDay;
            }

            int daysAhead = 1;

            int extraPieces = reqDay.OnHandPieces - reqDay.NetRequiredPieces; // netRequired is 0 if onHand fills the requirement
            while (extraPieces > 0)
            { 
                // Safe Access.
                if (RequiredPieces.Any(x => x.Key >= validDate.AddDays(daysAhead)))
                {
                    reqDay = GetRequirementDay(validDate.AddDays(daysAhead++));
                    reqDay.AddOnHand(pieces);
                    extraPieces = reqDay.OnHandPieces - reqDay.NetRequiredPieces; // netRequired is 0 if onHand fills the requirement
                }
                else
                {
                    break;
                }
            }
        }

       /// <summary>
        /// Used to make sure the dates used are all at the start of the day time.
        /// </summary>
        /// <param name="day"></param>
        /// <returns></returns>
        private DateTime ValidateDay(DateTime day)
        {
            return new DateTime(day.Year,day.Month,day.Day);
        }

        /// <summary>
        /// Get the requirements for a day. Creates a new day if there is not one
        /// </summary>
        /// <param name="day"></param>
        /// <returns></returns>
        public RequirementsDay GetRequirementDay(DateTime day)
        {
            var validDay = ValidateDay(day);


            // Update earliest and latest days
            if (validDay > _latestDay)
                _latestDay = validDay;
            if (validDay < _earliestDay)
                _earliestDay = validDay;

            RequirementsDay reqDay = null;

            if (RequiredPieces.ContainsKey(validDay))
            {
                reqDay = RequiredPieces[validDay];
            }
            else
            {
                reqDay = new RequirementsDay();
                RequiredPieces[validDay] = reqDay;
            }

            return reqDay;
        }

        #endregion

        #region Static Fuctions

        /// <summary>
        /// Accessor for the requirements list
        /// </summary>
        /// <param name="masterID"></param>
        /// <returns></returns>
        public static ProductRequirements GetRequirements(int masterID)
        {
            return _allRequirements.FirstOrDefault(x => x.MasterID == masterID);
        }

        /// <summary>
        /// Adds all current inventory to the requirements. ONLY CALL ONCE. This is additive.
        /// </summary>
        public static void AddCurrentInventory()
        {
            foreach (var item in StaticInventoryTracker.AllInventoryItems)
            {
                var requirement = _allRequirements.FirstOrDefault(x => x.MasterID == item.MasterID);
                requirement?.AddOnHandInventory(DateTime.Today, (int) (item.Units*item.PiecesPerUnit));
            }
        }

        public static Queue<MakeOrder> GetMakeOrders(DateTime date)
        {

            foreach (var masterItem in StaticInventoryTracker.ProductMasterList)
            {
                var config =
                    ConfigurationsHandler.GetInstance()
                        .Configurations.FirstOrDefault(x => x.ItemOutID == masterItem.MasterID);
                if (config != null) // no config found.
                {
                    CreateProductRequirements(masterItem.MasterID, config);
                }
            }

            AddSalesUntilDate(date);

            AddCurrentInventory();

            // output the requirements
            OutputStringToFile();

            var orders = new Queue<MakeOrder>();

            DateTime current = DateTime.Today;

            while (current <= _latestDay)
            {
                foreach (var productRequirements in _allRequirements)
                {
                    if (productRequirements.RequiredPieces.ContainsKey(current))
                    {
                        var day = productRequirements.RequiredPieces[current];
                        if(day.PurchaseOrderPieces > 0)
                            orders.Enqueue(new MakeOrder(productRequirements.MasterID,day.PurchaseOrderPieces));
                    }
                }
                current = current.AddDays(1);
            }

            return orders;
        } 


        /// <summary>
        /// Adds all sales order dated up to the passed date.
        /// </summary>
        /// <param name="day"></param>
        public static void AddSalesUntilDate(DateTime day)
        {
            foreach (var salesItem in StaticInventoryTracker.SalesItems.Where(x => x.Date < day))
            {
                var requirement = _allRequirements.FirstOrDefault(x => x.MasterID == salesItem.MasterID);
                requirement?.AddSale(salesItem.Date,(int) (salesItem.Pieces));
            }
        }

        /// <summary>
        /// This function takes the product requirements and created an XML output to the passed file.
        /// </summary>
        /// <param name="filename"></param>
        public static void OutputStringToFile(string filename = "ProductRequirements.txt")
        {
            using (TextWriter writer = new StreamWriter(filename))
            {
                StringBuilder outputString = new StringBuilder();

                outputString.Append($"#### {"Product",-20} ####");

                if (_earliestDay < _latestDay)
                {
                    _numDays = 1;
                    DateTime current = _earliestDay;
                    while (current <= _latestDay)
                    {
                        _numDays++;
                        outputString.Append($" {current.ToShortDateString(),-10} #");
                        current = current.AddDays(1);
                    }
                }

                foreach (var productRequirements in _allRequirements)
                {
                    outputString.AppendLine();
                    productRequirements.OutputString(outputString);
                }

                writer.Write(outputString.ToString());
            }
            _numDays = 0;
        }

        private void OutputString(StringBuilder outputString)
        {
            var product = StaticInventoryTracker.ProductMasterList.FirstOrDefault(x => x.MasterID == MasterID);
            string name = product?.ProductionCode;

            outputString.Append($"#### {name,-20} ####");
            for (int i = 0; i < _numDays; ++i)
            {
                outputString.Append($" {" ",-10} #"); // keep columns
            }
            outputString.AppendLine(); // end product line

            StringBuilder grossBuilder = new StringBuilder();
            StringBuilder onHandBuilder = new StringBuilder();
            StringBuilder netBuilder = new StringBuilder();
            StringBuilder purchaseBuilder = new StringBuilder();

            grossBuilder.Append($"#    {"Gross",-20}    #");
            onHandBuilder.Append($"#    {"OnHand",-20}    #");
            netBuilder.Append($"#    {"Net",-20}    #");
            purchaseBuilder.Append($"#    {"POS",-20}    #");

            DateTime current = _earliestDay;
            while (current <= _latestDay)
            {
                RequirementsDay day = null;
                if (RequiredPieces.ContainsKey(current))
                    day = RequiredPieces[current];
                if (day != null)
                {
                    grossBuilder.Append($" {day.GrossPieces,-10} #");
                    onHandBuilder.Append($" {day.OnHandPieces,-10} #");
                    netBuilder.Append($" {day.NetRequiredPieces,-10} #");
                    purchaseBuilder.Append($" {day.PurchaseOrderPieces,-10} #");
                }
                else
                {
                    grossBuilder.Append($" {"0",-10} #");
                    onHandBuilder.Append($" {"0",-10} #");
                    netBuilder.Append($" {"0",-10} #");
                    purchaseBuilder.Append($" {"0",-10} #");
                }
                current = current.AddDays(1);
            }

            outputString.AppendLine(grossBuilder.ToString());
            outputString.AppendLine(onHandBuilder.ToString());
            outputString.AppendLine(netBuilder.ToString());
            outputString.AppendLine(purchaseBuilder.ToString());
        }

        #endregion  

    }
}
