using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HouseOfTutorNew.Models.CustomClasses
{
    public class LearningCustom
    {
        public String studentemail { get; set; }
        public String tutoremail { get; set; }
        public int courseid { get; set; }
        public String coursename { get; set; }
        public int coursestatus { get; set; }
    }
}