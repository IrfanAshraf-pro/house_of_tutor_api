using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HouseOfTutorNew.Models.CustomClasses
{
    public class CourseGroupCustom
    {
        public String groupName { get; set; }
        public List<SubjectGroupCustom> subjectGroup { get; set; }
    }
}