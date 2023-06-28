using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HouseOfTutorNew.Models.CustomClasses
{
    public class FeeReport
    {
        public String studentemail { get; set; }
        public String tutoremail { get; set; }
        public int courseid { get; set; }
        public String coursename { get; set; }
        public int totalFee { get; set; }
        public int paidamount { get; set; }
        public int remainingamount { get; set; }
        public String name { get; set; }
        public int noOfLectures { get; set; }
        public List<ClassReportCustom> reportList { get; set; }
    }
}