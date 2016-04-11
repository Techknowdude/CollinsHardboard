using System.Windows;

namespace CoatingScheduler
{
    public interface ICoatingScheduleControl
    {
        // TODO: Fix vertical text of note boxes
        void Add_Button(object sender, RoutedEventArgs e);
        void AddControlToBottom(ICoatingScheduleLogic logic);
        void AddControlToTop(ICoatingScheduleLogic logic);
        void RemoveControl(ICoatingScheduleControl child);
        void Connect(ICoatingScheduleLogic logic);
        void Connect(ICoatingScheduleControl parent);
        ICoatingScheduleLogic GetLogic();
        void DestroySelf();
        void Disconnect();
        ICoatingScheduleControl SwapControlWithBottom(ICoatingScheduleControl newControl);
        ICoatingScheduleControl SwapControlWithTop(ICoatingScheduleControl newControl);
        void UpdateControlData();
        bool SwapChildControl(ICoatingScheduleControl oldControl, ICoatingScheduleControl newControl);
    }
}
