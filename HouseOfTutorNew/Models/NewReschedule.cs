//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace HouseOfTutorNew.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class NewReschedule
    {
        public int id { get; set; }
        public Nullable<int> classreportid { get; set; }
        public Nullable<int> rescheduledclassstatus { get; set; }
        public string rescheduleclassFrom { get; set; }
        public string rescheduleclassTo { get; set; }
        public Nullable<int> slotFrom { get; set; }
        public Nullable<int> slotTo { get; set; }
        public Nullable<int> classStatus { get; set; }
    
        public virtual ClassReport ClassReport { get; set; }
    }
}
