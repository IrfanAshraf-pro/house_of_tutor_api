using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HouseOfTutorNew.Models.CustomClasses
{
    public class Reschedule
    {
        public String temail { get; set; }
        public String semail { get; set; }
        public String coursename { get; set; }
        public int slotno { get; set; }
        public String date { get; set; }
        public String day { get; set; }
        public String tdate { get; set; }
        public String tday { get; set; }
        public int tslotno { get; set; }
    }
}