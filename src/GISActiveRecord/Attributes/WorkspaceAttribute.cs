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

using System;
using System.Collections.Generic;
using System.Text;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geodatabase;

namespace GISActiveRecord.Attributes
{
    [AttributeUsage(AttributeTargets.Class,AllowMultiple=false,Inherited=true)]
    public class WorkspaceAttribute:Attribute
    {
        public string WorkspacePath { get; set; }

        public esriWorkspaceType WorkspaceType { get; set; }
        public gisActiveRecordConnectionMethod ConnectionMethod { get; private set; }

        public string Server { get; set;}
        public string Port { get; set; }
        public string Database { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public string Version { get; set; }

        public IPropertySet ConnectionProperties
        {
            get
            {
                if (ConnectionMethod != gisActiveRecordConnectionMethod.PROPERTIES)
                    throw new ActiveRecordAttributeException(GetType(),"Não é possível criar as propriedades de conexão no modo Arquivo.");

                IPropertySet propSet = new PropertySetClass();
                propSet.SetProperty("SERVER", Server);
                propSet.SetProperty("INSTANCE", Port);
                propSet.SetProperty("DATABASE", Database);
                propSet.SetProperty("USER", User);
                propSet.SetProperty("PASSWORD", Password);
                propSet.SetProperty("VERSION", Version);

                return propSet;
            }
        }

        public WorkspaceAttribute(string workspacePath,esriWorkspaceType workspaceType)
        {
            WorkspacePath = workspacePath;
            WorkspaceType = workspaceType;
            ConnectionMethod = gisActiveRecordConnectionMethod.FILE;
        }

        public WorkspaceAttribute(string server,string port,string database,string user,string password, string version)
        {
            Server = server;
            Port = port;
            Database = database;
            User = user;
            Password = password;
            Version = version;
            WorkspaceType = esriWorkspaceType.esriRemoteDatabaseWorkspace;
            ConnectionMethod = gisActiveRecordConnectionMethod.PROPERTIES;
        }
    }
}
