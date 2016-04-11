using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace Configuration_windows
{
    /// <summary>
    /// Interaction logic for DayControl.xaml
    /// </summary>
    public partial class DayControl : UserControl
    {
        #region Fields
        private ObservableCollection<Label> _dayItems = new ObservableCollection<Label>();
        private String _title = "[Title Here]";
        private DateTime _date;
        private List<object> _shiftInfo = new List<object>();

        #endregion

        #region Properties
        
        public string Title
        {
            get { return _title; } 
            set { _title = value; }
        }

        public ObservableCollection<Label> DayItems
        {
            get { return _dayItems; }
            set { _dayItems = value; }
        }

        public DateTime Date
        {
            get { return _date; }
            set
            {
                if (_date != value)
                {
                    Title = value.Day.ToString();
                    _date = value;
                }
            }
        }

        public List<object> ShiftInfo
        {
            get { return _shiftInfo; }
            set { _shiftInfo = value; }
        }

        #endregion
        public DayControl()
        {
            InitializeComponent();
            DataContext = this;

            ItemsGrid.ItemsSource = DayItems;
        }

        public DayControl(DayControl day)
        {
            InitializeComponent();
            Date = day.Date;
            Title = day.Title;
            foreach (var dayItem in day.DayItems)
            {
                DayItems.Add(new Label(){Content = dayItem.Content, Background = dayItem.Background, Foreground = day.Foreground});
            }

            ShiftInfo = day.ShiftInfo;

            DataContext = this;

            ItemsGrid.ItemsSource = DayItems;
        }
    }
}
