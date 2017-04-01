using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Xml.Serialization;
using ImportLib;
using ModelLib;
using CoatingScheduler;
using Configuration_windows;
using ProductionScheduler;
using StaticHelpers;

namespace ScheduleGen
{
    public class ScheduleGenerator
    {
        private static ScheduleGenerator _inner = null;

        public static ScheduleGenerator Instance
        {
            get
            {
                if (_inner == null)
                    _inner = new ScheduleGenerator();
                return _inner;
            }
        }

        CoatingScheduleDay scheduleDay;
        CoatingScheduleLine scheduleLine;
        CoatingScheduleShift scheduleShift;
        CoatingSchedule schedule = new CoatingSchedule();
        private bool added;
        private SalesPrediction.SalesDurationEnum duration;
        static double avgSold;

        public DateTime SalesOutlook { get; set; }
        public DateTime StartGen { get; set; }
        public DateTime EndGen { get; set; }

        private ScheduleGenWindow _window;
        private DateTime _currentDay = DateTime.Today;
        public const string datFile = "ScheduleGenSettings.dat";
        List<InventoryItem> currentInventoryItems = new List<InventoryItem>();
        public List<InventoryItem> CurrentInventory { get { return currentInventoryItems; } }

        public DateTime CurrentDay
        {
            get { return _currentDay; }
            set { _currentDay = value; }
        }

        public ScheduleGenWindow Window
        {
            get { return _window; }
            set { _window = value; }
        }
        private Queue<String> _errors = new Queue<string>();

        internal GenerationData GenerationData;

        public void GenerateSchedule(DateTime salesOutlook, DateTime startGen, DateTime endGen,
            bool testing = false)
        {
            if (GenerationData == null)
                GenerationData = new GenerationData();
            GenerationData.Reset();

            if (StaticInventoryTracker.ProductMasterList.Count == 0)
            {
                MessageBox.Show("No master loaded. Please load master before generating schedule.");
                return;
            }

            if (MachineHandler.Instance.MachineList.Count == 0)
            {
                MessageBox.Show("No machines configured/loaded. Please load or configure the plant machines before generating schedule.");
                return;
            }

            currentInventoryItems.Clear();

            foreach (var allInventoryItem in StaticInventoryTracker.AllInventoryItems)
            {
                currentInventoryItems.Add(new InventoryItem(allInventoryItem.ProductCode, allInventoryItem.Units,
                    allInventoryItem.PiecesPerUnit, allInventoryItem.Grade, allInventoryItem.MasterID,
                    allInventoryItem.InventoryItemID));
            }

            try
            {
                SalesOutlook = salesOutlook;
                StartGen = startGen;
                EndGen = endGen;

                CoatingSchedule.CurrentSchedule?.ChildrenLogic.Clear();

                CoatingScheduleWindow scheduleWindow = new CoatingScheduleWindow(schedule);
                scheduleWindow.Show();
                PressScheduleWindow pressScheduleWindow = new PressScheduleWindow();
                pressScheduleWindow.Show();
                PressManager.PlateConfigurations.Clear();
                pressScheduleWindow.UpdateControls();


                // initialize with current waste, line, and width
                CurrentDay = StartGen;

                // add the first shift to the schedule
                schedule.AddLogic();
                scheduleDay = (CoatingScheduleDay)schedule.ChildrenLogic[0];
                scheduleDay.Date = CurrentDay;
                scheduleDay.AddLogic();
                scheduleLine = (CoatingScheduleLine)scheduleDay.ChildrenLogic[0];

                // get a list of all sales orders
                GenerationData.SalesList = RequirementsHandler.GetMakeOrders(SalesOutlook);

                // get list of items that can be made in the coating plant
                List<ProductMasterItem> masterItemsAvailableToMake =
                    StaticInventoryTracker.ProductMasterList.Where(
                        m => m.MadeIn.ToUpper().Equals("COATING")).ToList();

                // While we have not reached the end of the schedule
                bool moveToNext = false;

                while (CurrentDay <= EndGen)
                {
                    // TODO main loop for generation from flowchart

                    // attempt to schedule the highest priority item until the line is full. 
                    while (GenerationData.ScheduledItem.Any(s => !s.Value) && !LineIsFull())
                    {
                        foreach (var coatingLine in StaticFactoryValuesManager.CoatingLines)
                        {
                            if (moveToNext) break;

                            // Get item with highest priority
                            GenerationData.PriorityList = new List<PriorityItem>();
                            foreach (var productMasterItem in masterItemsAvailableToMake)
                            {
                                GenerationData.PriorityList.Add(Evaluator.Evaluate(productMasterItem, coatingLine));
                            }
                            int itemIndex = 0;

                            PriorityItem currentItem = GenerationData.PriorityList[itemIndex];

                            if (currentItem.Item.MadeIn.ToUpper().Equals("COATING"))
                            {
                                ScheduleCoating(currentItem.Item, coatingLine);
                            }
                            else if (currentItem.Item.MadeIn.ToUpper().Equals("PRESS"))
                            {
                                // Error. Press items should not be in the list.
                                MessageBox.Show("ERROR: Attempted to schedule item that cannot be made in coating");
                            }
                            else
                            {
                                // error
                                MessageBox.Show(
                                    "ERROR: Attempted to schedule item that cannot be made in coating or press");
                                throw new Exception("ERROR: Could not make item " + currentItem.Item +
                                                    " as it is not made in press or coating.");
                            }
                            // move to next item
                            ++itemIndex;

                            // Check if all the lines have been scheduled or if we have tried all the items and none can be scheduled, move on
                            if (GenerationData.ScheduledItem.All(s => s.Value) ||
                                itemIndex >= GenerationData.PriorityList.Count)
                            {
                                AddControl();
                                moveToNext = true;
                            }
                        }
                    }
                }
                scheduleWindow.LoadTrackingItems();
                pressScheduleWindow.UpdateControls();

                while (_errors.Count > 0)
                {
                    MessageBox.Show(_errors.Dequeue());
                }

                MessageBox.Show("Schedule done generating");

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private void SchedulePress(ProductMasterItem currentItem, double unitsRequired)
        {
            var scheduled = PressManager.ScheduleItem(currentItem , unitsRequired, scheduleLine.Date);

            // update waste
            if (scheduled)
            {
                GenerationData.CurrentWaste += currentItem.Waste * unitsRequired;
            }
        }

        /// <summary>
        /// Attempt to schedule an item on the coating line.
        /// </summary>
        /// <param name="currentItem"></param>
        /// <param name="coatingLine"></param>
        private bool ScheduleCoating(ProductMasterItem currentItem, string coatingLine)
        {
            Configuration config;
            double unitsToMake;
            bool itemMade = false;

                var bestMachine = GetBestMachine(currentItem, coatingLine);

                // if the item can be made
                if (bestMachine != null)
                {
                    config = bestMachine.Item2.Configurations.FirstOrDefault(c => c.CanMake(currentItem));
                    if (config != null)
                    {
                        unitsToMake = GetUnitsToMake(currentItem, bestMachine.Item1);

                        if (unitsToMake > 0)
                        {
                            // Check for required inventory
                            if (HasPrerequisites(config, unitsToMake))
                            {
                                // If inventory, schedule item
                                GenerationData.ScheduledItem[coatingLine] = ScheduleCoating(currentItem, unitsToMake, bestMachine, config);
                            }
                            else
                            {
                                // Else, select the highest priority item needed and schedule that
                                GenerationData.ScheduledItem[coatingLine] = SchedulePrerequisite(config, unitsToMake,coatingLine);
                            }
                            if (GenerationData.ScheduledItem[coatingLine])
                            {
                                // mark the machine as used
                                GenerationData.LastRunMachine[bestMachine.Item1] = new ConfigTime(bestMachine.Item2, scheduleLine.Date);
                            }
                        }
                    }

                // if the item was made on a line, mark as made and exit.
                if (GenerationData.ScheduledItem[coatingLine])
                {
                    itemMade = true;
                }
            }
            return itemMade;
        }

        private bool LineIsFull()
        {
            return scheduleLine.IsFull();
        }


        private List<MakeOrder> GetPredictions()
        {
            var predictions = new List<MakeOrder>();

            foreach (var master in StaticInventoryTracker.ProductMasterList)//.Where(p => p.MadeIn.Equals("Coating")))
            {
                double pieces = master.PiecesPerUnit * master.TargetSupply;

                StaticFunctions.OutputDebugLine("Creating new prediction for " + master);
                MakeOrder newOrder = new MakeOrder(master.MasterID, pieces) { DueDay = CurrentDay }; // assume the current day is the due date unless we have inventory data (Could have no inventory)
                // forecast out when the order should be due
                var inv = Instance.currentInventoryItems.FirstOrDefault(i => i.MasterID == master.MasterID);
                if (inv != null)
                {
                    double currentInv = inv.Units;
                    double usedPerDay = GetAvgUnitsPerDay(master) * 30;
                    int daysTillOut = (int)Math.Floor(currentInv / usedPerDay);
                    newOrder.DueDay = CurrentDay.AddDays(daysTillOut);
                    StaticFunctions.OutputDebugLine("Found inventory of " + currentInv + " for prediction " + master + " predicted to run out in " + daysTillOut + " days");
                }
                predictions.Add(newOrder);
            }

            return predictions;
        }

        /// <summary>
        /// Schedule the item with the given config.
        /// </summary>
        /// <param name="nextItem"></param>
        /// <param name="unitsToMake"></param>
        /// <param name="scheduleInfo"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        private bool ScheduleCoating(ProductMasterItem nextItem, double unitsToMake, Tuple<Machine, ConfigurationGroup, int> scheduleInfo, Configuration config)
        {
            Machine machine = scheduleInfo.Item1;
            int lineIndex = scheduleInfo.Item3;

            if (machine != null) // if the item can be made currently
            {
                //set shift to add to
                scheduleShift = (CoatingScheduleShift)scheduleLine.ChildrenLogic[lineIndex];

                if (unitsToMake > 0)
                {
                    var unitsMade = scheduleShift.ScheduleItem(machine, config, nextItem, unitsToMake);

                    var inv = currentInventoryItems.FirstOrDefault(i => i.MasterID == nextItem.MasterID);
                    if (inv != null)
                    {
                        inv.Units += unitsMade;
                    }
                    else
                    {
                        currentInventoryItems.Add(new InventoryItem(nextItem, unitsMade, "Dealer"));
                    }

                    // Remove the sale from the generation data.
                    var soldPcs = (int)unitsMade * nextItem.PiecesPerUnit;

                    var sales = GenerationData.SalesList.Where(s => s.MasterID == nextItem.MasterID);
                    foreach (var makeOrder in sales)
                    {
                        if (soldPcs <= 0) break;

                        // this is the only (or last sale to fill)
                        if (soldPcs <= makeOrder.PiecesToMake)
                        {
                            makeOrder.PiecesToMake -= soldPcs;
                        }
                        else
                        {
                            // order filled. Remove it.
                            soldPcs -= makeOrder.PiecesToMake;
                            GenerationData.SalesList.Remove(makeOrder);
                        }
                    }

                    added = true;
                    GenerationData.MarkItemScheduled(nextItem,lineIndex,unitsMade);
                    return true;
                }
            }
            return false;
        }


        private bool SchedulePrerequisite(Configuration config, double unitsNeeded, string coatingLine)
        {
            double unitsRequired = unitsNeeded;
            bool scheduled = false;
            var inv = currentInventoryItems.FirstOrDefault(i => i.MasterID == config.ItemInID);
            if (inv != null)
            {
                double unitsAvailable = inv.Units;
                unitsRequired = unitsNeeded * (config.ItemsIn / (double)config.ItemsOut);
                unitsRequired -= unitsAvailable;
            }

            ProductMasterItem prevItem =
                StaticInventoryTracker.ProductMasterList.FirstOrDefault(m => m.MasterID == config.ItemInID);

            if (prevItem == null) return false;

            // try to schedule item
            if (prevItem.MadeIn == "Coating")
            {
                scheduled = ScheduleCoating(prevItem, coatingLine);
            }
            else if (prevItem.MadeIn == "Press")
            {
                SchedulePress(prevItem,unitsRequired);
            }

            return scheduled;
        }

        private bool HasPrerequisites(Configuration config, double unitsToMake)
        {
            bool hasEnough = false;
            var inv = currentInventoryItems.FirstOrDefault(i => i.MasterID == config.ItemInID);
            if (inv != null)
            {
                // if there are enough units of the required item  
                double unitsAvailable = inv.Units;
                double unitsRequired = unitsToMake * (config.ItemsIn / (double)config.ItemsOut);
                hasEnough = unitsAvailable >= unitsRequired;

                // if there is enough inventory to make it, it will be made, so remove the used inventory
                if (hasEnough)
                {
                    inv.Units -= (Math.Min(unitsAvailable, unitsRequired));
                }
            }
            return hasEnough;
        }

        private double GetAvgUnitsPerDay(ProductMasterItem nextItem)
        {
            double unitUsage = 0;
            var forecast = StaticInventoryTracker.ForecastItems.FirstOrDefault(f => f.MasterID == nextItem.MasterID);
            if (forecast != null)
            {
                switch (duration)
                {
                    case SalesPrediction.SalesDurationEnum.LastMonth:
                        unitUsage = forecast.AvgOneMonth / 30;
                        break;
                    case SalesPrediction.SalesDurationEnum.Last3Months:
                        unitUsage = forecast.AvgThreeMonths / 30;
                        break;
                    case SalesPrediction.SalesDurationEnum.Last6Months:
                        unitUsage = forecast.AvgSixMonths / 30;
                        break;
                    case SalesPrediction.SalesDurationEnum.Last12Months:
                        unitUsage = forecast.AvgTwelveMonths / 30;
                        break;
                    case SalesPrediction.SalesDurationEnum.LastYear:
                        unitUsage = forecast.AvgPastYear / 30;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return unitUsage;
        }

        public Tuple<Machine, ConfigurationGroup, int> GetBestMachine(ProductMasterItem nextItem, string lineToRunOn)
        {
            Machine bestMachine = null;
            int lineIndex = 0;
            ConfigurationGroup configGroup = null;

            List<Machine> otherMachines = new List<Machine>();
            foreach (var shiftLogic in scheduleLine.ChildrenLogic)
            {
                otherMachines.AddRange(from proLogic in shiftLogic.ChildrenLogic select proLogic as CoatingScheduleProduct into prod where prod != null && prod.Machine != null select prod.Machine);
            }

            // get machines that can make
            List<Machine> machines = MachineHandler.Instance.MachineList.Where(machine => machine.LinesCanRunOn.Any(l => l.Equals(lineToRunOn)) &&
                machine.ConfigurationList.Any(conf => conf.CanMake(nextItem))).ToList();

            if (machines.Count == 0)
            {
                if (!_errors.Contains($"No machine can make {nextItem}. Please add a configuration to make this item to a machine."))
                    _errors.Enqueue(
                    $"No machine can make {nextItem}. Please add a configuration to make this item to a machine.");
            }



            foreach (var machine in machines)
            {
                if (otherMachines.Any(mac => mac.MachineConflicts.Contains(machine.Name))) continue; // if other machines conflict with this, continue
                if (otherMachines.Any(otherMachine => machine.MachineConflicts.Contains(otherMachine.Name))) continue; // if this machine conflicts with others, skip

                // remove this machine if line conflicts
                bool conflicted = scheduleLine.ChildrenLogic.Any(shift => shift.ChildrenLogic.Any(x => x is CoatingScheduleProduct) && machine.LineConflicts.Contains(shift.CoatingLine));
                if (conflicted) continue;

                bestMachine = machine;
                //get option that works
                lineIndex = StaticFactoryValuesManager.CoatingLines.IndexOf(lineToRunOn);
                var groups = machine.GetConfigGroups(nextItem);
                if (GenerationData.LastRunMachine.ContainsKey(bestMachine))
                {
                    var configTime = GenerationData.LastRunMachine[bestMachine];
                    // check if machine config is not the same
                    if (!groups.Any(c => c.Equals(configTime.Group)))
                    {
                        // check if the machine will be ready on time
                        if (configTime.Time <=
                            StaticFunctions.GetDayAndTime(scheduleLine.Date, scheduleLine.Shift.StartTime))
                        {
                            configGroup = configTime.Group;
                            break;
                        }
                        else
                        {
                            // can't use the config yet - not ready
                            StaticFunctions.OutputDebugLine(
                                $"Machine {machine.Name} can't be used. It is unavailable until {GenerationData.LastRunMachine[machine].Time /*+ config.ChangeTime*/}. It is needed by {nextItem.Description} by {StaticFunctions.GetDayAndTime(scheduleLine.Date, scheduleLine.Shift.StartTime)}");
                            bestMachine = null;
                            lineIndex = 0;
                            configGroup = null;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                else // no last use. No change time assumed.
                {
                    break;
                }
                
                // if machine can make, and shift is not full, add to shift
            }

            return new Tuple<Machine, ConfigurationGroup, int>(bestMachine, configGroup, lineIndex);
        }

        public bool LoadSettings(string fileName = datFile)
        {
            return LoadFileData(fileName, _window);
        }

        private bool LoadFileData(String file, ScheduleGenWindow window)
        {
            try
            {
                //using (BinaryReader reader = new BinaryReader(new FileStream(file, FileMode.Open)))
                //{
                //    ControlsList.Clear();
                //    int num = reader.ReadInt32();
                //    for (; num > 0; --num)
                //    {
                //        var genControl = GenControl.LoadControl(reader, window);
                //        ControlsList.Add(genControl);
                //    }
                //}

                return true;
            }
            catch (Exception exception)
            {
                return false;
            }
        }

        public bool SaveSettings()
        {
            return SaveFileData(datFile);
        }

        private bool SaveFileData(String fileName)
        {
            try
            {
                //using (BinaryWriter writer = new BinaryWriter(new FileStream(fileName, FileMode.OpenOrCreate)))
                //{
                //    writer.Write(ControlsList.Count);
                //    foreach (var control in ControlsList)
                //    {
                //        var gen = control;
                //        Debug.Assert(gen != null, "genControl save != null");
                //        gen.Save(writer);
                //    }
                //}
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public double GetUnitsToMake(ProductMasterItem nextItem, Machine machine)
        {
            double unitsToMake = 0;
            double maxInShift = 0;

            Configuration config = null;

            foreach (var configurationGroup in machine.ConfigurationList)
            {
                foreach (var configuration in configurationGroup.Configurations)
                {
                    config = config.GetFastestConfig(configuration, nextItem);
                }
            }

            if (config != null)
                maxInShift = config.UnitsToMakeInHours(nextItem, (scheduleLine.Shift.Hours(scheduleLine.Date)));

            var inventory = StaticInventoryTracker.InventoryItems.FirstOrDefault(inv => inv.MasterID == nextItem.MasterID);
            double currentInv = inventory?.Units ?? 0;

            double max = double.MaxValue;
            double min = 0;
            double target = 0;

            if (nextItem.TurnType == "T")
            {
                ForecastItem forecast = StaticInventoryTracker.ForecastItems.FirstOrDefault(forcast => forcast.MasterID == nextItem.MasterID);
                if (forecast != null)
                {
                    switch (duration)
                    {
                        case SalesPrediction.SalesDurationEnum.LastMonth:
                            avgSold = forecast.AvgOneMonth;
                            break;
                        case SalesPrediction.SalesDurationEnum.Last3Months:
                            avgSold = forecast.AvgThreeMonths;
                            break;
                        case SalesPrediction.SalesDurationEnum.Last6Months:
                            avgSold = forecast.AvgSixMonths;
                            break;
                        case SalesPrediction.SalesDurationEnum.Last12Months:
                            avgSold = forecast.AvgTwelveMonths;
                            break;
                        case SalesPrediction.SalesDurationEnum.LastYear:
                            avgSold = forecast.AvgPastYear;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    if (avgSold == 0)
                        avgSold = 40; // do this to prevent infinite looping


                    target = avgSold * nextItem.TargetSupply; // avg sold * months supply

                    max = avgSold * nextItem.MaxSupply;
                    min = avgSold * nextItem.MinSupply;

                    // if target is too low
                    if (target + currentInv < min)
                    {
                        unitsToMake = min;
                    }
                    // if target is too high
                    else if (target + currentInv > max)
                    {
                        unitsToMake = max;
                    }
                    // if target is acceptable
                    else
                    {
                        unitsToMake = target;
                    }

                    // fill shift if possible
                    if (unitsToMake < maxInShift && unitsToMake < max)
                    {
                        unitsToMake = maxInShift;
                    }
                }
            }
            else // create by units
            {
                max = nextItem.MaxSupply;
                min = nextItem.MinSupply;
                target = nextItem.TargetSupply;
                if (target == 0)
                {
                    unitsToMake = // run to order. Get sum of orders
                        StaticInventoryTracker.SalesItems.Where(sale => sale.MasterID == nextItem.MasterID && sale.Date < SalesOutlook && sale.Units - sale.Fulfilled < 1).Sum(sale => sale.Units - sale.Fulfilled);
                }
                else
                {
                    if (target + currentInv < min) // target too small
                    {
                        unitsToMake = min;
                    }
                    else if (target + currentInv > max) // target too big
                    {
                        unitsToMake = max;
                    }
                    else
                    {
                        unitsToMake = target;
                    }

                    // Make enough to fill the shift by default

                    double make = unitsToMake;

                    do
                    {
                        make -= nextItem.UnitsPerHour * 8; // 8 hours in a shift by default

                        if (make < 0)
                        {
                            unitsToMake += -(make); // add enough to cover shift.
                        }
                    } while (make > 0);
                }
            }

            if (unitsToMake > maxInShift)
                unitsToMake = maxInShift;
            // try to fill shift
            if (unitsToMake < maxInShift && unitsToMake < max)
                unitsToMake = maxInShift;

            return (int)unitsToMake; // round to nearest unit
        }

        private void AddControl()
        {
            DateTime beforeDay = CurrentDay;

            if (scheduleDay.CanAddShift()) // if all shifts not used.
            {
                scheduleDay.AddLogic();
                scheduleLine = (CoatingScheduleLine)scheduleDay.ChildrenLogic.Last(); // add a shift
            }
            else
            {
                // advance a day and remove expected inventory
                CurrentDay = ShiftHandler.CoatingInstance.GetNextWorkingDay(CurrentDay);

                schedule.AddLogic();
                scheduleDay = (CoatingScheduleDay)schedule.ChildrenLogic.Last();
                scheduleDay.Date = CurrentDay;
                scheduleDay.AddLogic();
                scheduleLine = (CoatingScheduleLine)scheduleDay.ChildrenLogic.Last();
            }

            // decrement inventory
            int daysAdded = (CurrentDay - beforeDay).Days;
            if (daysAdded > 0)
                DecrementInventory(daysAdded);
            
            // reset the state for the next shift
            GenerationData.ResetForNextShift();
        }

        private void DecrementInventory(int dayDif)
        {
            // update inventory by lowering it by the number of days past
            foreach (var inventoryItem in CurrentInventory)
            {
                var forecast = StaticInventoryTracker.ForecastItems.FirstOrDefault(f => f.MasterID == inventoryItem.MasterID);
                ProductMasterItem master = StaticInventoryTracker.ProductMasterList.FirstOrDefault(m => m.MasterID == inventoryItem.MasterID);
                if (forecast != null && master != null)
                {
                    try
                    {
                        double turnUnits = 0;
                        switch (duration)
                        {
                            case SalesPrediction.SalesDurationEnum.LastMonth:
                                inventoryItem.Units -= forecast.AvgOneMonth / 30 * dayDif;
                                turnUnits = forecast.AvgOneMonth;
                                break;
                            case SalesPrediction.SalesDurationEnum.Last3Months:
                                inventoryItem.Units -= forecast.AvgThreeMonths / 30 * dayDif;
                                turnUnits = forecast.AvgThreeMonths;
                                break;
                            case SalesPrediction.SalesDurationEnum.Last6Months:
                                inventoryItem.Units -= forecast.AvgSixMonths / 30 * dayDif;
                                turnUnits = forecast.AvgSixMonths;
                                break;
                            case SalesPrediction.SalesDurationEnum.Last12Months:
                                inventoryItem.Units -= forecast.AvgTwelveMonths / 30 * dayDif;
                                turnUnits = forecast.AvgTwelveMonths;
                                break;
                            case SalesPrediction.SalesDurationEnum.LastYear:
                                inventoryItem.Units -= forecast.AvgPastYear / 30 * dayDif;
                                turnUnits = forecast.AvgPastYear;
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                        // check if the master item should be triggered to be scheduled.
                        if (master.TurnType == "T")
                        {
                            if (turnUnits <= 0)
                                turnUnits = 1;

                            double turns = inventoryItem.Units / turnUnits;
                            if (master.MinSupply < turns)
                            {
                                GenerationData.AddPredictionOrder(master);
                            }
                        }
                        else
                        {
                            if (inventoryItem.Units < master.MinSupply)
                            {
                                GenerationData.AddPredictionOrder(master);
                                
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
            }

        }

        public DateTime GetSalesRange()
        {
            return SalesOutlook;
        }
    }
}