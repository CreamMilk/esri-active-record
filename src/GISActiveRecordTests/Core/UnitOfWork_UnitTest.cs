using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geodatabase;
using GISActiveRecord.Core;
using GISActiveRecord.Repository;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GISActiveRecordTests.Core
{
    [TestClass]
    public class UnitOfWork_UnitTests
    {
        private UnitOfWork uw;

        [TestInitialize]
        public void Init()
        {
            IAoInitialize init = new AoInitializeClass();
            init.Initialize(esriLicenseProductCode.esriLicenseProductCodeArcInfo);
        }

        private IWorkspace GetGeodatabase()
        {

            IPropertySet2 connectionProperties =  ConnectionProperties.GetConnectionProperties();
            IWorkspace wks = WorkspaceHandler.GetSdeWorkspace(connectionProperties);

            return wks;
        }

        [TestMethod]
        public void UnitOfWork_Constructor_Test()
        {
            IWorkspace wks = GetGeodatabase();

            uw = new UnitOfWork(wks, true);

            Assert.AreEqual(0, uw.Created.Count);
            Assert.AreEqual(0, uw.Updated.Count);
            Assert.AreEqual(0, uw.Deleted.Count);
        }

        [TestMethod]
        public void UnitOfWork_Events_Test()
        {
            IWorkspace wks = GetGeodatabase();
            uw = new UnitOfWork(wks, true);

            uw.Commit();
            Assert.Inconclusive("");
        }

        [TestMethod]
        public void UnitOfWork_Create_Test()
        {
            IWorkspace wks = GetGeodatabase();
            uw = new UnitOfWork(wks, true);

            IActiveRecord record = new EspacoAereoTests.Model.ActiveRecordMock();

            uw.Create(record);
            Assert.AreEqual(1, uw.Created);
        }

        [TestMethod]
        public void UnitOfWork_Update_Test()
        {
            IWorkspace wks = GetGeodatabase();
            uw = new UnitOfWork(wks, true);

            IActiveRecord record = new EspacoAereoTests.Model.ActiveRecordMock();

            uw.Update(record);
            Assert.AreEqual(1, uw.Updated);
        }

        [TestMethod]
        public void UnitOfWork_Delete_Test()
        {
            IWorkspace wks = GetGeodatabase();
            uw = new UnitOfWork(wks, true);

            IActiveRecord record = new EspacoAereoTests.Model.ActiveRecordMock();

            uw.Delete(record);
            Assert.AreEqual(1, uw.Deleted);
        }

        [TestMethod]
        public void UnitOfWork_OneFeatureMultipleActions_Test()
        {
            IWorkspace wks = GetGeodatabase();
            uw = new UnitOfWork(wks, true);

            IActiveRecord record = new ActiveRecordMock();
            uw.Create(record);
            uw.Update(record);
            uw.Delete(record);

            Assert.AreEqual(1, uw.Created.Count);
            Assert.AreEqual(1, uw.Updated.Count);
            Assert.AreEqual(1, uw.Deleted.Count);

            uw.Commit();

            Assert.AreEqual(0, uw.Created.Count);
            Assert.AreEqual(0, uw.Updated.Count);
            Assert.AreEqual(0, uw.Deleted.Count);

            Assert.Inconclusive();
        }

        [TestMethod]
        public void UnitOfWork_Rollback_Test()
        {
            IWorkspace wks = GetGeodatabase();
            uw = new UnitOfWork(wks, true);

            IActiveRecord record = new ActiveRecordFaultyMock();
            uw.Update(record);

            Assert.AreEqual(0, uw.Created.Count);
            Assert.AreEqual(1, uw.Updated.Count);
            Assert.AreEqual(0, uw.Deleted.Count);

            uw.Commit();

            Assert.AreEqual(0, uw.Created.Count);
            Assert.AreEqual(0, uw.Updated.Count);
            Assert.AreEqual(0, uw.Deleted.Count);

            Assert.Inconclusive();
        }
    }
}
