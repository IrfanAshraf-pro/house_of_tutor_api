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
    
    public partial class Schedule
    {
        public Schedule()
        {
            this.Students = new HashSet<Student>();
            this.Tutors = new HashSet<Tutor>();
        }
    
        public int scheduleid { get; set; }
        public string details { get; set; }
    
        public virtual ICollection<Student> Students { get; set; }
        public virtual ICollection<Tutor> Tutors { get; set; }
    }
}
