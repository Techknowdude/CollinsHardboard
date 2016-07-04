using System;
using System.Security.Cryptography;
using System.Windows.Media;
using Microsoft.Office.Interop.Excel;
using ProductionScheduler;
using System.Drawing;
using Brush = System.Drawing.Brush;
using Brushes = System.Drawing.Brushes;
using Color = System.Drawing.Color;

namespace ExtendedScheduleViewer
{
    class ExtendedScheduleExcelExporter
    {
        private ExtendedSchedule _schedule;

        private static int _minCol = 0;
        private static int _maxCol = 9;
        private static int Heading1FontSize = 20;
        private static int Heading2FontSize = 16;
        private static int Heading3FontSize = 14;
        private static int SubHeadingFontSize = 12;
        private static int TextFontSize = 12;
        private static int _columnWidth = 16;
        private static int _lastCol = _maxCol - _minCol + 1;
        private int _curRow = 1;
        private DoubleToBrushConverter converter;

        public ExtendedScheduleExcelExporter(ExtendedSchedule schedule)
        {
            converter = new DoubleToBrushConverter();
            _schedule = schedule;
        }

        public void Export()
        {
            Application _application;
            Worksheet _worksheet;

            _application = new Application();
            _application.DisplayAlerts = false;
            _application.Visible = true;
            _application.Workbooks.Add();

            _worksheet = (Worksheet)_application.ActiveSheet;
            _worksheet.PageSetup.FitToPagesWide = 1;
            _worksheet.PageSetup.Orientation = XlPageOrientation.xlLandscape;

            // output the title
            
            OutputText("Extended Schedule " + DateTime.Today.ToString("d"), 0,4,_curRow++,_worksheet);
            // output each day
            foreach (var trackingDay in _schedule.TrackingDays)
            {
                Export(trackingDay, _worksheet);
            }
        }

        private void Export(TrackingDay day, Worksheet worksheet)
        {
            // output the day
            OutputText(day.Day.ToString("ddd, MMM dd"),0,1,_curRow,worksheet);
            

            // output each shift
            foreach (var trackingShift in day.Shifts)
            {
                Export(trackingShift, worksheet);
                ++_curRow;
            }
        }

        private void Export(TrackingShift shift, Worksheet worksheet)
        {
            int currentCol = 1;

            // output shift name
            OutputText(shift.ShiftTitle,currentCol++,1,_curRow,worksheet);

            // output each summary
            foreach (var itemSummary in shift.ItemSummaries)
            {
                var foreground = DoubleToBrushConverter.GetBrushColor(itemSummary.RunningUnits);
                var background = MasterToBrushConverter.GetExcelColor(itemSummary.Item);
                OutputText(itemSummary.RunningUnits.ToString("N1"),currentCol++,1,_curRow,worksheet,foreground,background);   
                OutputText(itemSummary.AddedUnits.ToString("N1"),currentCol++,1,_curRow,worksheet);   
                OutputText(itemSummary.RemovedUnits.ToString("-N1"),currentCol++,1,_curRow,worksheet);
            }

        }

        /// <summary>
        /// Returns a Range for the given area
        /// </summary>
        /// <param name="startCol">Zero indexed starting column</param>
        /// <param name="startRow">starting row</param>
        /// <param name="endCol">Zero indexed ending column</param>
        /// <param name="endRow">ending row</param>
        /// <param name="worksheet">Worksheet to change</param>
        /// <returns></returns>
        public static Range GetRange(int startCol, int startRow, int endCol, int endRow, Worksheet worksheet)
        {
            string first = $"{(char)('A' + startCol)}{startRow}";
            string second = $"{(char)('A' + endCol)}{endRow}";
            Range rng = worksheet.Range[first, second];
            rng.Merge();
            return rng;
        }


        /// <summary>
        /// Writes the text into the given range. Does not add a row.
        /// </summary>
        /// <param name="text">Text to be written out</param>
        /// <param name="startColumn">Zero indexed column</param>
        /// <param name="columns">Total number of columns the text takes</param>
        /// <param name="row"></param>
        /// <param name="worksheet"></param>
        private static void OutputText(string text, int startColumn, int columns, int row, Worksheet worksheet, XlRgbColor color = XlRgbColor.rgbBlack, XlRgbColor bg = XlRgbColor.rgbWhite)
        {
            columns--;
            Range rng = GetRange(_minCol + startColumn, row, _minCol + startColumn + columns, row, worksheet);
            rng.UnMerge();
            rng.Cells.Font.Size = TextFontSize;
            rng.Merge();
            rng.WrapText = true;
            rng.Value = text;

            rng.Font.Color = color; // text color
            rng.Interior.Color = bg;
        }
    }
}
