using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HouseOfTutorNew.Models.CustomClasses
{
    public class StudentEnlistedCustom
    {
        public int courseid { get; set; }
        public String  coursecode { get; set; }
        public String  coursename { get; set; }
        public int coursefee { get; set; }
        public int isLearning { get; set; }
    }
}