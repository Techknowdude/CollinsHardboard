using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.Serialization;
using System.Windows;
using Microsoft.Office.Interop.Excel;

namespace CoatingScheduler
{
    [Serializable]
    public abstract class ICoatingScheduleLogic : DependencyObject
    {

        #region Fields
        private ObservableCollection<ICoatingScheduleLogic> _childrenLogic = new ObservableCollection<ICoatingScheduleLogic>();

        private ICoatingScheduleControl _control;
        private string _coatingLine = String.Empty;
        private ICoatingScheduleLogic _parentLogic;

        #endregion

        #region Properties

        public ObservableCollection<ICoatingScheduleLogic> ChildrenLogic
        {
            get { return _childrenLogic; }
            set { _childrenLogic = value; }
        }
        public ICoatingScheduleLogic ParentLogic
        {
            get { return _parentLogic; }
            set { _parentLogic = value; }
        }
        public ICoatingScheduleControl Control
        {
            get { return _control; }
            set { _control = value; }
        }

        public String CoatingLine
        {
            get { return _coatingLine; }
            set { _coatingLine = value; }
        }

        #endregion

        public abstract void ReconnectToControls();

        public abstract void Save(Stream stream, IFormatter formatter);
        public abstract Tuple<int, int> ExportToExcel(_Worksheet sheet, Int32 column, Int32 row);

        public abstract bool IsFull();

        public abstract void AddLogic(ICoatingScheduleLogic newController = null);
        public void Connect(ICoatingScheduleControl control)
        {
            Control = control;
            control.Connect(this);
            control.UpdateControlData();
          
        }

        public abstract void Connect(ICoatingScheduleLogic logic);
        public abstract void RemoveLogic(ICoatingScheduleLogic child);

        public void RemoveControl()
        {
            ParentLogic.Control.RemoveControl(Control);
        }
        public abstract void DestroySelf();

        public void Disconnect()
        {
            ParentLogic = null;
            Control = null;
        }
        public void SwapControls(ICoatingScheduleLogic other)
        {
            ICoatingScheduleControl otherControlParent = other.ParentLogic.Control;
            ICoatingScheduleControl currentControlParent = ParentLogic.Control;

            ICoatingScheduleControl thisControl = Control;
            ICoatingScheduleControl thatControl = other.Control;

            bool sameParent = otherControlParent.SwapChildControl(thatControl, thisControl);
            
            if(!sameParent) currentControlParent.SwapChildControl(thisControl, thatControl);

        }

        public abstract bool ChildIsTop(ICoatingScheduleLogic child);
        public abstract void SwapUp();
        public abstract void SwapChildUp(ICoatingScheduleLogic upChild, ICoatingScheduleLogic lastParent);
        public abstract void AddControlToBottom(ICoatingScheduleLogic newLogic);

        public abstract void SwapDown();
        public abstract void SwapChildDown(ICoatingScheduleLogic downChild, ICoatingScheduleLogic lastParent);
        public abstract void AddControlToTop(ICoatingScheduleLogic newLogic);
        public abstract void PushDown();
        public abstract void PushChildDown(ICoatingScheduleLogic upChild, ICoatingScheduleLogic lastParent);
        public abstract void PushDownChildren(ICoatingScheduleLogic upChild);
        public abstract void PushUp();
        public abstract void PushChildUp(ICoatingScheduleLogic upChild, ICoatingScheduleLogic lastParent);
        public abstract void PushUpChildren(ICoatingScheduleLogic upChild);

        public bool HasChild(ICoatingScheduleLogic child)
        {
            if (ChildrenLogic.Contains(child)) return true;

            bool childFound = false;

            for (Int32 index = 0; !childFound && index < ChildrenLogic.Count; index++)
            {
                var childLogic = ChildrenLogic[index];
                childFound = childLogic.HasChild(child);
            }
            return childFound;
        }

        public Int32 IndexOfChild(ICoatingScheduleLogic child)
        {
            Int32 foundIndex = -1;

            if (ChildrenLogic.Contains(child)) return ChildrenLogic.IndexOf(child);

            for (Int32 index = 0; foundIndex == -1 && index < ChildrenLogic.Count; index++)
            {
                var childLogic = ChildrenLogic[index];
                if (childLogic.HasChild(child))
                    foundIndex = index;
            }

            return foundIndex;
            
        }
    }
}
