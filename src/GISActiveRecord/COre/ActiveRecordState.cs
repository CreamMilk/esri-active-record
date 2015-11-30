using System;
using System.Collections.Generic;
using System.Linq;
using GISActiveRecord.Validator;

namespace GISActiveRecord.Core
{
    public class ActiveRecordState : IActiveRecordState
    {
        private bool _deleted;

        public ActiveRecordState()
        {
            Errors = new List<RuleViolation>();
        }

        public IActiveRecord Record { get; set; }

        public IEnumerable<RuleViolation> Errors { get; private set; }

        public bool Deleted
        {
            get { return _deleted; }
        }

        public bool IsValid
        {
            //get { return true; }
            get { return Errors.Count() <= 0; }
        }

        public string ValidationSummary
        {
            get { return "Ooops, não implentado ainda!"; }
        }

        public void Delete()
        {
            _deleted = true;
        }

        public void AddRuleViolation(RuleViolation violation)
        {
            (Errors as List<RuleViolation>).Add(violation);
        }

        public void AddRuleViolation(string property, string errorDescription)
        {
            RuleViolation violation = new RuleViolation(property, errorDescription);
            (Errors as List<RuleViolation>).Add(violation);
        }
    }
}