using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Configuration_windows;
using ImportLib;

namespace InventoryViewer
{
    /// <summary>
    /// Interaction logic for EfficiencyViewer.xaml
    /// </summary>
    public partial class EfficiencyViewer : Window
    {
        public ObservableCollection<WeeklyEfficiencyControl> WeekControls { get; set; } 

        public EfficiencyViewer()
        {
            InitializeComponent();

            WeekControls = new ObservableCollection<WeeklyEfficiencyControl>();

            EfficiencyListView.ItemsSource = WeekControls;
            LoadContolData();
        }

        private void LoadContolData()
        {

            if (StaticInventoryTracker.InventoryChanges.Count == 0 && StaticInventoryTracker.IsLoaded)
            {
                EfficiencyListView.ItemsSource = new List<Label>() { new Label() { Content = "No efficiency data" } };

                return;
            }

            // try to load if not loaded and no changes logged.
            if(StaticInventoryTracker .InventoryChanges.Count == 0 && !StaticInventoryTracker.IsLoaded)
                if (!StaticInventoryTracker.LoadDefaults())
                {
                    EfficiencyListView.ItemsSource = new List<Label>() { new Label() { Content = "Failed to load efficiency data" } };

                    return;
                }

            // if there are still none, exit
            if (StaticInventoryTracker.InventoryChanges.Count == 0)
            {
                EfficiencyListView.ItemsSource = new List<Label>() { new Label() { Content = "No efficiency data loaded" } };
                return;
            }

            var trackers = new List<InventoryChange>();
            
            trackers.AddRange(StaticInventoryTracker.InventoryChanges); // make a copy of the list

            DateTime date = trackers.Min(change => change.Date);

            while (trackers.Count > 0)
            {
                
                // get starting day, and then that days start of week

                while (date.DayOfWeek != CalendarControl.StartOfWeek)
                {
                    date = date.AddDays(-1);
                }

                var weeklyChanges = new List<InventoryChange>();
                weeklyChanges.AddRange(trackers.Where(change => change.Date >= date && change.Date < date.AddDays(7))); // get the weeks changes.

                WeekControls.Add(new WeeklyEfficiencyControl(weeklyChanges,date));

                foreach (var inventoryChange in weeklyChanges)
                {
                    trackers.Remove(inventoryChange);
                }

                date = date.AddDays(7);
            }
        }
    }
}
