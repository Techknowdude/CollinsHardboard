using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using ModelLib;
using StaticHelpers;

namespace ImportLib
{
    public class StaticInventoryTracker
    {
        #region DataMembers

        private const string datFile = "InventoryTrack.dat";

        private static List<Texture> _texturesList = new List<Texture>(){ Texture.GetTexture( "Old Mill"), Texture.GetTexture("Shake"), Texture.GetTexture("Smooth")}; 

        private static Char _wipMarker = 'W';
        public static Char WiPMarker { get { return _wipMarker; } set { _wipMarker = value; } }

        static StaticInventoryTracker _instance = new StaticInventoryTracker();

        static List<InventoryChange> _inventoryChanges = new List<InventoryChange>();

        static List<InventoryItem> _inventoryItems = new List<InventoryItem>();
        static ObservableCollection<InventoryItem> _wipItems = new ObservableCollection<InventoryItem>(); 

        static ObservableCollection<SalesItem> _salesItems = new ObservableCollection<SalesItem>();

        static ObservableCollection<ProductMasterItem> _productMasterList = new ObservableCollection<ProductMasterItem>();
        static ObservableCollection<ProductMasterItem> _pressMasterList = new ObservableCollection<ProductMasterItem>(); 
        private static bool _isLoaded = false;

        #endregion

        #region Properties
        public static ObservableCollection<ProductMasterItem> ProductMasterList
        {
            get
            {
                return _productMasterList;
            }
            set
            {
                _productMasterList = value;
            }
        }

        public static void AddMasterItem(ProductMasterItem item)
        {
            _productMasterList.Add(item);
            if (item.MadeIn == "Press") 
                _pressMasterList.Add(item);
        }

        public static ObservableCollection<SalesItem> SalesItems
        {
            get
            {
                return _salesItems;
            }
            set
            {
                _salesItems = value;
            }
        }

        public static void AddSalesItem(SalesItem item)
        {
            _salesItems.Add(item);
        }

        public static ObservableCollection<InventoryItem> WiPItems { get { return _wipItems; } set { _wipItems = value; } }
        public static StaticInventoryTracker Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new StaticInventoryTracker();
                return _instance;
            }
        }

        public static List<InventoryItem> AllInventoryItems
        {
            get
            {
                var list = new List<InventoryItem>();
                list.AddRange(InventoryItems);
                list.AddRange(WiPItems);
                return list;

            }
        }

        public static List<InventoryItem> InventoryItems
        {
            get
            {
                return _inventoryItems;
            }
            set
            {
                _inventoryItems = value;
            }
        }

        public static void AddInventoryItem(InventoryItem item)
        {
            InventoryItems.Add(item);
        }

        static ObservableCollection<ForecastItem> forecastItems = new ObservableCollection<ForecastItem>();

        public static ObservableCollection<ForecastItem> ForecastItems
        {
            get
            {   
                return forecastItems;
            }
        }

        static void ReCalcForecastItems()
        {
            forecastItems.Clear();

            foreach (ProductMasterItem productMasterItem in _productMasterList)
            {
                ForecastItem newForecastItem = new ForecastItem();
                newForecastItem.ProductCode = productMasterItem.ProductionCode;
                newForecastItem.ProductDescription = productMasterItem.Description;

                double inventory =
                    InventoryItems.Where(x => x.ProductCode == productMasterItem.ProductionCode).Sum(x => x.Units);

                for (Int32 i = 0; i < newForecastItem.UnitsPerMonth.Length; i++)
                {
                    SalesItem[] foundItems = SalesItems.Where(x => x.ProductionCode == productMasterItem.ProductionCode &&
                        x.Date.Month == DateTime.Now.AddMonths(-(i + 1)).Month && x.Date.AddYears(1).AddMonths(1) >= DateTime.Now).ToArray();

                    newForecastItem.UnitsPerMonth[i] = foundItems.Sum(x => (x.Units / productMasterItem.PiecesPerUnit));
                }

                newForecastItem.Units = inventory;

                forecastItems.Add(newForecastItem);
            }
        }
        public static List<InventoryChange> InventoryChanges
        {
            get { return _inventoryChanges; }
            set { _inventoryChanges = value; }
        }

        public static bool IsLoaded
        {
            get { return _isLoaded; }
            set { _isLoaded = value; }
        }

        public static ObservableCollection<ProductMasterItem> PressMasterList
        {
            get { return _pressMasterList; }
            set { _pressMasterList = value; }
        }

        #endregion


        /// <summary>
        /// Adds the sale to the list of sales
        /// </summary>
        /// <param name="prodCode"></param>
        /// <param name="invoiceNum"></param>
        /// <param name="date"></param>
        /// <param name="units">Pieces in the order expressed as a decimal in Units</param>
        /// <param name="pieces">Total pieces of the order</param>
        /// <param name="grade"></param>
        /// <param name="master"></param>
        /// <returns></returns>
        public static SalesItem AddSales(string prodCode, string invoiceNum, DateTime date, double units, double pieces, string grade, int master)
        {
            var sale = new SalesItem(prodCode, invoiceNum, units, pieces, grade, date, -1,master);
            AddSales(sale);
            return sale;
        }

        public static void AddSales(SalesItem item)
        {
            AddSalesItem(item);
        }

        public static bool AddInventory(string prodCode, double piecesPer, double units, string grade, int master)
        {
           return AddInventory(new InventoryItem(prodCode, units, piecesPer, grade, master));
        }

        public static bool AddInventory(InventoryItem item)
        {
            try
            {
                Int32 foundIndex = 0;
                if (item.ProductCode[0] == 'W')
                {
                    foundIndex = WiPItems.ToList().FindIndex(x => item.MasterID != -1 && x.MasterID == item.MasterID);
                    if (foundIndex == -1)
                        WiPItems.Add(item); // not found, add new
                    else
                    {
                        InventoryItem existingItem = WiPItems[foundIndex]; //update old inventory
                        existingItem.SetValues(item); // WiP inventory should only occur once, so set to the most recent value since this is a overwrite load.
                    }
                }
                else
                {
                    foundIndex = InventoryItems.FindIndex(x => item.MasterID != -1 && x.MasterID == item.MasterID);
                    if (foundIndex == -1)
                        AddInventoryItem(item); // not found, add new
                    else
                    {
                        InventoryItem existingItem = InventoryItems[foundIndex]; //update old inventory
                        existingItem.Units += item.Units; // add the units, as 
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public static bool AddProduct(ProductMasterItem item)
        {
            try
            {
                Int32 foundIndex = 0;
                foundIndex = ProductMasterList.ToList().FindIndex(x => x.ProductionCode == item.ProductionCode);
                if (foundIndex == -1)
                    AddMasterItem(item); // not found, add new
                else
                {

                    ProductMasterItem existingItem = ProductMasterList[foundIndex];
                    existingItem.SetValues(item);
                }
            }
            catch (Exception) // adding failed.
            {
                return false;
            }
            return true;
        }


        public static bool AddProduct(int idNumber, string prodCode, string description, double width, double length, string thickness, string texture, double waste, int pcsUnit, string grades, bool hasBarcode, string notes, string turnType, double minSupply, double maxSupply, double targetSupply, double unitsPerHour)
        {
            return AddProduct(new ProductMasterItem(idNumber,prodCode, description, width, length, thickness, texture, waste, pcsUnit, grades,
                hasBarcode,notes,turnType,minSupply,maxSupply,targetSupply,unitsPerHour));
        }

        public static bool LoadDefaults()
        {
            bool load = LoadFromBin(datFile);

            if (load)
            {
                IsLoaded = true;
            }
            return load;
        }

        public static void SaveDefaults()
        {
            SaveToBin(datFile);
        }

        private static bool LoadFromBin(string fileName)
        {
            try
            {
                using (BinaryReader reader = new BinaryReader(new FileStream(fileName, FileMode.Open)))
                {
                    // Order: Changes,Master,Inventory,Sales,Texture,WiP Marker,WiP,

                    int listLen = reader.ReadInt32();
                    InventoryChanges.Clear();
                    for (; listLen > 0; --listLen)
                    {
                        InventoryChanges.Add(InventoryChange.LoadFromBin(reader));
                    }
                    
                    
                    listLen = reader.ReadInt32();

                    ProductMasterList.Clear();
                    for(;listLen > 0; --listLen)
                    {
                        ProductMasterList.Add(ProductMasterItem.Load(reader));
                    }
                    

                    WiPMarker = reader.ReadChar();

                    listLen = reader.ReadInt32();
                    InventoryItems.Clear();
                    for (; listLen > 0; --listLen)
                    {
                        WiPItems.Add(InventoryItem.LoadItem(reader));
                    }
                }
                IsLoaded = true;
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        private static bool SaveToBin(string fileName)
        {
            try
            {
                using (BinaryWriter writer = new BinaryWriter(new FileStream(fileName, FileMode.OpenOrCreate)))
                {
                    // Order: Changes,Master,Texture,WiP Marker,WiP,
                    writer.Write(InventoryChanges.Count);
                    foreach (var inventoryChange in InventoryChanges)
                    {
                        inventoryChange.SaveToBin(writer);
                    }
                    writer.Write(ProductMasterList.Count);
                    foreach (var productMasterItem in ProductMasterList)
                    {
                        productMasterItem.Save(writer);
                    }

                    writer.Write(WiPMarker);
                    writer.Write(WiPItems.Count);
                    foreach (var inventoryItem in WiPItems)
                    {
                        inventoryItem.SaveItem(writer);
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public static double GetEfficiency(DateTime start, DateTime end)
        {
            var changes = InventoryChanges.Where(change => change.Date >= start && change.Date <= end).ToList();

            double expected = changes.Sum(change => change.UnitsExpected);
            double made = changes.Sum(change => change.UnitsMade);

            return made/expected;
        }

        public static double GetEfficiency(DateTime start, DateTime end, int masterId)
        {
            var changes = InventoryChanges.Where(change =>change.MasterId == masterId && change.Date >= start && change.Date <= end).ToList();

            double expected = changes.Sum(change => change.UnitsExpected);
            double made = changes.Sum(change => change.UnitsMade);

            return made / expected;
        }

        /// <summary>
        /// Gets the sale with the soonest due date.
        /// </summary>
        /// <param name="nextItem"></param>
        /// <returns></returns>
        public static SalesItem GetPrioritySale(ProductMasterItem nextItem)
        {
            SalesItem sale = null;

            DateTime minDate = DateTime.MaxValue;
            foreach (var item in SalesItems.Where(s => s.Fulfilled < s.Units))
            {
                if (item.Date < minDate)
                {
                    minDate = item.Date;
                    sale = item;
                }
            }

            return sale;
        }

        public static void AddPastSale(int master, DateTime duedate, double pieces)
        {
            if (duedate.Year == DateTime.Today.Year && duedate.Month == DateTime.Today.Month) return; //discard current month data

            ForecastItem item = ForecastItems.FirstOrDefault(i => master != -1 && i.MasterID == master);

            var inv = InventoryItems.FirstOrDefault(x => x.MasterID == master);
            var masterItem =
                ProductMasterList.FirstOrDefault(x => x.MasterID == master);
            if (masterItem != null)
            {
                double units = pieces / masterItem.PiecesPerUnit;

                if (item != null) // if tracking item already
                {
                    item.AddSale(duedate, units);
                }
                else // add tracking
                {
                    double invUnits = 0;
                    if (inv != null)
                        invUnits = inv.Units;
                    else
                    {
                        //MessageBox.Show(String.Format("No inventory for id:{0} master {1}", master, masterItem));
                    }
                    item = new ForecastItem(invUnits,masterItem);
                    item.AddSale(duedate,units);
                    ForecastItems.Add(item);
                }
            }
        }
    }

    public class InventoryChange
    {
        #region Fields
        private double _unitsExpected;
        private double _unitsMade;
        private int _masterId;
        private DateTime _date;
        #endregion

        #region Properties

        public double UnitsExpected
        {
            get { return _unitsExpected; }
            set { _unitsExpected = value; }
        }

        public int MasterId
        {
            get { return _masterId; }
            set { _masterId = value; }
        }

        public DateTime Date
        {
            get { return _date; }
            set { _date = value; }
        }

        public double UnitsMade
        {
            get { return _unitsMade; }
            set { _unitsMade = value; }
        }

        public double Efficiency
        {
            get { return UnitsMade/UnitsExpected; }
        }

        #endregion

        public InventoryChange(double unitsExpected, double unitsMade, int masterId, DateTime date)
        {
            _unitsExpected = unitsExpected;
            _masterId = masterId;
            _date = date;
            _unitsMade = unitsMade;
        }

        public static InventoryChange LoadFromBin(BinaryReader reader)
        {
            double units = reader.ReadDouble();
            int id = reader.ReadInt32();
            DateTime date;
            DateTime.TryParse(reader.ReadString(), out date);
            double made = reader.ReadDouble();

           return new InventoryChange(units,made,id,date);
        }

        public bool SaveToBin(BinaryWriter writer)
        {
            try
            {
                writer.Write(UnitsExpected);
                writer.Write(MasterId);
                writer.Write(Date.ToLongDateString());
                writer.Write(UnitsMade);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
    }
}
