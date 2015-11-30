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
using ESRI.ArcGIS.Geodatabase;

namespace GISActiveRecord.Attributes
{
    /// <summary>
    /// Class that represents a field
    /// </summary>
    /// <remarks>
    /// the position argument, thought it's not nullable, it can be passed as -1
    /// in the case of calculated attributes and such
    /// </remarks>
    [System.Diagnostics.DebuggerStepThrough]
    [AttributeUsage(AttributeTargets.Property,AllowMultiple=false,Inherited=true)]
    public class FieldAttribute:Attribute
    {
        protected string _fieldName;
        protected esriFieldType _fieldType;
        protected bool _notNullable;
        protected bool _isPrimary;
        protected bool _isForeign;
        protected int _index;

        public virtual string FieldName
        {
            get { return this._fieldName; }
            set { this._fieldName = value; }
        }

        public virtual esriFieldType FieldType
        {
            get { return this._fieldType; }
            set { this._fieldType = value; }
        }

        public virtual bool NotNullable
        {
            get { return this._notNullable; }
            set { this._notNullable = value; }
        }

        public virtual int Index
        {
            get { return this._index; }
            set { this._index = value; }
        }

        public virtual bool QuoteValue
        {
            get
            {
                switch (_fieldType)
                {
                    case esriFieldType.esriFieldTypeGUID:
                    case esriFieldType.esriFieldTypeGlobalID:
                    case esriFieldType.esriFieldTypeString:
                        return true;
                    case esriFieldType.esriFieldTypeSmallInteger:
                    case esriFieldType.esriFieldTypeInteger:
                    case esriFieldType.esriFieldTypeDouble:
                    case esriFieldType.esriFieldTypeSingle:
                    case esriFieldType.esriFieldTypeDate:
                    case esriFieldType.esriFieldTypeOID:
                        return false;
                    default:
                        throw new ArgumentException("Este tipo de campo não é pesquisável.");
                }
            }
        }

        public FieldAttribute(string fieldName, esriFieldType fieldType,int position)
        {
            FieldName = fieldName;
            FieldType = fieldType;
            _isPrimary = false;
            _isForeign = false;
            Index = position;
        }
    }


}