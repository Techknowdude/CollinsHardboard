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
        private const int DefaultWidthPriority = 250;
        private const int DefaultSalePredictionPriority = 225;
        private const int DefaultSalePriority = 200;

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
        private static Dictionary<Machine, LastConfigTime> _runningMachines = new Dictionary<Machine, LastConfigTime>(); 

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
        static Queue<MakeOrder> _orders = new Queue<MakeOrder>();
        static Queue<MakeOrder> _waitOrders = new Queue<MakeOrder>();

        private static Dictionary<ProductMasterItem, double> _fulfilled;
        private static Queue<String> _errors = new Queue<string>();

        public static void GenerateSalesSchedule(DateTime salesOutlook, DateTime startGen, DateTime endGen, bool testing = false)
        {
            _orders.Clear();
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

            SalesOutlook = salesOutlook;
            StartGen = startGen;
            EndGen = endGen;

            int lastWaitItems = 0;

            ProductItems = new List<ProductMasterItem>();
            ScheduledItems = new List<ProductMasterItem>();

            CoatingSchedule.CurrentSchedule?.ChildrenLogic.Clear();

            CoatingScheduleWindow scheduleWindow = new CoatingScheduleWindow(schedule);
            scheduleWindow.Show();
            PressScheduleWindow pressScheduleWindow = new PressScheduleWindow();
            pressScheduleWindow.Show();
            PressManager.PlateConfigurations.Clear();
            pressScheduleWindow.UpdateControls();


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


            _orders = RequirementsHandler.GetMakeOrders(SalesOutlook, DefaultWidthPriority > DefaultSalePriority);

            while (CurrentDay <= EndGen && _orders.Count > 0)
            {
                var nextOrder = _orders.Peek();

                while (nextOrder != null && nextOrder.PiecesToMake < 1) // get rid of empty orders
                {
                    _orders.Dequeue();
                    if (_orders.Count == 0)
                        break;
                    nextOrder = _orders.Peek();
                }

                if (nextOrder == null) continue;

                ProductMasterItem nextItem =
                    StaticInventoryTracker.ProductMasterList.FirstOrDefault(x => x.MasterID == nextOrder.MasterID);

                if(nextItem == null) continue;
                

                ScheduleSaleItem(nextItem,nextOrder.PiecesToMake);


                if (_orders.Count == 0)
                {
                    if(lastWaitItems == _waitOrders.Count)
                        AddControl(); // Making no progress, advance a shift

                    lastWaitItems = _waitOrders.Count;
                    while (_waitOrders.Count > 0)
                    {
                        _orders.Enqueue(_waitOrders.Dequeue());
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

        /// <summary>
        /// Schedules the prerequisite ASAP
        /// </summary>
        /// <param name="config"></param>
        /// <param name="unitsToMake"></param>
        /// <returns>True if the entire amount is made in time -- The parent item can now be scheduled</returns>
        private static bool ScheduleSalesPrerequisite(Configuration config, double unitsToMake)
        {
            double unitsRequired = unitsToMake;
            bool scheduled = false;
            var inv = currentInventoryItems.FirstOrDefault(i => i.MasterID == config.ItemInID);
            if (inv != null)
            {
                double unitsAvailable = inv.Units;
                unitsRequired = unitsToMake * (config.ItemsIn / (double)config.ItemsOut);
                unitsRequired -= unitsAvailable;
            }

            ProductMasterItem prevItem =
                StaticInventoryTracker.ProductMasterList.FirstOrDefault(m => m.MasterID == config.ItemInID);

            if (prevItem == null) return false;

            // try to schedule item
            if (prevItem.MadeIn == "Coating")
            {
                scheduled = ScheduleSaleItem(prevItem, unitsRequired*prevItem.PiecesPerUnit);
            }
            else if (prevItem.MadeIn == "Press")
            {
                scheduled = PressManager.ScheduleSalesItem(prevItem, unitsRequired, scheduleLine.Date);

                // update waste
                if (scheduled)
                {
                    CurrentWaste += prevItem.Waste * unitsRequired;
                }
            }

            return scheduled;
        }

        private static bool ScheduleSaleItem(ProductMasterItem nextItem, double pieces)
        {
            Tuple<Machine, Configuration, int> machineLine = GetBestMachine(nextItem);

            Machine machine = machineLine.Item1;
            int lineIndex = machineLine.Item3;
            Configuration config = machineLine.Item2;

            if (machine != null) // if the item can be made currently
            {
                //set shift to add to
                scheduleShift = (CoatingScheduleShift)scheduleLine.ChildrenLogic[lineIndex];

                // update running machine availability. Date is the time it will be available again
                _runningMachines[machine] = new LastConfigTime( StaticFunctions.GetDayAndTime(scheduleLine.Date,scheduleLine.Shift.StartTime) + scheduleLine.Shift.Duration, config);
                StaticFunctions.OutputDebugLine($"Machine {machine.Name} is being used until {_runningMachines[machine].LastShiftTime} to make {nextItem.Description}");

                double unitsToMake = pieces / (double)nextItem.PiecesPerUnit;


                if (Math.Abs(unitsToMake % nextItem.UnitsPerHour * 8) > 0.0000001) // if not filling a shift, add enough to do so
                {
                    unitsToMake += nextItem.UnitsPerHour * 8 - unitsToMake % (nextItem.UnitsPerHour * 8);
                }

                if (unitsToMake > 0)
                {
                    bool hasPrereqs = true;
                    // check for required items
                    hasPrereqs = HasPrerequisites(config, unitsToMake);

                    // try to make any required items
                    if (!hasPrereqs)
                    {
                        hasPrereqs = ScheduleSalesPrerequisite(config, unitsToMake);
                    }

                    if (hasPrereqs)
                    {
                        var unitsMade = scheduleShift.ScheduleItem(machine, config, nextItem, double.MaxValue);
                        var takeOff = unitsMade*nextItem.PiecesPerUnit;
                        foreach (var makeOrder in _orders.Where(m => m.MasterID == nextItem.MasterID))
                        {
                            if (takeOff > makeOrder.PiecesToMake)
                            {
                                takeOff -= makeOrder.PiecesToMake;
                                makeOrder.PiecesToMake = 0;
                            }
                            else
                            {
                                makeOrder.PiecesToMake -= (int)takeOff;
                            }
                        }

                        // factor in waste created
                        CurrentWaste += nextItem.Waste * unitsMade;

                        AddControl(); // All production uses up at least one shift.
                        return true;
                    }
                }
            }
            if(_orders.Count > 0)
                _waitOrders.Enqueue(_orders.Dequeue());

            return false;
        }

        public static void GeneratePredictionSchedule(DateTime salesOutlook, DateTime startGen, DateTime endGen, bool testing = false)
        {
            currentInventoryItems.Clear();
            _runningMachines.Clear();

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
            duration = SalesPrediction.SalesDurationEnum.LastYear;

            var prediction = ControlsList.FirstOrDefault(control => control is SalesPrediction) as SalesPrediction ??
                             new SalesPrediction(Window,DefaultSalePredictionPriority,SalesPrediction.SalesDurationEnum.LastYear);
            var widthController = ControlsList.FirstOrDefault(control => control is WidthControl) as WidthControl ??
                                  new WidthControl(Window, DefaultWidthPriority);

            if (!ControlsList.Contains(prediction))
                ControlsList.Add(prediction);
            if (!ControlsList.Contains(widthController))
                ControlsList.Add(widthController);

            duration = prediction.SalesDuration;

            SalesOutlook = salesOutlook;
            StartGen = startGen;
            EndGen = endGen;


            ProductItems = new List<ProductMasterItem>();
            var makeQueue = new List<Tuple<ProductMasterItem, int>>();
            ScheduledItems = new List<ProductMasterItem>();
            CoatingScheduleWindow scheduleWindow = new CoatingScheduleWindow(schedule);
            scheduleWindow.Show();
            scheduleWindow.Schedule.ChildrenLogic.Clear();
            scheduleWindow.DayControls.Clear();
            PressScheduleWindow pressWindow=  new PressScheduleWindow();
            pressWindow.Show();
            PressManager.PlateConfigurations.Clear();
            pressWindow.UpdateControls();

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

            makeQueue.AddRange((from productMasterItem in ProductItems
                                let currentPriority = prediction.GetCost(productMasterItem)
                                select new Tuple<ProductMasterItem, int>(productMasterItem, currentPriority)).Where(x => x.Item2 > 0).OrderByDescending(
                    x => x.Item2));

            int infinityPrevention = 1000;

            while (infinityPrevention > 0 && makeQueue.Count < 1)
            {
                DecrementInventory(1);

                makeQueue.AddRange((from productMasterItem in ProductItems
                                    let currentPriority = prediction.GetCost(productMasterItem)
                                    select new Tuple<ProductMasterItem, int>(productMasterItem, currentPriority)).Where(x => x.Item2 > 0).OrderByDescending(
                        x => x.Item2));

                --infinityPrevention;
            }

            while (CurrentDay <= EndGen) // until schedule is filled.
            {
                int makeIndex = 0;

                added = false;

                for (; CurrentDay <= EndGen && makeIndex < makeQueue.Count; ++makeIndex)
                {
                    ProductMasterItem nextItem = makeQueue[makeIndex].Item1;

                    added = ScheduleItem(nextItem);
                } // try to make all items

                if (!added) // if nothing can be made, continue
                {
                    DateTime beforeDay = CurrentDay;
                    AddControl();

                    int daysAdded = (CurrentDay - beforeDay).Days;
                    if (daysAdded > 0)
                        DecrementInventory(daysAdded);
                }

                // remake list of products to make
                makeQueue.Clear();
                makeQueue.AddRange((from productMasterItem in ProductItems
                                    let currentPriority = ControlsList.Sum(genControl => genControl.GetCost(productMasterItem))
                                    select new Tuple<ProductMasterItem, int>(productMasterItem, currentPriority)).Where(x => x.Item2 > 0).OrderByDescending(
                        x => x.Item2));

                // make sure there is something to make
                infinityPrevention = 1000;

                while (infinityPrevention > 0 && makeQueue.Count == 0)
                {
                    DecrementInventory(1);

                    makeQueue.AddRange((from productMasterItem in ProductItems
                                        let currentPriority = prediction.GetCost(productMasterItem)
                                        select new Tuple<ProductMasterItem, int>(productMasterItem, currentPriority)).Where(x => x.Item2 > 0).OrderByDescending(
                            x => x.Item2));
                    --infinityPrevention;
                }
            } // until all days are filled


            while (_errors.Count > 0)
            {
                MessageBox.Show(_errors.Dequeue());
            }


            scheduleWindow.LoadTrackingItems();
            pressWindow.UpdateControls();
            MessageBox.Show("Schedule done generating");
        }

        /// <summary>
        /// Used for prediction generation
        /// </summary>
        /// <param name="nextItem"></param>
        /// <returns></returns>
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

                // update running machine availability. Date is the time it will be available again
                _runningMachines[machine] = new LastConfigTime( StaticFunctions.GetDayAndTime(scheduleLine.Date,scheduleLine.Shift.StartTime) + scheduleLine.Shift.Duration, config);
                StaticFunctions.OutputDebugLine($"Machine {machine.Name} is being used until {_runningMachines[machine].LastShiftTime} to make {nextItem.Description}");

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

                    LastWidth = nextItem.Width;

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
                unitsRequired = unitsToMake * (config.ItemsIn / (double)config.ItemsOut);
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


        public static void RemoveIfCompleted(ProductMasterItem nextItem)
        {
            var inv = currentInventoryItems.FirstOrDefault(i => i.MasterID == nextItem.MasterID);
            if (inv != null)
            {
                if (nextItem.TurnType == "T") // by turns
                {

                    if (inv.Units > (nextItem.MinSupply * GetAvgUnitsPerDay(nextItem) * 30)) //  avg units sold*min supply
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
            List<Machine> machines = MachineHandler.Instance.MachineList.Where(machine => machine.ConfigurationList.Any(conf => conf.CanMake(nextItem))).ToList();

            if (machines.Count == 0)
            {
                if(!_errors.Contains($"No machine can make {nextItem}. Please add a configuration to make this item to a machine."))
                    _errors.Enqueue(
                    $"No machine can make {nextItem}. Please add a configuration to make this item to a machine.");
            }

           

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
                    List<LineControl> lineConflicControls = ControlsList.Where(control => control is LineControl && ((LineControl)control).CoatingLine == option).OfType<LineControl>().ToList();

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


                foreach (var lineOption in options)
                {
                    bestMachine = machine;
                    //get option that works
                    lineIndex = StaticFactoryValuesManager.CoatingLines.IndexOf(lineOption);
                    config = machine.GetBestConfig(nextItem);

                    // check if machine config is not the same
                    if (_runningMachines.ContainsKey(bestMachine))
                    {
                        var lastRun = _runningMachines.FirstOrDefault(r => r.Key == machine);
                        if (!Equals(lastRun.Value.LastConfiguration, config))
                        {
                            // check if the machine will be ready on time
                            //TODO: update the config change time
                            if (lastRun.Value.LastShiftTime /*+ config.ChangeTime*/ <= StaticFunctions.GetDayAndTime(scheduleLine.Date, scheduleLine.Shift.StartTime))
                            {
                                break;
                            }
                            else
                            {
                                // can't use the config yet - not ready
                                StaticFunctions.OutputDebugLine($"Machine {machine.Name} can't be used. It is unavailable until {_runningMachines[machine].LastShiftTime /*+ config.ChangeTime*/}. It is needed by {nextItem.Description} by {StaticFunctions.GetDayAndTime(scheduleLine.Date,scheduleLine.Shift.StartTime)}");
                                bestMachine = null;
                                lineIndex = 0;
                                config = null;
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


        private static void AddControl()
        {
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
            //ScheduleGenerator.GenerateSchedule(true);
        }
    }

    internal class LastConfigTime
    {
        public LastConfigTime(DateTime date, Configuration config)
        {
            LastShiftTime = date;
            LastConfiguration = config;
        }

        public DateTime LastShiftTime { get; set; }
        public Configuration LastConfiguration { get; set; }
    }
}
