using Microsoft.VisualStudio.TestTools.UnitTesting;
using Configuration_windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImportLib;
using ModelLib;

namespace Configuration_windows.Tests
{
    [TestClass()]
    public class ConfigurationTests
    {
        [TestMethod()]
        public void SaveTest()
        {
            InternalImport.GetInstance().ImportMaster();
            List<ProductMasterItem> Masters = new List<ProductMasterItem>();
            List<Configuration> Configurations = new List<Configuration>();

            ConfigurationsHandler.GetInstance().Configurations.Clear();

            for(int i = 0; i < StaticInventoryTracker.ProductMasterList.Count; ++i)
            {

                ProductMasterItem item = StaticInventoryTracker.ProductMasterList[i];

                var config = Configuration.CreateConfiguration(item.Description, i, i, i + 1, i * 2, i * 5, new TimeSpan(i, i + 2, i + 3, i + 4));
                ConfigurationsHandler.GetInstance().AddConfiguration(config);

                Configurations.Add(config);
            }

            ConfigurationsHandler.GetInstance().Save();
            ConfigurationsHandler.GetInstance().Load();

            Assert.AreEqual(ConfigurationsHandler.GetInstance().Configurations.Count, Configurations.Count);

            for(int i = 0; i < Configurations.Count; ++i)
            {
                Assert.IsTrue( Configurations[i].Equals(ConfigurationsHandler.GetInstance().Configurations[i]));
            }
        }
    }
}