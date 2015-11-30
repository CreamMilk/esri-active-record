/*
Copyright (c) 2011, George Rodrigues da Cunha Silva
All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:
    * Redistributions of source code must retain the above copyright
      notice, this list of conditions and the following disclaimer.
    * Redistributions in binary form must reproduce the above copyright
      notice, this list of conditions and the following disclaimer in the
      documentation and/or other materials provided with the distribution.
    * Neither the name of the <organization> nor the
      names of its contributors may be used to endorse or promote products
      derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL <COPYRIGHT HOLDER> BE LIABLE FOR ANY
DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geodatabase;
using GISActiveRecord.Controllers;
using GISActiveRecord.Examples;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EsriORMTests
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class GisEntityTest
    {
        public GisEntityTest()
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

        [TestMethod()]
        public void GisEntity_EntityController_SingleOid()
        {
            GisEntityController controller = new GisEntityController();
            GisEntity entity = controller.SingleByOid(1);
            
            Assert.AreEqual("TENT", entity.EntityCode);
            Assert.AreEqual("Entidades", entity.EntityName);
            Assert.AreEqual(String.Empty, entity.EntityFeatureDataset);
            Assert.AreEqual("TENT_ENTIDADES", entity.EntityFeatureClass);
            Assert.AreEqual(String.Empty, entity.EntityClassExtension);
            Assert.AreEqual(99, entity.EntityLevel);
            Assert.AreEqual(5, entity.EntityRanking);
            Assert.AreEqual("Alfanumérica", entity.EntityType);
        }

        [TestMethod()]
        public void GisEntity_EntityController_SinglePrimaryKey()
        {
            GisEntityController controller = new GisEntityController();
            GisEntity entity = controller.Single("TENT");

            Assert.AreEqual("TENT", entity.EntityCode);
            Assert.AreEqual("Entidades", entity.EntityName);
            Assert.AreEqual(String.Empty, entity.EntityFeatureDataset);
            Assert.AreEqual("TENT_ENTIDADES", entity.EntityFeatureClass);
            Assert.AreEqual(String.Empty, entity.EntityClassExtension);
            Assert.AreEqual(99, entity.EntityLevel);
            Assert.AreEqual(5, entity.EntityRanking);
            Assert.AreEqual("Alfanumérica", entity.EntityType);
        }

        [TestMethod()]
        public void GisEntity_EntityController_QueryFilter()
        {
            IQueryFilter filter = new QueryFilterClass();
            filter.WhereClause = "ENT_FD_ENTIDADE = 'PARCELAMENTOS'";

            GisEntityController controller = new GisEntityController();
            List<GisEntity> entities = controller.Filter(filter);
            foreach (GisEntity e in entities)
            {
                Console.WriteLine(e.EntityCode);
            }

            Assert.AreEqual(13, entities.Count());
        }

        [TestMethod()]
        public void GisEntity_EntityController_LazyQueryFilter()
        {
            IQueryFilter filter = new QueryFilterClass();
            filter.WhereClause = "ENT_FD_ENTIDADE = 'PARCELAMENTOS'";

            GisEntityController controller = new GisEntityController();
            
            Console.WriteLine("Início query " + DateTime.Now.ToString());

            List<GisEntity> entities = controller.FilterLazy(filter);
            
            Console.WriteLine("Fim query "+ DateTime.Now.ToString());

            foreach (GisEntity e in entities)
            {
                Assert.AreEqual(true, e.IsProxy);
            }

            foreach (GisEntity e in entities)
            {
                Console.WriteLine("Início fetch Object " + DateTime.Now.Millisecond.ToString());
                Console.WriteLine(e.EntityCode);
                Console.WriteLine("Fim fetch Object " + DateTime.Now.Millisecond.ToString());
                Assert.AreEqual(false, e.IsProxy);
            }
        }

        [TestMethod()]
        public void GisEntity_EntityController_SetValue()
        {
            GisEntityController controller = new GisEntityController();
            controller.StartEdit();
            GisEntity entity = controller.Single("TENT");

            Assert.AreEqual("TENT", entity.EntityCode);
            Assert.AreEqual("Entidades", entity.EntityName);
            Assert.AreEqual(String.Empty, entity.EntityFeatureDataset);
            Assert.AreEqual("TENT_ENTIDADES", entity.EntityFeatureClass);
            Assert.AreEqual(String.Empty, entity.EntityClassExtension);
            Assert.AreEqual(99, entity.EntityLevel);
            Assert.AreEqual(5, entity.EntityRanking);
            Assert.AreEqual("Alfanumérica", entity.EntityType);

            #region settting the values
            entity.EntityFeatureDataset = "TESTE_SET";
            Assert.AreEqual("TESTE_SET", entity.EntityFeatureDataset);

            entity.EntityType = "Georreferenciada";
            Assert.AreEqual("Georreferenciada", entity.EntityType);

            controller.Update(entity);
            #endregion

            #region setting them back
            entity.EntityFeatureDataset = String.Empty;
            Assert.AreEqual(String.Empty, entity.EntityFeatureDataset);

            entity.EntityType = "Alfanumérica";
            Assert.AreEqual("Alfanumérica", entity.EntityType);

            controller.Update(entity);
            #endregion

            controller.StopEdit(true);
        }

        [TestMethod()]
        public void GisEntity_EntityController_CreateDelete()
        {
            GisEntityController controller = new GisEntityController();
            controller.StartEdit();

            GisEntity entity = controller.Create();

            entity.EntityCode = "TST";
            entity.EntityName = "Teste";
            entity.EntityFeatureDataset = string.Empty;
            entity.EntityFeatureClass = "TTST_TESTE";
            entity.EntityClassExtension = string.Empty;
            entity.EntityLevel = 109;
            entity.EntityRanking = 0;
            entity.EntityType = "Alfanumérica";
            controller.Update(entity);

            Assert.AreEqual("TST", entity.EntityCode);
            Assert.AreEqual("Teste", entity.EntityName);
            Assert.AreEqual(string.Empty, entity.EntityFeatureDataset);
            Assert.AreEqual("TTST_TESTE", entity.EntityFeatureClass);
            Assert.AreEqual(string.Empty, entity.EntityClassExtension);
            Assert.AreEqual(109, entity.EntityLevel);
            Assert.AreEqual(0, entity.EntityRanking);
            Assert.AreEqual("Alfanumérica", entity.EntityType);

            controller.Delete(entity);

            IQueryFilter filter = new QueryFilterClass();
            filter.WhereClause = "ENT_CD_ENTIDADE = 'TST'";
            List<GisEntity> entities = controller.Filter(filter);

            Assert.AreEqual(0, entities.Count);

            controller.StopEdit(true);
        }

        [TestMethod()]
        public void GisEntity_EntityController_StartStopEdit()
        {
            GisEntityController controller = new GisEntityController();

            Assert.AreEqual(false, controller.IsBeingEdited);
            Assert.AreEqual(false, controller.IsInEditOperation);

            controller.StartEdit();

            Assert.AreEqual(true, controller.IsBeingEdited);
            Assert.AreEqual(true, controller.IsInEditOperation);

            controller.StopEdit(false);

            Assert.AreEqual(false, controller.IsBeingEdited);
            Assert.AreEqual(false, controller.IsInEditOperation);
        }

        [TestMethod()]
        [ExpectedException(typeof(ControllerQueryException))]
        public void GisEntity_EntityController_InexistentSingle()
        {
            GisEntityController controller = new GisEntityController();
            GisEntity entity = controller.Single("TST");
        }

        [TestMethod()]
        [ExpectedException(typeof(ControllerQueryException))]
        public void GisEntity_EntityController_InexistentSingleByOid()
        {
            GisEntityController controller = new GisEntityController();
            GisEntity entity = controller.SingleByOid(1000);
        }

        //[TestMethod()]
        //public void GisEntity_EntityController_MultipleWorkspaces()
        //{
        //    GisEntityController controller = new GisEntityController();
        //    GisEntity entity = controller.Single("FLTE");

        //    Assert.AreEqual(6, entity.Validations.Count);
        //}

        [TestMethod()]
        public void GisEntity_EntityController_InsertValidation()
        {
            using (GisEntityController controller = new GisEntityController())
            {
                controller.StartEdit();
                GisEntity entity = controller.Single("FLTE");

                using (GisValidationController valController = new GisValidationController())
                {
                    valController.StartEdit();

                    Assert.AreEqual(6, entity.Validations.Count);

                    GisValidation validation = valController.Create();
                    validation.EntityCode = "FLTE";
                    validation.ValidatioName = "Teste Validação";
                    validation.ValidationFieldName = "TESTE";
                    valController.Update(validation);

                    Assert.AreEqual(7, entity.Validations.Count);

                    valController.Delete(validation);
                }
            }
            Console.WriteLine("Teste Dispose");
        }
    }
}
