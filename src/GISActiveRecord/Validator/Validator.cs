using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GISActiveRecord.Core;

namespace GISActiveRecord.Validator
{
    public class RuleViolation
    {
        public string PropertyName { get; set; }
        public string ErrorDescription { get; set; }
    }
}
