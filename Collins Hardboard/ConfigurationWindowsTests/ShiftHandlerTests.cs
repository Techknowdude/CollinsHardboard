using Microsoft.VisualStudio.TestTools.UnitTesting;
using Configuration_windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Configuration_windows.Tests
{
    [TestClass()]
    public class ShiftHandlerTests
    {
        [TestMethod()]
        public void GetPreviousShiftEndTestCoating()
        {
            var daylist = new List<DayOfWeek>()
            {
                DayOfWeek.Monday,
                DayOfWeek.Tuesday,
                DayOfWeek.Wednesday,
                DayOfWeek.Thursday,
                DayOfWeek.Friday,
                DayOfWeek.Saturday,
                DayOfWeek.Sunday
            };

            var date = new DateTime(2016, 6, 30);
            var defaultDate = new DateTime(200, 1, 1);

            Shift gyd = Shift.ShiftFactory("Gyd", defaultDate + TimeSpan.FromHours(23), TimeSpan.FromHours(8), DateTime.MinValue, DateTime.MaxValue, null, daylist);
            Shift days = Shift.ShiftFactory("Days", defaultDate + TimeSpan.FromHours(7), TimeSpan.FromHours(8), DateTime.MinValue, DateTime.MaxValue, null, daylist);
            Shift swing = Shift.ShiftFactory("Swing", defaultDate + TimeSpan.FromHours(15), TimeSpan.FromHours(8), DateTime.MinValue, DateTime.MaxValue, null, daylist);

            DateTime gydStart = date + TimeSpan.FromHours(-1);
            DateTime daysStart = date + TimeSpan.FromHours(7);
            DateTime swingStart = date + TimeSpan.FromHours(-9);

            ShiftHandler.CoatingInstance.Shifts.Clear();
            ShiftHandler.CoatingInstance.Shifts.Add(gyd);
            ShiftHandler.CoatingInstance.Shifts.Add(days);
            ShiftHandler.CoatingInstance.Shifts.Add(swing);

            var prevDays = ShiftHandler.CoatingInstance.GetPreviousShiftStart(date, days);
            var prevSwing = ShiftHandler.CoatingInstance.GetPreviousShiftStart(date, swing);
            var prevGyd = ShiftHandler.CoatingInstance.GetPreviousShiftStart(date, gyd);

            Assert.AreEqual(gydStart, prevDays);
            Assert.AreEqual(daysStart, prevSwing);
            Assert.AreEqual(swingStart, prevGyd);
        }

        [TestMethod()]
        public void GetPreviousShiftTest()
        {
            var daylist = new List<DayOfWeek>()
            {
                DayOfWeek.Monday,
                DayOfWeek.Tuesday,
                DayOfWeek.Wednesday,
                DayOfWeek.Thursday,
                DayOfWeek.Friday,
                DayOfWeek.Saturday,
                DayOfWeek.Sunday
            };

            var defaultDate = new DateTime(200, 1, 1);

            Shift gyd = Shift.ShiftFactory("Gyd", defaultDate + TimeSpan.FromHours(23), TimeSpan.FromHours(8), DateTime.MinValue, DateTime.MaxValue, null, daylist);
            Shift days = Shift.ShiftFactory("Days", defaultDate + TimeSpan.FromHours(7), TimeSpan.FromHours(8), DateTime.MinValue, DateTime.MaxValue, null, daylist);
            Shift swing = Shift.ShiftFactory("Swing", defaultDate + TimeSpan.FromHours(15), TimeSpan.FromHours(8), DateTime.MinValue, DateTime.MaxValue, null, daylist);

            ShiftHandler.CoatingInstance.Shifts.Clear();
            ShiftHandler.CoatingInstance.Shifts.Add(gyd);
            ShiftHandler.CoatingInstance.Shifts.Add(days);
            ShiftHandler.CoatingInstance.Shifts.Add(swing);

            var prevSwing = ShiftHandler.CoatingInstance.GetPreviousShift(swing);
            var prevDays = ShiftHandler.CoatingInstance.GetPreviousShift(days);
            var prevGyd = ShiftHandler.CoatingInstance.GetPreviousShift(gyd);

            Assert.AreEqual(gyd,prevDays);
            Assert.AreEqual(days,prevSwing);
            Assert.AreEqual(swing,prevGyd);

        }
    }
}