using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Input;
using Configuration_windows;
using ModelLib;
using StaticHelpers;

namespace ProductionScheduler
{
    public class PressManager
    {

        #region Fields
        //New Stuff
        private static ObservableCollection<PlateConfiguration> _plateConfigurations = new ObservableCollection<PlateConfiguration>();

        public static double PressLoadsPerHour
        {
            get { return _pressLoadsPerHour; }
            set
            {
                _pressLoadsPerHour = value; 
            }
        }

        // Old Stuff
        private const string DatFile = "pressManager.dat";
        private static Int32 _numPlates = 0;
        public Int32 NumPlateChangesPerWeek {get { return _plateChangeDays.Count; } }
        private static ObservableCollection<DayOfWeek> _plateChangeDays = new ObservableCollection<DayOfWeek>();
        private TimeSpan _delayTime = TimeSpan.FromHours(16);

        #endregion

        #region Properties

        public TimeSpan DelayTime
        {
            get { return _delayTime; }
            set { _delayTime = value; }
        }

        public static Int32 NumPlates
        {
            get { return _numPlates; }
            set { _numPlates = value; }
        }
        #endregion

        #region Singleton

        private static PressManager _instance = null;
        private static double _pressLoadsPerHour;

        public static PressManager Instance
        {
            get { return _instance ?? (_instance = new PressManager()); }
        }
        

        public static ObservableCollection<DayOfWeek> PlateChangeDays
        {
            get { return _plateChangeDays; }
            set { _plateChangeDays = value; }
        }

        public static PressScheduleWindow Window { get; set; }

        public static ObservableCollection<PlateConfiguration> PlateConfigurations
        {
            get { return _plateConfigurations; }
            set { _plateConfigurations = value; }
        }

        #endregion

        private PressManager()
        {
            Load();
            ShiftHandler.ProductionInstance.LoadShifts();
        }

        public static bool Save(String filename = DatFile)
        {
            if (Instance == null) return false;

            bool success = true;

            try
            {
                using (FileStream stream = File.OpenWrite(DatFile))
                {
                    BinaryFormatter formatter = new BinaryFormatter();

                    formatter.Serialize(stream, PlateConfigurations.Count);
                    foreach (var plateConfiguration in PlateConfigurations)
                    {
                        formatter.Serialize(stream,plateConfiguration);
                    }
                    formatter.Serialize(stream,PlateChangeDays.Count);
                    foreach (var plateChangeDay in PlateChangeDays)
                    {
                        formatter.Serialize(stream,plateChangeDay);
                    }
                    formatter.Serialize(stream,NumPlates);
                    formatter.Serialize(stream,PressLoadsPerHour);
                }
                //writer.Write(Instance.NumPlates);
                //writer.Write(Instance.NumPlateChangesPerWeek);
                //writer.Write(Instance.PlateChangeDays.Count);
                //foreach (var plateChangeDay in Instance.PlateChangeDays)
                //{
                //    writer.Write((Int32)plateChangeDay);
                //}
            }
            catch
            {
                success = false;
            }

            return success;
        }

        public static bool Load()
        {
            try
            {
                using (FileStream stream = File.OpenRead(DatFile))
                {
                    BinaryFormatter formatter = new BinaryFormatter();

                    PlateConfigurations.Clear();
                    PlateChangeDays.Clear();
                    PressScheduleWindow.WeekControls?.Clear();

                    int plateCount = (int) formatter.Deserialize(stream);

                    for (; plateCount > 0; plateCount--)
                    {
                        PlateConfiguration plateConfiguration = (PlateConfiguration) formatter.Deserialize(stream);
                        PlateConfigurations.Add(plateConfiguration);
                    }

                    int daysCount = (int) formatter.Deserialize(stream);
                    for (; daysCount > 0; daysCount--)
                    {
                        DayOfWeek day = (DayOfWeek) formatter.Deserialize(stream);
                        PlateChangeDays.Add(day);
                    }

                    NumPlates = (int) formatter.Deserialize(stream);
                    PressLoadsPerHour = (double) formatter.Deserialize(stream);
                    
                }

                    //using (BinaryReader reader = new BinaryReader(new FileStream(DatFile, FileMode.Open)))
                    //{
                    //    return Load(reader);
                    //}
            }
            catch (Exception exception)
            {
                return false;
            }

            return true;
        }

        public static bool ScheduleItem(ProductMasterItem item, double count, DateTime targetTime)
        {
            DateTime finishedDateTime = DateTime.MinValue;

            bool completed = false;

            if (PlateConfigurations.Count == 0)
            {
                Instance.CreateNewConfig();
            }

            while (!completed && finishedDateTime < targetTime)
            {
                // Find closest plate config
                foreach (var plateConfiguration in PlateConfigurations)
                {
                    // try to fill
                    if (plateConfiguration.Add(item, count, ref finishedDateTime))
                    {
                        completed = true;
                        break;
                    }
                }

                if (!completed && finishedDateTime < targetTime)
                {
                    // add another config
                    Instance.CreateNewConfig();
                }
            }


            return finishedDateTime <= targetTime;
        }

        /// <summary>
        /// Retrieves the press shifts that finished within the time frame
        /// </summary>
        /// <param name="start">Lower bound for checking. Not inclusive.</param>
        /// <param name="end">Upper bound for checking. Is inclusive.</param>
        /// <returns>List of shifts. List is never null, but may be empty</returns>
        public List<PressShift> GetPressShifts(DateTime start, DateTime end)
        {
            List<PressShift> shifts = new List<PressShift>();

            foreach (var plateConfiguration in PlateConfigurations)
            {
                if (start > plateConfiguration.StartTime && end < plateConfiguration.EndTime + DelayTime)
                {
                    var shift = plateConfiguration.GetShift(start, end);
                    if(shift != null)
                        shifts.Add(shift);
                }
            }

            return shifts;
        }


        /// <summary>
        /// Tries to add the item to the schedule
        /// </summary>
        /// <param name="item">Item to add</param>
        /// <param name="unitCount">Units to make. Changed by function to be the leftover units not completed.</param>
        /// <param name="dueDate">Time the units should be completed.</param>
        /// <returns>True if any units were scheduled. Unit count is deducted any units scheduled</returns>
        public bool AddItem(ProductMasterItem item, ref double unitCount, DateTime dueDate)
        {
            bool added = false;
            for (int index = 0; unitCount > 0 && index < PlateConfigurations.Count; index++)
            {
                var configuration = PlateConfigurations[index];
                // keep true that it was added. Check unit count for full completion.
                added = added || configuration.Add(item, unitCount, ref dueDate);
            }
            if (unitCount > 0)
            {
                var config = CreateNewConfig();
                config.Add(item, unitCount, ref dueDate);
                PlateConfigurations.Add(config);
            }

            return added;
        }

        /// <summary>
        /// Creates a new configuration after the last configuration on the list that goes from then until the next plate change.
        /// </summary>
        /// <returns></returns>
        public PlateConfiguration CreateNewConfig()
        {
            DateTime begin = DateTime.Today;
            DateTime end = begin;
            if (PlateConfigurations != null && PlateConfigurations.Count > 0)
            {
                begin = PlateConfigurations.Last().EndTime.AddDays(1);
            }

            if(PlateChangeDays.Count > 0)
            {
                while (PlateChangeDays.All(d => d != begin.DayOfWeek))
                {
                    begin = begin.AddDays(-1);
                }
                end = begin.AddDays(1);
                while (PlateChangeDays.All(d => d != end.DayOfWeek))
                {
                    end = end.AddDays(1);
                }
                end = end.AddDays(-1);
            }
            else
            {
                // default to a week
                end = begin.AddDays(7);
            }

            var plateChange = new PlateConfiguration(begin, end);
            PlateConfigurations?.Add(plateChange);
            return plateChange;
        }
        
        public void Remove(PlateConfiguration configuration)
        {
            if (configuration != null)
            {
                int index = PlateConfigurations.IndexOf(configuration);
                if (index >= 0 && index < PlateConfigurations.Count)
                {
                    PlateConfigurations.RemoveAt(index);
                    PressScheduleWindow.WeekControls.RemoveAt(index);
                }
            }
        }
        
        public void AddPlateConfig()
        {
            CreateNewConfig();
        }

        /// <summary>
        /// Schedules the item needed ASAP. 
        /// </summary>
        /// <param name="item">Item to make</param>
        /// <param name="units">Units to make</param>
        /// <param name="date">Time to complete</param>
        /// <returns>Check if the completion time is before the needed time</returns>
        public static bool ScheduleSalesItem(ProductMasterItem item, double units, DateTime date)
        {
            DateTime finishedDateTime = DateTime.MaxValue;

            if (PlateConfigurations.Count == 0)
            {
                Instance.CreateNewConfig();
            }

            // Find closest plate config
            foreach (var plateConfiguration in PlateConfigurations)
            {
                if (plateConfiguration.AddSale(item, ref units, ref finishedDateTime))
                {
                    if (units <= 0) // if done making
                    {
                        break;
                    }
                }
            }

            return finishedDateTime <= date;
        }
    }
}
