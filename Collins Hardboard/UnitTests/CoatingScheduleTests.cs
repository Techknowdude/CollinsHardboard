using CoatingScheduler;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    /// <summary>
    /// Summary description for CoatingScheduleTests
    /// </summary>
    [TestClass]
    public class CoatingScheduleTests
    {
        public CoatingScheduleTests()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext _testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return _testContextInstance;
            }
            set
            {
                _testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void TestProductConnectionToControl()
        {
            //Initialize test data
            CoatingScheduleProduct product1 = new CoatingScheduleProduct((double)5/16,"Desc1","Code1","Grade1","Units1");
            CoatingScheduleProduct product2 = new CoatingScheduleProduct((double)1/2,"Desc2","Code2","Grades2","Units2");

            ProductControl control1 = ProductControl.CreateControl(product1);
            ProductControl control2 = ProductControl.CreateControl(product2);

            product1.Connect(control1);
            product2.Connect(control2);

            // Check for good starting data
            Assert.AreEqual(product1.Control, control1,"Product 1 references Control 1");
            Assert.AreEqual(product2.Control, control2, "Product 2 references Control 2");

            Assert.AreEqual(control1.GetLogic(), product1, "Control 1 references Product 1");
            Assert.AreEqual(control2.GetLogic(), product2, "Control 2 references Product 2");

            // Do action
            product1.SwapControls(product2);

            // Check result
            Assert.AreEqual(product1.Control, control2, "Product 1 references Control 2");
            Assert.AreEqual(product2.Control, control1, "Product 2 references Control 1");

            Assert.AreEqual(control1.GetLogic(), product2, "Control 1 references Product 2");
            Assert.AreEqual(control2.GetLogic(), product1, "Control 2 references Product 1");

        }
    }
}
