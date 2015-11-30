using GISActiveRecord.Core;

namespace GISActiveRecord.Bind
{
    public interface IBinder
    {
        /// <summary>
        /// THis method is responsible for
        /// binding the record data to the
        /// control.
        /// </summary>
        /// <param name="record">IActiveRecord</param>
        /// <returns>IActiveRecordState</returns>
        ActiveRecordState BindToControl(IActiveRecord record, object control);

        /// <summary>
        /// This method is responsible for
        /// binding the control data to the
        /// database object.
        /// </summary>
        /// <returns></returns>
        ActiveRecordState BindToDatabase(IActiveRecord record, object control);
    }
}