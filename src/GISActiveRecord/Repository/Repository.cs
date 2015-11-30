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
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.Geodatabase;
using GISActiveRecord.Attributes;
using GISActiveRecord.Core;
using GISActiveRecord.GIS.Geodatabase;
using log4net;

namespace GISActiveRecord.Repository
{
    class IdentityComparer<T> : IEqualityComparer<T>
    {
        #region IEqualityComparer<T> Members

        public bool Equals(T x, T y)
        {
            return ReferenceEquals(x, y);
        }

        public int GetHashCode(T obj)
        {
            return obj == null ? 0 : obj.GetHashCode();
        }

        #endregion
    }

    public class Repository<T> : IRepository<T> where T : IActiveRecord
    {



        static readonly HashSet<IWorkspace> hs = new HashSet<IWorkspace>(new IdentityComparer<IWorkspace>());

        private TableAttribute _tableAtt;
        private WorkspaceAttribute _workAtt;
        private IWorkspace workspace;
        private ITable _table;


        private IWorkspaceEditHandler _editHandler;

        private bool _disposed;
        private static readonly ILog _log = LogManager.GetLogger(typeof(Repository<T>));

        public Repository()
        {
            _tableAtt = GetTableDefinition;
            _workAtt = GetWorkspaceDefinition;

            if (_workAtt.WorkspaceType == esriWorkspaceType.esriRemoteDatabaseWorkspace)
                _editHandler = new RemoteWorkspaceEditHandler(GetWorkspace);
            else
                _editHandler = new LocalWorkspaceEditHandler(GetWorkspace);
        }

        public List<T> FindAll()
        {
            var results = new List<T>();
            var table = GetTable;

            ICursor cursor = null;

            if (table == null)
                throw new ArgumentException("Não foi possível obter a tabela para pesquisa.");

            try
            {
                IQueryFilter filter = new QueryFilterClass { WhereClause = String.Empty };

                var count = table.RowCount(filter);

                if (count > GetTableDefinition.RecordTreshold)
                    filter.SubFields = table.OIDFieldName;

                cursor = table.Search(null, false);
                IActiveRecordFactory<T> factory = new ActiveRecordFactory<T>();
                IObject tempObject = null;

                while ((tempObject = cursor.NextRow() as IObject) != null)
                {
                    if (count <= GetTableDefinition.RecordTreshold)
                    {
                        IActiveRecord wrapper = factory.CreateActiveRecord(tempObject);
                        results.Add((T)wrapper);
                    }
                    else
                    {
                        IActiveRecord proxyWrapper = factory.CreateActiveRecord(tempObject.OID);
                        results.Add((T)proxyWrapper);
                    }
                }
            }
            finally
            {
                if (cursor != null)
                    Marshal.ReleaseComObject(cursor);
            }
            return results;
        }

        public List<T> Find(List<string> attributeNames, List<object> values)
        {
            var results = new List<T>();
            var table = GetTable;

            ICursor cursor = null;

            if (table == null)
                throw new ArgumentException("Não foi possível obter a tabela para pesquisa.");

            try
            {
                if (attributeNames == null || attributeNames.Count.Equals(0))
                    throw new ArgumentException("Não foi informado nenhum nome de atributo.");
                else if (values == null || values.Count.Equals(0))
                    throw new ArgumentException("Não foi informado nenhum valor de atributo.");
                else if (!attributeNames.Count.Equals(values.Count))
                    throw new ArgumentException(
                        "A quantidade de atributos não confere com a quantidade de valores informado.");
                else
                {

                    var query = string.Empty;

                    for (var cont = 0; cont < attributeNames.Count; cont++)
                    {
                        var indexField = table.FindField(attributeNames[cont]);
                        if (indexField > -1)
                        {
                            if (String.IsNullOrEmpty(query))
                                query = attributeNames[cont];
                            else
                                query += string.Format(" and {0}", attributeNames[cont]);

                            if (values[cont] == null)
                                query += " is null";
                            else
                            {

                                IField field = table.Fields.get_Field(indexField);

                                switch (field.Type)
                                {
                                    case esriFieldType.esriFieldTypeDouble:
                                    case esriFieldType.esriFieldTypeInteger:
                                    case esriFieldType.esriFieldTypeSmallInteger:
                                    case esriFieldType.esriFieldTypeSingle:
                                    case esriFieldType.esriFieldTypeOID:
                                        query += string.Format(" = {0}", values[cont]);
                                        break;
                                    default:
                                        query += string.Format(" = '{0}'", values[cont]);
                                        break;
                                }
                            }
                        }
                        else
                            throw new ArgumentException(
                                "O atributo {0} não existe.", attributeNames[cont]);
                    }


                    IQueryFilter filter = new QueryFilterClass { WhereClause = query };


                    var count = table.RowCount(filter);

                    if (count > GetTableDefinition.RecordTreshold)
                        filter.SubFields = table.OIDFieldName;

                    cursor = table.Search(filter, false);
                    IActiveRecordFactory<T> factory = new ActiveRecordFactory<T>();
                    IObject tempObject = null;

                    while ((tempObject = cursor.NextRow() as IObject) != null)
                    {
                        if (count <= GetTableDefinition.RecordTreshold)
                        {
                            IActiveRecord wrapper = factory.CreateActiveRecord(tempObject);
                            results.Add((T)wrapper);
                        }
                        else
                        {
                            IActiveRecord proxyWrapper = factory.CreateActiveRecord(tempObject.OID);
                            results.Add((T)proxyWrapper);
                        }
                    }
                }
            }
            finally
            {
                if (cursor != null)
                    Marshal.ReleaseComObject(cursor);
            }
            return results;
        }

        public List<T> Filter(IQueryFilter filter)
        {
            var results = new List<T>();
            var table = this.GetTable;
            ICursor cursor = null;

            if (table == null)
                throw new ActiveRecordAttributeException(
                    this.GetType(),
                    "Não foi possível encontrar o atributo de tabela.");

            try
            {
                var count = table.RowCount(filter);

                if (count == 0)
                    return results;

                // if we have more records then treshold,
                // just grab the object id field
                if (count > GetTableDefinition.RecordTreshold)
                    filter.SubFields = table.OIDFieldName;

                cursor = table.Search(filter, false);
                IActiveRecordFactory<T> factory = new ActiveRecordFactory<T>();
                IObject tempObject = null;

                while ((tempObject = cursor.NextRow() as IObject) != null)
                {
                    if (count <= GetTableDefinition.RecordTreshold)
                    {
                        IActiveRecord wrapper = factory.CreateActiveRecord(tempObject);
                        results.Add((T)wrapper);
                    }
                    else
                    {
                        IActiveRecord proxyWrapper = factory.CreateActiveRecord(tempObject.OID);
                        results.Add((T)proxyWrapper);
                    }
                }

            }
            catch (Exception ex)
            {

            }
            finally
            {
                if (cursor != null)
                    Marshal.ReleaseComObject(cursor);
            }
            return results;
        }

        /// <summary>
        /// Retrieves a collection of IActiveRecord using a specific filter.
        /// </summary>
        /// <param name="filter">Filtering rules</param>
        /// <returns>List(T) (IActiveRecord)</returns>
        public List<T> FilterLazy(IQueryFilter filter)
        {
            List<T> results = new List<T>();
            ITable table = this.GetTable;
            ICursor cursor = null;

            if (table == null)
                throw new RepositoryException(
                    "Não foi possível obter a tabela para pesquisa",
                    GetWorkspaceDefinition,
                    GetTableDefinition,
                    IsBeingEdited);

            // lets just grab the object id field to speed things up a bit
            filter.SubFields = table.OIDFieldName;

            try
            {
                int count = table.RowCount(filter);

                if (count == 0)
                    return results;

                cursor = table.Search(filter, false);
                IActiveRecordFactory<T> factory = new ActiveRecordFactory<T>();
                IObject tempObject = null;

                while ((tempObject = cursor.NextRow() as IObject) != null)
                {
                    IActiveRecord proxyWrapper = factory.CreateActiveRecord(tempObject.OID);
                    results.Add((T)proxyWrapper);
                }
            }
            catch (COMException comEx)
            {
                throw new RepositoryQueryException(
                    "Ocorreu um erro durante a pesquisa.",
                    comEx,
                    GetWorkspaceDefinition,
                    GetTableDefinition,
                    IsBeingEdited,
                    filter.WhereClause);
            }
            finally
            {
                if (cursor != null)
                    Marshal.ReleaseComObject(cursor);
            }
            return results;
        }

        /// <summary>
        /// This method retrieves an IActiveRecord by it's ObjectID.
        /// </summary>
        /// <param name="oid">int</param>
        /// <exception cref="RepositoryQueryException"></exception>
        /// <returns>T (IActiveRecord)</returns>
        public T SingleByOID(int oid)
        {
            ITable table = GetTable;
            IObject esriObject = null;

            try
            {
                esriObject = table.GetRow(oid) as IObject;
            }
            catch (COMException comEx)
            {
                _log.Error("Ocorreu um erro COM ao tentar buscar o objeto via OBJECTID.", comEx);
            }
            catch (Exception ex)
            {
                _log.Error("Ocorreu um erro desconhecido ao tentar buscar o objeto via OBJECTID", ex);
            }

            if (esriObject == null)
                throw new RepositoryQueryException(
                    "O objeto pesquisado não pode ser encontrado.",
                    GetWorkspaceDefinition, GetTableDefinition, IsBeingEdited,
                    String.Format("OID = {0}", oid.ToString()));

            IActiveRecordFactory<T> factory = new ActiveRecordFactory<T>();

            IActiveRecord objectWrapper = factory.CreateActiveRecord(esriObject);

            return (T)objectWrapper;
        }

        /// <summary>
        /// This method retrieves an object using it's declared primary key.
        /// </summary>
        /// <param name="primaryKey"></param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="RepositoryQueryException"></exception>
        /// <returns>T (IActiveRecord)</returns>
        public T SingleByPrimaryKey(object primaryKey)
        {
            var lst = new List<object>();
            lst.Add(primaryKey);
            return (T)SingleByPrimaryKeys(lst);
        }

        public T SingleByPrimaryKeys(List<object> primaryKeys)
        {
            IActiveRecordFactory<T> factory = new ActiveRecordFactory<T>();
            IActiveRecord objectWrapper = factory.CreateActiveRecord();
            //PrimaryFieldAttribute pkAttribute = objectWrapper.GetPrimaryKeyDefinition();
            List<PrimaryFieldAttribute> lstPkAttribute = objectWrapper.GetPrimarysKeyDefinitions();
            ITable table = this.GetTable;

            if (primaryKeys == null || primaryKeys.Count.Equals(0))
            {
                throw new Exception(
                    "É obrigatório informar um valor de primary key.");
            }
            else if (lstPkAttribute == null || lstPkAttribute.Count.Equals(0))
            {
                throw new Exception(
                    "Este repositório não possui Chave[s] Primária[s] definidas.");
            }
            else if (!lstPkAttribute.Count.Equals(primaryKeys.Count))
            {
                throw new Exception(
                    string.Format("A quantidade de Chave[s] Primária[s] {0} é diferente da quantidade de valor[es] informado[s] {1}", lstPkAttribute.Count.ToString(), primaryKeys.Count.ToString()));
            }
            else
            {
                var query = String.Empty;
                var ct = 0;
                foreach (PrimaryFieldAttribute pkAttribute in lstPkAttribute)
                {
                    if (String.IsNullOrEmpty(query))
                        query = String.Format(pkAttribute.QuoteValue ? "{0} = '{1}'" : "{0} = {1}", pkAttribute.FieldName, primaryKeys[ct].ToString());
                    else
                        query += String.Format(pkAttribute.QuoteValue ? " and {0} = '{1}'" : " and {0} = {1}", pkAttribute.FieldName, primaryKeys[ct].ToString());
                    ct++;
                }
                IQueryFilter filter = new QueryFilterClass { WhereClause = query };

                if (table.RowCount(filter) == 0)
                    return default(T);
                try
                {
                var cursor = table.Search(filter, false);
                var esriObject = cursor.NextRow() as IObject;

                if (esriObject == null)
                    throw new RepositoryQueryException(
                        "O objeto pesquisado não pode ser encontrado.",
                        GetWorkspaceDefinition,
                        GetTableDefinition,
                        IsBeingEdited,
                        filter.WhereClause);
               
                    objectWrapper.UnderlyingObject = esriObject;
                }
                catch (Exception ex)
                {

                }
            }
            return (T)objectWrapper;
        }

        public int Count(IQueryFilter filter)
        {
            try
            {
                return GetTable.RowCount(filter);
            }
            catch (Exception ex)
            {
                return -1;
            }
        }

        public T Create()
        {
            ITable table = this.GetTable;
            if (table == null)
                throw new ArgumentException("A tabela não pode ser obtida.");

            IActiveRecordFactory<T> factory = new ActiveRecordFactory<T>();
            IActiveRecord objectWrapper = factory.CreateActiveRecord(table.CreateRow() as IObject);
            return (T)objectWrapper;
        }

        /// <summary>
        /// Inserts a new record to the controller table
        /// </summary>
        /// <remarks>
        /// This uses a insert cursor WITHOUT buffer. Do not use for large inserts.
        /// </remarks>
        /// <exception cref="RepositoryException"></exception>
        /// <param name="toBeInserted">IObjectWrapper</param>
        public void Insert(T toBeInserted)
        {
            // this is not finished
            ITable table = this.GetTable;

            if (table == null)
                throw new RepositoryException(
                    "It's not possible to get the controller underlying table.",
                    GetWorkspaceDefinition,
                    GetTableDefinition,
                    IsBeingEdited);

            ICursor cursor = null;
            try
            {
                cursor = table.Insert(false);

                cursor.InsertRow(toBeInserted.UnderlyingObject);
                cursor.Flush();
            }
            finally
            {
                if (cursor != null)
                    Marshal.ReleaseComObject(cursor);
            }
        }

        public void Update(T toBeUpdated)
        {
            if (toBeUpdated.IsProxy)
                return;

            try
            {
                toBeUpdated.UnderlyingObject.Store();
            }
            catch (COMException comEx)
            {
                _log.Error("Ocorreu uma falha COM ao tentar atualizar o objeto.", comEx);
            }
            catch (Exception ex)
            {
                _log.Error("Ocorreu uma falha ao tentar atualizar o objeto.", ex);
            }
        }

        public void Delete(T toBeDeleted)
        {
            if (toBeDeleted.UnderlyingObject != null)
            {
                IObject obj = toBeDeleted.UnderlyingObject;
                obj.Delete();
            }
        }

        public void StartEdit()
        {
            //TestEditHandler();

            if (!_editHandler.IsBeingEdited())
                _editHandler.StartEditing(true);

            if (!_editHandler.IsInEditOperation())
                _editHandler.StartEditingOperation();
        }

        public void StopEdit(bool save)
        {
            //TestEditHandler();

            if (!_editHandler.IsBeingEdited())
                return;

            if (_editHandler.IsInEditOperation())
                _editHandler.StopEditingOperation();

            try
            {
                _editHandler.StopEditing(save);
            }
            catch (COMException comEx)
            {

                _log.Error("Ocorreu um erro ao parar a edição", comEx);
            }
        }

        public bool IsBeingEdited
        {
            get
            {
                //TestEditHandler();

                return _editHandler.IsBeingEdited();
            }
        }

        public bool IsInEditOperation
        {
            get
            {
                //TestEditHandler();

                return _editHandler.IsInEditOperation();
            }
        }

        protected virtual TableAttribute GetTableDefinition
        {
            get
            {
                TableAttribute tableAtt = null;
                Type t = this.GetType();
                try
                {
                    object[] attributes = t.GetCustomAttributes(typeof(TableAttribute), true);
                    if (attributes.Length == 1)
                        tableAtt = (TableAttribute)attributes[0];
                }
                catch (AmbiguousMatchException ambEx)
                {
                    _log.Error("Ocorreu um erro ao tentar descobrir qual é a tabela desta classe. Existe mais de um atributo de tabela?", ambEx);
                }
                catch (ArgumentException argEx)
                {
                    _log.Error("Ocorreu um erro ao tentar descobrir qual é a tabela desta classe.", argEx);
                }

                return tableAtt;
            }
        }

        protected virtual WorkspaceAttribute GetWorkspaceDefinition
        {
            get
            {
                WorkspaceAttribute workAttribute = null;
                Type t = this.GetType();
                try
                {
                    object[] attributes = t.GetCustomAttributes(typeof(WorkspaceAttribute), true);
                    if (attributes.Length == 1)
                        workAttribute = (WorkspaceAttribute)attributes[0];

                    if (workAttribute == null)
                        throw new ArgumentException(
                            String.Format("O atributo workspace não existe na classe {0}.", t.FullName));
                }
                catch (AmbiguousMatchException ambEx)
                {
                    _log.Error("Ocorreu um erro ao tentar descobrir qual é o workspace desta classe. Existem mais de um atributo de workspace?", ambEx);
                }
                catch (ArgumentException argEx)
                {
                    _log.Error("Ocorreu um erro ao tentar descobrir qual é o workspace desta classe.", argEx);
                }

                return workAttribute;
            }
        }

        public virtual ITable GetTable
        {
            get
            {
                if (_table != null)
                    return _table;

                ITable controllerTable = null;

                try
                {
                    IFeatureWorkspace featWorkspace = GetWorkspace as IFeatureWorkspace;
                    ISQLSyntax syntax = featWorkspace as ISQLSyntax;

                    controllerTable = featWorkspace.OpenTable(syntax.QualifyTableName(_tableAtt.DatabaseName, _tableAtt.DataOwner,
                        _tableAtt.TableName));
                }
                catch (ArgumentException argEx)
                {
                    _log.Error("Ocorreu um falha ao tentar buscar uma referência à tabela desta classe.", argEx);
                }

                if (controllerTable == null)
                    throw new ArgumentException("A tabela não pode ser carregada.");

                _table = controllerTable;

                return controllerTable;
            }
        }

        public List<string> LoadDomains(string domain)
        {
            List<String> listDomains = new List<string>();
            IDataset dataset = (IDataset)GetWorkspace;
            IWorkspaceDomains2 domains = (IWorkspaceDomains2)dataset;
            ICodedValueDomain dom = null;
            try
            {
                dom = (ICodedValueDomain)domains.get_DomainByName(domain);
                for (int i = 0; i <= dom.CodeCount - 1; i++)
                {
                    listDomains.Add(dom.get_Name(i));
                }

            }
            catch (Exception ex)
            {
                //_log.Error("Ocorreu um problema ao tentar carregar os domínios do componente SegmentListComponent.");
            }
            finally
            {
                //if (domains != null)
                //    Marshal.ReleaseComObject(domains);

                //if (dom != null)
                //    Marshal.ReleaseComObject(dom);

                //Marshal.ReleaseComObject(dataset);
            }
            return listDomains;
        }
        public string GetDomainDisplay(string domain, object domainValu)
        {
            List<String> listDomains = new List<string>();
            IDataset dataset = (IDataset)GetWorkspace;
            IWorkspaceDomains2 domains = (IWorkspaceDomains2)dataset;
            ICodedValueDomain dom = null;
            try
            {
                dom = (ICodedValueDomain)domains.get_DomainByName(domain);
                for (int i = 0; i <= dom.CodeCount - 1; i++)
                {
                    if(dom.get_Value(i).Equals(domainValu))
                        return dom.get_Name(i);
                }

            }
            catch (Exception ex)
            {
                //_log.Error("Ocorreu um problema ao tentar carregar os domínios do componente SegmentListComponent.");
            }
            finally
            {
                //if (domains != null)
                //    Marshal.ReleaseComObject(domains);

                //if (dom != null)
                //    Marshal.ReleaseComObject(dom);

                //Marshal.ReleaseComObject(dataset);
            }
            return "";
        }

        public virtual IWorkspace GetWorkspace
        {
            get
            {
                var ws = workspace;
                if (ws != null)
                    return ws;

                IWorkspace controllerWorkspace = null;

                IWorkspaceFactory factory = null;

                lock (typeof(System.Object))
                {
                    try
                    {
                        switch (_workAtt.WorkspaceType)
                        {
                            case esriWorkspaceType.esriRemoteDatabaseWorkspace:
                                factory = new SdeWorkspaceFactoryClass();
                                if (_workAtt.ConnectionMethod == gisActiveRecordConnectionMethod.FILE)
                                    controllerWorkspace = factory.OpenFromFile(_workAtt.WorkspacePath, 0);
                                else
                                    controllerWorkspace = factory.Open(_workAtt.ConnectionProperties, 0);

                                break;
                            case esriWorkspaceType.esriLocalDatabaseWorkspace:
                                // personal
                                if (_workAtt.WorkspacePath.EndsWith("mdb"))
                                {
                                    factory = new AccessWorkspaceFactoryClass();
                                    controllerWorkspace = factory.OpenFromFile(_workAtt.WorkspacePath, 0);
                                }
                                else if (_workAtt.WorkspacePath.EndsWith("gdb"))
                                {
                                    factory = new FileGDBWorkspaceFactoryClass();
                                    controllerWorkspace = factory.OpenFromFile(_workAtt.WorkspacePath, 0);
                                }
                                else
                                    throw new ArgumentException("O geodatabase local não é do tipo personal ou file.");

                                break;
                            case esriWorkspaceType.esriFileSystemWorkspace:
                                throw new ArgumentException("Workspace do tipo diretório não é suportado.");
                            default:
                                throw new ArgumentException("Workspace de tipo não reconhecido.");
                        }

                        if (controllerWorkspace == null)
                            throw new ArgumentException("O workspace não pode ser carregado. Verifique se o mesmo existe.");
                    }
                    catch (COMException comEx)
                    {
                        _log.Error("Ocorreu um erro ao tentar buscar uma referência ao workspace desta classe.", comEx);
                    }
                    catch (Exception ex)
                    {
                        _log.Error("Ocorre um erro desconhecido ao tentar buscar uma referência ao workspace desta classe.", ex);
                    }


                    //if (hs.Contains(controllerWorkspace))
                    //    {
                    //        throw new Exception("Workspace not singleton!!!");
                    //    }

                    //    hs.Add(controllerWorkspace);
                        
                    workspace = controllerWorkspace;
                    return controllerWorkspace;
                }
            }
        }

        /// <summary>
        /// This method makes sure that the EditHandler is in place.
        /// </summary>
        protected void TestEditHandler()
        {
            if (_editHandler != null)
                return;

            if (GetWorkspace.Type == esriWorkspaceType.esriRemoteDatabaseWorkspace)
                _editHandler = new RemoteWorkspaceEditHandler(GetWorkspace);
            else
                _editHandler = new LocalWorkspaceEditHandler(GetWorkspace);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (IsBeingEdited)
                        StopEdit(true);
                }
            }
            _disposed = true;
        }
    }
}
