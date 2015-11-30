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

namespace GISActiveRecord.Attributes
{
    [AttributeUsage(AttributeTargets.Class,AllowMultiple=false,Inherited=true)]
    public class TableAttribute:Attribute
    {
        public TableAttribute(string tableName,string className)
        {
            RecordTreshold = 100;
            DataOwner = string.Empty;
            DatabaseName = string.Empty;
            TableName = tableName;
            ClassName = className;
        }

        /// <summary>
        /// This member indicates the table name;
        /// </summary>
        /// <remarks>
        /// Required;
        /// </remarks>
        public string TableName { get; set; }

        /// <summary>
        /// This member indicates the class name along the namespace of a class;
        /// </summary>
        /// <remarks>
        /// The classname is required because it will be used to instantiate a new proxy object;
        /// Required;
        /// </remarks>
        public string ClassName { get; set; }

        public string DatabaseName { get; set; }

        public string DataOwner { get; set; }

        public int RecordTreshold { get; set; }
    }
}
