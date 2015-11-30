using System;
using System.Collections.Generic;
using ESRI.ArcGIS.Geodatabase;
using GISActiveRecord.Core;

namespace GISActiveRecord.Repository
{
    public interface IUnitOfWork
    {
        IWorkspace CurrentWorkspace { get; }
        bool BypassEvents { get; }

        List<IActiveRecord> Created { get; }
        List<IActiveRecord> Updated { get; }
        List<IActiveRecord> Deleted { get; }

        void Create(IActiveRecord record);
        void Update(IActiveRecord record);
        void Delete(IActiveRecord record);

        void Commit();
        void Rollback();
    }

    public class ActiveRecordTransactionEventArgs : EventArgs
    {
        private readonly IActiveRecord _record;

        public IActiveRecord Record
        {
            get { return _record; }
        }

        public ActiveRecordTransactionEventArgs(IActiveRecord record)
        {
            _record = record;
        }
    }

    public interface IUnitOfWorkEvents
    {
        event EventHandler BeforeCommit;
        void OnBeforeCommit(EventArgs e);

        event EventHandler BeforeRollback;
        void OnBeforeRollback(EventArgs e);

        event EventHandler AfterCommit;
        void OnAfterCommit(EventArgs e);

        event EventHandler AfterRollback;
        void OnAfterRollback(EventArgs e);

        event EventHandler BeforeCreate;
        void OnBeforeCreate(ActiveRecordTransactionEventArgs e);

        event EventHandler BeforeUpdate;
        void OnBeforeUpdate(ActiveRecordTransactionEventArgs e);

        event EventHandler BeforeDelete;
        void OnBeforeDelete(ActiveRecordTransactionEventArgs e);
    }
}
