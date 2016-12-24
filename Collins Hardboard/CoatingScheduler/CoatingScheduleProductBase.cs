using System;
using System.IO;
using System.Runtime.Serialization;
using Microsoft.Office.Interop.Excel;

namespace CoatingScheduler
{
    [Serializable]
    public abstract class CoatingScheduleProductBase : ICoatingScheduleLogic
    {

        private static Int32 _excelWidth = 4;

        public static Int32 ExcelWidth
        {
            get { return _excelWidth; }
            set { _excelWidth = value; }
        }

        public override void AddLogic(ICoatingScheduleLogic newController = null)
        {
            throw new NotImplementedException();
        }

        public override void Connect(ICoatingScheduleLogic logic)
        {
            ParentLogic = logic;
        }

        public override void RemoveLogic(ICoatingScheduleLogic child)
        {
            throw new NotImplementedException();
        }

        public override void DestroySelf()
        {
            ParentLogic.RemoveLogic(this);
            Disconnect();
        }

        public override bool ChildIsTop(ICoatingScheduleLogic child)
        {
            return ParentLogic.ChildIsTop(this);
        }

        public override void SwapUp()
        {
            ParentLogic.SwapChildUp(this, this);
        }

        public override void SwapChildUp(ICoatingScheduleLogic upChild, ICoatingScheduleLogic lastParent)
        {
            throw new NotImplementedException();
        }

        public override void AddControlToBottom(ICoatingScheduleLogic newLogic)
        {
            throw new NotImplementedException();
        }

        public override void SwapDown()
        {
            ParentLogic.SwapChildDown(this, this);
        }

        public override void SwapChildDown(ICoatingScheduleLogic downChild, ICoatingScheduleLogic lastParent)
        {
            throw new NotImplementedException();
        }

        public override void AddControlToTop(ICoatingScheduleLogic newLogic)
        {
            throw new NotImplementedException();
        }

        public override void PushDown()
        {
            throw new NotImplementedException();
        }

        public override void PushChildDown(ICoatingScheduleLogic upChild, ICoatingScheduleLogic lastParent)
        {
            throw new NotImplementedException();
        }

        public override void PushDownChildren(ICoatingScheduleLogic upChild)
        {
            throw new NotImplementedException();
        }

        public override void PushUp()
        {
            throw new NotImplementedException();
        }

        public override void PushChildUp(ICoatingScheduleLogic upChild, ICoatingScheduleLogic lastParent)
        {
            throw new NotImplementedException();
        }

        public override void PushUpChildren(ICoatingScheduleLogic upChild)
        {
            throw new NotImplementedException();
        }

        public virtual void SpreadUnits()
        {
        }

        public abstract override void Save(Stream stream, IFormatter formatter);

        public abstract override Tuple<int, int> ExportToExcel(_Worksheet sheet, Int32 column, Int32 row);

        public static CoatingScheduleProductBase Load(BinaryReader reader)
        {
            string baseType = reader.ReadString();

            if (baseType == "Product")
            {
                return CoatingScheduleProduct.Load(reader);
            }
            else if (baseType == "Note")
            {
                return CoatingScheduleNote.Load(reader);
            }

            return null;
        }

        public override void ReconnectToControls()
        {
            // Do nothing. Only parent controls need to reconnect their children
        }
    }
}
