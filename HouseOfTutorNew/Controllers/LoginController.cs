using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;
using HouseOfTutorNew.Models;
using HouseOfTutorNew.Controllers;

namespace HouseOfTutorNew.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class LoginController : ApiController
    {
        houseoftutorEntities db = new houseoftutorEntities();
        [HttpGet]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public HttpResponseMessage Login(string email, string password)
        {
            try
            {
                Tutor u = db.Tutors.Where(s => s.email == email && s.password == password).FirstOrDefault();
                if (u == null)
                {
                    Student st = db.Students.Where(s => s.email == email && s.password == password).FirstOrDefault();
                    if (st == null)
                    {
                        Admin ad = db.Admins.Where(a => a.email == email && a.password == password).FirstOrDefault();
                        if (ad == null)
                        {
                            
                            return Request.CreateResponse(HttpStatusCode.OK, "User Not Found");

                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, new { data = new { ad.name, ad.password, ad.email }, Role = "Admin" });

                        }
                    }
                    else
                    {
                        Task.Run(() => AutoFinishCourse(st.email));
                        return Request.CreateResponse(HttpStatusCode.OK, new { data = new { st.name, st.email, st.password, st.cgpa, st.semester, st.fathercnic, st.image }, Role = "Student" });

                    }
                }
                Task.Run(() => AutoFinishCourse(u.email));
                return Request.CreateResponse(HttpStatusCode.OK, new { data = new { u.name, u.email, u.password, u.cgpa, u.semester, u.image }, Role = "Tutor" });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public HttpResponseMessage ParentLogin(string cnic)
        {
            try
            {
                var Students = db.Students.Where(s => s.fathercnic == cnic).Select(s => new { s.email, s.cgpa, s.contact, s.fathercnic, s.gender,s.name }).ToList();
                if (Students == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, "You Don't have any registered Childrens");
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { data = Students, Role = "Parent" });
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        private async Task AutoFinishCourse(String email)
        {
            
            DateTime endTime = DateTime.Now;
            // Perform any cleanup or final operations after the autoReject function ends
            var tt = db.TemporaryEnrolleds.Where(ss => ss.studentemail == email).ToList();
            if (tt.Count>0) 
            {
                foreach (var item in tt)
                {
                    var endDate = item.dateToEnd;
                    bool ret = isEndDate(endDate);
                    if (ret)
                    {
                        TemporaryEnrolled rrr = db.TemporaryEnrolleds.Where(tr => tr.id == item.id).FirstOrDefault();
                        rrr.coursestatus = 1;
                        db.SaveChanges();
                        Student ss = db.Students.Where(sz => sz.email == item.studentemail).FirstOrDefault();
                        Tutor tz = db.Tutors.Where(q => q.email == item.tutoremail).FirstOrDefault();
                        Schedule std = db.Schedules.Where(sp => sp.scheduleid == ss.scheduleid).FirstOrDefault();
                        Schedule ttd = db.Schedules.Where(l => l.scheduleid == tz.scheduleid).FirstOrDefault();
                        String eSchedule = item.schedule;
                        char[] eScheduleArr = eSchedule.ToCharArray();
                        List<int> enrolledClasses = new List<int>();
                        for (int i = 0; i < eSchedule.Length; i++)
                        {
                            if (eSchedule[i] == '2')
                            {
                                enrolledClasses.Add(i);
                            }
                        }
                        char[] stdScheduleArr = std.details.ToCharArray();
                        char[] tScheduleArr = ttd.details.ToCharArray();
                        foreach (var item2 in enrolledClasses)
                        {
                            stdScheduleArr[item2] = '1';
                            tScheduleArr[item2] = '1';
                        }
                        String stdString = "";
                        String tString = "";
                        for (int i = 0; i < stdScheduleArr.Length; i++)
                        {
                            stdString = String.Concat(stdString, stdScheduleArr[i]);
                            tString = String.Concat(tString, tScheduleArr[i]);
                        }
                        std.details = stdString;
                        ttd.details = tString;
                        db.SaveChanges();
                    }
                }
            }
            var ts = db.TemporaryEnrolleds.Where(ss => ss.tutoremail == email).ToList();
            if (ts.Count > 0)
            {
                foreach (var item in ts)
                {
                    var endDate = item.dateToEnd;
                    bool ret=isEndDate(endDate);
                    if (ret)
                    {
                        TemporaryEnrolled rrr= db.TemporaryEnrolleds.Where(tr => tr.id == item.id).FirstOrDefault();
                        rrr.coursestatus = 1;
                        db.SaveChanges();
                        Student ss = db.Students.Where(sz => sz.email == item.studentemail).FirstOrDefault();
                        Tutor tz = db.Tutors.Where(q => q.email == item.tutoremail).FirstOrDefault();
                        Schedule std = db.Schedules.Where(sp => sp.scheduleid == ss.scheduleid).FirstOrDefault();
                        Schedule ttd = db.Schedules.Where(l => l.scheduleid == tz.scheduleid).FirstOrDefault();
                        String eSchedule = item.schedule;
                        char[] eScheduleArr = eSchedule.ToCharArray();
                        List<int> enrolledClasses = new List<int>();
                        for (int i = 0; i < eSchedule.Length; i++)
                        {
                            if (eSchedule[i] == '2')
                            {
                                enrolledClasses.Add(i);
                            }
                        }
                        char[] stdScheduleArr = std.details.ToCharArray();
                        char[] tScheduleArr = ttd.details.ToCharArray();
                        foreach (var item2 in enrolledClasses)
                        {
                            stdScheduleArr[item2] = '1';
                            tScheduleArr[item2] = '1';
                        }
                        String stdString = "";
                        String tString = "";
                        for (int i = 0; i < stdScheduleArr.Length; i++)
                        {
                            stdString = String.Concat(stdString, stdScheduleArr[i]);
                            tString = String.Concat(tString, tScheduleArr[i]);
                        }
                        std.details = stdString;
                        ttd.details = tString;
                        db.SaveChanges();
                    }
                }
            }
        }
        public bool isEndDate(String endDate)
        {
            DateTime dateEE = DateTime.ParseExact(endDate, "MM/dd/yyyy", null);
            DateTime currentDate = DateTime.Now;
            if (  currentDate> dateEE)
            {
                return true;
            }
            else
            {
                return false;
            }

        }
    }
}
