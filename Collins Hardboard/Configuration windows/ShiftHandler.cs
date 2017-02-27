using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using StaticHelpers;

namespace Configuration_windows
{
    [Serializable]
    public class ShiftHandler
    {
        protected XmlSerializer Serializer = new XmlSerializer(typeof(ShiftHandler));

        protected ShiftHandler()
        {
            // used for serialization
        }
        // Singleton
        [NonSerialized]
        private static ShiftHandler _coatingInstance;
        [NonSerialized]
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
                using (Stream stream = new FileStream(fileName, FileMode.Open))
                {
                    ShiftHandler loadedHandler = (ShiftHandler)Serializer.Deserialize(stream);
                    Shifts.Clear();
                    foreach (var shift in loadedHandler.Shifts)
                    {
                        Shifts.Add(shift);
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
                // clear file
                File.Create(fileName).Close();
                using (Stream stream = new FileStream(fileName, FileMode.OpenOrCreate))
                {
                    Serializer.Serialize(stream,this);
                }
            }
            catch (Exception exception)
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

            while (Shifts.Count > 0 && !Shifts.Any(shift => shift.DaysList.Contains(currentDay.AddDays(daysUntil).DayOfWeek)))
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

        public DateTime GetPreviousShiftStart(DateTime date, Shift currentShift)
        {
            DateTime startTime = StaticFunctions.GetDayAndTime(date, currentShift.StartTime);

            // account for shifts that span multiple days. Use the starting day...
            if ((new TimeSpan(currentShift.StartTime.Hour, currentShift.StartTime.Minute, currentShift.StartTime.Second) +
                 currentShift.Duration).TotalHours > 24)
                startTime = startTime.AddDays(-1);

            DateTime currentDateTime = startTime;
            bool closer = true;
            // Find the end time closest to the current shift start time while not going over - Welcome to The Shift is Right?
            TimeSpan closestdif = TimeSpan.MaxValue;

            int infinityPrevention = 356; // dont go back more than a year...

            do
            {
                --infinityPrevention;

                if (Shifts.Count == 1)
                {
                    var shift = Shifts[0];
                    var found = false;
                    var shiftStart = StaticFunctions.GetDayAndTime(currentDateTime, shift.StartTime);
                    // go back a day until there is a shift on that day
                    do
                    {
                        shiftStart = shiftStart.AddDays(-1);
                        if (shift.DaysList.Contains(shiftStart.DayOfWeek))
                        {
                            closestdif = startTime - shiftStart;
                            found = true;
                        }

                    } while (!found && infinityPrevention > 0);
                    break;
                }
                else
                {
                    foreach (var shift in Shifts)
                {
                    if (shift.DaysList.Contains(currentDateTime.DayOfWeek))
                    {
                        closer = false; // something runs on this day. If it's not closer, then quit
                        var shiftStart = StaticFunctions.GetDayAndTime(currentDateTime, shift.StartTime);
                        if ((new TimeSpan(shiftStart.Hour, shiftStart.Minute, shiftStart.Second) + shift.Duration).TotalHours > 24)
                            shiftStart = shiftStart.AddDays(-1);


                        TimeSpan dif = startTime - shiftStart;
                        if (dif.TotalHours > 0 && dif < closestdif)
                        {
                            closestdif = dif;
                            closer = true;
                        }
                        else if(dif.TotalHours < 0)
                        {
                            closer = true;
                        }
                    }
                }}
                currentDateTime = currentDateTime.AddDays(-1); // go back a day

            } while (closer && infinityPrevention > 0);

            if(infinityPrevention == 0) throw new ArgumentOutOfRangeException("No shifts before the given one.");
            
            return startTime - closestdif;
        }

        public Shift GetPreviousShift(Shift current)
        {
            int index = Shifts.IndexOf(current);
            if (index > 0)
                return Shifts[index - 1];
            return Shifts.Last();
        }

        /// <summary>
        /// Gets the number of available hours on this day
        /// </summary>
        /// <param name="day"></param>
        /// <param name="line"></param>
        /// <returns></returns>
        public double GetHoursOnDay(DateTime day, string line)
        {
            double hours = 0;
            foreach (var shift in Shifts.Where(s => s.LinesCanRunOn.Contains(line)))
            {
                hours += shift.Hours(day);
            }

            return 0;
        }
    }
}
