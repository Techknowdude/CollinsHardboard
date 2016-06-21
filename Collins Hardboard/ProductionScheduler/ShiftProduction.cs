using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoatingScheduler;

namespace ProductionScheduler
{
    class ShiftProduction
    {
        private PressShift _pressShift;
        private CoatingScheduleLine _coatingScheduleShift;

        public DateTime Start {
            get { return _coatingScheduleShift.Date; } }
        public DateTime End { get { return Start + _coatingScheduleShift.Shift.Duration; } }

        public ShiftProduction(CoatingScheduleLine coatingShift)
        {
            _coatingScheduleShift = coatingShift;
            //PressManager.Instance.GetPressShift(Start, End);
        }
    }
}
