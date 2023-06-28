using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HouseOfTutorNew.Models.CustomClasses
{
    public class StudentRequestCustom
    {
        public int courseid { get; set; }
        public String studentemail { get; set; }
        public String tutoremail { get; set; }
        public String coursename { get; set; }
        public String slot { get; set; }
        public String studentname { get; set; }
        public String  enrollDate { get; set; }
    }
}