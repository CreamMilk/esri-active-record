using System.Collections.Generic;
using System.Text;
using GISActiveRecord.Validator;

namespace GISActiveRecord.Core
{
    public interface IActiveRecordState
    {
        IActiveRecord Record { get; set; }
        IEnumerable<RuleViolation> Errors { get; }
        bool Deleted { get; }
        bool IsValid { get; }
        string ValidationSummary { get; }

        void Delete();
        void AddRuleViolation(RuleViolation violation);
        void AddRuleViolation(string property, string errorDescription);
    }
}
