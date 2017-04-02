﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
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
        static double avgSold;


        private ScheduleGenWindow _window;
        public const string datFile = "ScheduleGenSettings.dat";


        public ScheduleGenWindow Window
        {
            get { return _window; }
            set { _window = value; }
        }
        private Queue<String> _errors = new Queue<string>();

        public GenerationData GenerationData;
        public GenerationSettings GenerationSettings;

        public void GenerateSchedule(GenerationSettings settings)
        {
            GenerationSettings = settings;
            if (GenerationData == null)
                GenerationData = new GenerationData();

            if(!MachineHandler.Instance.IsLoaded)
                MachineHandler.Instance.Load();
            if (!MachineHandler.Instance.IsLoaded)
            {
                MessageBox.Show("Could not load machine config data. Please open the machine config file.");
                if (!MachineHandler.Instance.LoadDialog())
                {
                    MessageBox.Show("Failed to load or canceled. Cannot generate schedule without machine config data. Stopping generation.");
                    return;
                }
            }

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

            try
            {
                CoatingSchedule.CurrentSchedule?.ChildrenLogic.Clear();

                CoatingScheduleWindow scheduleWindow = new CoatingScheduleWindow(schedule);
                scheduleWindow.Show();
                PressScheduleWindow pressScheduleWindow = new PressScheduleWindow();
                pressScheduleWindow.Show();
                PressManager.PlateConfigurations.Clear();
                pressScheduleWindow.UpdateControls();



                // add the first shift to the schedule
                schedule.AddLogic();
                scheduleDay = (CoatingScheduleDay)schedule.ChildrenLogic[0];
                scheduleDay.Date = GenerationSettings.StartGen;
                scheduleDay.AddLogic();
                scheduleLine = (CoatingScheduleLine)scheduleDay.ChildrenLogic[0];

                GenerationData.InitializeData(RequirementsHandler.GetMakeOrders(GenerationSettings.SalesOutlook),
                    GenerationSettings);
                

                // get list of items that can be made in the coating plant
                List<ProductMasterItem> masterItemsAvailableToMake =
                    StaticInventoryTracker.ProductMasterList.Where(
                        m => m.MadeIn.ToUpper().Equals("COATING")).ToList();

                List<ProductMasterItem> unmakeableItems =
                masterItemsAvailableToMake.Where(
                    item => MachineHandler.Instance.AllConfigurations.All(c => !c.CanMake(item))).ToList();

                if (unmakeableItems.Any())
                {
                    StringBuilder badItems = new StringBuilder();
                    badItems.AppendLine("No configuration found for " + unmakeableItems.Count + " items:");
                    foreach (var productMasterItem in unmakeableItems)
                    {
                        masterItemsAvailableToMake.Remove(productMasterItem);
                        badItems.AppendLine(productMasterItem.ToString());
                    }
                    badItems.AppendLine(
                        "Please note that these items cannot be scheduled until they have configurations that output their masterID.");
                    MessageBox.Show(badItems.ToString());
                }

                // While we have not reached the end of the schedule
                while (GenerationData.CurrentDay <= GenerationSettings.EndGen)
                {
                    // attempt to schedule the highest priority item until the line is full. 
                    while (GenerationData.ScheduledItem.Any(s => !s.Value) && !LineIsFull())
                    {
                        int itemIndex = 0;
                        bool wasScheduled = false;

                        foreach (var coatingLine in StaticFactoryValuesManager.CoatingLines)
                        {
                            // Get item with highest priority
                            GenerationData.CreatePriorityList(masterItemsAvailableToMake, coatingLine);
                            
                            //TODO Add handling of adding prerequisites for an item using the GenData PrereqOrders. (check other tag too)
                            // Use the next item in the prereq. list as the most favorable.
                            PriorityItem currentItem = GenerationData.PriorityList[itemIndex];

                            if (currentItem.Item.MadeIn.ToUpper().Equals("COATING"))
                            {
                                wasScheduled = ScheduleCoating(currentItem.Item, coatingLine);
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

                            // Check if all the lines have been scheduled or if we have tried all the items and none can be scheduled, move on
                            if (GenerationData.ScheduledItem.All(s => s.Value) ||
                                itemIndex >= GenerationData.PriorityList.Count)
                            {
                                AddControl();
                            }
                        }
                        if (!wasScheduled)
                        {
                            // move to the next item, as the highest priority item can't be made currently.
                            itemIndex++;
                        }
                    }

                    if(LineIsFull())
                        AddControl();

                }
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
        /// <param name="minimumUnits"></param>
        private bool ScheduleCoating(ProductMasterItem currentItem, string coatingLine)
        {
            Configuration config;
            double unitsToMake;
            bool itemMade = false;

                var bestMachine = GetBestMachine(currentItem, coatingLine);

                // if the item can be made
                if (bestMachine != null && bestMachine.Item1 != null && bestMachine.Item2 != null)
                {
                    config = bestMachine.Item2.Configurations.FirstOrDefault(c => c.CanMake(currentItem));
                    if (config != null)
                    {
                        unitsToMake = GetUnitsToMake(currentItem, config);

                        if (unitsToMake > 0)
                        {
                            // Check for required inventory
                            if (HasPrerequisites(config, unitsToMake, currentItem))
                            {
                                // If inventory, schedule item
                                GenerationData.ScheduledItem[coatingLine] = ScheduleCoating(currentItem, bestMachine, config);
                            }
                            else
                            {
                                // Else, select the highest priority item needed and schedule that
                                GenerationData.ScheduledItem[coatingLine] = SchedulePrerequisite(config, unitsToMake,coatingLine, currentItem);
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


        /// <summary>
        /// Schedule the item with the given config.
        /// </summary>
        /// <param name="nextItem"></param>
        /// <param name="maxToMake"></param>
        /// <param name="scheduleInfo"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        private bool ScheduleCoating(ProductMasterItem nextItem, Tuple<Machine, ConfigurationGroup, int> scheduleInfo, Configuration config)
        {
            Machine machine = scheduleInfo.Item1;
            int lineIndex = scheduleInfo.Item3;

            if (machine != null) // if the item can be made currently
            {
                //set shift to add to
                scheduleShift = (CoatingScheduleShift)scheduleLine.ChildrenLogic[lineIndex];

                var unitsMade = scheduleShift.ScheduleItem(machine, config, nextItem);

                added = true;
                GenerationData.MarkItemScheduled(nextItem, lineIndex, unitsMade);

                return true;
            }
            return false;
        }


        private bool SchedulePrerequisite(Configuration config, double unitsNeeded, string coatingLine, ProductMasterItem item)
        {
            //TODO Add handling of adding prerequisites for an item using the GenData PrereqOrders.
            double unitsRequired = unitsNeeded;
            bool scheduled = false;
            double minimumUnits = 0;
            bool makeItem = false;

            // get the first item required to make this item and schedule it.
            ProductMasterItem prevItem = null;

            foreach (var configInputItem in config.InputItems)
            {
                var inv = GenerationData.CurrentInventory.FirstOrDefault(i => i.MasterID == configInputItem.MasterID);
                if (inv != null)
                {
                    prevItem = StaticInventoryTracker.ProductMasterList.FirstOrDefault(
                        m => m.MasterID == configInputItem.MasterID);
                    if(prevItem != null)
                    {
                        unitsRequired = (unitsNeeded * item.PiecesPerUnit * configInputItem.Pieces) /
                                        prevItem.PiecesPerUnit;
                        // if not enough in inventory, make the item
                        if (inv.Units < unitsRequired)
                        {
                            makeItem = true;
                            minimumUnits = unitsRequired - inv.Units;
                            break;
                        }
                    }
                }
            }
            

            if (prevItem == null || !makeItem) return false;

            // try to schedule item
            if (prevItem.MadeIn == "Coating")
            {
                scheduled = ScheduleCoating(prevItem, coatingLine);
            }
            else if (prevItem.MadeIn == "Press")
            {
                SchedulePress(prevItem, minimumUnits);
            }

            return scheduled;
        }

        private bool HasPrerequisites(Configuration config, double unitsToMake, ProductMasterItem item)
        {
            bool hasEnough = false;
            foreach (var configInputItem in config.InputItems)
            {
                var inv = GenerationData.CurrentInventory.FirstOrDefault(i => i.MasterID == configInputItem.MasterID);
                if (inv != null)
                {
                    var inputMaster =
                        StaticInventoryTracker.ProductMasterList.FirstOrDefault(
                            m => m.MasterID == configInputItem.MasterID);

                    // if there are enough units of the required item  
                    double unitsAvailable = inv.Units;
                    double unitsRequired = (unitsToMake * item.PiecesPerUnit * configInputItem.Pieces)/inputMaster.PiecesPerUnit;
                    hasEnough = unitsAvailable >= unitsRequired;
                }
            }

            return hasEnough;
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
                    // check for last used config
                    if (GenerationData.LastRunMachine.ContainsKey(bestMachine))
                    {
                        configGroup = GenerationData.LastRunMachine[bestMachine].Group;
                    }
                    else // assuming first thing to be made on line
                    {
                        configGroup = bestMachine.ConfigurationList.FirstOrDefault(c => c.CanMake(nextItem));
                    }
                    break;
                }
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

        public double GetUnitsToMake(ProductMasterItem nextItem, Configuration config)
        {
            double unitsToMake = 0;
            double maxInShift = 0;

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
                    switch (GenerationData.SalesOutlookDuration)
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
                        StaticInventoryTracker.SalesItems.Where(sale => sale.MasterID == nextItem.MasterID && sale.Date < GenerationSettings.SalesOutlook && sale.Units - sale.Fulfilled < 1).Sum(sale => sale.Units - sale.Fulfilled);
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
            DateTime beforeDay = GenerationData.CurrentDay;

            if (scheduleDay.CanAddShift()) // if all shifts not used.
            {
                scheduleDay.AddLogic();
                scheduleLine = (CoatingScheduleLine)scheduleDay.ChildrenLogic.Last(); // add a shift
            }
            else
            {
                // advance a day and remove expected inventory
                GenerationData.CurrentDay = ShiftHandler.CoatingInstance.GetNextWorkingDay(GenerationData.CurrentDay);

                schedule.AddLogic();
                scheduleDay = (CoatingScheduleDay)schedule.ChildrenLogic.Last();
                scheduleDay.Date = GenerationData.CurrentDay;
                scheduleDay.AddLogic();
                scheduleLine = (CoatingScheduleLine)scheduleDay.ChildrenLogic.Last();
            }

            // decrement inventory
            int daysAdded = (GenerationData.CurrentDay - beforeDay).Days;
            if (daysAdded > 0)
                GenerationData.DecrementInventory(daysAdded);
            
            // reset the state for the next shift
            GenerationData.ResetForNextShift();
        }


        public DateTime GetSalesRange()
        {
            return GenerationSettings.SalesOutlook;
        }
    }
}