using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GISActiveRecord.Core;

namespace GISActiveRecord.Validator
{
    public class RuleViolation
    {
        public RuleViolation(string PropertyName, string ErrorDescription)
        {
            this.ErrorDescription = ErrorDescription;
            this.PropertyName = PropertyName;
        }
        public string PropertyName { get; set; }
        public string ErrorDescription { get; set; }
    }
}
