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
using System.Text;
using System.Reflection;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geodatabase;
using GISActiveRecord.Core;
using GISActiveRecord.Attributes;
using GISActiveRecord.Repository;

namespace GISActiveRecord.Examples
{
    public static class ValidationFunctions
    {
        public static IActiveRecordState ValidateCar(IActiveRecord record)
        {
            IActiveRecordState state = new ActiveRecordState();
            state.Record = record;
            
            var car = (Car)record;

            if (car.CarName.Length > 10)
                state.AddRuleViolation("CarName","O nome do carro não pode ter mais de 10 letras.");

            return state;
        }
    }

    [Workspace("C:\\GisActiveRecordTests.gdb", esriWorkspaceType.esriLocalDatabaseWorkspace)]
    [Table("Car", "Car", RecordTreshold = 5)]
    public class CarRepository : Repository<Car>
    { }

    [Repository(typeof(CarRepository))]
    public class Car : ActiveRecord
    {
        [PrimaryField("CarName", esriFieldType.esriFieldTypeString, 2)]
        public string CarName
        {
            get { return this.GetValue("CarName").ToString(); }
            set { this.SetValue("CarName", value); }
        }

        [DomainField("CarType", esriFieldType.esriFieldTypeSmallInteger, 3, "CarType", esriDomainType.esriDTCodedValue)]
        public string CarType
        {
            get { return this.GetValue("CarType").ToString(); }
            set { this.SetValue("CarType", value); }
        }

        [OneToOne(typeof(Driver), "CarName", esriFieldType.esriFieldTypeString,3)]
        public Driver CarDriver
        {
            get
            {
                List<Driver> drivers = new List<Driver>();
                drivers = this.GetValue("CarDriver") as List<Driver>;
                return drivers[0];
            }
        }

        [HasMany(typeof(Passenger), "CarName", esriFieldType.esriFieldTypeString)]
        public List<Passenger> Passengers
        {
            get { return this.GetValue("Passengers") as List<Passenger>; }
        }
    }




    /// <summary>
    /// IDENTICA a classe CarRepository acima, porem, informamos que temos uma Chave Primaria COMPOSTA
    /// </summary>
    [Workspace("C:\\GisActiveRecordTests.gdb", esriWorkspaceType.esriLocalDatabaseWorkspace)]
    [Table("Car", "Car", RecordTreshold = 5)]
    public class CarRepositoryCompositeKey : Repository<CarCompositeKey>
    { }

    [Repository(typeof(CarRepositoryCompositeKey))]
    public class CarCompositeKey : ActiveRecord
    {
        [PrimaryField("CarName", esriFieldType.esriFieldTypeString, 2)]
        public string CarName
        {
            get { return this.GetValue("CarName").ToString(); }
            set { this.SetValue("CarName", value); }
        }

        [PrimaryField("CarYear", esriFieldType.esriFieldTypeSmallInteger,4)]
        public int CarYear
        {
            get { return Convert.ToInt32(this.GetValue("CarYear")); }
            set { this.SetValue("CarYear", value); }
        }

        [DomainField("CarType", esriFieldType.esriFieldTypeSmallInteger, 3, "CarType", esriDomainType.esriDTCodedValue)]
        public string CarType
        {
            get { return this.GetValue("CarType").ToString(); }
            set { this.SetValue("CarType", value); }
        }

        [OneToOne(typeof(Driver), "CarName", esriFieldType.esriFieldTypeString,4)]
        public Driver CarDriver
        {
            get
            {
                List<Driver> drivers = new List<Driver>();
                drivers = this.GetValue("CarDriver") as List<Driver>;
                return drivers[0];
            }
        }

        [HasMany(typeof(Passenger), "CarName", esriFieldType.esriFieldTypeString)]
        public List<Passenger> Passengers
        {
            get { return this.GetValue("Passengers") as List<Passenger>; }
        }
    }

    [Workspace("C:\\GisActiveRecordTests.gdb", esriWorkspaceType.esriLocalDatabaseWorkspace)]
    [Table("Driver", "Driver")]
    public class DriverRepository : Repository<Driver>
    { }

    [Repository(typeof(DriverRepository))]
    public class Driver : ActiveRecord
    {
        [PrimaryField("DriverName", esriFieldType.esriFieldTypeString, 1)]
        public string DriverName
        {
            get { return this.GetValue("DriverName").ToString(); }
            set { this.SetValue("DriverName", value); }
        }

        [Field("DriverAge", esriFieldType.esriFieldTypeSmallInteger, 2)]
        public int DriverAge
        {
            get { return Convert.ToInt32(this.GetValue("DriverAge")); }
            set { this.SetValue("DriverAge", value); }
        }
    }

    [Workspace("C:\\GisActiveRecordTests.gdb", esriWorkspaceType.esriLocalDatabaseWorkspace)]
    [Table("Passenger", "Passenger")]
    public class PassengerRepository : Repository<Passenger>
    { }

    [Repository(typeof(PassengerRepository))]
    public class Passenger : ActiveRecord
    {
        [Field("PassengerName", esriFieldType.esriFieldTypeString, 2)]
        public string PassengerName
        {
            get { return this.GetValue("PassengerName").ToString(); }
            set
            {
                this.SetValue("PassengerName", value);
            }
        }
    }

    [Workspace("C:\\localhost_sde.sde", esriWorkspaceType.esriRemoteDatabaseWorkspace)]
    [Table("BLOBBER","Blobber")]
    public class BlobberRepository : Repository<Blobber>
    {
    }
 
    [Repository(typeof(BlobberRepository))]
    public class Blobber : ActiveRecord
    {
        [PrimaryField("ID",esriFieldType.esriFieldTypeString,1)]
        public string BlobberId
        {
            get { return this.GetValue("BlobberId").ToString(); }
            set { this.SetValue("BlobberId", value); }
        }

        [Field("Blobber",esriFieldType.esriFieldTypeBlob,2)]
        public IPropertySet Properties
        {
            get 
            { 
                IPropertySet set = this.GetValue("Properties") as IPropertySet;
                return set.GetProperty("Blobber") as IPropertySet;
            }
            set { this.SetValue("Properties", value); }
        }
    }
}
