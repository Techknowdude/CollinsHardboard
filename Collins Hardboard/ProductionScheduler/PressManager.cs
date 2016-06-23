﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
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
        private List<PlateConfiguration> _plateConfigurations = new List<PlateConfiguration>(); 

        // Old Stuff
        private const string DatFile = "pressManager.dat";
        private Int32 _numPlates = 0;
        private Int32 _numPlateChangesPerWeek = 1;
        private ObservableCollection<DayOfWeek> _plateChangeDays = new ObservableCollection<DayOfWeek>();
        private List<PressItem> _pressItems = new List<PressItem>();
        private TimeSpan _delayTime = TimeSpan.FromHours(16);

        #endregion

        #region Properties

        public TimeSpan DelayTime
        {
            get { return _delayTime; }
            set { _delayTime = value; }
        }

        public Int32 NumPlates
        {
            get { return _numPlates; }
            set { _numPlates = value; }
        }
        #endregion

        #region Singleton

        private static PressManager _instance = null;
        public static PressManager Instance
        {
            get { return _instance ?? (_instance = new PressManager()); }
        }

        public Int32 NumPlateChangesPerWeek
        {
            get { return _numPlateChangesPerWeek; }
            set { _numPlateChangesPerWeek = value; }
        }

        public ObservableCollection<DayOfWeek> PlateChangeDays
        {
            get { return _plateChangeDays; }
            set { _plateChangeDays = value; }
        }

        public List<PressItem> PressItems
        {
            get { return _pressItems; }
            set { _pressItems = value; }
        }

        public static PressScheduleWindow Window { get; set; }

        #endregion

        private PressManager()
        {
            Load();
            ShiftHandler.ProductionInstance.LoadShifts();
        }

        public static bool Save()
        {
            try
            {
                using (BinaryWriter writer = new BinaryWriter(new FileStream(DatFile, FileMode.OpenOrCreate)))
                {
                    return Save(writer);
                }
            }
            catch (Exception exception)
            {
                return false;
            }
        }

        public static bool Save(BinaryWriter writer)
        {
            bool success = true;

            try
            {
                writer.Write(Instance.NumPlates);
                writer.Write(Instance.NumPlateChangesPerWeek);
                writer.Write(Instance.PlateChangeDays.Count);
                foreach (var plateChangeDay in Instance.PlateChangeDays)
                {
                    writer.Write((Int32)plateChangeDay);
                }
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
                using (BinaryReader reader = new BinaryReader(new FileStream(DatFile, FileMode.Open)))
                {
                    return Load(reader);
                }
            }
            catch (Exception exception)
            {
                return false;
            }
        }
        public static bool Load(BinaryReader reader)
        {
            bool success = true;
            try
            {
                Instance.NumPlates = reader.ReadInt32();
                Instance.NumPlateChangesPerWeek = reader.ReadInt32();

                Instance.PlateChangeDays.Clear();
                Int32 numChanges = reader.ReadInt32();
                for (; numChanges > 0; --numChanges)
                {
                    DayOfWeek day = (DayOfWeek)reader.ReadInt32();
                    Instance.PlateChangeDays.Add(day);
                }
            }
            catch (Exception)
            {
                success = false;
            }

            return success;
        }

        public static DateTime ScheduleItem(ProductMasterItem item, double count)
        {
            DateTime finishedDateTime = DateTime.MaxValue;

            // Find closest plate config
            foreach (var plateConfiguration in Instance._plateConfigurations)
            {
                plateConfiguration.Add(item, ref count);
            }

            return finishedDateTime;
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

            foreach (var plateConfiguration in _plateConfigurations)
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
            for (int index = 0; unitCount > 0 && index < _plateConfigurations.Count; index++)
            {
                var configuration = _plateConfigurations[index];
                // keep true that it was added. Check unit count for full completion.
                added = added || configuration.Add(item, ref unitCount, dueDate);
            }
            if (unitCount > 0)
            {
                var config = CreateNewConfig();
                config.Add(item, ref unitCount, dueDate);
                _plateConfigurations.Add(config);
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
            if (_plateConfigurations != null && _plateConfigurations.Count > 0)
            {
                begin = _plateConfigurations.Last().EndTime.AddDays(1);
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
            _plateConfigurations?.Add(plateChange);
            return plateChange;
        }
        
        public void Remove(PlateConfiguration configuration)
        {
            if (configuration != null)
            {
                int index = _plateConfigurations.IndexOf(configuration);
                if (index >= 0)
                {
                    _plateConfigurations.RemoveAt(index);
                    PressScheduleWindow.WeekControls.RemoveAt(index);
                }
            }
        }
    }
}
