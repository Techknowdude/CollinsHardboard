using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Configuration_windows
{
    /// <summary>
    /// Interaction logic for CalendarControl.xaml
    /// </summary>
    public partial class CalendarControl : UserControl
    {
        #region Fields

        private Int32 _month;
        private Int32 _year;
        private static DayOfWeek _startOfWeek = DayOfWeek.Monday;
        private DayControl[] _dayControls;
        private Int32 _numWeeks = 6;
        private Label[] _weekLabels;
        private DateTime _startDate;
        private DateTime _endDate;
        #endregion

        #region Properties
        public Int32 Month
        {
            get { return _month; }
            set
            {
                if (value > 12)
                {
                    _month = 1; // ensure month is always 1 through 12
                    Year++; // add years
                }
                else if (value < 1)
                {
                    _month = 12;
                    Year--;
                }
                else
                {
                    _month = value;
                }
                MonthLabel.Content = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(_month);
            }
        }

        public Int32 Year
        {
            get { return _year; }
            set
            {
                if (_year != value)
                {
                    _year = value;
                    YearLabel.Content = _year; 
                }
            }
        }

        public static DayOfWeek StartOfWeek
        {
            get { return _startOfWeek; }
            set { _startOfWeek = value; }
        }

        public DayControl[] DayControls
        {
            get { return _dayControls; }
            set { _dayControls = value; }
        }

        public Int32 NumWeeks
        {
            get { return _numWeeks; }
            set { _numWeeks = value; }
        }

        public Label[] WeekLabels
        {
            get { return _weekLabels; }
            set { _weekLabels = value; }
        }

        public DateTime StartDate
        {
            get { return _startDate; }
        }

        public DateTime EndDate
        {
            get { return _endDate; }
        }

        public bool IsCoating { get; set; }

        public ObservableCollection<Shift> Shifts
        {
            get
            {
                return IsCoating ? ShiftHandler.CoatingInstance.Shifts : ShiftHandler.ProductionInstance.Shifts;
            }
        }
        #endregion

        public CalendarControl()
        {
            InitializeComponent();
            Year = DateTime.Now.Year;
            Month = DateTime.Now.Month;
        }

        public void Refresh()
        {
            if(IsCoating)
            {
                if (ShiftHandler.CoatingInstance.Shifts.Count == 0)
                    ShiftHandler.CoatingInstance.LoadShifts();
            }
            else
            {
                if (ShiftHandler.ProductionInstance.Shifts.Count == 0)
                    ShiftHandler.ProductionInstance.LoadShifts();
            }

            LoadControls();
            LoadShiftInformation();
        }

        private void LoadControls()
        {
            MonthLabel.Content = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(_month);
            if (DayControls != null)
            {
                foreach (var dayControl in DayControls)
                {
                    dayControl.MouseDown -= day_MouseDown;
                    dayControl.ShiftInfo.Clear();
                    CalendarGrid.Children.Remove(dayControl);
                }
            }
            if (WeekLabels != null)
            {
                foreach (var weekLabel in WeekLabels)
                {
                    CalendarGrid.Children.Remove(weekLabel);
                }
            }
            

            DayControls = new DayControl[7*NumWeeks];
            WeekLabels = new Label[NumWeeks];
            Int32 weekNumber = 0;
            DateTime month = new DateTime(_year, _month, 1);
            DateTime startDay = month;

            if (startDay.DayOfWeek < _startOfWeek)
                startDay = startDay.AddDays(-7);

            startDay = startDay.AddDays(_startOfWeek - startDay.DayOfWeek);
            _startDate = startDay;
        
            //while (startDay.DayOfWeek != _startOfWeek)
              //  startDay = startDay.AddDays(-1);

            weekNumber = CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(month, CalendarWeekRule.FirstDay,
                    _startOfWeek);
            
            for (Int32 week = 0; week < NumWeeks; ++week, ++weekNumber)
            {
                Int32 row = CalendarGrid.RowDefinitions.IndexOf(Week1Row) + week;

                Label weekLabel = new Label();
                WeekLabels[week] = weekLabel;
                weekLabel.SetValue(Grid.ColumnProperty, CalendarGrid.ColumnDefinitions.IndexOf(WeekNumberColumn));
                weekLabel.SetValue(Grid.RowProperty, row);

                weekLabel.Content = "Week " + weekNumber;
                CalendarGrid.Children.Add(weekLabel);

                for (Int32 numDay = 0; numDay < 7; ++numDay)
                {
                    DayControl day = new DayControl();
                    day.MouseDown += day_MouseDown;
                    DayControls[7*week + numDay] = day;
                    day.SetValue(Grid.RowProperty, row);
                    day.SetValue(Grid.ColumnProperty, CalendarGrid.ColumnDefinitions.IndexOf(DayColumn1) + numDay);
                    CalendarGrid.Children.Add(day);
                    day.Date = startDay;
                    _endDate = startDay;
                    startDay = startDay.AddDays(1);
            
                }
            }
            DayOfWeek _currentDay = _startOfWeek;
            for (Int32 i = 0; i < 7; i++)
            {
                Label dayLabel = new Label();
                dayLabel.Content = _currentDay.ToString();
                dayLabel.SetValue(Grid.RowProperty,CalendarGrid.RowDefinitions.IndexOf(DayTitleRow));
                dayLabel.SetValue(Grid.ColumnProperty,CalendarGrid.ColumnDefinitions.IndexOf(DayColumn1) + i);
                CalendarGrid.Children.Add(dayLabel);
                _currentDay = ((int) _currentDay) < (int) DayOfWeek.Saturday ? _currentDay + 1 : 0;
            }
        }

        //// testing stuff
        //private static Int32 backnum = 0;
        //static Brush[] backBrushes = {Brushes.Black,Brushes.Blue,Brushes.Red,Brushes.SlateGray};
        //static Brush[] frontBrushes = {Brushes.White,Brushes.LightGray,Brushes.White,Brushes.Black};
        void day_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Testing stuff
            DayControl day = sender as DayControl;
            if (day != null)
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    DayInformationWindow dayInformationWindow = new DayInformationWindow(day);
                    dayInformationWindow.ShowDialog();
                    Refresh();
                } 
                
               // MessageBox.Show(String.Format("Day clicked. Date is: {0}", day.Date));
                
            }
        }

        public void LoadShiftInformation()
        {
            foreach (var dayControl in DayControls)
            {
                var control = dayControl;
                foreach (var shift in Shifts.Where(shift => Shift.DateWithinRange(control.Date,shift.StartDate,shift.EndDate)))
                {
                    if (shift.DaysList.Any(day => day == control.Date.DayOfWeek))
                    {
                        bool labelAdded = false;

                        // add sepecific times
                        foreach (var source in
                                shift.ExceptionList.Where(
                                    exep => Shift.SameDay(exep.StartTime, control.Date) && exep.IsActive && !exep.IsOvertime))
                        {
                            labelAdded = true;
                            AddDayLabel(control.Date, source, shift.MakeLabel(source), new SolidColorBrush(shift.ForegroundColor),
                                new SolidColorBrush(shift.BackgroundColor));
                        }
                        // If no special times and not canceled.
                        if (!labelAdded && !shift.ExceptionList.Any(
                                exep => Shift.SameDay(exep.StartTime, control.Date) && exep.IsActive == false))
                        {
                            AddDayLabel(control, shift);
                        }
                        else if (!labelAdded)
                        {
                            // add place holder for restoration
                            AddDayLabel(control.Date, shift, shift.LabelName, Shift.CanceledForeground, Shift.CanceledBackground);
                        }

                        // add overtime
                        foreach (var source in
                                shift.ExceptionList.Where(
                                    exep => Shift.SameDay(exep.StartTime, control.Date) && exep.IsActive && exep.IsOvertime))
                        {
                            AddDayLabel(control.Date, source, shift.MakeLabel(source), new SolidColorBrush(shift.ForegroundColor),
                                new SolidColorBrush(shift.BackgroundColor));
                        }
                    }
                }
            }
        }

        private void AddDayLabel(DateTime date, ShiftTime source, string text, SolidColorBrush foreground, SolidColorBrush background)
        {
            DayControl day = DayControls.FirstOrDefault(x => x.Date.DayOfYear == date.DayOfYear && x.Date.Year == date.Year);

            if (day != null)
            {
                Label label = new Label();
                if (text != null) label.Content = text;
                if (background != null) label.Background = background;
                if (foreground != null) label.Foreground = foreground;
                day.DayItems.Add(label);
                day.ShiftInfo.Add(source);
            }
        }

        /// <summary>
        /// Set the day of the week to start on
        /// </summary>
        /// <param name="day"></param>
        public void SetStartDay(DayOfWeek day)
        {
            _startOfWeek = day;
            LoadControls();
        }

        /// <summary>
        /// Sets the year of the calendar
        /// </summary>
        /// <param name="month">Jan = 1, Feb = 2, etc. Exception on value > 12</param>
        public void SetMonth(Int32 month)
        {
            Month = month;
            Refresh();
        }

        /// <summary>
        /// Set the year of the calendar
        /// </summary>
        /// <param name="year">Example: 2015</param>
        public void SetYear(Int32 year)
        {
            Year = year;
            Refresh();
        }

        /// <summary>
        /// Advance the calendar to the next month
        /// </summary>
        public void NextMonth()
        {
            ++Month;
            Refresh();
        }

        /// <summary>
        /// Move the calendar one month backwards
        /// </summary>
        public void PrevMonth()
        {
            --Month;
            Refresh();
        }

        private void PrevMonthButton_OnClick(object sender, RoutedEventArgs e)
        {
            PrevMonth();
        }

        private void NextMonthButton_OnClick(object sender, RoutedEventArgs e)
        {
            NextMonth();
        }

        public void ClearDayLabels(DateTime date)
        {
            DayControl day = DayControls.FirstOrDefault(x => x.Date == date);

            if(day != null)
                day.DayItems.Clear();
        }

        public void AddDayLabel(DateTime date, Shift shift, string text, Brush foreground = null, Brush background = null)
        {
            DayControl day = DayControls.FirstOrDefault(x => x.Date.DayOfYear == date.DayOfYear && x.Date.Year == date.Year);

            if (day != null)
            {
                Label label = new Label();
                if (text != null) label.Content = text;
                if (background != null) label.Background = background;
                if (foreground != null) label.Foreground = foreground;
                day.DayItems.Add(label);
                day.ShiftInfo.Add(shift);
            }
        }
        public void AddDayLabel(DayControl day, Shift shift)
        {
            if (day != null && shift != null)
            {
                Label label = new Label();
                label.Content = shift.LabelName;
                label.Background = new SolidColorBrush(shift.BackgroundColor);
                label.Foreground = new SolidColorBrush(shift.ForegroundColor);
                day.DayItems.Add(label);
                day.ShiftInfo.Add(shift);
            }
        }

        public void RemoveDayLabel(DateTime date, String text)
        {
            DayControl day = DayControls.FirstOrDefault(x => x.Date == date);

            if (day != null)
            {
                Label label = day.DayItems.FirstOrDefault(x => x.Content == text);
                if (label != null)
                    day.DayItems.Remove(label);
            }
        }
    }
}
