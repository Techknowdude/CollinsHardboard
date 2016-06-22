using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ModelLib;

namespace ExtendedScheduleViewer
{
    [Serializable]
    public class ExtendedSchedule
    {
        private List<ProductMasterItem> _watchList;
        private ObservableCollection<TrackingShift> _trackingPairs; 

        public void Update()
        {
            
        }
    }
}
