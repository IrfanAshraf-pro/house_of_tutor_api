using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HouseOfTutorNew.Models.CustomClasses
{
    public class TutorCourseListCustom
    {
        public int courseid { get; set; }
        public bool isSelected{ get; set; }
        public String tutoremail { get; set; }
        public int type { get; set; }
        public String coursename { get; set; }
    }
}