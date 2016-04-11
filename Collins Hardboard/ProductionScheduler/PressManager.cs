using System;
using System.Collections.Generic;
using System.IO;
using ModelLib;

namespace ProductionScheduler
{
    public class PressManager
    {

        #region Fields

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

    }
}
