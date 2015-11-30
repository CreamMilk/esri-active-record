using ESRI.ArcGIS.Geodatabase;

namespace GISActiveRecord.GIS.Geodatabase
{
    public interface IRemoteWorkspaceEditHandler : IWorkspaceEditHandler
    {
        IMultiuserWorkspaceEdit GetEditingWorkspace { get; }
        IWorkspaceVersionOptions Options { get; }
    }
}