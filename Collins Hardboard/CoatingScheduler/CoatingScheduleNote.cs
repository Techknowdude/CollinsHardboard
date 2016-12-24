using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using CoatingScheduler.Annotations;
using Microsoft.Office.Interop.Excel;
using StaticHelpers;

namespace CoatingScheduler
{
    [Serializable]
    public class CoatingScheduleNote : CoatingScheduleProductBase, INotifyPropertyChanged
    {
        #region Fields
        private string _text;


        #endregion

        #region Properties
        public String Text
        {
            get { return _text; }
            set { _text = value; }
        }
        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        public CoatingScheduleNote(string text = "",string coatingLine = "")
        {
            InitializeMembers(text,coatingLine);
        }

        private void InitializeMembers(String text, string coatingLine)
        {
            Text = text;
            CoatingLine = coatingLine;
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public override bool IsFull()
        {
            throw new NotImplementedException();
        }

        public override void Save(Stream stream, IFormatter formatter)
        {
            formatter.Serialize(stream,this);
        }

        public static CoatingScheduleNote Load(Stream stream, IFormatter formatter)
        {
            return (CoatingScheduleNote) formatter.Deserialize(stream);
        }

        public override Tuple<int, int> ExportToExcel(_Worksheet sheet, Int32 column, Int32 row)
        {
            Int32 nextRow = row;

            Range range = sheet.Range[StaticFunctions.GetRangeIndex(column, row)];
            StaticFunctions.SaveRichTextToCell(range, Text);
            ++nextRow;

            return new Tuple<int, int>(nextRow, column + ExcelWidth);
        }

    }
}
