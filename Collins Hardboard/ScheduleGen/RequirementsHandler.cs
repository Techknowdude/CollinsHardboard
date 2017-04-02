using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using Configuration_windows;
using ImportLib;
using ModelLib;
using StaticHelpers;

namespace ScheduleGen
{
    internal static class RequirementsHandler
    {

        public static int LeadTimeDays = 1;
        private static DateTime _earliestDay = DateTime.MaxValue;
        private static DateTime _latestDay = DateTime.MinValue;
        private static List<ProductRequirements> _allRequirements = new List<ProductRequirements>();
        private static int _numDays;

        public static void Register(ProductRequirements req)
        {
            _allRequirements.Add(req);
        }

        /// <summary>
        /// Used to make sure the dates used are all at the start of the day time.
        /// </summary>
        /// <param name="day"></param>
        /// <returns></returns>
        public static DateTime ValidateDay(DateTime day)
        {
            var validDay = new DateTime(day.Year, day.Month, day.Day);
            // Update earliest and latest days
            if (validDay > _latestDay)
                _latestDay = validDay;
            if (validDay < _earliestDay)
                _earliestDay = validDay;
            return validDay;
        }

        public static DateTime GetEarliestDay()
        {
            return _earliestDay;
        }


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

        public static List<MakeOrder> GetMakeOrders(DateTime date)
        {
            foreach (var masterItem in StaticInventoryTracker.ProductMasterList)
            {
                Configuration config = null;
                foreach (
                    var conGroup in MachineHandler.Instance.MachineList.SelectMany(machine => machine.ConfigurationList)
                    )
                {
                    config = conGroup.Configurations.FirstOrDefault(con => con.CanMake(masterItem));
                }
                    ProductRequirements.CreateProductRequirements(masterItem, config);
                if (config == null)
                {
                    StaticFunctions.OutputDebugLine("***ProductRequirements failed to find configuration for " +
                                                    masterItem);
                }
            }


            AddSalesUntilDate(date);

            //AddCurrentInventory();

            CalculateAllRequirements();

            // output the requirements
            OutputStringToFile();

            var orders = new List<MakeOrder>();

            DateTime current = _earliestDay;

            while (current <= _latestDay)
            {
                foreach (var productRequirements in _allRequirements)//.Where(req => req.MasterItem.MadeIn.ToUpper().Equals("COATING")))
                {
                    if (productRequirements.RequiredPieces.ContainsKey(current))
                    {
                        var day = productRequirements.RequiredPieces[current];
                        if (day.PurchaseOrderPieces > 0)
                        {
                            orders.Insert(0,
                                new MakeOrder(productRequirements.MasterItem.MasterID, day.PurchaseOrderPieces, current));
                        }
                    }
                }
                current = current.AddDays(1);
            }

            return orders;
        }

        private static void CalculateAllRequirements()
        {
            Queue<ProductRequirements> updateQueue = new Queue<ProductRequirements>(_allRequirements);

            // Update all master requirements
            while (updateQueue.Count > 0)
            {
                ProductRequirements currentRequirement = updateQueue.Dequeue();

                currentRequirement.UpdateInventory(_earliestDay);

                DateTime day = _earliestDay;

                while (day <= _latestDay)
                {
                    var reqDay = currentRequirement.GetRequirementDay(day);
                    if (reqDay.NetRequiredPieces > 0)
                    {
                        double leadHours = 0;
                        Configuration config =
                            MachineHandler.Instance.AllConfigurations.FirstOrDefault(
                                c => c.CanMake(currentRequirement.MasterItem));
                        if (config != null)
                        {
                            leadHours = config.HoursToMake(currentRequirement.MasterItem, reqDay.NetRequiredPieces/currentRequirement.MasterItem.PiecesPerUnit);
                        }

                        DateTime POday = day.AddDays(Math.Ceiling(leadHours/24 + LeadTimeDays));
                        var POreqDay = currentRequirement.GetRequirementDay(POday);
                        POreqDay.PurchaseOrderPieces = (reqDay.NetRequiredPieces);
                        // add gross for any dependent items
                        if (currentRequirement.Input != null)
                        {
                            foreach (var configItem in currentRequirement.Input)
                            {
                                var ratio = configItem.Pieces/currentRequirement.OutPieces;
                                var childReq = GetRequirements(configItem.MasterID);
                                var childDay = childReq.GetRequirementDay(POday);
                                childDay.AddGrossRequirement(ratio*reqDay.NetRequiredPieces);

                                // Add this child item to be updated
                                if(!updateQueue.Contains(childReq))
                                    updateQueue.Enqueue(childReq);
                            }
                        }
                    }
                    day = day.AddDays(1);
                }

            }
        }


        public static void UpdateGross(DateTime POdate, double change, ProductRequirements requirements)
        {
            DateTime grossDate = POdate.AddDays(-RequirementsHandler.LeadTimeDays);

            // get each item required to make this
            foreach (var configItem in requirements.Input)
            {
                var conversionRatio = requirements.OutPieces / configItem.Pieces;
                var prodRequirement = RequirementsHandler.GetRequirements(configItem.MasterID);

                if (prodRequirement == null)
                {
                    Configuration childConfiguration = MachineHandler.Instance.AllConfigurations.FirstOrDefault(conf => conf.CanMake(configItem.MasterItem));
                    prodRequirement = ProductRequirements.CreateProductRequirements(configItem.MasterItem, childConfiguration);
                }

                if (prodRequirement != null)
                {
                    var reqDay = prodRequirement.GetRequirementDay(grossDate);
                    reqDay.GrossPieces += (change * conversionRatio);
                }
            }
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
                requirement?.AddSale(salesItem.Date, (salesItem.TotalPieces));
            }
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


        public static DateTime GetLatestDay()
        {
            return _latestDay;
        }

        public static int GetNumDays()
        {
            return _numDays;
        }
    }
}