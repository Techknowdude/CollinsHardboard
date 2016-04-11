using Microsoft.VisualStudio.TestTools.UnitTesting;
using CoatingScheduler;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoatingScheduler.Tests
{
    [TestClass()]
    public class CoatingScheduleTests
    {
        [TestMethod()]
        public void SaveLoadTestEmpty()
        {
            CoatingSchedule schedule = new CoatingSchedule();
            var mem = new MemoryStream();
            using (BinaryWriter writer = new BinaryWriter(mem))
            {
                schedule.Save(writer);
            }

            CoatingSchedule other = null;

            using (BinaryReader reader = new BinaryReader(mem))
            {
                other = CoatingSchedule.Load(reader);
            }


        }
    }
}