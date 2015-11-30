using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.Geodatabase;
using GISActiveRecord.Core;

namespace GISActiveRecord.Repository
{
    /// <summary>
    /// Implementa o padrão de projeto UnitOfWork.
    /// </summary>
    /// <remarks>
    /// Desenvolvido para disparar operações em batch diretamente no banco de dados, em uma única transação.
    /// UnitOfWork somente opera com IWorkspaceEdit.
    /// 
    /// CommitWithoutEvents NÃO ESTÁ IMPLEMENTADO.
    /// </remarks>
    public class UnitOfWork : IUnitOfWork,IUnitOfWorkEvents,IDisposable
    {
        private readonly IWorkspace _currentWorkspace;
        private readonly List<IActiveRecord> _created;
        private readonly List<IActiveRecord> _updated;
        private readonly List<IActiveRecord> _deleted;

        private readonly bool _bypassEvents;
        private readonly bool _withUndo;

        #region Events

        public event EventHandler BeforeCommit;

        public void OnBeforeCommit(EventArgs e)
        {
            if (BeforeCommit != null)
                BeforeCommit(this, e);
        }

        public event EventHandler BeforeRollback;

        public void OnBeforeRollback(EventArgs e)
        {
            if (BeforeRollback != null)
                BeforeRollback(this, e);
        }

        public event EventHandler AfterCommit;

        public void OnAfterCommit(EventArgs e)
        {
            if (AfterCommit != null)
                AfterCommit(this, e);
        }

        public event EventHandler AfterRollback;

        public void OnAfterRollback(EventArgs e)
        {
            if (AfterRollback != null)
                AfterRollback(this, e);
        }

        public event EventHandler BeforeCreate;

        public void OnBeforeCreate(ActiveRecordTransactionEventArgs e)
        {
            if (BeforeCreate != null)
                BeforeCreate(this, e);
        }

        public event EventHandler BeforeUpdate;

        public void OnBeforeUpdate(ActiveRecordTransactionEventArgs e)
        {
            if (BeforeUpdate != null)
                BeforeUpdate(this, e);
        }

        public event EventHandler BeforeDelete;

        public void OnBeforeDelete(ActiveRecordTransactionEventArgs e)
        {
            if (BeforeDelete != null)
                BeforeDelete(this, e);
        }

        #endregion

        #region event handlers

        void UnitOfWork_AfterCommit(object sender, EventArgs e)
        {
            StopEdition(true);
        }

        void UnitOfWork_BeforeCommit(object sender, EventArgs e)
        {
            StartEdition(_withUndo);
        }

        #endregion

        public IWorkspace CurrentWorkspace
        {
            get { return _currentWorkspace; }
        }

        public bool BypassEvents
        {
            get { return false; }
        }

        /// <summary>
        /// Constrói uma nova unidade de trabalho
        /// </summary>
        /// <remarks>
        /// O default para BypassEvents é false.
        /// </remarks>
        /// <param name="currentWorkspace">IWorkspace</param>
        /// <param name="withUndo">bool</param>
        public UnitOfWork(IWorkspace currentWorkspace,bool withUndo)
        {
            _created = new List<IActiveRecord>();
            _updated = new List<IActiveRecord>();
            _deleted = new List<IActiveRecord>();
            _currentWorkspace = currentWorkspace;
            _withUndo = withUndo;
            _bypassEvents = false;

            // BeforeCommit +=new EventHandler(UnitOfWork_BeforeCommit);
            // AfterCommit += new EventHandler(UnitOfWork_AfterCommit);
        }

        /// <summary>
        /// Constrói uma nova unidade de trabalho
        /// </summary>
        /// <remarks>
        /// O default para BypassEvents é false.
        /// </remarks>
        /// <param name="currentWorkspace">IWorkspace</param>
        /// <param name="withUndo">bool</param>
        /// <param name="bypassEvents">bool</param>
        public UnitOfWork(IWorkspace currentWorkspace, bool withUndo,bool bypassEvents)
        {
            _created = new List<IActiveRecord>();
            _updated = new List<IActiveRecord>();
            _deleted = new List<IActiveRecord>();
            _currentWorkspace = currentWorkspace;
            _withUndo = withUndo;
            _bypassEvents = bypassEvents;

            // BeforeCommit += UnitOfWork_BeforeCommit;
            // AfterCommit += UnitOfWork_AfterCommit;
        }

        public List<IActiveRecord> Created
        {
            get
            {
                return _created;
            }
        }

        public List<IActiveRecord> Updated
        {
            get
            {
                return _updated;
            }
        }

        public List<IActiveRecord> Deleted
        {
            get
            {
                return _deleted;
            }
        }

        /// <summary>
        /// Creates a new IActiveRecord object inside the database
        /// </summary>
        /// <remarks>
        /// This method must be refactored.
        /// The way to go here is to actually get an IActiveRecord as the return
        /// of this method, being used without any parameters.
        /// This is because there is no way to currently delay the creation of an
        /// object inside a geodatabase, we must create it and return it right
        /// away.
        /// </remarks>
        /// <param name="record">IActiveRecord</param>
        [Obsolete]
        public void Create(IActiveRecord record)
        {
            if (record == null)
                return;

            if (Created.Contains(record))
                return;

            OnBeforeCreate(new ActiveRecordTransactionEventArgs(record));

            _created.Add(record);
        }

        public void Update(IActiveRecord record)
        {
            if (record == null)
                return;

            if (Updated.Contains(record))
                return;

            OnBeforeUpdate(new ActiveRecordTransactionEventArgs(record));

            _updated.Add(record);
        }

        public void Delete(IActiveRecord record)
        {
            if (record == null)
                return;

            if (Deleted.Contains(record))
                return;

            OnBeforeDelete(new ActiveRecordTransactionEventArgs(record));

            _deleted.Add(record); 
        }

        public void Commit()
        {
            try
            {
                OnBeforeCommit(EventArgs.Empty);

                if (BypassEvents)
                    CommitWithoutEvents();
                else
                    CommitWithEvents();

                OnAfterCommit(EventArgs.Empty);

                ClearPendingRecords();
            }
            catch (ActiveRecordException arEx)
            {
                // LOG
                System.Diagnostics.Debug.WriteLine(arEx.Message);
                // Rollback();
            }
            catch (COMException comEx)
            {
                // LOG logar erro
                System.Diagnostics.Debug.WriteLine(comEx);
                // Rollback();
            }
        }

        private void CommitWithEvents()
        {
            foreach (IActiveRecord activeRecord in Created)
            {
                try
                {
                    activeRecord.Store();
                }
                catch (ActiveRecordException recEx)
                {
                    continue;
                }
            }

            foreach (IActiveRecord activeRecord in Updated)
            {
                try
                {
                    activeRecord.Store();
                }
                catch (ActiveRecordException recEx)
                {
                    continue;
                }
            }

            foreach (IActiveRecord activeRecord in Deleted)
            {
                try
                {
                    activeRecord.Delete();
                }
                catch (ActiveRecordException recEx)
                {
                    continue;
                }
            }
        }

        private void CommitWithoutEvents()
        {
            throw new NotImplementedException();
        }

        public void Rollback()
        {
            if (_currentWorkspace == null)
            {
                System.Diagnostics.Debug.WriteLine("Current Workspace em UnitOfWork é nulo.");
                return;
            }

            OnBeforeRollback(EventArgs.Empty);

            IWorkspaceEdit edit = _currentWorkspace as IWorkspaceEdit;
            ClearPendingRecords();

            edit.AbortEditOperation();
            edit.StopEditing(false);

            OnAfterRollback(EventArgs.Empty); 
        }

        private void ClearPendingRecords()
        {
            // função que executa o rollback
            // deleted.Clear pois ninguem será deletado com o rollback

            //_deleted.Clear();
            //_updated.Clear();
            //_created.Clear();
        }

        private void StartEdition(bool canUndo)
        {
            IWorkspaceEdit edit = _currentWorkspace as IWorkspaceEdit;

            bool hasEdits = false;
            edit.HasEdits(ref hasEdits);

            if (hasEdits && edit.IsBeingEdited())
                return;

            edit.StartEditing(canUndo);
            edit.StartEditOperation();

            // BeforeCommit += new EventHandler(UnitOfWork_BeforeCommit);
        }

        private void StopEdition(bool save)
        {
            var edit = _currentWorkspace as IWorkspaceEdit;

            var hasEdits = false;
            edit.HasEdits(ref hasEdits);

            //if (_created.Count == 0 && _updated.Count == 0 && _deleted.Count == 0 && !hasEdits)
            //    return;

            if (!edit.IsBeingEdited())
                return;

            edit.StopEditOperation();
            edit.StopEditing(save);
        }

        #region IDisposable Members

        public void Dispose()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion
    }
}