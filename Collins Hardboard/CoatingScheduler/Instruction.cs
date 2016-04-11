using System;
using System.IO;
using Microsoft.Office.Interop.Excel;
using StaticHelpers;

namespace CoatingScheduler
{
    public class Instruction 
    {
        // excel info
        private static Int32 _width = 4;

        public static Int32 Width
        {
            get { return _width; }
            set { _width = value; }
        }

        // data
        public String Text { get; set; }

        public String Barcode { set; get; }

        public Instruction(string text = "", string barcode = "")
        {
            Text = text;
            Barcode = barcode;
        }

        public void Save(BinaryWriter writer)
        {
            writer.Write(Text);
            writer.Write(Barcode);
        }

        public static Instruction Load(BinaryReader reader)
        {
            string text = reader.ReadString();
            string barcode = reader.ReadString();

            return new Instruction(text,barcode);
        }


        public Tuple<int, int> ExportToExcel(_Worksheet sheet, Int32 column, Int32 row)
        {
            Int32 nextRow = row;

            Range range = sheet.Range[StaticFunctions.GetRangeIndex(column, nextRow)];
            StaticFunctions.SaveRichTextToCell(range, Text);
            range = sheet.Range[StaticFunctions.GetRangeIndex(column + 3, nextRow)];
            range.HorizontalAlignment = XlHAlign.xlHAlignRight;
            StaticFunctions.SaveRichTextToCell(range, Barcode);
            ++nextRow;

            return new Tuple<int, int>(nextRow, column + Width);
        }
    }
}
