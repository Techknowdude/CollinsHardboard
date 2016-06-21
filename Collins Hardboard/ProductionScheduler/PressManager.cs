using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ModelLib;

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
        private List<DayOfWeek> _plateChangeDays = new List<DayOfWeek>();
        private List<PressItem> _pressItems = new List<PressItem>(); 
        #endregion

        #region Properties
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

        public List<DayOfWeek> PlateChangeDays
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
            catch (Exception)
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
            catch
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
                if (plateConfiguration.EndTime <= end && plateConfiguration.EndTime > start)
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
                var config = AddNewConfig();
                config.Add(item, ref unitCount, dueDate);
                _plateConfigurations.Add(config);
            }

            return added;
        }

        /// <summary>
        /// Creates a new configuration after the last configuration on the list that goes from then until the next plate change.
        /// </summary>
        /// <returns></returns>
        private PlateConfiguration AddNewConfig()
        {
            DateTime begin = DateTime.Today;
            DateTime end = begin;
            if (_plateConfigurations != null && _plateConfigurations.Count > 0)
            {
                begin = _plateConfigurations.Last().EndTime.AddDays(1);
            }
            end = begin.AddDays(1);
            while (PlateChangeDays.All(d => d != end.DayOfWeek))
            {
                end = end.AddDays(1);
            }

            return new PlateConfiguration(begin,end);
        }
    }
}
