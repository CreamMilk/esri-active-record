using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Geodatabase;

namespace GISActiveRecord.Attributes
{
    /// <summary>
    /// Defines a special type of attribute.
    /// </summary>
    /// <remarks>
    /// ActiveRecord classes use it to find out
    /// if there is a need for it to be unique.
    /// 
    /// If there are various Unique attributes,
    /// they will be considered "UniqueTogether".
    /// </remarks>
    public class UniqueAttribute:FieldAttribute
    {
        public UniqueAttribute(string fieldName,esriFieldType fieldType,int fieldIndex)
            :base(fieldName,fieldType,fieldIndex)
        {
        }
    }
}
