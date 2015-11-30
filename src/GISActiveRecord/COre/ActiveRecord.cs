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
using System.Text;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.esriSystem;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.Geometry;
using GISActiveRecord.Attributes;
using System.Reflection.Emit;
using System.Reflection;
using System.Diagnostics;

namespace GISActiveRecord.Core
{
    /// <summary>
    /// Concrete implementation of ActiveRecord
    /// </summary>
    /// <remarks>
    /// Defines a few constructor methods.
    /// </remarks>
    public class ActiveRecord : IActiveRecord, IDisposable
    {
        protected static Dictionary<string, FieldAttribute> _attributes;

        protected int _objectId;

        protected IObject _underlyingObject;

        protected bool _isProxy;

        protected bool _deleted;

        protected bool _disposed;

        public ActiveRecord()
        {
            _isProxy = true;
        }

        public ActiveRecord(int oid)
        {
            this._objectId = oid;
            _isProxy = true;
        }

        public ActiveRecord(IObject esriObject)
        {
            this._underlyingObject = esriObject;
            _isProxy = false;
        }

        protected Dictionary<string, FieldAttribute> Attributes
        {
            get
            {
                if (_attributes == null)
                    _attributes = CacheAttributes();

                return _attributes;
            }
        }

        protected Dictionary<string, FieldAttribute> CacheAttributes()
        {
            _attributes = new Dictionary<string, FieldAttribute>();

            PropertyInfo[] properties = this.GetType().GetProperties(BindingFlags.GetProperty | BindingFlags.SetProperty | BindingFlags.Public);

            foreach (PropertyInfo p in properties)
            {
                object[] attributes = p.GetCustomAttributes(typeof(FieldAttribute), true);
                foreach (FieldAttribute a in attributes.OfType<FieldAttribute>())
                {
                    _attributes.Add(p.Name, a);
                }
            }

            return _attributes;
        }

        public virtual IObject UnderlyingObject
        {
            get { return _underlyingObject; }
            set
            {
                _underlyingObject = value;
                _objectId = _underlyingObject.OID;
                _isProxy = false;
            }
        }

        public virtual int ObjectId
        {
            get { return this._objectId; }
            set { _objectId = value; }
        }

        public virtual bool Deleted
        {
            get { return _deleted; }
        }

        public virtual bool IsProxy
        {
            get { return _isProxy; }
        }

        public virtual void Store()
        {
            try
            {
                if (_isProxy)
                    throw new ActiveRecordException("Não é possível persistir um objeto proxy.");

                if (_deleted)
                    throw new ActiveRecordException("Não é possível persistir um objeto que já foi deletado.");

                _underlyingObject.Store();
            }
            catch (COMException comEx)
            {
                throw new ActiveRecordException("Ocorreu um problema durante a tentativa de persistir este objeto.", comEx);
            }
        }

        public virtual void Delete()
        {
            try
            {
                if (_isProxy)
                    return;

                _underlyingObject.Delete();
            }
            catch (COMException comEx)
            {
                throw new ActiveRecordException("Ocorreu um problema durante a tentativa de deletar este objeto.", comEx);
            }
            finally
            {
                _deleted = true;
            }
        }

        public virtual IGeometry Geometry
        {
            get
            {
                if (_underlyingObject is IFeature)
                    return ((IFeature)_underlyingObject).ShapeCopy;

                return null;
            }
            set
            {
                if (_underlyingObject is IFeature)
                    ((IFeature)_underlyingObject).Shape = value;
            }
        }

        [Obsolete]
        public void RehydrateRecord()
        {
            // todo verificar qual é a maneira de buscar o objeto (fetchUnderlyingObject)
            if (IsProxy)
                return;

            ITable t = _underlyingObject.Table;
            _underlyingObject = t.GetRow(ObjectId) as IObject;
        }

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                // release other managed resources
            }

            // if autostore is enabled, check to see if hte object has changes
            if (this.GetRepositoryDefinition().AutoStore)
            {
                var changes = UnderlyingObject as IRowChanges;

                // if during garbage collection, our underlying object has changed
                // store it.
                // just call store once. after calling it, get out of the loop.
                for (int i = 1; i <= UnderlyingObject.Fields.FieldCount - 1; i++)
                {
                    if (!changes.get_ValueChanged(i))
                        continue;

                    UnderlyingObject.Store();
                    break;
                }

                // remove COM baddies
                if (changes != null)
                    Marshal.ReleaseComObject(changes);

                // test to see if this won't break anything
                if (UnderlyingObject != null)
                    Marshal.ReleaseComObject(UnderlyingObject);
            }

            if (UnderlyingObject != null)
                Marshal.FinalReleaseComObject(UnderlyingObject);

            _disposed = true;
        }
    }

    /// <summary>
    /// Class that holds out all extension methods for IObjectWrappers
    /// </summary>
    public static class ObjectWrapperExtensions
    {
        public static RepositoryAttribute GetRepositoryDefinition(this IActiveRecord wrapper)
        {
            object[] attributes = wrapper.GetType().GetCustomAttributes(typeof(RepositoryAttribute), true);

            if (attributes.Length != 1)
                throw new ActiveRecordAttributeException(wrapper.GetType(), "Não foi possível definir o atributo repositório para este tipo.");

            return attributes[0] as RepositoryAttribute;
        }

        /// <summary>
        /// Retrieves the Primary Key attribute without the need to know
        /// the property name
        /// </summary>
        /// <param name="wrapper">IActiveRecord</param>
        /// <returns>PrimaryFieldAttribute</returns>
        public static PrimaryFieldAttribute GetPrimaryKeyDefinition(this IActiveRecord wrapper)
        {
            PrimaryFieldAttribute pkAttribute = null;
            var t = wrapper.GetType();

            try
            {
                PropertyInfo[] properties = t.GetProperties();
                foreach (PropertyInfo pi in properties)
                {
                    object[] attributes = pi.GetCustomAttributes(typeof(PrimaryFieldAttribute), true);

                    if (attributes.Length != 1)
                        continue;

                    pkAttribute = (PrimaryFieldAttribute)attributes[0];
                    break;
                }
            }
            catch (AmbiguousMatchException ambEx)
            {
                throw new ActiveRecordAttributeException(
                     wrapper.GetType(),
                     String.Format("A classe {0} possui dois ou mais atributos de chave primária.", t.Name),
                     ambEx);
            }
            catch (ArgumentException argEx)
            {
                throw new ActiveRecordAttributeException(
                    wrapper.GetType(),
                    String.Format("A classe {0} não possui nenhum atributo de chave primária.", t.Name),
                    argEx);
            }

            return pkAttribute;
        }

        /// <summary>
        /// Retrieves the Primary Keys attributes without the need to know
        /// the property name
        /// </summary>
        /// <param name="wrapper">IActiveRecord</param>
        /// <returns>List of PrimaryFieldAttribute</returns>
        public static List<PrimaryFieldAttribute> GetPrimarysKeyDefinitions(this IActiveRecord wrapper)
        {

            var lstPkAttribute = new List<PrimaryFieldAttribute>();
            var t = wrapper.GetType();

            try
            {
                var properties = t.GetProperties();
                foreach (var pi in properties)
                {
                    var attributes = pi.GetCustomAttributes(typeof(PrimaryFieldAttribute), true);

                    if (Equals(attributes.Length, 0))
                        continue;

                    lstPkAttribute.AddRange(attributes.Cast<PrimaryFieldAttribute>());
                }
            }
            catch (AmbiguousMatchException ambEx)
            {
                throw new ActiveRecordAttributeException(
                     wrapper.GetType(),
                     String.Format("A classe {0} possui dois ou mais atributos de chave primária.", t.Name),
                     ambEx);
            }
            catch (ArgumentException argEx)
            {
                throw new ActiveRecordAttributeException(
                    wrapper.GetType(),
                    String.Format("A classe {0} não possui nenhum atributo de chave primária.", t.Name),
                    argEx);
            }

            return lstPkAttribute;
        }

        /// <summary>
        /// Gets the FieldAttribute located at property
        /// of a certain type
        /// </summary>
        /// <remarks>
        /// If type is null, it defaults to FieldAttribute
        /// </remarks>
        /// <param name="wrapper">IActiveWrapper</param>
        /// <param name="fieldAttributeType">Type</param>
        /// <param name="propertyName">string</param>
        /// <returns>FieldAttribute</returns>
        public static FieldAttribute GetFieldDefinition(this IActiveRecord wrapper, Type fieldAttributeType, string propertyName)
        {
            if (fieldAttributeType == null)
                fieldAttributeType = typeof(FieldAttribute);

            FieldAttribute propertyAttribute = null;

            try
            {
                Type t = wrapper.GetType();
                PropertyInfo propInfo = t.GetProperty(propertyName);
                object[] attributes = propInfo.GetCustomAttributes(typeof(FieldAttribute), true);
                if (attributes.Length == 1)
                    propertyAttribute = (FieldAttribute)attributes[0];
            }
            catch (AmbiguousMatchException ambEx)
            {
                throw new ActiveRecordAttributeException(
                    wrapper.GetType(),
                    String.Format("A propriedade {0} possui dois ou mais atributos de campo.", propertyName),
                    ambEx);

            }
            catch (ArgumentException argEx)
            {
                throw new ActiveRecordAttributeException(
                    wrapper.GetType(),
                    String.Format("A propriedade {0} não possui nenhum atributo de campo.", propertyName),
                    argEx);
            }

            return propertyAttribute;
        }

        /// <summary>
        /// Gets the other right side of the relationship
        /// </summary>
        /// <param name="wrapper">IActiveRecord</param>
        /// <param name="fieldAttribute">FieldAttribute</param>
        /// <returns>object</returns>
        private static object OneToOne(this IActiveRecord wrapper, FieldAttribute relationshipAttribute)
        {
            if (!(relationshipAttribute is RelationshipAttribute))
                throw new ActiveRecordAttributeException(wrapper.GetType(),
                    "O atributo não é do tipo HasMany. Não foi possível retornar os objetos relacionados.");

            RelationshipAttribute hasMany = relationshipAttribute as RelationshipAttribute;

            object[] customAttributes = hasMany.ClassType.GetCustomAttributes(typeof(RepositoryAttribute), true);
            if (customAttributes.Length != 1)
                throw new ActiveRecordAttributeException(hasMany.ClassType, "Não foi possível encontrar o controlador da classe.");

            RepositoryAttribute controlled = (RepositoryAttribute)customAttributes[0];
            object relatedController = Activator.CreateInstance(controlled.ControllerType);

            if (relatedController == null)
                throw new ActiveRecordAttributeException(controlled.ControllerType, "Não foi possível instanciar o controller.");

            PrimaryFieldAttribute pkAtt = wrapper.GetPrimaryKeyDefinition();
            if (pkAtt == null)
                throw new ActiveRecordAttributeException(wrapper.GetType(), "Não foi possível encontrar a chave primária da classe.");

            IQueryFilter filter = new QueryFilterClass();

            filter.WhereClause = String.Format(pkAtt.QuoteValue ? "{0} = '{1}'" : "{0} = {1}", hasMany.FieldName, wrapper.UnderlyingObject.get_Value(pkAtt.Index));

            // prepare the method for invoke
            // check if the method is lazy or not
            MethodInfo singleMethod = null;
            singleMethod = controlled.ControllerType.GetMethod(hasMany.Lazy ? "Filter" : "FilterLazy");

            var parameters = new object[1];
            parameters[0] = filter;

            // invoke and return
            return singleMethod.Invoke(relatedController, parameters);
        }

        /// <summary>
        /// Gets the Owner of a specific IActiveRecord
        /// </summary>
        /// <param name="wrapper">IActiveRecord</param>
        /// <param name="propertyName">string</param>
        /// <returns>object</returns>
        private static object Belongs(this IActiveRecord wrapper, FieldAttribute relationshipAttribute)
        {
            if (!(relationshipAttribute is RelationshipAttribute))
                throw new ActiveRecordAttributeException(wrapper.GetType(),
                    "O atributo não é do tipo HasMany. Não foi possível retornar os objetos relacionados.");

            var attribute = relationshipAttribute as BelongsToAttribute;

            object[] customAttributes = attribute.ClassType.GetCustomAttributes(typeof(RepositoryAttribute), true);
            if (customAttributes.Length != 1)
                throw new ActiveRecordAttributeException(attribute.ClassType, "Não foi possível encontrar o controlador da classe.");

            RepositoryAttribute controlled = (RepositoryAttribute)customAttributes[0];
            object relatedController = Activator.CreateInstance(controlled.ControllerType);

            if (relatedController == null)
                throw new ActiveRecordAttributeException(controlled.ControllerType, "Não foi possível instanciar o controller.");

            IQueryFilter filter = new QueryFilterClass();
            filter.WhereClause = String.Format(attribute.QuoteValue ? "{0} = '{1}'" : "{0} = {1}", attribute.FieldName, wrapper.UnderlyingObject.get_Value(attribute.ParentValueFieldIndex));

            // prepare the method for invoke
            // check if the method is lazy or not
            MethodInfo singleMethod = null;
            singleMethod = controlled.ControllerType.GetMethod("SingleByPrimaryKey");

            var parameters = new object[1];
            parameters[0] = wrapper.UnderlyingObject.get_Value(attribute.ParentValueFieldIndex);
            //case not exist value in foreign key, return null
            if (parameters[0] == null || parameters[0].ToString().Equals(""))
                return null;

            // invoke and return
            return singleMethod.Invoke(relatedController, parameters);
        }

        private static object HasMany(this IActiveRecord wrapper, FieldAttribute relationshipAttribute)
        {
            if (!(relationshipAttribute is RelationshipAttribute))
                throw new ActiveRecordAttributeException(wrapper.GetType(),
                    "O atributo não é do tipo HasMany. Não foi possível retornar os objetos relacionados.");

            HasManyAttribute hasMany = relationshipAttribute as HasManyAttribute;

            object[] customAttributes = hasMany.ClassType.GetCustomAttributes(typeof(RepositoryAttribute), true);
            if (customAttributes.Length != 1)
                throw new ActiveRecordAttributeException(hasMany.ClassType, "Não foi possível encontrar o controlador da classe.");

            RepositoryAttribute controlled = (RepositoryAttribute)customAttributes[0];
            object relatedController = Activator.CreateInstance(controlled.ControllerType);

            if (relatedController == null)
                throw new ActiveRecordAttributeException(controlled.ControllerType, "Não foi possível instanciar o controller.");

            PrimaryFieldAttribute pkAtt = wrapper.GetPrimaryKeyDefinition();
            if (pkAtt == null)
                throw new ActiveRecordAttributeException(wrapper.GetType(), "Não foi possível encontrar a chave primária da classe.");

            IQueryFilter filter = new QueryFilterClass();

            filter.WhereClause = String.Format(pkAtt.QuoteValue ? "{0} = '{1}'" : "{0} = {1}", hasMany.FieldName, wrapper.UnderlyingObject.get_Value(pkAtt.Index));

            if (!String.IsNullOrEmpty(hasMany.OrderBy))
            {
                IQueryFilterDefinition definition = filter as IQueryFilterDefinition;
                definition.PostfixClause = hasMany.OrderBy;
            }

            // prepare the method for invoke
            // check if the method is lazy or not
            MethodInfo filterMethod = null;
            filterMethod = controlled.ControllerType.GetMethod(hasMany.Lazy ? "FilterLazy" : "Filter");

            var parameters = new object[1];
            parameters[0] = filter;

            // invoke and return
            return filterMethod.Invoke(relatedController, parameters);
        }

        /// <summary>
        /// Gets all children objects related to wrapper
        /// </summary>
        /// <param name="wrapper">IActiveRecord</param>
        /// <param name="relationshipAttribute">FieldAttribute</param>
        /// <returns>object</returns>
        [Obsolete("Testando novos métodos", true)]
        private static object GetRelatedObjects(this IActiveRecord wrapper, FieldAttribute relationshipAttribute)
        {
            if (!(relationshipAttribute is RelationshipAttribute))
                throw new ActiveRecordAttributeException(wrapper.GetType(),
                    "O atributo não é do tipo HasMany. Não foi possível retornar os objetos relacionados.");

            RelationshipAttribute hasMany = relationshipAttribute as RelationshipAttribute;

            object[] customAttributes = hasMany.ClassType.GetCustomAttributes(typeof(RepositoryAttribute), true);
            if (customAttributes.Length != 1)
                throw new ActiveRecordAttributeException(hasMany.ClassType, "Não foi possível encontrar o controlador da classe.");

            RepositoryAttribute controlled = (RepositoryAttribute)customAttributes[0];
            object relatedController = Activator.CreateInstance(controlled.ControllerType);

            if (relatedController == null)
                throw new ActiveRecordAttributeException(controlled.ControllerType, "Não foi possível instanciar o controller.");

            PrimaryFieldAttribute pkAtt = wrapper.GetPrimaryKeyDefinition();
            if (pkAtt == null)
                throw new ActiveRecordAttributeException(wrapper.GetType(), "Não foi possível encontrar a chave primária da classe.");

            IQueryFilter filter = new QueryFilterClass();

            filter.WhereClause = String.Format(pkAtt.QuoteValue ? "{0} = '{1}'" : "{0} = {1}", hasMany.FieldName, wrapper.UnderlyingObject.get_Value(pkAtt.Index));

            // prepare the method for invoke
            // check if the method is lazy or not
            MethodInfo filterMethod = null;
            filterMethod = controlled.ControllerType.GetMethod(hasMany.Lazy ? "FilterLazy" : "Filter");

            var parameters = new object[1];
            parameters[0] = filter;

            // invoke and return
            return filterMethod.Invoke(relatedController, parameters);
        }

        [Obsolete("Testando novos métodos", true)]
        private static object GetParentObject(this IActiveRecord wrapper, FieldAttribute relationshipAttribute)
        {
            if (!(relationshipAttribute is RelationshipAttribute))
                throw new ActiveRecordAttributeException(wrapper.GetType(),
                    "O atributo não é do tipo HasMany. Não foi possível retornar os objetos relacionados.");

            var attribute = relationshipAttribute as BelongsToAttribute;

            object[] customAttributes = attribute.ClassType.GetCustomAttributes(typeof(RepositoryAttribute), true);
            if (customAttributes.Length != 1)
                throw new ActiveRecordAttributeException(attribute.ClassType, "Não foi possível encontrar o controlador da classe.");

            RepositoryAttribute controlled = (RepositoryAttribute)customAttributes[0];
            object relatedController = Activator.CreateInstance(controlled.ControllerType);

            if (relatedController == null)
                throw new ActiveRecordAttributeException(controlled.ControllerType, "Não foi possível instanciar o controller.");

            IQueryFilter filter = new QueryFilterClass();
            filter.WhereClause = String.Format(attribute.QuoteValue ? "{0} = '{1}'" : "{0} = {1}", attribute.FieldName, wrapper.UnderlyingObject.get_Value(attribute.ParentValueFieldIndex));

            // prepare the method for invoke
            // check if the method is lazy or not
            MethodInfo filterMethod = null;
            filterMethod = controlled.ControllerType.GetMethod(attribute.Lazy ? "FilterLazy" : "Filter");

            var parameters = new object[1];
            parameters[0] = filter;

            // invoke and return
            return filterMethod.Invoke(relatedController, parameters);
        }

        private static object FindRelatedObjects(this IActiveRecord wrapper, FieldAttribute relationshipAttribute)
        {
            List<IActiveRecord> result;

            if (!(relationshipAttribute is RelationshipAttribute))
                throw new ActiveRecordAttributeException(wrapper.GetType(),
                    "O atributo não é do tipo HasMany. Não foi possível retornar os objetos relacionados.");

            var attribute = relationshipAttribute as RelationshipAttribute;

            if (attribute.GetType() == typeof(HasManyAttribute))
                return HasMany(wrapper, relationshipAttribute);

            if (attribute.GetType() == typeof(OneToOneAttribute))
                return OneToOne(wrapper, relationshipAttribute);

            if (attribute.GetType() == typeof(BelongsToAttribute))
                return Belongs(wrapper, relationshipAttribute);

            throw new ActiveRecordAttributeException(wrapper.GetType(), "Não foi possível buscar os objetos relacionados.");
        }

        /// <summary>
        /// This method is responsible for fetching lazy IObjects;
        /// </summary>
        /// <exception cref="ActiveRecordAttributeException"></exception>
        /// <exception cref="ArgumentException"></exception>
        private static void FetchUnderlyingObject(this IActiveRecord wrapper)
        {
            // we can only fetch this object if it's a proxy
            if (!wrapper.IsProxy && wrapper.UnderlyingObject != null)
                return;

            if (wrapper.ObjectId == 0)
                return;

            // find out which controller is responsible for this type of object
            RepositoryAttribute controlledAtt = null;
            var customAtts = wrapper.GetType().GetCustomAttributes(typeof(RepositoryAttribute), true);

            // test if out attribute is configured properly
            if (customAtts.Length == 1)
                controlledAtt = (RepositoryAttribute)customAtts[0];

            if (controlledAtt == null)
                throw new ActiveRecordAttributeException(wrapper.GetType(), "Não foi possível localizar o atributo controller.");

            var controllerType = controlledAtt.ControllerType;
            var controller = Activator.CreateInstance(controllerType);

            // lets get the method we need and set our parameters
            var singleMethod = controllerType.GetMethod("SingleByOID");
            var parameters = new object[1];
            parameters[0] = wrapper.ObjectId;

            // finally invoke the method and set our underlyingObject.
            wrapper.UnderlyingObject =
                ((IActiveRecord)singleMethod.Invoke(controller, parameters)).UnderlyingObject;
        }

        /// <summary>
        /// This method is responsible for translanting between coded domain names
        /// and their values, setting into UnderlyingObject the correct value.
        /// </summary>
        /// <remarks>
        /// Imagine a ICodedDomain like:
        /// 1 = "Alphanumeric"
        /// 2 = "Geographic"
        /// 
        /// This method will aways set the value to 1 when passed
        /// "Alphanumeric"
        /// </remarks>
        /// <exception cref="ActiveRecrdAttributeException"></exception>
        /// <param name="wrapper"></param>
        /// <param name="propertyName"></param>
        /// <param name="displayValue"></param>
        /// <returns></returns>
        private static void SetDisplayValue(this IActiveRecord wrapper, FieldAttribute fieldAtt, string displayValue)
        {
            if (wrapper.UnderlyingObject == null)
                FetchUnderlyingObject(wrapper);

            if (!(fieldAtt is DomainFieldAttribute))
                throw new ActiveRecordAttributeException(wrapper.GetType(), "O atributo não é tipo domínio.");

            DomainFieldAttribute domainField = fieldAtt as DomainFieldAttribute;
            ICodedValueDomain domain = wrapper.UnderlyingObject.Fields.get_Field(domainField.Index).Domain as ICodedValueDomain;

            if (domain == null)
                throw new ActiveRecordAttributeException(wrapper.GetType(), "Não foi possível localizar o domínio.");

            for (var i = 0; i <= domain.CodeCount - 1; i++)
            {
                if (domain.get_Name(i) == displayValue)
                {
                    object codedValue = domain.get_Value(i);
                    wrapper.UnderlyingObject.set_Value(domainField.Index, codedValue);
                }
            }
        }

        /// <summary>
        /// This method is responsible for translating between coded domain values
        /// and their names, returning the correct name.
        /// </summary>
        /// Imagine a ICodedDomain like:
        /// 1 = "Alphanumeric"
        /// 2 = "Geographic"
        /// 
        /// This method will always return "Alphanumeric" for the value 1,
        /// fetched from UnderlyingObject
        /// </remarks>
        /// <exception cref="ActiveRecrdAttributeException"></exception>
        /// <param name="wrapper">IActiveRecord</param>
        /// <param name="propertyName">string</param>
        /// <returns>string</returns>
        private static string GetDisplayValue(this IActiveRecord wrapper, FieldAttribute fieldAtt)
        {
            if (wrapper.UnderlyingObject == null)
                FetchUnderlyingObject(wrapper);

            if (!(fieldAtt is DomainFieldAttribute))
                throw new ActiveRecordAttributeException(wrapper.GetType(), "Não é possível obter o atributo de domínio.");

            DomainFieldAttribute domainField = fieldAtt as DomainFieldAttribute;
            ICodedValueDomain domain = wrapper.UnderlyingObject.Fields.get_Field(domainField.Index).Domain as ICodedValueDomain;

            if (domain == null)
                throw new ActiveRecordAttributeException(wrapper.GetType(),
                    String.Format("Não foi possível localizar o domínio {0}.", domainField.DomainName));

            string codedValue = wrapper.UnderlyingObject.get_Value(domainField.Index).ToString();
            string displayValue = String.Empty;

            for (int i = 0; i <= domain.CodeCount - 1; i++)
            {
                if (domain.get_Value(i).ToString() == codedValue)
                {
                    displayValue = domain.get_Name(i);
                    break;
                }
            }

            if (displayValue == String.Empty)
                return codedValue;
            else
                return displayValue;
        }

        /// <summary>
        /// Gets a value from a specific property.
        /// </summary>
        /// <remarks>
        /// If the IObjectWrapper is a proxy, this method fetches the IObject
        /// from the database and assigns it to this.UnderlyingObject
        /// </remarks>
        /// <exception cref="ActiveRecordAttributeException"></exception>
        /// <param name="propertyName">string</param>
        /// <returns>object</returns>
        public static object GetValue(this IActiveRecord wrapper, string propertyName)
        {
            if (wrapper.IsProxy)
                wrapper.FetchUnderlyingObject();

            if (wrapper.Deleted)
                throw new ActiveRecordException("O objeto foi deletado e não é possível acessar seus atributos.");

            FieldAttribute att = wrapper.GetFieldDefinition(typeof(FieldAttribute), propertyName);
            if (att == null)
                throw new ActiveRecordAttributeException(wrapper.GetType(),
                    String.Format("Não foi possível localizar o atributo de campo para a propriedade {0}", propertyName));

            if (att is DomainFieldAttribute)
                return GetDisplayValue(wrapper, att);
            if (att is RelationshipAttribute)
                return FindRelatedObjects(wrapper, att);

            if (att.FieldType == esriFieldType.esriFieldTypeBlob)
                return GetBlobValue(wrapper, att);

            return att.Index == -1 ? null : wrapper.UnderlyingObject.get_Value(att.Index);
        }

        public static void SetCascadingValue(this IActiveRecord wrapper, FieldAttribute fieldAtt, object value)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Sets a value from a specific property.
        /// </summary>
        /// <exception cref="ActiveRecordAttributeException"></exception>
        /// <param name="propertyName">string</param>
        /// <param name="value">object</param>
        public static void SetValue(this IActiveRecord wrapper, string propertyName, object value)
        {
            if (wrapper.IsProxy)
                wrapper.FetchUnderlyingObject();

            if (wrapper.Deleted)
                throw new ActiveRecordException("Não é possível setar o valor deste atributo, pois o objeto foi deletado.");

            var att = wrapper.GetFieldDefinition(typeof(FieldAttribute), propertyName);

            if (att == null)
                throw new ActiveRecordAttributeException(wrapper.GetType(),
                    String.Format("Não foi possível localizar o atributo de campo para a propriedade {0}", propertyName));

            if (att is DomainFieldAttribute)
                SetDisplayValue(wrapper, att, value.ToString());

            if (att is BelongsToAttribute)
            {
                BelongsToAttribute attribute = att as BelongsToAttribute;
                if (value == null)
                    wrapper.UnderlyingObject.set_Value(attribute.ParentValueFieldIndex, DBNull.Value);
                else
                    wrapper.UnderlyingObject.set_Value(attribute.ParentValueFieldIndex, value);


                return;
            }

            if (att is OneToOneAttribute)
            {
                OneToOneAttribute attribute = att as OneToOneAttribute;
                if (value == null)
                    wrapper.UnderlyingObject.set_Value(attribute.RelatedAttributeIndex, DBNull.Value);
                else
                    wrapper.UnderlyingObject.set_Value(attribute.RelatedAttributeIndex, value);
                return;
            }

            // if the field does not have an index, just exit
            if (att.Index == -1)
                return;

            // if the field cannot be null, but it is, throw a new exception
            if (att.NotNullable && value == null)
                throw new ActiveRecordException(String.Format("O atributo {0} não pode ser nulo.", att.FieldName));

            if (att.FieldType == esriFieldType.esriFieldTypeBlob)
            {
                SetBlobValue(wrapper, att, value);
                return;
            }
            if (value == null)
                wrapper.UnderlyingObject.set_Value(att.Index, DBNull.Value);
            else
                wrapper.UnderlyingObject.set_Value(att.Index, value);
        }

        public static object GetBlobValue(this IActiveRecord wrapper, FieldAttribute attribute)
        {
            IMemoryBlobStream blobStream = wrapper.UnderlyingObject.get_Value(attribute.Index) as IMemoryBlobStream;

            if (blobStream == null)
                return null;

            IObjectStream stream = new ObjectStreamClass() { Stream = blobStream };

            IPersistStream blob = new PropertySetClass();
            blob.Load(blobStream);

            return blob as IPropertySet;
        }

        public static void SetBlobValue(this IActiveRecord wrapper, FieldAttribute attribute, object value)
        {
            if (!(value is IPersistStream))
                throw new ActiveRecordException("Nâo é possível persistir um objeto que não implementa IPersistStream.");

            IMemoryBlobStream memStream = new MemoryBlobStreamClass();
            IObjectStream objStream = new ObjectStreamClass();
            objStream.Stream = memStream;
            IPersistStream persist = (IPersistStream)value;
            persist.Save(memStream, 0);

            wrapper.UnderlyingObject.set_Value(attribute.Index, memStream);
        }
    }
}
