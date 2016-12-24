using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup.Localizer;
using System.Xml;
using Configuration_windows;
using ImportLib;
using ModelLib;
using StaticHelpers;

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

        #endregion

        /// <summary>
        /// Factory for product requirements
        /// </summary>
        /// <returns></returns>
        public static ProductRequirements CreateProductRequirements(ProductMasterItem master, Configuration usedConfig)
        {
            var req = new ProductRequirements(master);
            req.OutPieces = (int) (usedConfig.ItemsOut > 0 ? usedConfig.ItemsOut : 1);
            req.Input = usedConfig.InputItems.ToList();
            _allRequirements.Add(req);
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
                requirement = new RequirementsDay(this, validDate);
                RequiredPieces[validDate] = requirement;
                requirement.GrossPieces = pieces;
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
            var validDate = ValidateDay(_earliestDay);

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
                reqDay = new RequirementsDay(this,validDay);
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
            return _allRequirements.FirstOrDefault(x => x.MasterItem.MasterID == masterID);
        }

        /// <summary>
        /// Adds all current inventory to the requirements. ONLY CALL ONCE. This is additive.
        /// </summary>
        public static void AddCurrentInventory()
        {
            foreach (var item in StaticInventoryTracker.AllInventoryItems)
            {
                var requirement = _allRequirements.FirstOrDefault(x => x.MasterItem.MasterID == item.MasterID);
                requirement?.AddOnHandInventory((int) (item.Units*item.PiecesPerUnit));
            }
        }

        public static Queue<MakeOrder> GetMakeOrders(DateTime date, bool scheduleByWidth)
        {
            foreach (var masterItem in StaticInventoryTracker.ProductMasterList)
            {
                Configuration config = null;
                foreach (var conGroup in MachineHandler.Instance.MachineList.SelectMany(machine => machine.ConfigurationList))
                {
                    config = conGroup.Configurations.FirstOrDefault(con => con.CanMake(masterItem));
                }
                if (config != null) // config found.
                {
                    CreateProductRequirements(masterItem, config);
                }
                else
                {
                    StaticFunctions.OutputDebugLine("***ProductRequirements failed to find configuration for " + masterItem);
                }
            }


            AddSalesUntilDate(date);

            // output the requirements
            OutputStringToFile();

            var orders = new List<MakeOrder>();

            DateTime current = _earliestDay;

            while (current <= _latestDay)
            {
                foreach (var productRequirements in _allRequirements)
                {
                    if (productRequirements.RequiredPieces.ContainsKey(current))
                    {
                        var day = productRequirements.RequiredPieces[current];
                        if(day.PurchaseOrderPieces > 0)
                            orders.Insert(0, new MakeOrder(productRequirements.MasterItem.MasterID,day.PurchaseOrderPieces));
                    }
                }
                current = current.AddDays(1);
            }

            var queue = new Queue<MakeOrder>();
            var masterList = new List<ProductMasterItem>();
            masterList.AddRange(StaticInventoryTracker.ProductMasterList.Where(master => orders.Any(order => order.MasterID == master.MasterID)));

            if (scheduleByWidth)
            {
                while (orders.Any())
                {
                    // Match up widths by order they come in.
                    var currentOrder = orders.First();
                    var currentMaster = masterList.FirstOrDefault(master => master.MasterID == currentOrder.MasterID);
                    queue.Enqueue(currentOrder);
                    orders.RemoveAt(0);

                    if (currentMaster == null)
                    {
                        // can't make that
                        MessageBox.Show("No matching master for order. Master ID: " + currentOrder.MasterID + " for " +
                                        currentOrder.PiecesToMake + " pieces. Unable to schedule like widths.");
                    }
                    else
                    {
                        // get all other pending orders of same width and add them.
                        List<ProductMasterItem> otherMasters = masterList.Where(master => master.Width == currentMaster.Width).ToList();
                        
                        if(otherMasters.Count == 0) continue;

                        List<MakeOrder> matchingOrders =
                            orders.Where(order => otherMasters.Any(master => master.MasterID == order.MasterID)).ToList();
                        foreach (var matchingOrder in matchingOrders)
                        {
                            // queue the orders by previous priority, grouped by width
                            queue.Enqueue(matchingOrder);
                            // remove order so you don't double schedule
                            orders.Remove(matchingOrder);
                        }
                    }
                }
            }
            else
            {
                foreach (var makeOrder in orders)
                {
                    queue.Enqueue(makeOrder);
                }
            }

            return queue;
        }


        /// <summary>
        /// Adds all sales order dated up to the passed date.
        /// </summary>
        /// <param name="day"></param>
        public static void AddSalesUntilDate(DateTime day)
        {
            foreach (var salesItem in StaticInventoryTracker.SalesItems.Where(x => x.Date < day))
            {
                var requirement = _allRequirements.FirstOrDefault(x => x.MasterItem.MasterID == salesItem.MasterID);
                requirement?.AddSale(salesItem.Date,(salesItem.Units));
            }
            AddCurrentInventory();
        }

        /// <summary>
        /// This function takes the product requirements and created an XML output to the passed file.
        /// </summary>
        /// <param name="filename"></param>
        public static void OutputStringToFile(string filename = "ProductRequirements.csv")
        {
            using (TextWriter writer = new StreamWriter(filename))
            {
                StringBuilder outputString = new StringBuilder();

                outputString.Append($"{"Product",-20},");

                if (_earliestDay < _latestDay)
                {
                    _numDays = 1;
                    DateTime current = _earliestDay;
                    while (current <= _latestDay)
                    {
                        _numDays++;
                        outputString.Append($",{current.ToShortDateString(),-10}");
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
            var product = StaticInventoryTracker.ProductMasterList.FirstOrDefault(x => x.MasterID == MasterItem.MasterID);
            string name = product?.ProductionCode;

            outputString.Append($"{name}");
            for (int i = 0; i < _numDays; ++i)
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

            DateTime current = _earliestDay;
            while (current <= _latestDay)
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

        #endregion

        public void UpdateNextInventory(DateTime day, double change)
        {
            RequirementsDay nextDay;
            if (day.AddDays(1) <= _latestDay)
            {
                nextDay = GetRequirementDay(day.AddDays(1));
                nextDay.Inventory += change;
            }

        }

        public void UpdateGross(DateTime POdate, double change)
        {
            DateTime grossDate = POdate.AddDays(-LeadTimeDays);
            
            // get each item required to make this
            foreach (var configItem in Input)
            {
                var prodRequirement =
                    _allRequirements.FirstOrDefault(req => req.MasterItem.Equals(configItem.MasterItem));
                if (prodRequirement == null)
                {
                    Configuration childConfiguration = MachineHandler.Instance.AllConfigurations.FirstOrDefault(conf => conf.CanMake(configItem.MasterItem));
                    if (childConfiguration != null)
                    {
                        prodRequirement = CreateProductRequirements(configItem.MasterItem, childConfiguration);
                    }
                }

                if(prodRequirement != null)
                {
                    var reqDay = prodRequirement.GetRequirementDay(grossDate);
                    reqDay.GrossPieces += change;
                }
            }
        }

        public void SetInventory(double pieces)
        {
            var day = GetRequirementDay(_earliestDay);
            day.Inventory = pieces;
        }

        public void SetEarlistDate(DateTime day)
        {
            var validatedDate = ValidateDay(day);
            _earliestDay = validatedDate;
        }
    }
}
