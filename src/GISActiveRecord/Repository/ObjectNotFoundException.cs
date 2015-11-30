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
using GISActiveRecord.Attributes;

namespace GISActiveRecord.Repository
{
    /// <summary>
    /// Exception thrown when an object cannot be found via
    /// Single or SingleByOid method.
    /// </summary>
    public class ObjectNotFoundException:RepositoryException
    {
        private object _Identifier;

        public object Identifier
        {
            get { return _Identifier; }
        }

        public ObjectNotFoundException(WorkspaceAttribute workatt, TableAttribute tableAtt, bool editing, object identifier)
            : base(workatt, tableAtt, editing)
        {
            _Identifier = identifier;
        }

        public ObjectNotFoundException(string message, WorkspaceAttribute workAtt, TableAttribute tableAtt, bool editing, object identifier)
            : base(message, workAtt, tableAtt, editing)
        {
            _Identifier = identifier;
        }

        public ObjectNotFoundException(string message, Exception innerEx, WorkspaceAttribute workAtt, TableAttribute tableAtt, bool editing, object identifier)
            : base(message, innerEx, workAtt, tableAtt, editing)
        {
            _Identifier = identifier;
        }
    }
}
