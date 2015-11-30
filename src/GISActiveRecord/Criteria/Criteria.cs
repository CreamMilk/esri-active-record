using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;

namespace GISActiveRecord.Criteria
{
    /// <summary>
    /// Just a draft implementation for spatial criteria
    /// </summary>
    public interface ICriteria
    {
        IWorkspace CurrentWorkspace { get; }
        IQueryFilter Filter { get; }
        bool IsValidFilter { get; }
    }

    public class Criteria:ICriteria
    {
        public IWorkspace CurrentWorkspace { get; private set; }
        public IQueryFilter Filter { get; private set; }
        public bool IsValidFilter { get; private set; }

        public Criteria(IWorkspace workspace,IQueryFilter filter)
        {
            CurrentWorkspace = workspace;
            filter = filter;
        }

        public Criteria(IWorkspace workspace,string query)
        {
            CurrentWorkspace = workspace;
            Filter = new QueryFilterClass { WhereClause = query };
        }
    }

    public static class CriteriaExtensions
    {
        public static ICriteria And(this ICriteria criteria,ICriteria other)
        {
            throw new NotImplementedException();
        }

        public static ICriteria And(this ICriteria criteria,IQueryFilter filter)
        {
            throw new NotImplementedException();
        }

        public static ICriteria And(this ICriteria criteria,string query)
        {
            throw new NotImplementedException();
        }

        public static ICriteria Or(this ICriteria criteria,ICriteria other)
        {
            throw new NotImplementedException();
        }

        public static ICriteria Or(this ICriteria criteria,IQueryFilter filter)
        {
            throw new NotImplementedException();
        }

        public static ICriteria Or(this ICriteria criteria,string filter)
        {
            throw new NotImplementedException();
        }

        public static ICriteria OrderBy(string[] fieldNames)
        {
            throw new NotImplementedException();
        }
    }
}
