using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HouseOfTutorNew.Models.CustomClasses
{
    public class ChangeSchedule
    {
        public String froom { get; set; }
        public String to { get; set; }
        public bool isFromInWeek { get; set; }
        public bool isToInWeek { get; set; }

    }
}