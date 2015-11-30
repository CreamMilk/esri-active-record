using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Geodatabase;

namespace GISActiveRecord.Attributes
{
    /// <summary>
    /// Exception thrown when basic attribute finding operations
    /// cannot be fulfilled.
    /// </summary>
    [Serializable]
    public class ActiveRecordAttributeException:Exception
    {
        private Type _classType;

        public ActiveRecordAttributeException(Type classType)
        {
            _classType = classType;
        }

        public ActiveRecordAttributeException(Type classType,string message)
            : base(message)
        {
            _classType = classType;
        }

        public ActiveRecordAttributeException(Type classType,string message, Exception innerEx)
            : base(message, innerEx)
        {
            _classType = classType;
        }

        public string BuildActiveRecordReport()
        {
            return String.Empty;
        }
    }
}
