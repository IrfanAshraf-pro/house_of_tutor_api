using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HouseOfTutorNew.Models.CustomClasses
{
    public class ParentTutorMatched
    {
        public String name { get; set; }
        public String email { get; set; }
        public String cgpa { get; set; }
        public String semester { get; set; }
        public String grade { get; set; }
        public String slotMatched { get; set; }
        public String rating { get; set; }
        public int ratingCount { get; set; }
        public List<TutorMatchedMessage> message { get; set; }
        public List<String> coursegroup { get; set; }
    }
}