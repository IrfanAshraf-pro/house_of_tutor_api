using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HouseOfTutorNew.Models.CustomClasses
{
    public class TempTodayClass
    {
        public String coursename { get; set; }
        public String name { get; set; }
        public String temail { get; set; }
        public String semail { get; set; }

        public String slot { get; set; }
        public String classDate { get; set; }
        public bool isReschedule { get; set; }
        public bool isPreSchedule { get; set; }
        public bool isStudent { get; set; }
        public bool isTemp { get; set; }
    }
}