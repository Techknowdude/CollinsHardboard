using System.Collections.ObjectModel;
using System.Linq;
using CoatingScheduler;
using ModelLib;
using ProductionScheduler;

namespace ExtendedScheduleViewer
{
    internal class TrackingShift
    {
        private ObservableCollection<PressShift> pressShifts;
        private CoatingScheduleShift coatingShift;

        public TrackingShift(CoatingScheduleShift shift)
        {
            pressShifts = new ObservableCollection<PressShift>();
            coatingShift = shift;
        }

        public double GetProduced(ProductMasterItem item)
        {
            double modification = 0;

            foreach (var pressShift in pressShifts)
            {
                foreach (var prod in pressShift.Produced.Where(i => i.MasterItem == item))
                {
                    modification += prod.UnitsMade;
                }
            }
            return modification;
        }

        public double GetConsumed(ProductMasterItem item)
        {
            double consumed = 0;

            consumed = coatingShift.UnitsConsumed(item);

            return consumed;
        }
    }
}