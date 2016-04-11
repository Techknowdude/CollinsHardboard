using System;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using ImportLib;
using Microsoft.Office.Interop.Excel;
using ModelLib;
using Application = Microsoft.Office.Interop.Excel.Application;
using Window = System.Windows.Window;

namespace WarehouseManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class ForcastWindow : Window
    {
        public ForcastWindow()
        {
            InitializeComponent();
            ProductListView.ItemsSource = StaticInventoryTracker.ForecastItems;
            Sold1MonthAgoColumn.Content = DateTime.Now.AddMonths(-1).ToString("MMM\nyyyy");
            Sold2MonthsAgoColumn.Content = DateTime.Now.AddMonths(-2).ToString("MMM\nyyyy");
            Sold3MonthsAgoColumn.Content = DateTime.Now.AddMonths(-3).ToString("MMM\nyyyy");
            Sold4MonthsAgoColumn.Content = DateTime.Now.AddMonths(-4).ToString("MMM\nyyyy");
            Sold5MonthsAgoColumn.Content = DateTime.Now.AddMonths(-5).ToString("MMM\nyyyy");
            Sold6MonthsAgoColumn.Content = DateTime.Now.AddMonths(-6).ToString("MMM\nyyyy");
            Sold7MonthsAgoColumn.Content = DateTime.Now.AddMonths(-7).ToString("MMM\nyyyy");
            Sold8MonthsAgoColumn.Content = DateTime.Now.AddMonths(-8).ToString("MMM\nyyyy");
            Sold9MonthsAgoColumn.Content = DateTime.Now.AddMonths(-9).ToString("MMM\nyyyy");
            Sold10MonthsAgoColumn.Content = DateTime.Now.AddMonths(-10).ToString("MMM\nyyyy");
            Sold11MonthsAgoColumn.Content = DateTime.Now.AddMonths(-11).ToString("MMM\nyyyy");
            Sold12MonthsAgoColumn.Content = DateTime.Now.AddMonths(-12).ToString("MMM\nyyyy");
            Sold13MonthsAgoColumn.Content = DateTime.Now.AddMonths(-13).ToString("MMM\nyyyy");

        }
        private GridViewColumnHeader listViewSortCol = null;
        private SortAdorner listViewSortAdorner = null;
        private void ColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader column = (sender as GridViewColumnHeader);
            string sortBy = column.Tag.ToString();
            if (listViewSortCol != null)
            {
                AdornerLayer.GetAdornerLayer(listViewSortCol).Remove(listViewSortAdorner);
                ProductListView.Items.SortDescriptions.Clear();
            }

            ListSortDirection newDir = ListSortDirection.Ascending;
            if (listViewSortCol == column && listViewSortAdorner.Direction == newDir)
                newDir = ListSortDirection.Descending;

            listViewSortCol = column;
            listViewSortAdorner = new SortAdorner(listViewSortCol, newDir);
            AdornerLayer.GetAdornerLayer(listViewSortCol).Add(listViewSortAdorner);
            ProductListView.Items.SortDescriptions.Add(new SortDescription(sortBy, newDir));
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ConvertForecastToExcel();
            //ProgressDialog.Execute(null, "Exporting forecast data", () => { ConvertForecastToExcel(); }, new ProgressDialogSettings(true, true, false));
        }
        
        public bool ConvertForecastToExcel()
        {
            GridView myList = ProductListView.View as GridView;
            Application oXL;
            _Workbook oWB;
            _Worksheet oSheet;
            Range oRng;

            //ProgressDialog.Current.ReportWithCancellationCheck(0, "Loading column headers", 0);

            try
            {
                //Start Excel and get Application object.
                oXL = new Application();
                oXL.Visible = true;
                oXL.UserControl = false;

                //Get a new workbook.
                oWB = oXL.Workbooks.Add(Missing.Value);
                oSheet = (_Worksheet)oWB.ActiveSheet;
                
                Int32 forecastCount = StaticInventoryTracker.ForecastItems.Count;
                Int32 columnCount = myList.Columns.Count;
                // Create Int32 array for the location of the headers.
                // [0] is prodcode and stores the index of the header for prodcode, [1] is for description and so on
                Int32 [] ordinals = new int[columnCount];
                    //prodCode,prodDesc,units,oneMonthAvg,threeMonth,sixMonth,yearAvg,pastYearAvg,sold1Mo,sold2Mo,sold3Mo,sold4Mo,sold5Mo,sold6Mo,sold7Mo,sold8Mo,sold9Mo,sold10Mo,sold11Mo,sold12Mo,sold13Mo;
                String headerContent;
                //Add table headers going cell by cell.
                for (Int32 index = 0; index < columnCount; index++)
                {
                    GridViewColumn column = myList.Columns[index];
                    headerContent = ((GridViewColumnHeader) column.Header).Content.ToString();
                    char loc = (char) ('A' + index);

                    oSheet.Cells[1, index + 1] = headerContent;
                    oSheet.get_Range( String.Format("{0}1",loc)).ColumnWidth = column.ActualWidth * .125;

                    if (headerContent == HrProdCode.Content.ToString())
                    {
                        ordinals[index] = 0;
                    }
                    else if (headerContent == HrDesc.Content.ToString())
                    {
                        ordinals[index] = 1;
                    }
                    else if (headerContent == HrInv.Content.ToString())
                    {
                        ordinals[index] = 2;
                    }
                    else if (headerContent == Hr1MoAvg.Content.ToString())
                    {
                        ordinals[index] = 3;
                    }
                    else if (headerContent == Hr3MoAvg.Content.ToString())
                    {
                        ordinals[index] = 4;
                    }
                    else if (headerContent == Hr6MoAvg.Content.ToString())
                    {
                        ordinals[index] = 5;
                    }
                    else if (headerContent == Hr1YrAvg.Content.ToString())
                    {
                        ordinals[index] = 6;
                    }
                    else if (headerContent == HrLyAvg.Content.ToString())
                    {
                        ordinals[index] = 7;
                    }
                    else if (headerContent == Sold1MonthAgoColumn.Content.ToString())
                    {
                        ordinals[index] = 8;
                    }
                    else if (headerContent == Sold2MonthsAgoColumn.Content.ToString())
                    {
                        ordinals[index] = 9;
                    }
                    else if (headerContent == Sold3MonthsAgoColumn.Content.ToString())
                    {
                        ordinals[index] = 10;
                    }
                    else if (headerContent == Sold4MonthsAgoColumn.Content.ToString())
                    {
                        ordinals[index] = 11;
                    }
                    else if (headerContent == Sold5MonthsAgoColumn.Content.ToString())
                    {
                        ordinals[index] = 12;
                    }
                    else if (headerContent == Sold6MonthsAgoColumn.Content.ToString())
                    {
                        ordinals[index] = 13;
                    }
                    else if (headerContent == Sold7MonthsAgoColumn.Content.ToString())
                    {
                        ordinals[index] = 14;
                    }
                    else if (headerContent == Sold8MonthsAgoColumn.Content.ToString())
                    {
                        ordinals[index] = 15;
                    }
                    else if (headerContent == Sold9MonthsAgoColumn.Content.ToString())
                    {
                        ordinals[index] = 16;
                    }
                    else if (headerContent == Sold10MonthsAgoColumn.Content.ToString())
                    {
                        ordinals[index] = 17;
                    }
                    else if (headerContent == Sold11MonthsAgoColumn.Content.ToString())
                    {
                        ordinals[index] = 18;
                    }
                    else if (headerContent == Sold12MonthsAgoColumn.Content.ToString())
                    {
                        ordinals[index] = 19;
                    }
                    else if (headerContent == Sold13MonthsAgoColumn.Content.ToString())
                    {
                        ordinals[index] = 20;
                    }
                }
                //Format A1:D1 as bold, vertical alignment = center.
                oSheet.get_Range("A1", "U1").Font.Bold = true;
                oSheet.get_Range("A1", "U1").VerticalAlignment =
                    XlVAlign.xlVAlignCenter;

                //Exit if no rows exist
                if (forecastCount == 0) return true;

                // Create an array to insert multiple values at once.
                string[,] forecastValues = new string[forecastCount,columnCount];

                DateTime start = DateTime.Now;
                for (Int32 rowIndex = 0; rowIndex < forecastCount; rowIndex++)
                {
                    //ProgressDialog.Current.ReportWithCancellationCheck(rowIndex/forecastCount, "Writing row {0}/{1}", rowIndex,forecastCount);

                    ForecastItem forecastItem = StaticInventoryTracker.ForecastItems[rowIndex];

                    forecastValues[rowIndex, ordinals[0]] = forecastItem.ProductCode;
                    forecastValues[rowIndex, ordinals[1]] = forecastItem.ProductDescription;
                    forecastValues[rowIndex, ordinals[2]] = forecastItem.Units.ToString();
                    forecastValues[rowIndex, ordinals[3]] = forecastItem.OneMonthAvg;
                    forecastValues[rowIndex, ordinals[4]] = forecastItem.ThreeMonthAvg;
                    forecastValues[rowIndex, ordinals[5]] = forecastItem.SixMonthAvg;
                    forecastValues[rowIndex, ordinals[6]] = forecastItem.TwelveMonthAvg;
                    forecastValues[rowIndex, ordinals[7]] = forecastItem.PastYearAvg;
                    forecastValues[rowIndex, ordinals[8]] = forecastItem.SoldOneMonthAgo;
                    forecastValues[rowIndex, ordinals[9]] = forecastItem.SoldTwoMonthsAgo;
                    forecastValues[rowIndex, ordinals[10]] = forecastItem.SoldThreeMonthsAgo;
                    forecastValues[rowIndex, ordinals[11]] = forecastItem.SoldFourMonthsAgo;
                    forecastValues[rowIndex, ordinals[12]] = forecastItem.SoldFiveMonthsAgo;
                    forecastValues[rowIndex, ordinals[13]] = forecastItem.SoldSixMonthsAgo;
                    forecastValues[rowIndex, ordinals[14]] = forecastItem.SoldSevenMonthsAgo;
                    forecastValues[rowIndex, ordinals[15]] = forecastItem.SoldEightMonthsAgo;
                    forecastValues[rowIndex, ordinals[16]] = forecastItem.SoldNineMonthsAgo;
                    forecastValues[rowIndex, ordinals[17]] = forecastItem.SoldTenMonthsAgo;
                    forecastValues[rowIndex, ordinals[18]] = forecastItem.SoldElevenMonthsAgo;
                    forecastValues[rowIndex, ordinals[19]] = forecastItem.SoldTweleveMonthsAgo;
                    forecastValues[rowIndex, ordinals[20]] = forecastItem.SoldThirteenMonthsAgo;

                }
                oSheet.Range["A2", String.Format("{0}{1}", (char)('A' + columnCount - 1), forecastCount + 1)].Value2 = forecastValues;

                Console.WriteLine(DateTime.Now - start);
                //AutoFit columns A:D.
                oRng = oSheet.get_Range("A1", String.Format("{0}1", (char)('A' + columnCount - 1)));
                oRng.EntireColumn.AutoFit();

                oXL.UserControl = true;
            }
            catch (Exception theException)
            {
                String errorMessage;
                errorMessage = "Error: ";
                errorMessage = String.Concat(errorMessage, theException.Message);
                errorMessage = String.Concat(errorMessage, " Line: ");
                errorMessage = String.Concat(errorMessage, theException.Source);
                
                MessageBox.Show(errorMessage, "Error");
            }
            return true;
        }
    }
}
