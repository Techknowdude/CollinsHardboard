using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using LumenWorks.Framework.IO.Csv;
using Microsoft.Win32;
using ModelLib;
using StaticHelpers;
using static System.Int32;

namespace ImportLib
{
    public class InternalImport
    {
        public const string LTConn = @"Server=PDX-LTDB; Database=ltproddw; User Id=crystal_dw; Password=cR!st@!9Qn$";
        public const string LTSalesConn = @"Server=PDX-LTDB; Database=ltprod; User Id=crystal_dw; Password=cR!st@!9Qn$";

        #region DataMembers/Properties

        public static bool CSVImport = false;

        public String OutputDebugFile = Path.GetFullPath("debugFile" + DateTime.Today.ToString("yy-MM-dd") + ".dat");
        public const String ExcelProvider = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source='";
        public const String ExcelExtended = @"';Extended Properties=""Excel 12.0 Xml;HDR=YES"";";
        public const String FileFilter = "Excel Spreadsheets (" + ExcelExtention + ")|*" + ExcelExtention;
        public const String ExcelExtention = ".xlsx";


        private static InternalImport _instance;

        private String _masterFileName = String.Empty;

        public String MasterFileName
        {
            get { return _masterFileName; }
            set { _masterFileName = value; }
        }

        private String _inventoryFileName = String.Empty;

        public String InventoryFileName
        {
            get { return _inventoryFileName; }
            set { _inventoryFileName = value; }
        }

        private String _salesFileName = String.Empty;

        public String SaleFileName
        {
            get { return _salesFileName; }
            set { _salesFileName = value; }
        }

        #endregion

        public static InternalImport GetInstance()
        {
            if (_instance == null)
                _instance = new InternalImport();
            return _instance;
        }

        private InternalImport()
        {

        }

        public void ImportAll()
        {
            if (ImportMaster())
            {
                bool importedInv = ImportInventory();
                bool importedSales = ImportSales();
                if (importedSales && importedInv)
                    ImportSalesForecast();
            }
        }

        public bool ImportMaster()
        {
            var imported = GetExcelProductMaster();
            return imported > 0;
        }

        private static int GetExcelProductMaster()
        {
            // need to save the last known file
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.FileName = "Product Master File" + ExcelExtention; // Default file name 
            dlg.DefaultExt = ExcelExtention; // Default file extension 
            dlg.Filter = FileFilter; // Filter files by extension 
            int numImported = 0;

            if (dlg.ShowDialog() == false) return 0;

            List<int> failedRows = new List<int>();
            OleDbCommand myCommand = new OleDbCommand();
            string sql = null;
            String connectionString = ExcelProvider + dlg.FileName + ExcelExtended;

            Int32 rowNum = 0;
            Int32 rowCount = 0;

            using (OleDbConnection MyConnection = new OleDbConnection(connectionString))
            {
                StaticInventoryTracker.ProductMasterList.Clear();
                try
                {
                    bool valid = false;
                    string product;
                    double width;
                    double length;
                    string thickness;
                    string texture;
                    double waste;
                    Int32 pcsUnit;
                    String grades;
                    String barcode;
                    String description;
                    String notes = string.Empty;
                    String turnType;
                    double minSupply;
                    double maxSupply;
                    double targetSupply;
                    bool hasBarcode;
                    double unitsPerShift;
                    int masterID;
                    String madeIn;

                    MyConnection.Open();
                    myCommand.Connection = MyConnection;
                    String sheet = "Data";
                    String columns = "*";
                    sql = "Select " + columns + " from [" + sheet + "$]";
                    myCommand.CommandText = sql;

                    try
                    {
                        OleDbDataReader excelData = myCommand.ExecuteReader();
                        Int32 prodOrdinal = excelData.GetOrdinal("Product Code"); //set to the sales columns.
                        //Int32 descOrdinal = excelData.GetOrdinal("");
                        Int32 widthOrdinal = excelData.GetOrdinal("Width");
                        Int32 idOrdinal = excelData.GetOrdinal("Master ID");
                        Int32 lengthOrdinal = excelData.GetOrdinal("Length");
                        Int32 thickOrdinal = excelData.GetOrdinal("Thickness");
                        Int32 textureOrdinal = excelData.GetOrdinal("Texture");
                        Int32 wasteOrdinal = excelData.GetOrdinal("Waste/Unit (lbs)");
                        Int32 pcsOrdinal = excelData.GetOrdinal("Pcs/Package");
                        Int32 gradesOrdinal = excelData.GetOrdinal("Grades");
                        Int32 barcodeOrdinal = excelData.GetOrdinal("Barcode");
                        Int32 descriptionOrdinal = excelData.GetOrdinal("Description");
                        Int32 notesOrdinal = excelData.GetOrdinal("Notes");
                        Int32 turnTypeOrdinal = excelData.GetOrdinal("Units/Turns");
                        Int32 minOrdinal = excelData.GetOrdinal("Min");
                        Int32 maxOrdinal = excelData.GetOrdinal("Target");
                        Int32 targetOrdinal = excelData.GetOrdinal("Max");
                        Int32 shiftUnitsOrdinal = excelData.GetOrdinal("Avg run Coating");
                        Int32 madeOrdinal = excelData.GetOrdinal("Made In");

                        while (excelData.Read())
                        {
                            rowNum++;
                            try
                            {
                                bool assigned = false;
                                try
                                {
                                    valid = excelData.GetBoolean(excelData.GetOrdinal("ValidRow"));
                                    assigned = true;
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine(e.Message);
                                }
                                try
                                {
                                    valid = excelData.GetString(excelData.GetOrdinal("ValidRow")).ToLower() == "true";
                                    assigned = true;
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine(e.Message);
                                }

                                if (!valid) continue;

                                masterID = Convert.ToInt32(excelData.GetDouble(idOrdinal));
                                product = excelData.GetString(prodOrdinal);
                                width = excelData.GetDouble(widthOrdinal);
                                length = excelData.GetDouble(lengthOrdinal);
                                thickness = excelData.GetValue(thickOrdinal).ToString();
                                texture = excelData.GetString(textureOrdinal);
                                waste = excelData.GetDouble(wasteOrdinal);
                                pcsUnit = (int)excelData.GetDouble(pcsOrdinal);
                                grades = excelData.GetString(gradesOrdinal);
                                barcode = excelData.GetString(barcodeOrdinal);
                                description = excelData.GetString(descriptionOrdinal);
                                try
                                {
                                    notes = excelData.GetString(notesOrdinal);
                                }
                                catch (Exception)
                                {
                                }
                                turnType = excelData.GetString(turnTypeOrdinal);
                                minSupply = excelData.GetDouble(minOrdinal);
                                maxSupply = excelData.GetDouble(maxOrdinal);
                                targetSupply = excelData.GetDouble(targetOrdinal);
                                hasBarcode = !(barcode.ToUpper() == "NO" || barcode.ToUpper() == "FALSE");
                                unitsPerShift = excelData.GetDouble(shiftUnitsOrdinal);
                                madeIn = excelData.GetString(madeOrdinal);

                                if (maxSupply < targetSupply)
                                    maxSupply = targetSupply;

                                StaticInventoryTracker.AddMasterItem(new ProductMasterItem(masterID, product, description, width, length,
                                    thickness, texture, waste, pcsUnit, grades, hasBarcode, notes, turnType, minSupply, maxSupply, targetSupply, unitsPerShift / 8) {MadeIn = madeIn});
                                ++numImported;
                            }
                            catch (Exception e)
                            {
                                StackTrace trace = new StackTrace(e);
                                Console.WriteLine(e.Message);
                                Console.WriteLine(trace.GetFrame(0).GetFileLineNumber());
                                failedRows.Add(rowNum);
                            }
                        }
                    }
                    catch (Exception e)
                    {

                        Console.WriteLine("Exception!: " + e.Message); //query failed
                    }

                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message); // could not open file
                }
            }

            MessageBox.Show("Completed master import with " + numImported + " items");

            return numImported;
        }

        public bool ImportInventory()
        {
            bool completed;
                StaticInventoryTracker.InventoryItems.Clear();
            if (CSVImport)
            {
                completed = ImportInventoryFromCSV();
                if (!completed)
                {
                    MessageBox.Show("There was an issue opening the CSV file");
                }
            }
            else
            {
                completed = ImportInventoryFromSQL();
                if (!completed)
                {
                    MessageBox.Show(
                        "There was an issue connecting to the database.Please check your network connection.");
                }
            }
            return completed;
        }

        private bool ImportInventoryFromSQL()
        {
            bool successful = true;
            int numImported = 0;


            try
            {

                SqlConnection sqlConnection = new SqlConnection(LTConn);
                string command = @"SELECT		DISTINCT IR.[PROGRESS_RECID] AS InventoryDWInventoryRecordID
			                                    , P.productkey AS InventoryDWProductKey
			                                    , C.calendardate AS InventorySnapshotDate
			                                    , UPPER(L.branch) AS Branch
			                                    , L.location AS Location
			                                    , P.producttype AS ProductType
			                                    , P.product AS Product
												, IR.onhandpieces AS Pieces
			                                    , HBP.hbgrade AS Grade
			                                    , HBP.hbcertified AS Certified
			                                    , HBP.hbthickness AS Thickness
			                                    , HBP.hbtexture AS Texture
			                                    , HBP.hblength AS Length
			                                    , HBP.hbwidth AS Width


                                    FROM		ltprdinvdw.dbo.inventory_record IR -- main fact table within Inventory DW
                                    INNER JOIN	ltprdinvdw.dbo.calendar C
                                    ON			IR.calendarkey = C.calendarkey
                                    INNER JOIN	ltprdinvdw.dbo.product P
                                    ON			IR.productkey = P.productkey
                                    INNER JOIN	ltprdinvdw.dbo.hb_product HBP -- hb_product table = hardboard type products only
                                    ON			P.productkey = HBP.productkey
                                    INNER JOIN	ltprdinvdw.dbo.location L
                                    ON			IR.locationkey = L.locationkey

                                    WHERE		C.calendardate = CAST(CONVERT(NVARCHAR(8), DATEADD(dd, -1, GETDATE()), 112) AS DATETIME)
                                                AND location = 'HB';


                                    ";

                sqlConnection.Open();
                SqlCommand getSalesCommand = new SqlCommand(command, sqlConnection);

                var result = getSalesCommand.ExecuteReader();
                bool ignore = false;

                while (result.Read())
                {
                    try
                    {
                        String prodCode = result["Product"].ToString();
                        double pcs = StaticFunctions.StringToDouble(result["Pieces"].ToString());
                        String grade = result["Grade"].ToString();
                        String tex = result["Texture"].ToString();
                        String thick = result["Thickness"].ToString();
                        String width = result["Width"].ToString();
                        String length = result["Length"].ToString();

                        int master = FindMaster(prodCode, thick, width, length);

                        if (master != -1)
                        {

                            var foundMaster =
                                StaticInventoryTracker.ProductMasterList.FirstOrDefault(
                                    x => x.MasterID == master);
                            if (foundMaster != null)
                            {
                                var units = pcs/foundMaster.PiecesPerUnit;
                                StaticInventoryTracker.AddInventory(prodCode, foundMaster.PiecesPerUnit, units, grade, master);
                                ++numImported;
                            }
                        }
                        else
                        {

                            OutputDebugLine("## Inventory Import ## " +
                                            String.Format(
                                                "No Matching Master Found:: Code: {0}, Tex: {1}, Thickness: {2}, Width: {3}, Length: {4}",
                                                prodCode, tex, thick, width, length));

                            if (!ignore &&
                                MessageBox.Show(
                                    String.Format(
                                        "No master file etry for {0}. Unable to add inventory. Ignore future errors?",
                                        prodCode), "Error",
                                    MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                            {
                                ignore = true;
                            }
                        }
                    }
                    catch (Exception exception) // bad record
                    {
                        Console.WriteLine(exception.Message);
                    }
                }

            }
            catch (Exception exception) // issue with connection
            {
                Console.WriteLine(exception.Message);
                successful = false;
            }

            MessageBox.Show("Inventory import finished with " + numImported + " imported");

            try
            {
                using (StreamWriter writer = new StreamWriter(Path.GetFullPath("ImportInfo/ImportedInventory.txt")))
                {
                    foreach (var item in StaticInventoryTracker.InventoryItems)
                    {
                        writer.WriteLine(item);
                    }
                }
            }
            catch (Exception)
            {
            }


            return successful;
        }

        private bool ImportInventoryFromCSV()
        {
            bool successful = true;

            OpenFileDialog open = new OpenFileDialog();
            open.Title = "Open inventory csv file";

            int rows = 0;
            if (open.ShowDialog() == true)
            {
                try
                {
                    int curRow = 0;
                    using (CsvReader reader = new CsvReader(new StreamReader(open.FileName), true))
                    {

                        while (reader.ReadNextRecord())
                        {
                            try
                            {


                                curRow++;
                                String prodCode = reader["Product"];
                                double pcs = StaticFunctions.StringToDouble(reader["Pieces"]);
                                String grade = reader["Grade"];
                                String thick = reader["Thickness"];
                                String width = reader["Width"];
                                String length = reader["Length"];

                                int master = FindMaster(prodCode, thick, width, length);

                                if (master != -1)
                                {
                                    var foundMaster =
                                        StaticInventoryTracker.ProductMasterList.FirstOrDefault(
                                            x => x.MasterID == master);
                                    if (foundMaster != null)
                                    {
                                        var units = pcs/foundMaster.PiecesPerUnit;
                                        StaticInventoryTracker.AddInventory(prodCode, foundMaster.PiecesPerUnit, units, grade, master);
                                        Console.WriteLine("{0},{1},{2},{3}", prodCode, thick, width, length);
                                    }
                                }
                                ++rows;
                            }
                            catch (Exception rowException)
                            {
                                Console.WriteLine("Row isssue on line {0}: {1}", curRow, rowException.Message);
                            }
                        }
                    }

                }
                catch (Exception exception)
                {
                    MessageBox.Show("Could not open file. Is it in use?");
                    successful = false;
                }
            }

            MessageBox.Show($"Imported {rows} rows");


            return successful;
        }

        public bool ImportSales()
        {
            bool complete = false;
            if (CSVImport)
                complete = ImportSalesFromCSV();
            else
                complete = ImportSalesFromSQL();

            if (!complete)
                MessageBox.Show("There was an issue connecting to the database. Please check your network connection.");
            return complete;
        }

        private bool ImportSalesFromCSV()
        {
            if (!StaticInventoryTracker.IsLoaded)
                StaticInventoryTracker.LoadDefaults();

            bool successful = true;

            OpenFileDialog open = new OpenFileDialog();
            open.Title = "Open sales csv file.";

            if (open.ShowDialog() == true)
            {
                int rows = 0;

                try
                {
                    using (CsvReader result = new CsvReader(new StreamReader(open.FileName), true))
                    {
                        bool ignore = false;
                        SalesItem sale = null;
                        while (result.ReadNextRecord())
                        {
                            try
                            {
                                String prodCode = result["ProductCode"];
                                String invoiceNum = result["OrderNum"];
                                String grade = result["Grade"];
                                DateTime duedate = DateTime.Parse(result["DueDate"]);
                                String thick = result["Thickness"];
                                String width = result["Width"];
                                String length = result["Length"];
                                int pieces;
                                Int32.TryParse(result["TotalPieces"], out pieces);

                                int master = FindMaster(prodCode, thick, width, length);

                                if (master != -1)
                                {
                                    var foundMaster =
                                        StaticInventoryTracker.ProductMasterList.FirstOrDefault(
                                            x => x.MasterID == master);
                                    double units = pieces/foundMaster.PiecesPerUnit;
                                    StaticInventoryTracker.AddSales(prodCode, invoiceNum, duedate, units, pieces, grade, master);
                                }
                                else
                                {
                                    if (!ignore && MessageBox.Show(String.Format("No master file etry for Code:{0}, Thickness:{1}, Width:{2}, Length{3}. Ignore future errors?", 
                                        prodCode, thick, width, length), "Error",
                                        MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                                    {
                                        ignore = true;
                                    }
                                }
                                ++rows;
                            }
                            catch (Exception rowException) // bad record
                            {
                                Console.WriteLine(rowException.Message);
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    MessageBox.Show("CSV file could not be opened. Is it in use?");
                    successful = false;
                }
                MessageBox.Show(String.Format("Sales imported {0} rows", rows));
            }


            return successful;
        }


        private bool ImportSalesFromSQL()
        {
            if (!StaticInventoryTracker.IsLoaded)
                StaticInventoryTracker.LoadDefaults();

            bool successful = true;


            int numImported = 0;
            try
            {
                /* -------------------------------------------------------------------------
Brandon: here are some quick notes for you -

    1. See my notes below on the Certified field. If you really need it, let me
        know.

    2. FYI, the DW-based code you'd sent me today had not only 'original' sales
        orders, but also any subsequent GL reversals and adjustments. My code
        below should only have what's currently visible in LumberTrack (i.e.
        SumDisp = 1). To the best of my knowledge, this should be considering
        any reversals and adjustments since the original sales order.

    3. The OrderDate below should be the original sales order date. After further
        inspection, the 'SalesOrderHeaderDate' field from your DW-based code is
        the underlying GL transaction date. My apologies if that was wrong, confusing,
        etc.
------------------------------------------------------------------------- */

                OutputDebugLine("Creating connection...");
                SqlConnection sqlConnection = new SqlConnection(LTSalesConn.ToString());

                OutputDebugLine("Creating command.");
                string command = @"SELECT DISTINCT OH.branch AS Branch
				, OH.loc AS Location
				, OH.orddate AS OrderDate-- original sales order date
               , OH.ordnum AS OrderNum
				, OI.dispnum AS OrderItemDispNum-- purely a display number from LT on the Order Item tab / screen...
				, CH.cust AS CustomerCode
				, CH.name AS CustomerName
				, OH.custpo AS CustomerPONum
				, OH.duedate AS DueDate
				, OH.ordstatus AS OrderStatus-- Q = Quoted, R = Reserved, C = Complete, V = Void
				, OH.reltolocn AS OrderReleasedToLocation-- 0 = 'No', 1 = 'Yes'
				, OI.volumequantity AS OrderItemVolumeQuantity
				, OI.volumeunit AS OrderItemVolumeUOM
				, OI.piecesquantity - ((OI.piecesquantity / OI.desc3) *OI.desc3) AS Pieces
				, OI.piecesquantity / OI.desc3 AS Units
				, OI.piecesquantity AS TotalPieces
				, OI.piecesunit AS OrderItemPiecesUOM
				, PD.protype AS ProductType
				, PD.product AS ProductCode
				, PD.[description] AS ProductDescription
                , PD.desc1 AS Texture
				, PD.desc2 AS Thickness
				, PD.desc3 AS Grade
				, PD.desc4 AS[Type]
                , PD.desc5 AS Style
				, PD.desc6 AS WidthLabel
				, OI.desc1 AS Width
				, OI.desc2 AS[Length]
                , OI.desc3 AS PiecesPerPackage
                --, OI.desc4 AS Certified-- Brandon: I commented this out since it's not what comes thru as 'Certified' in the data warehouse. Email me if you actually need this field (i.e. from the underlying invoice/acct recv'able table). AU

				, OH.shiptocity +
                    CASE
                        WHEN OH.shiptocity<> N'' AND OH.shiptoprov<> N''
                        THEN N', '
                        ELSE N''
                    END +
                    OH.shiptoprov +
                    CASE
                        WHEN OH.shiptoprov = N'USA' OR OH.shiptoprov IS NULL
                        THEN N''
                        ELSE N' ' + CC.codedesc
                    END AS ShipDestination

				, SH.shpnum AS ShipNum
				, SH.shpstatus AS ShipStatus-- A = Assigned, P = Picked, R = Ready, O = Outbound, D = Delivered, V = Void
				, SH.shipvia AS ShipMode
				, SH.shipdate AS ShipDate
				, SH.pickeddate AS ShipPickedDate
				, SH.readydate AS ShipReadyDate
				, SH.estarrdate AS ShipEstimatedArrivalDate
				, SH.arrdate AS ShipArrivalDate
				, UF.udfdate02 AS ShipEstimatedPickupDate
				, OH.ordfunction AS SalesOrderFunction

FROM            dbo.ord_hdr OH (NOLOCK)
INNER JOIN      dbo.cus_hdr CH(NOLOCK)
ON              OH.cust = CH.cust
INNER JOIN      dbo.ord_item OI(NOLOCK)
ON(OH.ordnum = OI.ordnum
                AND OI.sumdisp = 1)-- taken from query behind Sales Order Item tab in LumberTrack; shows only the visible items from front LT UI.
LEFT OUTER JOIN dbo.inv_ord IORD (NOLOCK)
ON              OH.ordnum = IORD.ordnum
LEFT OUTER JOIN dbo.shp_ord SO(NOLOCK)-- allows for orders to come through that don't have a shipment header set up yet.
ON              OH.ordnum = SO.ordnum
LEFT OUTER JOIN dbo.shp_hdr SH(NOLOCK)-- allows for orders to come through that don't have a shipment header set up yet.
ON              SO.shpnum = SH.shpnum
INNER JOIN      dbo.pro_duct PD(NOLOCK)
ON(OI.protype = PD.protype
                AND OI.product = PD.product)
LEFT OUTER JOIN dbo.udf_fields UF(NOLOCK)
ON              OH.udffieldskey = UF.udffieldskey
LEFT OUTER JOIN dbo.cde_codes CC(NOLOCK)
ON              OH.shiptoctry = CC.code

WHERE           PD.protype = N'HB' AND OH.ordstatus = 'R'
ORDER BY        OrderNum, PD.Product;
";

                OutputDebugLine("Opening connection...");
                sqlConnection.Open(); // try to connect

                OutputDebugLine("Creating command.");
                SqlCommand getSalesCommand = new SqlCommand(command, sqlConnection);

                OutputDebugLine("Executing read");
                var result = getSalesCommand.ExecuteReader();
                bool ignore = false;
                OutputDebugLine("Starting read");
                // remove any old data.
                StaticInventoryTracker.SalesItems.Clear();

                while (result.Read())
                {
                    try
                    {
                        String prodCode = result["ProductCode"].ToString();
                        String invoiceNum = result["OrderNum"].ToString();
                        String grade = result["Grade"].ToString();
                        DateTime duedate = DateTime.Parse(result["DueDate"].ToString());
                        String thick = result["Thickness"].ToString();
                        String width = result["Width"].ToString();
                        String length = result["Length"].ToString();
                        int pieces;
                        Int32.TryParse(result["Pieces"].ToString(), out pieces);

                        int master = FindMaster(prodCode, thick, width, length);

                        if (master != -1)
                        {
                            var foundMaster =
                                StaticInventoryTracker.ProductMasterList.FirstOrDefault(x => x.MasterID == master);
                            var units = pieces/foundMaster.PiecesPerUnit;
                            StaticInventoryTracker.AddSales(prodCode, invoiceNum, duedate, units, pieces, grade, master);
                            numImported++;
                        }
                        else
                        {
                            OutputDebugLine("## Sales Import ## " +
                                            String.Format(
                                                "No Matching Master Found:: Code: {0}, Thickness: {1}, Width: {2}, Length: {3}",
                                                prodCode, thick, width, length));
                            if (!ignore &&
                                MessageBox.Show(
                                    String.Format(
                                        "No master file entry for {0}. Unable to add sale. Ignore future errors?", prodCode),
                                    "Error",
                                    MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                            {
                                ignore = true;
                            }
                        }
                    }
                    catch (Exception exception) // bad record
                    {
                        OutputDebugLine("## Import Sales Exception ## " + exception.Message);
                        //Console.WriteLine(exception.Message);
                        //MessageBox.Show("Encountered an error: " + exception.Message);
                    }
                }

            }
            catch (Exception exception) // issue with connection
            {
                OutputDebugLine(" ## Import Sales File Exception ##" + exception.Message);
                //Console.WriteLine(exception.Message);
                //MessageBox.Show("Encountered an error: " + exception.Message);
                successful = false;
            }
            MessageBox.Show("Sales import finished with " + numImported + " sales imported");

            try
            {
                using (StreamWriter writer = new StreamWriter(Path.GetFullPath("ImportInfo/ImportedSales.txt")))
                {
                    foreach (var item in StaticInventoryTracker.SalesItems)
                    {
                        writer.WriteLine(item);
                    }
                }
            }
            catch (Exception)
            {
            }


            return successful;
        }


        public void ImportSalesForecast()
        {
            bool complete;
            if (CSVImport)
                complete = ImportForecastFromCSV();
            else
                complete = ImportForecastFromSQL();
            if (complete)
            {
                MessageBox.Show("Sales forecast import finished.");
            }
            else
            {
                MessageBox.Show("Forecast import failed. Please check network connection");
            }
        }

        private bool ImportForecastFromSQL()
        {
            bool successful = false;
            int numImported = 0;
            try
            {



                SqlConnection sqlConnection = new SqlConnection(LTSalesConn);
                string command = @"SELECT		 
			                                  P.Product AS ProductCode
			                                , M.Pieces AS Pieces
                                            , Cal.FullDate AS DueDate
			                                , P.Descriptor1 AS Texture
			                                , P.Descriptor2 AS Thickness
			                                , P.Descriptor3 AS Grade
			                                , S.Attribute1 AS Width
			                                , S.Attribute2 AS Length
											, TransactionType


                                FROM        ltproddw.dbo.Measure M (NOLOCK)
                                INNER JOIN  ltproddw.dbo.Calendar CAL (NOLOCK)
                                ON M.CalendarKey = CAL.calendarkey 
                                INNER JOIN  ltproddw.dbo.Sale S(NOLOCK)
                                ON M.SaleKey = S.SaleKey
                                INNER JOIN  ltproddw.dbo.Product P(NOLOCK)
                                ON M.ProductKey = P.ProductKey

                                WHERE P.ProductType = N'HB'-- HB = Hardboard
								AND Cal.FullDate BETWEEN DATEADD(MONTH, DATEDIFF(MONTH, '19000101', GETDATE()) - 13, '19000101') 
													 AND DATEADD(MONTH, DATEDIFF(MONTH, '19000101', GETDATE()), '19000101')
								AND TransactionType = 'Original' -- Removes everything for adjustments.
								order by Product, Cal.FullDate;
  ";

                sqlConnection.Open(); // try to connect

                SqlCommand getPastSalesCommand = new SqlCommand(command, sqlConnection);

                var result = getPastSalesCommand.ExecuteReader();
                bool ignore = false;

                StaticInventoryTracker.ForecastItems.Clear();
                while (result.Read())
                {
                    String prodCode;
                    DateTime duedate;
                    String tex;
                    String thick;
                    String width;
                    String length;
                    int pieces;

                    try
                    {
                        prodCode = result["ProductCode"].ToString();    
                        duedate = DateTime.Parse(result["DueDate"].ToString());
                        tex = result["Texture"].ToString();
                        thick = result["Thickness"].ToString();
                        width = result["Width"].ToString();
                        length = result["Length"].ToString();
                        Int32.TryParse(result["Pieces"].ToString(), out pieces);

                        int master = FindMaster(prodCode, thick, width, length);

                        if (master != -1)
                        {
                                StaticInventoryTracker.AddPastSale(master, duedate, pieces);
                                ++numImported;
                        }
                        else
                        {
                            OutputDebugLine("## Forecast Import ## " +
                                            $"No Matching Master Found:: Code: {prodCode}, Tex: {tex}, Thickness: {thick}, Width: {width}, Length: {length}");
                        }
                    }
                    catch (Exception exception) // bad record
                    {
                        OutputDebugLine("## Forecast Import ## " + exception.Message);
                        Console.WriteLine(exception.Message);
                    }
                }
                successful = true;
            }

            catch (Exception exception) // issue with connection
            {
                OutputDebugLine("## Forecast File Import ## " + exception.Message);
                Console.WriteLine(exception.Message);
                successful = false;
            }

            MessageBox.Show("Forecast import finished with " + numImported + " past sales imported");

            try
            {
                using (StreamWriter writer = new StreamWriter(Path.GetFullPath("ImportInfo/ImportedForecast.txt")))
                {
                    foreach (var item in StaticInventoryTracker.ForecastItems)
                    {
                        writer.WriteLine(item);
                    }
                }
            }

            catch
            {
                // ignored
            }

            return successful;
        }


        private bool ImportForecastFromCSV()
        {
            var successful = true;
            try
            {
                OpenFileDialog open = new OpenFileDialog {Title = "Open inventory csv file"};

                int rows = 0;
                if (open.ShowDialog() == true)
                {
                    try
                    {
                        int curRow = 0;
                        using (CsvReader reader = new CsvReader(new StreamReader(open.FileName), true))
                        {

                            while (reader.ReadNextRecord())
                            {
                                try
                                {
                                    curRow++;
                                    var prodCode = reader["ProductCode"];
                                    var pieces = StaticFunctions.StringToDouble(reader["Pieces"]);
                                    var tex = reader["Texture"];
                                    var thick = reader["Thickness"];
                                    var width = reader["Width"];
                                    var length = reader["Length"];

                                    var duedate = DateTime.Parse(reader["DueDate"]);


                                    int master = FindMaster(prodCode, thick, width, length);

                                    if (master != -1)
                                    {
                                        StaticInventoryTracker.AddPastSale(master, duedate, pieces);
                                        Console.WriteLine(@"{0},{1},{2},{3},{4}", prodCode, tex,
                                            thick,
                                             width, length);
                                    }
                                    ++rows;
                                }
                                catch (Exception rowException)
                                {
                                    if (CSVImport)
                                        MessageBox.Show($"Row isssue on line {curRow}: {rowException.Message}");
                                    Console.WriteLine(@"Row isssue on line {0}: {1}", curRow, rowException.Message);
                                }
                            }
                        }

                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Could not open file. Is it in use?");
                        successful = false;
                    }
                }

                MessageBox.Show($"Imported {rows} rows");
            }
            catch (Exception)
            {
                successful = false;
            }

            return successful;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="prodCode"></param>
        /// <param name="thick"></param>
        /// <param name="width"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        private int FindMaster(string prodCode, string thick, string width, string length)
        {
            int id = -1;

            double thickness = GetThickness(thick);
            double wid = StringFractionToDouble(width);
            double len = StringFractionToDouble(length);
            StaticFunctions.StringToDouble(thick);


            ProductMasterItem masterItem = StaticInventoryTracker.ProductMasterList.FirstOrDefault(master =>
                master.ProductionCode == prodCode &&
                Math.Abs(master.Thickness - thickness) < 0.001 &&
                Math.Abs(master.Width - wid) < 0.001 &&
                Math.Abs(master.Length - len) < 0.001);

            if (masterItem != null)
                id = masterItem.MasterID;

            return id;
        }

        /// <summary>
        /// Parses pased string such as 10 1/2 into double: 10.5d
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public double StringFractionToDouble(string number)
        {
            double result;

            string[] splitStrings = number.Split(' ');

            if (splitStrings[0].Contains('/')) //fraction only
            {

                result = GetFraction(splitStrings[0]);
            }
            else // leading whole number
            {
                result = Double.Parse(splitStrings[0]);

                if (splitStrings.Length > 1 && splitStrings[1].Contains('/')) // if there is a fraction at the end
                {
                    result += GetFraction(splitStrings[1]);
                }
            }

            return result;
        }

        private double GetFraction(string fraction)
        {
            string[] parts = fraction.Split('/');

            return Double.Parse(parts[0]) / Double.Parse(parts[1]);
        }

        private double GetThickness(string sqlOutput)
        {
            // rules are as follows: the first character is the numerator. All following are the denominator
            double numberator = Double.Parse(sqlOutput[0].ToString());
            double denom = Double.Parse(sqlOutput.Substring(1));

            var thickness = numberator / denom;

            return thickness;
        }

        /// <summary>
        /// Used for debug logs in release builds.
        /// </summary>
        /// <param name="line"></param>
        private void OutputDebugLine(String line)
        {
            using (StreamWriter file = new StreamWriter(OutputDebugFile, true))
            {
                //MessageBox.Show("Error detected. Outputpath: " + OutputDebugFile);
                file.WriteLine(line);
            }
        }

    }

}
