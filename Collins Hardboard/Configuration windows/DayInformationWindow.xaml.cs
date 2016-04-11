using System;
using System.Globalization;
using System.Windows;

namespace Configuration_windows
{
    /// <summary>
    /// Interaction logic for DayInformationWindow.xaml
    /// </summary>
    public partial class DayInformationWindow : Window
    {
        public DayControl Day { get; set; }
        public DayInformationWindow(DayControl day)
        {
            InitializeComponent();
            Day = new DayControl( day);

            Refresh();
        }

        public void Refresh()
        {
            MainPanel.Children.Clear();

            foreach (var shift in Day.ShiftInfo)
            {
                DayShiftControl control = new DayShiftControl(shift, Day.Date);
                control.ParentControl = this;
                MainPanel.Children.Add(control);
            }
        }

    }
}
