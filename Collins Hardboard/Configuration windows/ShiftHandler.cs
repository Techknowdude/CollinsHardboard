using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace Configuration_windows
{
    public class ShiftHandler
    {
        // Singleton
        private static ShiftHandler _coatingInstance;
        private static ShiftHandler _productionInstance;

        public static ShiftHandler ProductionInstance
        {
            get { return _productionInstance ?? (_productionInstance = new ShiftHandler(false)); }
        }
        public static ShiftHandler CoatingInstance
        {
            get { return _coatingInstance ?? (_coatingInstance = new ShiftHandler(true)); }
        }

        private readonly bool _isCoating;

        private string DatFileName
        {
            get { return _isCoating ? "shiftconfig.dat" : "productionshiftconfig.dat"; }
        }

        private ShiftHandler(bool isCoating)
        {
            _isCoating = isCoating;
        }

        public ObservableCollection<Shift> Shifts
        {
            get { return _shifts; }
            set { _shifts = value; }
        }

        private ObservableCollection<Shift> _shifts = new ObservableCollection<Shift>();

        public bool LoadShifts(string fileName = "")
        {
            if (fileName == "")
                fileName = DatFileName;

            try
            {
                using (BinaryReader reader = new BinaryReader(new FileStream(fileName, FileMode.Open)))
                {
                    Shifts.Clear();
                    for (Int32 numShifts = reader.ReadInt32(); numShifts > 0; --numShifts)
                    {
                        Shift.Load(reader,_isCoating);
                    }
                }

            }
            catch
            {
                return false;
            }
            return true;
        }

        public bool SaveShifts(string fileName = "")
        {
            if (fileName == "")
                fileName = DatFileName;
            try
            {

                using (BinaryWriter writer = new BinaryWriter(new FileStream(fileName, FileMode.OpenOrCreate)))
                {
                    writer.Write(Shifts.Count);
                    foreach (var shift in Shifts)
                    {
                        shift.Save(writer);
                    }
                }

            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public void AddShift(Shift shift)
        {
            if (Shifts.Count == 0)
            {
                Shifts.Add(shift);
            }
            else
            {
                bool inserted = false;
                for (Int32 i = 0; i < Shifts.Count; i++)
                {
                    Shift current = Shifts[i];
                    bool currentOverlap = false;
                    bool shiftOverlap = false;
                    Int32 currentHour;
                    if (current.StartTime.Hour < (current.StartTime + current.Duration).Hour)
                    {
                        currentHour = current.StartTime.Hour;
                    }
                    else
                    {
                        currentHour  = (current.StartTime + current.Duration).Hour;
                        currentOverlap = true;
                    }
                    Int32 shiftHour;
                    if (shift.StartTime.Hour < (shift.StartTime + shift.Duration).Hour)
                    {
                        shiftHour = shift.StartTime.Hour;
                    }
                    else
                    {
                        shiftHour = (shift.StartTime + shift.Duration).Hour;
                        shiftOverlap = true;
                    }

                    if (shiftHour < currentHour || (shiftHour == currentHour && shiftOverlap && !currentOverlap))
                    {
                        inserted = true;
                        Shifts.Insert(i, shift);
                        i = Shifts.Count;
                    }
                }

                if (!inserted)
                {
                    Shifts.Insert(Shifts.Count, shift);
                }
            }
        }

        public int ShiftsRan(DateTime date)
        {
            return Shifts.Where(shift => shift.DaysList.Contains(date.DayOfWeek)).Count(shift => shift.StartDate <= date && shift.EndDate > date);
        }

        public DateTime GetNextWorkingDay(DateTime currentDay)
        {

            int daysUntil = 1;

            while (!Shifts.Any(shift => shift.DaysList.Contains(currentDay.AddDays(daysUntil).DayOfWeek)))
            {
                daysUntil++;
            }

            return currentDay.AddDays(daysUntil);
        }

        public Shift GetShift(DateTime time)
        {
            return Shifts.FirstOrDefault(
                shift => shift.DaysList.Contains(time.DayOfWeek) && shift.StartTime <= time && time <= shift.EndDate);
        }
    }
}
