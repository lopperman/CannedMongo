using System;
using CS.Common.Enums;

namespace CS.Common.Attributes
{
    /// <summary>
    /// Apply to a Mongo object type (Implements IEntity) to save a copy of any object that is edited and/or deleted into the 'DeletedObjects' collection
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public class DocumentAuditAttr : Attribute
    {
        public DocumentAuditEnum AuditEnum { get; set; }
    }
}