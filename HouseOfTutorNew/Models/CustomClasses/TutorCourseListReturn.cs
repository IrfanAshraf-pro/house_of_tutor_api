using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HouseOfTutorNew.Models.CustomClasses
{
    public class TutorCourseListReturn
    {
        public int courseid { get; set; }
        public String coursegrade { get; set; }
        public String tutoremail { get; set; }
        public int type { get; set; }
        public String coursename { get; set; }
        public String email { get; set; }
    }
}