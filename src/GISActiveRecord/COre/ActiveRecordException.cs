using System;

namespace GISActiveRecord.Core
{
    [Serializable]
    public class ActiveRecordException : Exception
    {
        public ActiveRecordException()
        {
        }

        public ActiveRecordException(string message)
            : base(message)
        {
        }

        public ActiveRecordException(string message, Exception innerEx)
            : base(message, innerEx)
        {
        }
    }
}