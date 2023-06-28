using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HouseOfTutorNew.Models.CustomClasses
{
    public class TutorCustom
    {
        public String  email { get; set; }
        public String name { get; set; }
        public int isBlocked { get; set; }
        public double rating { get; set; }
    }
}