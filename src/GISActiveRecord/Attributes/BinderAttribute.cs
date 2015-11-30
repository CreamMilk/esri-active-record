using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using GISActiveRecord.Bind;
using GISActiveRecord.Core;

namespace GISActiveRecord.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class BinderAttribute:Attribute
    {
        private Type _binderType;

        public Type BinderType
        {
            get { return _binderType; }
            set
            {
                if (!(value is IBinder))
                    throw new ArgumentException("O tipo de binder não é honrado. Não é possível utilizar este tipo.");

                _binderType = value;
            }
        }

        public BinderAttribute(Type t)
        {
            BinderType = t;
        }
    }
}
