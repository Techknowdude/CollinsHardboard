using System.Windows.Media;
using ModelLib;
using StaticHelpers;

namespace ExtendedScheduleViewer
{
    public class ItemSummary : ObservableObject
    {
        public ProductMasterItem Item { get; set; }
        public double RunningUnits { get { return CurrentUnits + AddedUnits - RemovedUnits; } }
        public double AddedUnits { get; set; }
        public double RemovedUnits { get; set; }
        public double CurrentUnits { get; set; }

        public Color BackgroundColor { get; set; }

        public ItemSummary(ProductMasterItem item, double current, double added, double removed)
        {
            Item = item;
            CurrentUnits = current;
            AddedUnits = added;
            RemovedUnits = removed;
            BackgroundColor = GetColor();
        }

        private static Color GetColor()
        {
            int current = index;
            index++;
            if (index >= colors.Length)
                index = 0;

            return colors[current];
        }

        private static int index = 0;
        private static Color[] colors = new[] {Colors.Aquamarine,Colors.DarkKhaki,Colors.Orange,Colors.IndianRed};
    }
}