using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Configuration_windows
{
    /// <summary>
    /// Interaction logic for DayShiftControl.xaml
    /// </summary>
    public partial class DayShiftControl : UserControl
    {

        #region Fields
        private bool _shiftActive;
        private object _shiftInfo;
        private DateTime _date;
        private bool _isOvertime;

        #endregion

        #region Properties

        public bool ShiftActive
        {
            get { return _shiftActive; }
            set
            {
                _shiftActive = value;
                ActiveButton.Content = value ? "Cancel shift" : "Run shift";
            }
        }

        public object ShiftInfo
        {
            get { return _shiftInfo; }
            set { _shiftInfo = value; }
        }

        public bool IsOvertime
        {
            get { return _isOvertime; }
            set
            {
                _isOvertime = value;

                OvertimeButton.Content = value ? "Remove" : "Add OT";
            }
        }

        public DayInformationWindow ParentControl { get; set; }

        private Shift _shift = null;
        private ShiftTime _time = null;

        #endregion
        
        public DayShiftControl(object shiftInfo, DateTime date)
        {
            InitializeComponent();
            _date = date;

            ShiftInfo = shiftInfo;

            _shift = shiftInfo as Shift;
            _time = shiftInfo as ShiftTime;

            if (_shift != null) // if a shift
            {
                ShiftActive = !_shift.ExceptionList.Any(exep => !exep.IsActive && exep.StartTime == date);
                IsOvertime = false;
                ShiftLabel.Content = _shift.LabelName;
                ShiftLabel.Background = new SolidColorBrush(_shift.BackgroundColor);
                ShiftLabel.Foreground = new SolidColorBrush(_shift.ForegroundColor);

            }
            else if (_time != null) // if an exception
            {
                ShiftActive = _time.IsActive;
                IsOvertime = _time.IsOvertime;

                if (!ShiftActive)
                {
                    MainPanel.Children.Remove(TimeButton);
                    MainPanel.Children.Remove(OvertimeButton);
                }
                if (IsOvertime)
                {
                    MainPanel.Children.Remove(OvertimeButton);
                }

                ShiftLabel.Content = _time.Shift.MakeLabel(_time);
                ShiftLabel.Background = new SolidColorBrush(_time.Shift.BackgroundColor);
                ShiftLabel.Foreground = new SolidColorBrush(_time.Shift.ForegroundColor);

            }
        }

        private void ActiveButton_OnClick(object sender, RoutedEventArgs e)
        {
            ShiftActive = !ShiftActive;

            if (_shift != null) // if is a shift, then canceling
            {
                _shift.CancelShift(_date);
            }
            else if (_time != null)
            {
                foreach (var shift in ShiftHandler.CoatingInstance.Shifts.Where(shift => shift.ExceptionList.Contains(_time)))
                {
                    shift.ExceptionList.Remove(_time);
                }
            }
            ParentControl.Refresh();
        }

        private void OvertimeButton_OnClick(object sender, RoutedEventArgs e)
        {
            // show time edit window
            if (ShiftActive)
            {
                TimeSpan duration = _shift == null ? _time.Duration : _shift.Duration;

                TimeSelectWindow selectWindow = new TimeSelectWindow(_date, duration);
                selectWindow.ShowDialog();

                if (selectWindow.Accepted)
                {
                    if (_shift != null)
                    {
                        bool different = _shift.StartTime.TimeOfDay != selectWindow.Start.TimeOfDay || _shift.Duration != selectWindow.Duration;
                        if (different)
                        {
                            _shift.ExceptionList.Add(ShiftTime.ShiftTimeFactory(selectWindow.Start, selectWindow.Duration, true, true, _shift));
                        }
                    }
                    else if (_time != null)
                    {
                        bool different = _time.StartTime.TimeOfDay != selectWindow.Start.TimeOfDay ||
                                         _time.Duration != selectWindow.Duration;
                        if (different)
                        {
                            _time.Shift.ExceptionList.Add(ShiftTime.ShiftTimeFactory(selectWindow.Start, selectWindow.Duration, true, true, _time.Shift));
                        }
                    }
                }
            }
        }

        private void TimeButton_OnClick(object sender, RoutedEventArgs e)
        {
            // show time edit window
            if (ShiftActive)
            {
                TimeSpan duration = _shift == null ? _time.Duration : _shift.Duration;

                TimeSelectWindow selectWindow = new TimeSelectWindow(_date, duration);
                selectWindow.ShowDialog();

                if (selectWindow.Accepted)
                {
                    if (_shift != null)
                    {
                        bool different = _shift.StartTime.TimeOfDay != selectWindow.Start.TimeOfDay || _shift.Duration != selectWindow.Duration;
                        if (different)
                        {
                            _shift.ExceptionList.Add(ShiftTime.ShiftTimeFactory(selectWindow.Start,selectWindow.Duration,true,false,_shift));
                        }
                    }
                    else if(_time != null)
                    {
                        _time.StartTime = selectWindow.Start;
                        _time.Duration = selectWindow.Duration;
                    }
                }
            }
        }
    }
}
