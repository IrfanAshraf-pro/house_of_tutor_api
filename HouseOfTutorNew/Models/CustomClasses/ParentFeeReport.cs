using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HouseOfTutorNew.Models.CustomClasses
{
    public class ParentFeeReport
    {
        public String studentemail { get; set; }
        public String tutoremail { get; set; }
        public int courseid { get; set; }
        public String coursename { get; set; }
        public int totalFee { get; set; }
        public String name { get; set; }
        public bool showClassReport { get; set; }
        public bool showAbsents { get; set; }
        public bool showRescheduleTutor { get; set; }
        public bool showRescheduleStudent { get; set; }
        public List<ClassReportCustom> reportList { get; set; }
    }
}