using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Media;
using System.Xml.Serialization;

namespace Configuration_windows
{
    [Serializable]
    public class Shift
    {
        Shift()
        {
            // For serialization
        }

        #region Fields

        private List<string> _linesCanRunOn = new List<string>();
        private DateTime _startDate;
        private DateTime _endDate;
        private List<ShiftTime> _exceptionList = new List<ShiftTime>();
        private List<DayOfWeek> _daysList = new List<DayOfWeek>();
        private string _name;
        private Color _backgroundColor = Colors.White;
        private Color _foregroundColor = Colors.Black;
        private DateTime _startTime;
        private TimeSpan _duration;
        private static Brush _canceledForeground = Brushes.DarkSlateGray;
        private static Brush _canceledBackground = Brushes.LightSlateGray;

        #endregion

        #region Properties

        [Browsable(false)]
        [XmlElement("Duration")]
        public long ChangeTimeTicks
        {
            get { return _duration.Ticks; }
            set { _duration = new TimeSpan(value); }
        }

        public static Brush CanceledForeground
        {
            get { return _canceledForeground; }
            set { _canceledForeground = value; }
        }

        public static Brush CanceledBackground
        {
            get { return _canceledBackground; }
            set { _canceledBackground = value; }
        }
        public DateTime StartDate
        {
            get { return _startDate; }
            set { _startDate = value; }
        }

        public DateTime EndDate
        {
            get { return _endDate; }
            set { _endDate = value; }
        }

        public List<ShiftTime> ExceptionList
        {
            get { return _exceptionList; }
            set { _exceptionList = value; }
        }

        public List<DayOfWeek> DaysList
        {
            get { return _daysList; }
            set { _daysList = value; }
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public Color BackgroundColor
        {
            get { return _backgroundColor; }
            set { _backgroundColor = value; }
        }

        public Color ForegroundColor
        {
            get { return _foregroundColor; }
            set { _foregroundColor = value; }
        }

        [XmlIgnore]
        public TimeSpan Duration
        {
            get { return _duration; }
            set { _duration = value; }
        }

        public double Hours(DateTime day)
        {
            if (ExceptionList.Any(ex => ex.IsActive == false && SameDay(ex.StartTime, day))) return 0;

            var exception = ExceptionList.FirstOrDefault(ex => ex.IsOvertime && SameDay(ex.StartTime, day));
            if (exception != null)
            {
                return exception.Duration.TotalHours + Duration.TotalHours;
            }
            
            return Duration.TotalHours;
        }

        public DateTime StartTime
        {
            get { return _startTime; }
            set { _startTime = value; }
        }

        #endregion
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="startTime">Start time of shift</param>
        /// <param name="duration">Length of shift</param>
        /// <param name="start">Date the shift is started</param>
        /// <param name="end">End of shift planning</param>
        /// <param name="exceptionList"></param>
        /// <param name="daysList"></param>
        /// <param name="isCoating"></param>
        /// <param name="foreground"></param>
        /// <param name="background"></param>
        /// <returns></returns>
        public static Shift ShiftFactory(string name, DateTime startTime, TimeSpan duration, DateTime start = default (DateTime), 
            DateTime end = default (DateTime), List<ShiftTime> exceptionList = null, List<DayOfWeek> daysList = null, 
           bool isCoating = true, Color foreground = default (Color), Color background = default (Color))
        {
            Shift shift = new Shift(start, end, exceptionList, daysList, name, startTime, duration, foreground,background);
            foreach (var exep in shift.ExceptionList)
            {
                exep.Shift = shift;
            }
            if(isCoating)
                ShiftHandler.CoatingInstance.AddShift(shift);
            else
                ShiftHandler.ProductionInstance.AddShift(shift);
            
            return shift;
        }

        private Shift(DateTime start, DateTime end, List<ShiftTime> exceptionList, List<DayOfWeek> daysList, string name, 
            DateTime startTime, TimeSpan duration, Color foreground, Color background)
        {
            if (exceptionList != null) _exceptionList = exceptionList;
            if (daysList != null) _daysList = daysList;
            _name = name;
            _startDate = start;
            _endDate = end;
            _startTime = startTime;
            _duration = duration;
            _foregroundColor = foreground;
            _backgroundColor = background;
        }

        public override string ToString()
        {
            return Name;
        }

        public string LabelName
        {
            get
            {
                return String.Format("{0} ({1} - {2})", Name, StartTime.ToString("h:mm tt"),
                    (StartTime + Duration).ToString("h:mm tt"));
            }
        }

        public List<string> LinesCanRunOn
        {
            get { return _linesCanRunOn; }
            set { _linesCanRunOn = value; }
        }

        public string MakeLabel(ShiftTime excep)
        {
                return String.Format("{0} ({1} - {2}){3}", Name, excep.StartTime.ToString("h:mm tt"),
                    (excep.StartTime + excep.Duration).ToString("h:mm tt"),excep.IsOvertime ? "OT" : "");
        }

        public static string DateTimeTo12H(DateTime date)
        {
            return date.ToString("h:mm tt");
        }

        public static bool DateWithinRange(DateTime target, DateTime start, DateTime end)
        {
            return (target.Year > start.Year || (target.Year == start.Year && target.DayOfYear >= start.DayOfYear)) &&
                (target.Year < end.Date.Year || (target.Year == end.Year && target.DayOfYear <= end.DayOfYear));
        }

        public static bool SameDay(DateTime left, DateTime right)
        {
            return left.DayOfYear == right.DayOfYear && left.Year == right.Year;
        }

        public void Save(BinaryWriter writer)
        {
            writer.Write(StartDate.ToShortDateString());
            writer.Write(EndDate.ToShortDateString());
            writer.Write(ExceptionList.Count);
            foreach (var shiftTime in ExceptionList)
            {
                shiftTime.Save(writer);
            }
            writer.Write(DaysList.Count);
            foreach (var dayOfWeek in DaysList)
            {
                writer.Write((int)dayOfWeek);
            }
            writer.Write(Name);
            writer.Write(BackgroundColor.ToString());
            writer.Write(ForegroundColor.ToString());
            writer.Write(StartTime.ToShortTimeString());
            writer.Write(Duration.ToString());
        }

        public static Shift Load(BinaryReader reader, bool isCoating)
        {

            DateTime start = DateTime.Parse( reader.ReadString());
            DateTime end = DateTime.Parse(reader.ReadString());

            
            List<ShiftTime> exceptionList = new List<ShiftTime>();
            for(Int32 numExeptions = reader.ReadInt32(); numExeptions > 0; -- numExeptions)
            {
                ShiftTime newTime = ShiftTime.Load(reader);
                exceptionList.Add(newTime);
            }

            List<DayOfWeek> daysList = new List<DayOfWeek>();
            for (Int32 numDays = reader.ReadInt32(); numDays > 0; --numDays)
            {
                DayOfWeek day = (DayOfWeek) reader.ReadInt32();
                daysList.Add(day);
            }

            String name = reader.ReadString();
            Color foreground = default(Color);
            Color background = default (Color);
            var convertFromString = ColorConverter.ConvertFromString(reader.ReadString());
            if (convertFromString != null)
            {
                background = (Color) convertFromString;
            }
            convertFromString = ColorConverter.ConvertFromString(reader.ReadString());
            if (convertFromString != null)
            {
                foreground = (Color)convertFromString;
            }

            DateTime startTime = DateTime.Parse(reader.ReadString());
            TimeSpan duration = TimeSpan.Parse(reader.ReadString());

            return ShiftFactory(name,startTime,duration,start,end,exceptionList,daysList,isCoating,foreground,background);
        }

        public void CancelShift(DateTime date)
        {
            if(ExceptionList.All(exep => exep.StartTime != date)) // toggle shift as canceled
                ExceptionList.Add(ShiftTime.ShiftTimeFactory(date,_duration,false,false,this));
            else // toggle shift to not canceled
            {
                ExceptionList.Remove(ExceptionList.FirstOrDefault(exep => exep.StartTime == date));
            }
        }
    }
}
