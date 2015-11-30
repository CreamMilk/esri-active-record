using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Geodatabase;

namespace GISActiveRecord.Attributes
{
    public class SpatialRelationshipAttribute : RelationshipAttribute
    {
        public virtual esriSpatialRelEnum SpatialRelation
        {
            get { return esriSpatialRelEnum.esriSpatialRelUndefined; }
        }

        public SpatialRelationshipAttribute(Type classType)
            : base(classType, "SHAPE", esriFieldType.esriFieldTypeGeometry)
        {
        }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class ContainsAttribute:SpatialRelationshipAttribute
    {
        public override esriSpatialRelEnum SpatialRelation
        {
            get
            {
                return esriSpatialRelEnum.esriSpatialRelContains;
            }
        }

        public ContainsAttribute(Type classType)
            : base(classType)
        { }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class CrossesAttribute :SpatialRelationshipAttribute
    {
        public override esriSpatialRelEnum SpatialRelation
        {
            get
            {
                return esriSpatialRelEnum.esriSpatialRelCrosses;
            }
        }

        public CrossesAttribute(Type classType)
            : base(classType)
        { }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class IntersectsAttribute : SpatialRelationshipAttribute
    {
        public override esriSpatialRelEnum SpatialRelation
        {
            get
            {
                return esriSpatialRelEnum.esriSpatialRelIntersects;
            }
        }

        public IntersectsAttribute(Type classType)
            : base(classType)
        { }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class TouchesAttribute: SpatialRelationshipAttribute
    {
        public override esriSpatialRelEnum SpatialRelation
        {
            get
            {
                return esriSpatialRelEnum.esriSpatialRelTouches;
            }
        }

        public TouchesAttribute(Type classType)
            : base(classType)
        { }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class OverlapsAttribute: SpatialRelationshipAttribute
    {
        public override esriSpatialRelEnum SpatialRelation
        {
            get
            {
                return esriSpatialRelEnum.esriSpatialRelOverlaps;
            }
        }

        public OverlapsAttribute(Type classType)
            : base(classType)
        { }
    }
    
    [AttributeUsage(AttributeTargets.Property,AllowMultiple=false,Inherited=true)]
    public class WithinAttribute : SpatialRelationshipAttribute
    {
        public override esriSpatialRelEnum SpatialRelation
        {
            get
            {
                return esriSpatialRelEnum.esriSpatialRelWithin;
            }
        }

        public WithinAttribute(Type classType)
            : base(classType)
        { }
    }
}
