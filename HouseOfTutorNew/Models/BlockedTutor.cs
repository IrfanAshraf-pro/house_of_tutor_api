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
    
    public partial class BlockedTutor
    {
        public int id { get; set; }
        public string email { get; set; }
        public Nullable<int> isBlock { get; set; }
    
        public virtual Tutor Tutor { get; set; }
    }
}
