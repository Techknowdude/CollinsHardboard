using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModelLib;

namespace UnitTests
{
    [TestClass]
    public class ForecastItemTests
    {
        private TestContext testContextInstance;
        public TestContext TestContext
        {
            get { return testContextInstance; }
            set { testContextInstance = value; }
        }

        private static Int32 devRowCount = 1;
        private static Int32 avgRowCount = 1;

        [TestMethod]
        [DeploymentItem("TestingData.xls")]
        [DataSource("System.Data.OleDb",
            "Provider=Microsoft.Jet.OLEDB.4.0;Data Source='TestingData.xls';Extended Properties='Excel 8.0'",
            "ForecastItem$", // The table name, in this case, the sheet name with a '$' appended.				
            DataAccessMethod.Sequential)]
        public void CheckDeviationCalculation()
        {
            devRowCount++;
            ForecastItem testItem = new ForecastItem();
            double[] unitsByMonth = new double[testItem.UnitsPerMonth.Length];
            for (Int32 index = 0; index < testItem.UnitsPerMonth.Length; index++)
            {
                unitsByMonth[index] = (double)TestContext.DataRow["Month" + (index + 1)];
            }
            double threeMonthDeviation = (double)TestContext.DataRow["3MonthDeviation"];
            double sixMonthDeviation = (double)TestContext.DataRow["6MonthDeviation"];
            double tweleveMonthDeviation = (double)TestContext.DataRow["12MonthDeviation"];
            double pastYearDeviation = (double)TestContext.DataRow["PastYearDeviation"];

            testItem.UnitsPerMonth = unitsByMonth;

            Assert.IsTrue(Math.Abs(threeMonthDeviation - testItem.GetDeviation(0, 3)) < 0.00001, String.Format("3MonthDeviation incorrect. Row: {0}. Expected {1}, Got {2}", devRowCount, threeMonthDeviation, testItem.GetDeviation(0, 3)));
            Assert.IsTrue(Math.Abs(sixMonthDeviation - testItem.GetDeviation(0, 6)) < 0.00001, String.Format("6MonthDeviation incorrect. Row: {0}. Expected {1}, Got {2}", devRowCount, sixMonthDeviation, testItem.GetDeviation(0, 6)));
            Assert.IsTrue(Math.Abs(tweleveMonthDeviation - testItem.GetDeviation(0, 12)) < 0.00001, String.Format("12MonthDeviation incorrect. Row: {0}. Expected {1}, Got {2}", devRowCount, tweleveMonthDeviation, testItem.GetDeviation(0, 12)));
            Assert.IsTrue(Math.Abs(pastYearDeviation - testItem.GetDeviation(10, 3)) < 0.00001, String.Format("PastYearDeviation incorrect. Row: {0}. Expected {1}, Got {2}", devRowCount, pastYearDeviation, testItem.GetDeviation(10, 3)));
        }
        [TestMethod]
        [DeploymentItem("TestingData.xls")]
        [DataSource("System.Data.OleDb",
            "Provider=Microsoft.Jet.OLEDB.4.0;Data Source='TestingData.xls';Extended Properties='Excel 8.0'",
            "ForecastItem$", // The table name, in this case, the sheet name with a '$' appended.				
            DataAccessMethod.Sequential)]
        public void CheckAvgCalculation()
        {
            avgRowCount++;
            ForecastItem testItem = new ForecastItem();
            double[] unitsByMonth = new double[testItem.UnitsPerMonth.Length];
            for (Int32 index = 0; index < testItem.UnitsPerMonth.Length; index++)
            {
                unitsByMonth[index] = (double)TestContext.DataRow["Month" + (index + 1)];
            }
            double threeMonthAvg = (double)TestContext.DataRow["3MonthAvg"];
            double sixMonthAvg = (double)TestContext.DataRow["6MonthAvg"];
            double tweleveMonthAvg = (double)TestContext.DataRow["12MonthAvg"];
            double pastYearAvg = (double)TestContext.DataRow["PastYearAvg"];
            double units = (double) TestContext.DataRow["Units"];

            testItem.Units = units;
            testItem.UnitsPerMonth = unitsByMonth;

            Assert.IsTrue(Math.Abs(threeMonthAvg - testItem.AvgThreeMonths) < 0.00001, String.Format("3MonthAvg incorrect. Row: {0}. Expected {1}, Got {2}", devRowCount, threeMonthAvg, testItem.AvgThreeMonths));
            Assert.IsTrue(Math.Abs(sixMonthAvg - testItem.AvgSixMonths) < 0.00001, String.Format("6MonthAvg incorrect. Row: {0}. Expected {1}, Got {2}", devRowCount, sixMonthAvg, testItem.AvgSixMonths));
            Assert.IsTrue(Math.Abs(tweleveMonthAvg - testItem.AvgTwelveMonths) < 0.00001, String.Format("12MonthAvg incorrect. Row: {0}. Expected {1}, Got {2}", devRowCount, tweleveMonthAvg, testItem.AvgTwelveMonths));
            Assert.IsTrue(Math.Abs(pastYearAvg - testItem.AvgPastYear) < 0.00001, String.Format("PastYearAvg incorrect. Row: {0}. Expected {1}, Got {2}", devRowCount, pastYearAvg, testItem.AvgPastYear));
        }
    }
}
