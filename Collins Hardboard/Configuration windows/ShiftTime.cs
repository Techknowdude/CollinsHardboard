﻿using System;
using System.IO;

namespace Configuration_windows
{
    public class ShiftTime
    {
        #region Fields

        private DateTime _startTime;
        private TimeSpan _duration;
        private bool _isActive = true;
        private bool _isOvertime = false;
        private static Shift _shift;

        #endregion

        #region Properties

        public DayOfWeek Day
        {
            get { return _startTime.DayOfWeek; }
        }

        public DateTime StartTime
        {
            get { return _startTime; }
            set { _startTime = value; }
        }

        public TimeSpan Duration
        {
            get { return _duration; }
            set { _duration = value; }
        }

        public bool IsActive
        {
            get { return _isActive; }
            set { _isActive = value; }
        }

        public bool IsOvertime
        {
            get { return _isOvertime; }
            set { _isOvertime = value; }
        }

        public Shift Shift
        {
            get { return _shift; }
            set { _shift = value; }
        }

        #endregion

        public static ShiftTime ShiftTimeFactory(DateTime startTime, TimeSpan duration, bool isActive = true, bool isOvertime = false, Shift shift = null)
        {
            return new ShiftTime(startTime,duration, isActive, isOvertime, shift);
        }

        private ShiftTime(DateTime startTime, TimeSpan duration, bool isActive, bool isOvertime, Shift shift)
        {
            _startTime = startTime;
            _duration = duration;
            _isActive = isActive;
            _isOvertime = isOvertime;
            _shift = shift;
        }

        public void Save(BinaryWriter writer)
        {
            writer.Write(StartTime.ToString());
            writer.Write(Duration.ToString());
            writer.Write(IsActive);
            writer.Write(IsOvertime);
        }

        public static ShiftTime Load(BinaryReader reader)
        {
            String readString = reader.ReadString();
            DateTime startTime = DateTime.Parse(readString);
            readString = reader.ReadString();
            TimeSpan duration = TimeSpan.Parse(readString);
            bool active = reader.ReadBoolean();
            bool overtime = reader.ReadBoolean();

            return ShiftTimeFactory(startTime, duration, active, overtime, _shift);
        }
    }
}
