namespace GISActiveRecord.GIS.Geodatabase
{
    public class WorkspaceVersionOptions : IWorkspaceVersionOptions
    {
        /// <summary>
        /// Gets or sets if the version should acquire locks for reconciling
        /// </summary>
        /// <remarks>Default is false</remarks>
        public bool AcquireLocks { get; set; }

        /// <summary>
        /// Gets or sets who wins in case of conflicts'
        /// </summary>
        /// <remarks>
        /// By default, childWins = true
        /// </remarks>
        public bool ChildWins { get; set; }

        /// <summary>
        /// Gets or sets abort if the parent version conflicts with child
        /// </summary>
        /// <remarks>
        /// default is false
        /// </remarks>
        public bool AbortIfConflicts { get; set; }

        /// <summary>
        /// Gets or sets if the conflict detection should happen at column level
        /// </summary>
        /// <remarks>
        /// Default is true
        /// </remarks>
        public bool ColumnLevel { get; set; }

        public WorkspaceVersionOptions()
        {
            AcquireLocks = false;
            ChildWins = true;
            AbortIfConflicts = false;
            ColumnLevel = true;
        }
    }
}