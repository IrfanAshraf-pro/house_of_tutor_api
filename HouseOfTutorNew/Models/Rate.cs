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
    
    public partial class Rate
    {
        public string studentemail { get; set; }
        public string tutoremail { get; set; }
        public int courseid { get; set; }
        public Nullable<double> rating { get; set; }
    
        public virtual Course Course { get; set; }
        public virtual Student Student { get; set; }
        public virtual Tutor Tutor { get; set; }
    }
}
