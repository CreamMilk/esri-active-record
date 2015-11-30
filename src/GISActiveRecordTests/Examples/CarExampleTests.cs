using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.esriSystem;
using GISActiveRecord.Examples;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GISActiveRecordTests.Examples
{
    /// <summary>
    /// Summary description for CarExampleTests
    /// </summary>
    [TestClass]
    public class CarExampleTests
    {
        public CarExampleTests()
        {
            IAoInitialize init = new AoInitializeClass();
            init.Initialize(esriLicenseProductCode.esriLicenseProductCodeArcInfo);
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
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
        public void CarExample_SingleByOid()
        {
            var carRepository = new CarRepository();
            var bmw = carRepository.SingleByOID(4);

            Assert.AreEqual("BMW", bmw.CarName);
        }

        [TestMethod]
        public void CarExample_Single()
        {
            var carRepository = new CarRepository();
            var camaro = carRepository.SingleByPrimaryKey("Camaro");

            Assert.AreEqual("Camaro", camaro.CarName);
        }

        [TestMethod]
        public void CarExample_SinglCompositeKey()
        {
            var carRepository = new CarRepositoryCompositeKey();
            var pk = new List<object> { "Camaro", "2000" };

            var camaro = carRepository.SingleByPrimaryKeys(pk);

            Assert.AreEqual("Camaro", camaro.CarName);
        }
        [TestMethod]
        public void CarExample_FindAll()
        {
            var repository = new CarRepository();
            var cars = repository.FindAll();

            Assert.AreEqual(8, cars.Count);
        }

        [TestMethod]
        public void CarExample_Find()
        {
            //testando com valores
            var attributes = new List<string> { "CarName", "CarYear" };
            var values = new List<object> { "Camaro", "2000" };
            
            var repository = new CarRepository();
            var cars = repository.Find(attributes,values);

            Assert.AreEqual(1, cars.Count);


            //testando com um valor NULO
            attributes = new List<string> { "CarName", "CarYear" };
            values = new List<object> { "Chevete", null };

            repository = new CarRepository();
            cars = repository.Find(attributes, values);

            Assert.AreEqual(1, cars.Count);
        }

        [TestMethod]
        public void CarExample_GetCamaroDriver()
        {
            var repository = new CarRepository();
            var camaro = repository.SingleByPrimaryKey("Camaro");
            var george = camaro.CarDriver;

            Assert.AreEqual("George", george.DriverName);
            Assert.AreEqual(19, george.DriverAge);
        }

        [TestMethod]
        public void CarExample_GetCamaroPassengers()
        {
            var repository = new CarRepository();
            var camaro = repository.SingleByPrimaryKey("Camaro");
            var passengers = camaro.Passengers;

            Assert.AreEqual(3, passengers.Count);
            Assert.AreEqual(3, camaro.Passengers.Count);
        }

        [TestMethod]
        public void CarExample_SetCamaroDriverName()
        {
            var repository = new CarRepository();
            var camaro = repository.SingleByPrimaryKey("Camaro");

            var dRepository = new DriverRepository();
            dRepository.StartEdit();

            camaro.CarDriver.DriverAge = 25;
            camaro.CarDriver.DriverName = "GEORGE";
            dRepository.Update(camaro.CarDriver);

            Assert.AreEqual(25, camaro.CarDriver.DriverAge);
            Assert.AreEqual("GEORGE", camaro.CarDriver.DriverName);

            camaro.CarDriver.DriverAge = 19;
            camaro.CarDriver.DriverName = "George";
            dRepository.Update(camaro.CarDriver);

            dRepository.StopEdit(false);

            Assert.AreEqual(19, camaro.CarDriver.DriverAge);
            Assert.AreEqual("George", camaro.CarDriver.DriverName);
        }
    }

    [TestClass]
    public class BlobberExampleTests
    {
        [ClassInitialize]
        public static void BlobberExampleInit(TestContext context)
        {
            IAoInitialize init = new AoInitializeClass();
            init.Initialize(esriLicenseProductCode.esriLicenseProductCodeArcInfo);
        }

        public BlobberExampleTests()
        {
        }

        private const string BLOBBER_ID = "TEST_ONE";
        private const string BLOBBER_VALUE_1 = "1";
        private const string BLOBBER_VALUE_2 = "2";
        private const int BLOBBER_VALUE_3 = 3;

        [TestMethod]
        public void BlobberCreateBlobberTest()
        {
            Blobber b = CreateBlobber();

            Assert.AreEqual(BLOBBER_ID, b.BlobberId);
            Assert.IsInstanceOfType(b.Properties, typeof(IPropertySet));

            IPropertySet persistedProperties = b.Properties;
            Assert.AreEqual(3, persistedProperties.Count);
            Assert.AreEqual(BLOBBER_VALUE_1, persistedProperties.GetProperty("VALUE1").ToString());
            Assert.AreEqual(BLOBBER_VALUE_2, persistedProperties.GetProperty("VALUE2").ToString());
            Assert.AreEqual(BLOBBER_VALUE_3, Convert.ToInt32(persistedProperties.GetProperty("VALUE3")));
        }

        public static Blobber CreateBlobber()
        {
            BlobberRepository repository = new BlobberRepository();

            repository.StartEdit();

            Blobber b = repository.Create();
            b.BlobberId = BLOBBER_ID;

            IFeatureLayer layer = new FeatureLayerClass();
            layer.Name = BLOBBER_VALUE_1;

            IPropertySet properties = new PropertySetClass();
            properties.SetProperty("VALUE1", BLOBBER_VALUE_1);
            properties.SetProperty("VALUE2", BLOBBER_VALUE_2);
            properties.SetProperty("VALUE3", BLOBBER_VALUE_3);

            b.Properties = properties;

            b.Store();

            repository.StopEdit(true);

            return b;
        }

        public static int CreateAndGetOID()
        {
            BlobberRepository repository = new BlobberRepository();

            repository.StartEdit();

            Blobber b = repository.Create();
            b.BlobberId = BLOBBER_ID;

            IPropertySet properties = new PropertySetClass();
            properties.SetProperty("VALUE1", BLOBBER_VALUE_1);
            properties.SetProperty("VALUE2", BLOBBER_VALUE_2);
            properties.SetProperty("VALUE3", BLOBBER_VALUE_3);

            b.Properties = properties;

            b.Store();

            repository.StopEdit(true);

            return b.ObjectId;
        }
    }
}
