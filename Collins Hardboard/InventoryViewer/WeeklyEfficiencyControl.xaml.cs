using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using ImportLib;

namespace InventoryViewer
{
    /// <summary>
    /// Interaction logic for WeeklyEfficiencyControl.xaml
    /// </summary>
    public partial class WeeklyEfficiencyControl : UserControl
    {
        public DateTime Date { get; set; }
        public ObservableCollection<ItemEfficiencyControl> ItemControls { get; set; } 

        public WeeklyEfficiencyControl(List<InventoryChange> weeklyChanges, DateTime date)
        {
            ItemControls = new ObservableCollection<ItemEfficiencyControl>();
            InitializeComponent();

            Date = date;

            double efficiency = weeklyChanges.Average(change => change.Efficiency);
            EfficiencyTextBox.Text = efficiency.ToString("P");

            foreach (var inventoryChange in weeklyChanges)
            {
                ItemControls.Add(new ItemEfficiencyControl(inventoryChange));
            }

            ControlsListView.ItemsSource = ItemControls;
        }
    }
}
