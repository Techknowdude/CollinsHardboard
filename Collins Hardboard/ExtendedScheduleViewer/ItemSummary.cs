using System.Windows.Media;
using ModelLib;
using StaticHelpers;

namespace ExtendedScheduleViewer
{
    public class ItemSummary : ObservableObject
    {
        public ProductMasterItem Item
        {
            get { return _item; }
            set { _item = value; }
        }

        public double RunningUnits { get { return CurrentUnits + AddedUnits - RemovedUnits; } }

        public double AddedUnits
        {
            get { return _addedUnits; }
            set
            {
                _addedUnits = value;
                RaisePropertyChangedEvent("AddedFGUnits");
                RaisePropertyChangedEvent("RunningFGUnits");
                RaisePropertyChangedEvent();
            }
        }

        public double RemovedUnits
        {
            get { return _removedUnits; }
            set
            {
                _removedUnits = value;
                RaisePropertyChangedEvent("RunningFGUnits");
                RaisePropertyChangedEvent("RemovedFGUnits");
                RaisePropertyChangedEvent();
            }
        }

        public double CurrentUnits
        {
            get { return _currentUnits; }
            set
            {
                _currentUnits = value;
                RaisePropertyChangedEvent("RunningFGUnits");
                RaisePropertyChangedEvent("RemovedFGUnits");
                RaisePropertyChangedEvent();
            }
        }

        public double RunningFGUnits { get { return RunningUnits*Item.FGRatio;} }
        public double AddedFGUnits { get { return AddedUnits*Item.FGRatio;} }
        public double RemovedFGUnits { get { return RemovedUnits*Item.FGRatio; } }

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
        private ProductMasterItem _item;
        private double _addedUnits;
        private double _removedUnits;
        private double _currentUnits;
    }
}