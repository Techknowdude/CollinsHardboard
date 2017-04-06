using System;

namespace ModelLib
{
    public class ForecastItem
    {
        #region DataMembers

        public String ProductCode { get; set; }
        public String ProductDescription { get; set; }

        /// <summary>
        /// Current units in inventory
        /// </summary>
        public double Units { get; set; }

        private double[] _unitsPerMonth = new double[13];

        /// <summary>
        /// Master item sold
        /// </summary>
        public int MasterID { get; set; }

        /// <summary>
        /// Units sold per month. 0 is this month, 1 is last month
        /// </summary>
        public double[] UnitsPerMonth { get { return _unitsPerMonth; } set { _unitsPerMonth = value; } }

        public String OneMonthAvg { get { return String.Format("{0,-5:F1} Mo", AvgOneMonth); } }
        public String ThreeMonthAvg { get { return GetAvgDeriv(0,3); } }
        public String SixMonthAvg { get { return GetAvgDeriv(0,6); } }
        public String TwelveMonthAvg { get { return GetAvgDeriv(0,12); } }
        public String PastYearAvg { get { return GetAvgDeriv(10, 3); } }
        private String GetAvgDeriv(Int32 monthsPrior, Int32 duration)
        {
            double supply = Double.IsNaN(Units/GetAvg(monthsPrior, duration)) ? 0.0 : Units/GetAvg(monthsPrior, duration);
            double deriv = Double.IsNaN(GetDeviation(monthsPrior, duration)) ? 0.0 : GetDeviation(monthsPrior, duration);
            return String.Format("{0,-5:F1}Mo ± {1,-5:F1}u", supply, deriv);
        }

        public double AvgPastYear
        {
            get
            {
                double retVal = Units/GetAvg(10, 3);
                if (Double.IsNaN(retVal))
                    retVal = 0;
                return retVal;
            }
        }
        public double AvgOneMonth
        {
            get
            {
                double retVal = Units/GetAvg(0,1);
                if (Double.IsNaN(retVal))
                    retVal = 0;
                return retVal;
            }
        }

        public double AvgThreeMonths
        {
            get
            {
                double retVal = Units / GetAvg(0,3);
                if (Double.IsNaN(retVal))
                    retVal = 0;
                return retVal;
            }
        }
        public double AvgSixMonths
        {
            get
            {
                double retVal = Units / GetAvg(0,6);
                if (Double.IsNaN(retVal))
                    retVal = 0;
                return retVal;
            }
        }
        public double AvgTwelveMonths
        {
            get
            {
                double retVal = Units / GetAvg(0,12);
                if (Double.IsNaN(retVal))
                    retVal = 0;
                return retVal;
            }
        }

        public String SoldOneMonthAgo { get { return UnitsPerMonth[0].ToString("F1"); } }
        public String SoldTwoMonthsAgo { get { return UnitsPerMonth[1].ToString("F1"); } }
        public String SoldThreeMonthsAgo { get { return UnitsPerMonth[2].ToString("F1"); } }
        public String SoldFourMonthsAgo { get { return UnitsPerMonth[3].ToString("F1"); } }
        public String SoldFiveMonthsAgo { get { return UnitsPerMonth[4].ToString("F1"); } }
        public String SoldSixMonthsAgo { get { return UnitsPerMonth[5].ToString("F1"); } }
        public String SoldSevenMonthsAgo { get { return UnitsPerMonth[6].ToString("F1"); } }
        public String SoldEightMonthsAgo { get { return UnitsPerMonth[7].ToString("F1"); } }
        public String SoldNineMonthsAgo { get { return UnitsPerMonth[8].ToString("F1"); } }
        public String SoldTenMonthsAgo { get { return UnitsPerMonth[9].ToString("F1"); } }
        public String SoldElevenMonthsAgo { get { return UnitsPerMonth[10].ToString("F1"); } }
        public String SoldTweleveMonthsAgo { get { return UnitsPerMonth[11].ToString("F1"); } }
        public String SoldThirteenMonthsAgo { get { return UnitsPerMonth[12].ToString("F1"); } }
        
        public double SoldUnits1MonthAgo { get { return UnitsPerMonth[0]; } }
        public double SoldUnits2MonthsAgo { get { return UnitsPerMonth[1]; } }
        public double SoldUnits3MonthsAgo { get { return UnitsPerMonth[2]; } }
        public double SoldUnits4MonthsAgo { get { return UnitsPerMonth[3]; } }
        public double SoldUnits5MonthsAgo { get { return UnitsPerMonth[4]; } }
        public double SoldUnits6MonthsAgo { get { return UnitsPerMonth[5]; } }
        public double SoldUnits7MonthsAgo { get { return UnitsPerMonth[6]; } }
        public double SoldUnits8MonthsAgo { get { return UnitsPerMonth[7]; } }
        public double SoldUnits9MonthsAgo { get { return UnitsPerMonth[8]; } }
        public double SoldUnits10MonthsAgo { get { return UnitsPerMonth[9]; } }
        public double SoldUnits11MonthsAgo { get { return UnitsPerMonth[10]; } }
        public double SoldUnits12MonthsAgo { get { return UnitsPerMonth[11]; } }
        public double SoldUnits13MonthsAgo { get { return UnitsPerMonth[12]; } }

        #endregion

        public ForecastItem()
        { }


        public ForecastItem(double units, ProductMasterItem master, double[] sales = null)
        {
            if (sales != null && sales.Length != 13)
                throw new Exception("Sales must be 13 months into past.");
            Units = units;
            ProductCode = master.ProductionCode;
            ProductDescription = master.Description;
            MasterID = master.MasterID;

            UnitsPerMonth = sales;
            InitializeSoldUnits(); // double check initialization
        }


        public double GetAvg(Int32 monthsPrior, Int32 duration)
        {
            double runningTotal = 0;

            for (Int32 i = monthsPrior; i < monthsPrior + duration; ++i)
                runningTotal += UnitsPerMonth[i];
            return runningTotal/duration;
        }

        public double GetDeviation(Int32 monthsPrior, Int32 duration)
        {
            double avg = GetAvg(monthsPrior, duration);
            double runningTotal = 0;
            for (Int32 i = monthsPrior; i < monthsPrior + duration; i++)
            {
                runningTotal += Math.Pow((UnitsPerMonth[i] - avg),2);
            }
            return Math.Sqrt(runningTotal/duration);
        }

        public void AddSale(DateTime date, double units)
        {
            // don't accept sales this month
            if (date.Year == DateTime.Today.Year && date.Month == DateTime.Today.Month) return;

            //                   Now (say, 12)  -   sale (say, 10) - 1 (for zero index starting at last month). Would be 1, for 2 months ago
            int monthsAgo = DateTime.Today.Month - date.Month - 1;
            // check for negative months... like 2 - 12 - 1, which should be 1

            if (monthsAgo < 0)
                monthsAgo += 12; // add 12 months to offset error

            // varify initialization
            InitializeSoldUnits();

            UnitsPerMonth[monthsAgo] += units;
        }

        private void InitializeSoldUnits()
        {
            if (UnitsPerMonth == null)
            {
                UnitsPerMonth = new double[13];
                for (int index = 0; index < 13; index++)
                {
                    UnitsPerMonth[index] = 0;
                }
            }
        }

        public double GetAvgUnitsSold(SalesDurationEnum generationDataSalesOutlookDuration)
        {
            double unitSoldAvg = 0;

            switch (generationDataSalesOutlookDuration)
            {
                case SalesDurationEnum.LastMonth:
                    unitSoldAvg = GetAvg(0,1);
                    break;
                case SalesDurationEnum.Last3Months:
                    unitSoldAvg = GetAvg(0,3);
                    break;
                case SalesDurationEnum.Last6Months:
                    unitSoldAvg = GetAvg(0,6);
                    break;
                case SalesDurationEnum.Last12Months:
                    unitSoldAvg = GetAvg(0,2);
                    break;
                case SalesDurationEnum.LastYear:
                    unitSoldAvg = GetAvg(10,3);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(generationDataSalesOutlookDuration), generationDataSalesOutlookDuration, null);
            }

            return unitSoldAvg;
        }
    }
}
