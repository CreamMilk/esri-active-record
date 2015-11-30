/*
Copyright (c) 2011, George Rodrigues da Cunha Silva
All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:
    * Redistributions of source code must retain the above copyright
      notice, this list of conditions and the following disclaimer.
    * Redistributions in binary form must reproduce the above copyright
      notice, this list of conditions and the following disclaimer in the
      documentation and/or other materials provided with the distribution.
    * Neither the name of the <organization> nor the
      names of its contributors may be used to endorse or promote products
      derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL <COPYRIGHT HOLDER> BE LIABLE FOR ANY
DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/

using ESRI.ArcGIS.Geodatabase;

namespace GISActiveRecord.GIS.Geodatabase
{
    public abstract class WorkspaceEditHandler : IWorkspaceEditHandler
    {
        protected IWorkspace _currentWorkspace;

        public IWorkspace CurrentWorkspace
        {
            get
            {
                return this._currentWorkspace;
            }
            set
            {
                this._currentWorkspace = value;
            }
        }

        public bool IsBeingEdited()
        {
            var workspaceEdit = CurrentWorkspace as IWorkspaceEdit2;
            return workspaceEdit.IsBeingEdited();
        }

        public bool IsInEditOperation()
        {
            var workspaceEdit = CurrentWorkspace as IWorkspaceEdit2;
            return workspaceEdit.IsInEditOperation;
        }

        public abstract void StartEditing(bool withUndo);

        public void StartEditingOperation()
        {
            var workspaceEdit = CurrentWorkspace as IWorkspaceEdit2;
            workspaceEdit.StartEditOperation();
        }

        public void StopEditing(bool saveChanges)
        {
            var workspaceEdit = CurrentWorkspace as IWorkspaceEdit2;

            if (CurrentWorkspace.Type == esriWorkspaceType.esriRemoteDatabaseWorkspace)
            {
                IVersionedWorkspace versionWorkspace = (IVersionedWorkspace)CurrentWorkspace;
                IVersion2 version2 = (IVersion2)versionWorkspace;

                if (version2 != null && version2.IsRedefined)
                {
                    IVersionEdit4 version4 = (IVersionEdit4)CurrentWorkspace;
                    version4.Reconcile4("SDE.DEFAULT",
                        false,
                        false,
                        true,
                        true);
                }
            }

            workspaceEdit.StopEditing(saveChanges);
        }

        public void StopEditingOperation()
        {
            IWorkspaceEdit workspaceEdit = (IWorkspaceEdit)CurrentWorkspace;
            workspaceEdit.StopEditOperation();
        }

        public IDataChangesEx DataChanges(esriEditDataChangesType dataChangesType)
        {
            var workspaceEdit = CurrentWorkspace as IWorkspaceEdit2;
            return workspaceEdit.get_EditDataChanges(dataChangesType);
        }
    }
}
