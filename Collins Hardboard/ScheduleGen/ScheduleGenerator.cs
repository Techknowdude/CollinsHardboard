using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using ImportLib;
using ModelLib;
using CoatingScheduler;
using Configuration_windows;
using ProductionScheduler;
using StaticHelpers;

namespace ScheduleGen
{
    public static class ScheduleGenerator
    {
        static CoatingScheduleDay scheduleDay;
        static CoatingScheduleLine scheduleLine;
        static CoatingScheduleShift scheduleShift;
        static CoatingSchedule schedule = new CoatingSchedule();
        private static bool added;
        private static SalesPrediction.SalesDurationEnum duration;
        static double avgSold;

        public static DateTime SalesOutlook { get; set; }
        public static DateTime StartGen { get; set; }
        public static DateTime EndGen { get; set; }

        private static ScheduleGenWindow _window;
        private static ObservableCollection<GenControl> _controlsList = new ObservableCollection<GenControl>();
        private static DateTime _currentDay = DateTime.Today;
        public const string datFile = "ScheduleGenSettings.dat";
        static List<InventoryItem> currentInventoryItems = new List<InventoryItem>();
        public static List<InventoryItem> CurrentInventory { get { return currentInventoryItems; } } 

        public static DateTime CurrentDay
        {
            get { return _currentDay; }
            set { _currentDay = value; }
        }

        public static List<ProductMasterItem> ProductItems { get; set; }

        public static ObservableCollection<GenControl> ControlsList
        {
            get { return _controlsList; }
            set { _controlsList = value; }
        }

        public static ScheduleGenWindow Window
        {
            get { return _window; }
            set { _window = value; }
        }

        public static List<ProductMasterItem> ScheduledItems { get; set; }
        public static double CurrentWaste { get; set; }
        public static double LastWidth { get; set; }
        public static String CurrentLine { get; set; }
        private static Dictionary<ProductMasterItem, double> _fulfilled;

        public static void GenerateSalesSchedule(DateTime salesOutlook, DateTime startGen, DateTime endGen, bool testing = false)
        {
            currentInventoryItems.Clear();

            foreach (var allInventoryItem in StaticInventoryTracker.AllInventoryItems)
            {
                currentInventoryItems.Add(new InventoryItem(allInventoryItem.ProductCode, allInventoryItem.Units, allInventoryItem.PiecesPerUnit, allInventoryItem.Grade, allInventoryItem.MasterID, allInventoryItem.InventoryItemID));
            }

            if (StaticInventoryTracker.ProductMasterList.Count == 0)
            {
                MessageBox.Show("No master loaded. Please load master before generating schedule.");
                return;
            }

            _fulfilled = new Dictionary<ProductMasterItem, double>();

                SalesOutlook = salesOutlook;
                StartGen = startGen;
                EndGen = endGen;

            ProductItems = new List<ProductMasterItem>();
            ScheduledItems = new List<ProductMasterItem>();
            CoatingScheduleWindow scheduleWindow = new CoatingScheduleWindow(schedule);
            scheduleWindow.Show();

            // get list of all items that are made
            ProductItems.AddRange(StaticInventoryTracker.ProductMasterList.Where(x => x.MadeIn == "Coating"));

            // initialize with current waste, line, and width
            CurrentWaste = StaticFactoryValuesManager.CurrentWaste;
            CurrentDay = StartGen;
            LastWidth = ProductItems.Max(x => x.Width);
            // for each item, get highest priority for that 

            schedule.AddLogic();
            scheduleDay = (CoatingScheduleDay) schedule.ChildrenLogic[0];
            scheduleDay.Date = CurrentDay;
            scheduleDay.AddLogic();
            scheduleLine = (CoatingScheduleLine) scheduleDay.ChildrenLogic[0];


                var orders = ProductRequirements.GetMakeOrders(SalesOutlook);

                while (CurrentDay <= EndGen && orders.Count > 0)
                {
                    var nextOrder = orders.Peek();

                    while (nextOrder != null && nextOrder.PiecesToMake < 1) // get rid of empty orders
                    {
                        orders.Dequeue();
                        nextOrder = orders.Peek();
                    }

                    if(nextOrder == null) continue;

                    ProductMasterItem nextItem =
                        StaticInventoryTracker.ProductMasterList.FirstOrDefault(x => x.MasterID == nextOrder.MasterID);
                    
                    Tuple<Machine, Configuration, int> machineLine = GetBestMachine(nextItem);

                    Machine machine = machineLine.Item1;
                    int lineIndex = machineLine.Item3;
                    Configuration config = machineLine.Item2;

                    if (machine != null) // if the item can be made currently
                    {
                        //set shift to add to
                        scheduleShift = (CoatingScheduleShift)scheduleLine.ChildrenLogic[lineIndex];

                        double unitsToMake = nextOrder.PiecesToMake/nextItem.PiecesPerUnit;

                        if (Math.Abs(unitsToMake % nextItem.UnitsPerHour*8) > 0.0000001) // if not filling a shift, add enough to do so
                        {
                            unitsToMake += nextItem.UnitsPerHour*8 - unitsToMake%(nextItem.UnitsPerHour*8);
                        }

                        if (unitsToMake > 0)
                        {
                            var unitsMade = scheduleShift.ScheduleItem(machine, config, nextItem, double.MaxValue);
                            var takeOff = unitsMade;
                            foreach (var makeOrder in orders.Where(m => m.MasterID == nextItem.MasterID))
                            {
                                if (takeOff > makeOrder.PiecesToMake/nextItem.PiecesPerUnit)
                                {
                                    takeOff = takeOff - makeOrder.PiecesToMake/nextItem.PiecesPerUnit;
                                    makeOrder.PiecesToMake = 0; // remove this
                                }
                                else
                                {
                                    makeOrder.PiecesToMake -= (int)takeOff*nextItem.PiecesPerUnit;
                                }
                            }

                            // factor in waste created
                            CurrentWaste += nextItem.Waste * unitsMade;

                            if (unitsMade*nextItem.PiecesPerUnit >= nextOrder.PiecesToMake)// change to nextOrder.PiecesToMake == 0
                                orders.Dequeue();
                            else
                            {
                                Console.WriteLine("Not done making this...");
                            }

                        }

                        AddControl(); // All production uses up at least one shift.
                    }
                    else
                    {
                        orders.Dequeue(); // skips things we can't make. This is most likely an error by the operator's configuration of the plant.
                    }

                }
            
            scheduleWindow.LoadTrackingItems();
            MessageBox.Show("Schedule done generating"); 
        }

        public static void GeneratePredictionSchedule(DateTime salesOutlook,DateTime startGen,DateTime endGen,bool testing = false)
        {
            currentInventoryItems.Clear();

            foreach (var allInventoryItem in StaticInventoryTracker.AllInventoryItems)
            {
                currentInventoryItems.Add(new InventoryItem(allInventoryItem.ProductCode,allInventoryItem.Units,allInventoryItem.PiecesPerUnit,allInventoryItem.Grade,allInventoryItem.MasterID,allInventoryItem.InventoryItemID));
            }

            if (StaticInventoryTracker.ProductMasterList.Count == 0)
            {
                MessageBox.Show("No master loaded. Please load master before generating schedule.");
                return;
            }
            _fulfilled = new Dictionary<ProductMasterItem, double>();
            duration = SalesPrediction.SalesDurationEnum.LastYear;

            var prediction = ControlsList.FirstOrDefault(control => control is SalesPrediction) as SalesPrediction;

           
            duration = prediction.SalesDuration;

            SalesOutlook = salesOutlook;
            StartGen = startGen;
            EndGen = endGen;


            ProductItems = new List<ProductMasterItem>();
            var MakeQueue = new List<Tuple<ProductMasterItem, int>>();
            ScheduledItems = new List<ProductMasterItem>();
            CoatingScheduleWindow scheduleWindow = new CoatingScheduleWindow(schedule);
            scheduleWindow.Show();

            // get list of all items that are made
            ProductItems.AddRange(StaticInventoryTracker.ProductMasterList.Where(x => x.MadeIn == "Coating"));

            // initialize with current waste, line, and width
            CurrentWaste = StaticFactoryValuesManager.CurrentWaste;
            CurrentDay = StartGen;
            LastWidth = ProductItems.Max(x => x.Width);
            // for each item, get highest priority for that 

            schedule.AddLogic();
            scheduleDay = (CoatingScheduleDay)schedule.ChildrenLogic[0];
            scheduleDay.Date = CurrentDay;
            scheduleDay.AddLogic();
            scheduleLine = (CoatingScheduleLine)scheduleDay.ChildrenLogic[0];
            
                MakeQueue.AddRange((from productMasterItem in ProductItems
                                    let currentPriority = prediction.GetCost(productMasterItem)
                                    select new Tuple<ProductMasterItem, int>(productMasterItem, currentPriority)).OrderByDescending(
                        x => x.Item2));

            int infinityPrevention = 1000;

            while (infinityPrevention > 0 && !MakeQueue.Any(m => m.Item2 > 0))
            {
                DecrementInventory(1);

                MakeQueue.AddRange((from productMasterItem in ProductItems
                                    let currentPriority = prediction.GetCost(productMasterItem)
                                    select new Tuple<ProductMasterItem, int>(productMasterItem, currentPriority)).OrderByDescending(
                        x => x.Item2));
            }

            while (CurrentDay <= EndGen) // until schedule is filled.
            {
                int makeIndex = 0;

                added = false;

                for (; CurrentDay <= EndGen && makeIndex < MakeQueue.Count; ++makeIndex)
                {
                    ProductMasterItem nextItem = MakeQueue[makeIndex].Item1;

                    added = ScheduleItem(nextItem);
                } // try to make all items

                if (!added) // if nothing can be made, continue
                {
                    DateTime beforeDay = CurrentDay;
                    AddControl();

                    int daysAdded = (CurrentDay - beforeDay).Days;
                    if(daysAdded > 0)
                        DecrementInventory(daysAdded);
                }

                // remake list of products to make
                MakeQueue.Clear();
                MakeQueue.AddRange((from productMasterItem in ProductItems
                                    let currentPriority = ControlsList.Sum(genControl => genControl.GetCost(productMasterItem))
                                    select new Tuple<ProductMasterItem, int>(productMasterItem, currentPriority)).OrderByDescending(
                        x => x.Item2));

                // make sure there is something to make
                infinityPrevention = 1000;

                while (infinityPrevention > 0 && !MakeQueue.Any(m => m.Item2 > 0))
                {
                    DecrementInventory(1);

                    MakeQueue.AddRange((from productMasterItem in ProductItems
                                        let currentPriority = prediction.GetCost(productMasterItem)
                                        select new Tuple<ProductMasterItem, int>(productMasterItem, currentPriority)).OrderByDescending(
                            x => x.Item2));
                }
            } // until all days are filled

            scheduleWindow.LoadTrackingItems();
            MessageBox.Show("Schedule done generating");
        }

        private static bool ScheduleItem(ProductMasterItem nextItem)
        {
            while (LineFull())
            {
                DateTime beforeDay = CurrentDay;
                AddControl();

                int daysAdded = (CurrentDay - beforeDay).Days;
                if (daysAdded > 0)
                    DecrementInventory(daysAdded);
            }

            // get a prioritized list of items that still needs to be made

            // try to make each item. Once one id added, exit loop

            Tuple<Machine, Configuration, int> machineLine = GetBestMachine(nextItem);

            Machine machine = machineLine.Item1;
            int lineIndex = machineLine.Item3;
            Configuration config = machineLine.Item2;

            if (machine != null) // if the item can be made currently
            {
                //set shift to add to
                scheduleShift = (CoatingScheduleShift)scheduleLine.ChildrenLogic[lineIndex];

                //CoatingScheduleProduct product = new CoatingScheduleProduct(nextItem);
                double unitsToMake = GetUnitsToMake(nextItem, machine);

                if (unitsToMake > 0)
                {
                    //scheduleShift.AddLogic(product);
                    bool canMake = true;
                    if (!HasPrerequisites(config, unitsToMake))
                    {
                        canMake = SchedulePrerequisite(config, unitsToMake);
                    }

                    if (!canMake) return false;

                    var unitsMade = scheduleShift.ScheduleItem(machine, config, nextItem, unitsToMake);
                    //product.Units = unitsToMake.ToString(); // add to schedule
                    //product.Machine = machine;
                    //product.Config = config;

                    var inv = currentInventoryItems.FirstOrDefault(i => i.MasterID == nextItem.MasterID);
                    if (inv != null)
                    {
                        inv.Units += unitsMade;
                    }
                    else
                    {
                        currentInventoryItems.Add(new InventoryItem(nextItem,unitsMade,"Dealer"));
                    }

                    added = true;

                    // factor in waste created
                    CurrentWaste += nextItem.Waste * unitsMade;
                    
                    DateTime beforeDay = CurrentDay;
                    AddControl();

                    int daysAdded = (CurrentDay - beforeDay).Days;
                    if (daysAdded > 0)
                        DecrementInventory(daysAdded);

                    return true;
                }
                else
                {
                    ProductItems.Remove(nextItem); // remove item.  no prediction
                }
            }

            return false;
        }

        private static bool SchedulePrerequisite(Configuration config, double unitsToMake)
        {
            double unitsRequired = unitsToMake;
            bool scheduled = false;
            var inv = currentInventoryItems.FirstOrDefault(i => i.MasterID == config.ItemInID);
            if (inv != null)
            {
                double unitsAvailable = inv.Units;
                unitsRequired = unitsToMake*(config.ItemsIn/(double) config.ItemsOut);
                unitsRequired -= unitsAvailable;
            }

            ProductMasterItem prevItem =
                StaticInventoryTracker.ProductMasterList.FirstOrDefault(m => m.MasterID == config.ItemInID);

            if (prevItem == null) return false;

            // try to schedule item
            if (prevItem.MadeIn == "Coating")
            {
                scheduled = ScheduleItem(prevItem);
            }
            else if (prevItem.MadeIn == "Press")
            {
                scheduled = PressManager.ScheduleItem(prevItem, unitsRequired, scheduleLine.Date);
                
                // update waste
                if (scheduled)
                {
                    CurrentWaste += prevItem.Waste * unitsRequired;
                }
            }

            return scheduled;
        }

        private static bool HasPrerequisites(Configuration config, double unitsToMake)
        {
            bool hasEnough = false;
            var inv = currentInventoryItems.FirstOrDefault(i => i.MasterID == config.ItemInID);
            if (inv != null)
            {
                // if there are enough units of the required item  
                double unitsAvailable = inv.Units;
                double unitsRequired = unitsToMake*(config.ItemsIn/(double) config.ItemsOut);
                hasEnough = unitsAvailable >= unitsRequired;
            }
            return hasEnough;
        }


        public static void RemoveIfCompleted(ProductMasterItem nextItem)
        {
            var inv = currentInventoryItems.FirstOrDefault(i => i.MasterID == nextItem.MasterID);
            if (inv != null)
            {
                if (nextItem.TurnType == "T") // by turns
                {
                
                    if (inv.Units > (nextItem.MinSupply*GetAvgUnitsPerDay(nextItem)*30)) //  avg units sold*min supply
                        ProductItems.Remove(nextItem); // made enough. Move on.
                }
                else // units
                {
                    if (inv.Units > nextItem.MinSupply)
                        ProductItems.Remove(nextItem); // made enough. Move on.
                }
            }
        }

        private static double GetAvgUnitsPerDay(ProductMasterItem nextItem)
        {
            double unitUsage = 0;
            var forecast = StaticInventoryTracker.ForecastItems.FirstOrDefault(f => f.MasterID == nextItem.MasterID);
            if (forecast != null)
            {
                switch (duration)
                {
                    case SalesPrediction.SalesDurationEnum.LastMonth:
                        unitUsage = forecast.AvgOneMonth/30;
                        break;
                    case SalesPrediction.SalesDurationEnum.Last3Months:
                        unitUsage = forecast.AvgThreeMonths/30;
                        break;
                    case SalesPrediction.SalesDurationEnum.Last6Months:
                        unitUsage = forecast.AvgSixMonths/30;
                        break;
                    case SalesPrediction.SalesDurationEnum.Last12Months:
                        unitUsage = forecast.AvgTwelveMonths/30;
                        break;
                    case SalesPrediction.SalesDurationEnum.LastYear:
                        unitUsage = forecast.AvgPastYear/30;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return unitUsage;
        }

        /// <summary>
        /// Fill orders with scheduled item from closest sale to farthest
        /// </summary>
        /// <param name="nextItem"></param>
        /// <param name="unitsToMake"></param>
        public static void AddSalesFulfillment(ProductMasterItem nextItem, double unitsToMake)
        {
            SalesItem sale = StaticInventoryTracker.GetPrioritySale(nextItem);
            double remainder = 1;

            while (remainder != 0 && sale != null)
            {
                remainder = sale.FulfillOrder(unitsToMake);
                unitsToMake = remainder;
                sale = StaticInventoryTracker.GetPrioritySale(nextItem);
            }
        }


        public static Tuple<Machine, Configuration, int> GetBestMachine(ProductMasterItem nextItem)
        {
            Machine bestMachine = null;
            int lineIndex = 0;
            Configuration config = null;

            List<Machine> otherMachines = new List<Machine>();
            foreach (var shiftLogic in scheduleLine.ChildrenLogic)
            {
                otherMachines.AddRange(from proLogic in shiftLogic.ChildrenLogic select proLogic as CoatingScheduleProduct into prod where prod != null && prod.Machine != null select prod.Machine);
            }

            // get machines that can make
            List<Machine> machines = MachineHandler.Instance.MachineList.Where(machine => machine.ConfigurationList.Any(conf => conf.ItemOutID == nextItem.MasterID)).ToList();
            foreach (var machine in machines)
            {
                if (otherMachines.Any(mac => mac.MachineConflicts.Contains(machine.Name))) continue; // if other machines conflict with this, continue
                if (otherMachines.Any(otherMachine => machine.MachineConflicts.Contains(otherMachine.Name))) continue; // if this machine conflicts with others, skip

                // get line to put on
                List<String> options = machine.LinesCanRunOn.Where(line => StaticFactoryValuesManager.CoatingLines.Contains(line)).ToList();

                // mark full shift as not an option.
                for (int index = 0; index < StaticFactoryValuesManager.CoatingLines.Count; index++)
                {
                    var coatingLine = StaticFactoryValuesManager.CoatingLines[index];
                    if (scheduleLine.ChildrenLogic[index].IsFull())
                        options.Remove(coatingLine);
                }

                // remove other machine line conflicts
                foreach (var lineConflict in from otherMachine in otherMachines from lineConflict in otherMachine.LineConflicts where options.Contains(lineConflict) select lineConflict)
                {
                    options.Remove(lineConflict);
                }

                // remove this machine if line conflicts
                bool conflicted = scheduleLine.ChildrenLogic.Any(shift => shift.ChildrenLogic.Any(x => x is CoatingScheduleProduct) && machine.LineConflicts.Contains(shift.CoatingLine));
                if (conflicted) continue;


                // remove line conflics from control
                List<String> lineOptions = new List<string>();
                lineOptions.AddRange(options);
                foreach (var lineOption in lineOptions)
                {
                    var option = lineOption;
                    List<LineControl> lineConflicControls = ControlsList.Where(control => control is LineControl && ((LineControl) control).CoatingLine == option).OfType<LineControl>().ToList();

                    if (lineConflicControls.Any())
                    {
                        List<CoatingScheduleProduct> products = scheduleLine.ChildrenLogic.SelectMany(logic => logic.ChildrenLogic).OfType<CoatingScheduleProduct>().ToList();
                        foreach (var lineConflicControl in lineConflicControls)
                        {
                            if (products.Any(prod => prod.MasterID == lineConflicControl.MasterItem.MasterID))
                            {
                                options.Remove(option);
                            }
                        }
                    }
                }


                if (options.Count != 0)
                {
                    bestMachine = machine;
                    //get option that works
                    lineIndex = StaticFactoryValuesManager.CoatingLines.IndexOf(options[0]);
                    config = machine.GetBestConfig(nextItem);
                    break;
                }
                // if machine can make, and shift is not full, add to shift
            }

            return new Tuple<Machine, Configuration, int>(bestMachine, config, lineIndex);
        }

        private static bool LineFull()
        {
            return scheduleLine.IsFull();
        }


        public static bool LoadSettings(string fileName = datFile)
        {
            return LoadFileData(fileName, _window);
        }

        private static bool LoadFileData(String file, ScheduleGenWindow window)
        {
            try
            {
                using (BinaryReader reader = new BinaryReader(new FileStream(file, FileMode.Open)))
                {
                    ControlsList.Clear();
                    int num = reader.ReadInt32();
                    for (; num > 0; --num)
                    {
                        var genControl = GenControl.LoadControl(reader, window);
                        ControlsList.Add(genControl);
                    }
                }

                return true;
            }
            catch (Exception exception)
            {
                return false;
            }
        }

        public static bool SaveSettings()
        {
            return SaveFileData(datFile);
        }

        private static bool SaveFileData(String fileName)
        {
            try
            {
                using (BinaryWriter writer = new BinaryWriter(new FileStream(fileName, FileMode.OpenOrCreate)))
                {
                    writer.Write(ControlsList.Count);
                    foreach (var control in ControlsList)
                    {
                        var gen = control;
                        Debug.Assert(gen != null, "genControl save != null");
                        gen.Save(writer);
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public static void UpdateControlOrder()
        {
            ObservableCollection<GenControl> controls = new ObservableCollection<GenControl>();

            while (ControlsList.Count > 0)
            {
                GenControl highControl = null;
                int low = Int32.MaxValue;
                for (int index = 0; index < ControlsList.Count; index++)
                {
                    var control = ControlsList[index];
                    if (control.Priority < low)
                    {
                        highControl = control;
                        low = control.Priority;
                    }
                }

                controls.Insert(0, highControl);
                ControlsList.Remove(highControl);
            }

            foreach (var genControl in controls)
            {
                ControlsList.Add(genControl);
            }
        }


        public static double GetUnitsToMake(ProductMasterItem nextItem, Machine machine)
        {
            double unitsToMake = 0;
            double maxInShift = 0;

            var config = machine.ConfigurationList.FirstOrDefault(conf => conf.ItemOutID == nextItem.MasterID);
            //                             hours                            rate              to units        to minutes
            if (config != null)
                maxInShift = (scheduleLine.Shift.Hours(scheduleLine.Date)*config.ItemsOutPerMinute/nextItem.PiecesPerUnit*60);

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


                    target = avgSold*nextItem.TargetSupply; // avg sold * months supply

                    max = avgSold*nextItem.MaxSupply;
                    min = avgSold*nextItem.MinSupply;

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
                        make -= nextItem.UnitsPerHour*8; // 8 hours in a shift by default

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

            return (int) unitsToMake; // round to nearest unit
        }


        private static void AddControl()
        {
            if (scheduleDay.CanAddShift()) // if all shifts not used.
            {
                scheduleDay.AddLogic();
                scheduleLine = (CoatingScheduleLine) scheduleDay.ChildrenLogic.Last(); // add a shift
            }
            else
            {
                // advance a day and remove expected inventory
                CurrentDay = ShiftHandler.CoatingInstance.GetNextWorkingDay(CurrentDay);

                schedule.AddLogic();
                scheduleDay = (CoatingScheduleDay) schedule.ChildrenLogic.Last();
                scheduleDay.Date = CurrentDay;
                scheduleDay.AddLogic();
                scheduleLine = (CoatingScheduleLine) scheduleDay.ChildrenLogic.Last();
            }
        }

        private static void DecrementInventory(int dayDif)
        {
            // update inventory by lowering it by the number of days past
            foreach (var inventoryItem in CurrentInventory)
            {
                var forecast = StaticInventoryTracker.ForecastItems.FirstOrDefault(f => f.MasterID == inventoryItem.MasterID);
                if (forecast != null)
                {
                    try
                    {
                        switch (duration)
                        {
                            case SalesPrediction.SalesDurationEnum.LastMonth:
                                inventoryItem.Units -= forecast.AvgOneMonth / 30 * dayDif;
                                break;
                            case SalesPrediction.SalesDurationEnum.Last3Months:
                                inventoryItem.Units -= forecast.AvgThreeMonths / 30 * dayDif;
                                break;
                            case SalesPrediction.SalesDurationEnum.Last6Months:
                                inventoryItem.Units -= forecast.AvgSixMonths / 30 * dayDif;
                                break;
                            case SalesPrediction.SalesDurationEnum.Last12Months:
                                inventoryItem.Units -= forecast.AvgTwelveMonths / 30 * dayDif;
                                break;
                            case SalesPrediction.SalesDurationEnum.LastYear:
                                inventoryItem.Units -= forecast.AvgPastYear / 30 * dayDif;
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
            }

        }

        public static DateTime GetSalesRange()
        {
            return SalesOutlook;
        }

        public static void CreateTestGen()
        {
            GenerateScheduleTest();
        }


        private static void GenerateScheduleTest()
        {
            return;
            // outdated code
            StaticFactoryValuesManager.CoatingLines = new ObservableCollection<string>() {"Lap", "Panel"};
            StaticFactoryValuesManager.CurrentWaste = 4000;
            StaticFactoryValuesManager.WasteMin = 1000;
            StaticFactoryValuesManager.WasteMax = 10000;


            // Mock masters
            ProductMasterItem mItem1 = new ProductMasterItem(1, "CODE1", "Test master 1", 48, 92, .5, "OM", 20, 100, "D,W", true, "", "T", 2, 4, 3, 5);
            ProductMasterItem mItem2 = new ProductMasterItem(2, "CODE2", "Test master 2", 50, 92, .5, "OM", 20, 100, "D,W", true, "", "T", 2, 4, 3, 5);
            ProductMasterItem mItem3 = new ProductMasterItem(3, "CODE3", "Test master 3", 49, 92, .5, "OM", 20, 100, "D,W", true, "", "T", 2, 4, 3, 5);
            ProductMasterItem mItem4 = new ProductMasterItem(4, "CODE4", "Test master 4", 40, 92, .5, "OM", 20, 100, "D,W", true, "", "U", 20, 100, 60, 5);
            ProductMasterItem mItemR = new ProductMasterItem(5, "Rough", "Test master 5", 40, 92, .5, "OM", 20, 100, "D,W", true, "", "U", 20, 100, 60, 5);

            StaticInventoryTracker.ProductMasterList.Add(mItem1);
            StaticInventoryTracker.ProductMasterList.Add(mItem2);
            StaticInventoryTracker.ProductMasterList.Add(mItem3);
            StaticInventoryTracker.ProductMasterList.Add(mItem4);

            // mock inventory
            InventoryItem iItem1 = new InventoryItem(mItem1, 100, "D");
            InventoryItem iItem2 = new InventoryItem(mItem2, 10, "D");
            InventoryItem iItem3 = new InventoryItem(mItem3, 100, "D");

            StaticInventoryTracker.InventoryItems.Add(iItem1);
            StaticInventoryTracker.InventoryItems.Add(iItem2);
            StaticInventoryTracker.InventoryItems.Add(iItem3);

            // mock sales
            SalesItem sItem1 = new SalesItem(mItem1, "1001", 50, 0, "D", DateTime.Today.AddDays(8));
            SalesItem sItem11 = new SalesItem(mItem1, "1011", 150, 0, "D", DateTime.Today.AddDays(6));
            SalesItem sItem2 = new SalesItem(mItem2, "1002", 50, 0, "D", DateTime.Today.AddDays(8));
            SalesItem sItem4 = new SalesItem(mItem4, "1004", 50, 0, "D", DateTime.Today.AddDays(8));

            StaticInventoryTracker.SalesItems.Add(sItem1);
            StaticInventoryTracker.SalesItems.Add(sItem11);
            StaticInventoryTracker.SalesItems.Add(sItem2);
            StaticInventoryTracker.SalesItems.Add(sItem4);

            // mock sales history
            var highSales = new double[] {180, 170, 150, 140, 100, 40, 50, 70, 80, 90, 130, 150, 190};
            var medSales = new double[] {90, 85, 75, 70, 50, 20, 25, 35, 40, 45, 65, 75, 95};
            var lowSales = new double[] {20, 15, 12, 14, 10, 5, 7, 12, 14, 10, 13, 15, 22};
            ForecastItem fItem1 = new ForecastItem(iItem1.Units, mItem1, highSales);
            ForecastItem fItem2 = new ForecastItem(iItem2.Units, mItem2, medSales);
            ForecastItem fItem3 = new ForecastItem(iItem3.Units, mItem3, medSales);
            ForecastItem fItem4 = new ForecastItem(0, mItem4, lowSales);

            StaticInventoryTracker.ForecastItems.Add(fItem1);
            StaticInventoryTracker.ForecastItems.Add(fItem2);
            StaticInventoryTracker.ForecastItems.Add(fItem3);
            StaticInventoryTracker.ForecastItems.Add(fItem4);

            // factory config
            Configuration cI1 = Configuration.CreateConfiguration("Make item 1", mItemR.MasterID, 1, mItem1.MasterID, 2, 60);
            Configuration cI2 = Configuration.CreateConfiguration("Make item 2", mItemR.MasterID, 1, mItem2.MasterID, 2, 40);
            Configuration cI3 = Configuration.CreateConfiguration("Make item 3", mItemR.MasterID, 1, mItem3.MasterID, 2, 50);
            Configuration cI4 = Configuration.CreateConfiguration("Make item 4", mItemR.MasterID, 1, mItem4.MasterID, 2, 80);

            Machine machineLap = Machine.CreateMachine("LapMachine"); // runs lap, cant run w/ panel. Makes 1 and 2
            machineLap.LinesCanRunOn.Add("Lap");
            machineLap.LineConflicts.Add("Panel");
            machineLap.MachineConflicts.Add("PanelMachine");
            machineLap.AddConfiguration(cI1);
            machineLap.AddConfiguration(cI2);

            Machine machinePanel = Machine.CreateMachine("PanelMachine"); // runs panel, cant run w/lap. Makes 3 and 4
            machinePanel.LinesCanRunOn.Add("Panel");
            machineLap.LineConflicts.Add("Lap");
            machinePanel.MachineConflicts.Add("LapMachine");
            machinePanel.AddConfiguration(cI3);
            machinePanel.AddConfiguration(cI4);

            MachineHandler.Instance.AddMachine(machinePanel);
            MachineHandler.Instance.AddMachine(machineLap);


            // shifts
            var days = Shift.ShiftFactory("Days", DateTime.Today.AddHours(14), TimeSpan.FromHours(8), DateTime.Today.AddDays(-100), DateTime.MaxValue, null, new List<DayOfWeek>() {DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday});
            var swing = Shift.ShiftFactory("Swing", DateTime.Today.AddHours(22), TimeSpan.FromHours(8), DateTime.Today.AddDays(-100), DateTime.MaxValue, null, new List<DayOfWeek>() {DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday});
            var gyd = Shift.ShiftFactory("Graveyard", DateTime.Today.AddHours(6), TimeSpan.FromHours(8), DateTime.Today.AddDays(-100), DateTime.MaxValue, null, new List<DayOfWeek>() {DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday});

            ShiftHandler.CoatingInstance.Shifts.Clear();
            ShiftHandler.CoatingInstance.AddShift(days);
            ShiftHandler.CoatingInstance.AddShift(swing);
            ShiftHandler.CoatingInstance.AddShift(gyd);

            // Priority controls
            ScheduleGenWindow genWindow = new ScheduleGenWindow();
            var saleControl = new SalesNumbersControl(genWindow, 1000);
            var predictionControl = new SalesPrediction(genWindow, 500, SalesPrediction.SalesDurationEnum.Last12Months);
            var wasteControl = new WasteControl(genWindow, 100);
            var widControl = new WidthControl(genWindow, 800);
            genWindow.Show();

            ScheduleGenerator.ControlsList.Add(saleControl);
            ScheduleGenerator.ControlsList.Add(predictionControl);
            ScheduleGenerator.ControlsList.Add(wasteControl);
            ScheduleGenerator.ControlsList.Add(widControl);

            ScheduleGenerator.StartGen = DateTime.Today;
            ScheduleGenerator.EndGen = DateTime.Today.AddDays(1);
            ScheduleGenerator.SalesOutlook = DateTime.Today.AddDays(14);

            // outdated
            //ScheduleGenerator.GenerateSchedule(true);
        }
    }
}
