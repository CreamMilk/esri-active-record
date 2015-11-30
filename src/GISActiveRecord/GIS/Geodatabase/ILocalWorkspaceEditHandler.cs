using ESRI.ArcGIS.Geodatabase;

namespace GISActiveRecord.GIS.Geodatabase
{
    public interface ILocalWorkspaceEditHandler : IWorkspaceEditHandler
    {
        IWorkspaceEdit2 GetEditingWorkspace { get; }
    }
}