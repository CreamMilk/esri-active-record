using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GISActiveRecord.Core;
using GISActiveRecord.Validator;

namespace GISActiveRecord.Bind
{
    public class BaseBinder : IBinder
    {
        public BaseBinder()
        {
        }

        /// <summary>
        /// This method is responsible for
        /// binding the control data to the
        /// database object.
        /// </summary>
        /// <returns></returns>
        public virtual ActiveRecordState BindToDatabase(IActiveRecord record, object control)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// THis method is responsible for
        /// binding the record data to the
        /// control.
        /// </summary>
        /// <param name="record">IActiveRecord</param>
        /// <returns>IActiveRecordState</returns>
        public virtual ActiveRecordState BindToControl(IActiveRecord record, object control)
        {
            throw new NotImplementedException();
        }

        protected virtual void PaintControlError(object control,RuleViolation rv)
        {
            throw new NotImplementedException();
        }

        protected virtual void ClearControlError(object control)
        {
            throw new NotImplementedException();
        }

        protected virtual void ClearErrors(object control)
        {
            throw new NotImplementedException();
        }
    }
}
