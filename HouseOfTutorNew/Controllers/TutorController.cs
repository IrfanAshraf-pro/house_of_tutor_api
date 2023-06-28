using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Timers;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;
using HouseOfTutorNew.Models;
using HouseOfTutorNew.Models.CustomClasses;


namespace HouseOfTutorNew.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class TutorController : ApiController
    {
        private Timer cancelTimer;
        houseoftutorEntities db = new houseoftutorEntities();
        [HttpPost]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public HttpResponseMessage TutorSignup(Tutor t)
        {
            try
            {
                var v = db.Students.Where(i => i.email.ToLower() == t.email.ToLower()).FirstOrDefault();
                var tutor = db.Tutors.Where(i => i.email.ToLower() == t.email.ToLower()).FirstOrDefault();
                if (v == null)
                {
                    if (tutor == null)
                    {
                        Tutor std = new Tutor();
                        std.name = t.name;
                        std.email = t.email;
                        std.password = t.password;
                        std.semester = t.semester;
                        std.contact = t.contact;
                        std.cgpa = t.cgpa;
                        std.gender = t.gender;
                        db.Tutors.Add(std);
                        db.SaveChanges();
                        String details = "0000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000";
                        TutorSchedule(t.email, details);
                        db.SaveChanges();
                        return Request.CreateResponse(HttpStatusCode.OK, "Signed up successfully");
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, "Already Exist");
                    }
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, "You Can't Sign Up as Tutor Because You're already Student");
                }

            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, e.Message);
            }
        }
        [HttpPost]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public HttpResponseMessage UpdateProfilePic()
        {
            try
            {
                HttpRequest request = HttpContext.Current.Request;
                var imgpath = request.Files["photo"];
                String ext = imgpath.FileName.Split('.')[1];
                String email = request["email"];
                DateTime dt = DateTime.Now;

                String filename = "_" + dt.Year + dt.Month + dt.Day + dt.Hour + dt.Minute + dt.Second + "." + ext;
                imgpath.SaveAs(HttpContext.Current.Server.MapPath("~/Image/" + filename));
                var user = db.Tutors.Where(x => x.email == email).FirstOrDefault();
                user.image = filename;
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, "Image Uploaded Successfuly");
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, e.Message);
            }
        }

        [HttpPost]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public HttpResponseMessage TutorSchedule(String email, string details)
        {
            try
            {
                Tutor u = db.Tutors.Where(s => s.email == email).FirstOrDefault();
                if (u.scheduleid == null)
                {
                    Schedule s = new Schedule();
                    s.details = details;
                    db.Schedules.Add(s);
                    db.SaveChanges();
                    Schedule seDb = db.Schedules.OrderByDescending(se => se.scheduleid).FirstOrDefault();
                    u.scheduleid = seDb.scheduleid;
                    db.SaveChanges();
                    return Request.CreateResponse(HttpStatusCode.OK, "Schedule Set successfully");
                }
                else
                {
                    Schedule sd = db.Schedules.Where(ss => ss.scheduleid == u.scheduleid).FirstOrDefault();
                    sd.details = details;
                    db.SaveChanges();
                    return Request.CreateResponse(HttpStatusCode.OK, "Schedule Updated successfully");

                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, ex.Message);
            }
        }
        public string GetTSchedule(String email)
        {
            Tutor t = db.Tutors.Where(s => s.email == email).FirstOrDefault();
            Schedule sd = db.Schedules.Where(ss => ss.scheduleid == t.scheduleid).FirstOrDefault();
            var tutorEnrolled = db.CourseEnrolleds.Where(s => s.tutoremail == email).ToList();
            List<ChangeSchedule> toChangeSlots = new List<ChangeSchedule>();
            if (tutorEnrolled != null && tutorEnrolled.Count > 0)
            {
                foreach (var item in tutorEnrolled)
                {
                    var classReports = db.ClassReports.Where(cr => cr.courseenrolledid == item.id && cr.classesstatus == 2).ToList();
                    if (classReports != null && classReports.Count > 0)
                    {
                        foreach (var crItem in classReports)
                        {
                            NewReschedule r = db.NewReschedules.Where(rr => rr.classreportid == crItem.id && rr.rescheduledclassstatus == 0).FirstOrDefault();
                            if (r != null)
                            {
                                String cdate = GetCurrentDate();
                                String date = r.rescheduleclassTo;
                                String fromDate = r.rescheduleclassFrom;
                                String currentDate = cdate;
                                DateTime myDateTime2 = DateTime.ParseExact(date, "MM/dd/yyyy", null);
                                DateTime myDateTime = DateTime.ParseExact(currentDate, "MM/dd/yyyy", null);
                                DateTime myDateFrom = DateTime.ParseExact(fromDate, "MM/dd/yyyy", null);
                                DayOfWeek day = System.DayOfWeek.Monday;
                                bool ans = AreFallingInSameWeek(myDateTime, myDateTime2, day);
                                bool erp = AreFallingInSameWeek(myDateTime, myDateFrom, day);
                                if (ans || erp)
                                {
                                    ChangeSchedule chs = new ChangeSchedule();
                                    chs.isToInWeek = false;
                                    chs.isFromInWeek = false;
                                    if (ans)
                                    {
                                        chs.isToInWeek = ans;
                                        chs.to = r.slotTo.ToString();
                                    }
                                    if (erp)
                                    {
                                        chs.froom = r.slotFrom.ToString();
                                        chs.isFromInWeek = erp;

                                    }
                                    toChangeSlots.Add(chs);
                                }

                            }
                        }
                    }
                }
                if (toChangeSlots.Count > 0)
                {
                    //change schedule here
                    String schedule = sd.details;
                    char[] sr = schedule.ToCharArray();
                    foreach (var item in toChangeSlots)
                    {
                        if (item.isFromInWeek)
                        {
                            sr[int.Parse(item.froom)] = '3';
                        }
                        if (item.isToInWeek)
                        {
                            sr[int.Parse(item.to)] = '4';
                        }
                    }
                    String sn = "";
                    for (int i = 0; i < sr.Length; i++)
                    {
                        sn = String.Concat(sn, sr[i]);
                    }
                    String details = sn;
                    return details;
                }
                return sd.details;
            }
            else
                return sd.details;
        }
        //Student Schedule
        public string GetStudentSchedule(String email)
        {
            Student u = db.Students.Where(s => s.email == email).FirstOrDefault();
            Schedule sd = db.Schedules.Where(ss => ss.scheduleid == u.scheduleid).FirstOrDefault();
            var tutorEnrolled = db.CourseEnrolleds.Where(s => s.studentemail == email).ToList();
            List<ChangeSchedule> toChangeSlots = new List<ChangeSchedule>();
            if (tutorEnrolled != null && tutorEnrolled.Count > 0)
            {
                foreach (var item in tutorEnrolled)
                {
                    var classReports = db.ClassReports.Where(cr => cr.courseenrolledid == item.id && cr.classesstatus == 2).ToList();
                    if (classReports != null && classReports.Count > 0)
                    {
                        foreach (var crItem in classReports)
                        {
                            NewReschedule r = db.NewReschedules.Where(rr => rr.classreportid == crItem.id && rr.rescheduledclassstatus == 0).FirstOrDefault();
                            if (r != null)
                            {
                                String cdate = GetCurrentDate();
                                String date = r.rescheduleclassTo;
                                String fromDate = r.rescheduleclassFrom;
                                String currentDate = cdate;
                                DateTime myDateTime = DateTime.ParseExact(currentDate, "MM/dd/yyyy", null);
                                DateTime myDateFrom = DateTime.ParseExact(fromDate, "MM/dd/yyyy", null);
                                DateTime myDateTime2 = DateTime.ParseExact(date, "MM/dd/yyyy", null);

                                DayOfWeek day = System.DayOfWeek.Monday;
                                bool ans = AreFallingInSameWeek(myDateTime, myDateTime2, day);
                                bool erp = AreFallingInSameWeek(myDateTime, myDateFrom, day);
                                if (ans || erp)
                                {
                                    ChangeSchedule chs = new ChangeSchedule();
                                    chs.isToInWeek = false;
                                    chs.isFromInWeek = false;
                                    if (ans)
                                    {
                                        chs.isToInWeek = ans;
                                        chs.to = r.slotTo.ToString();
                                    }
                                    if (erp)
                                    {
                                        chs.froom = r.slotFrom.ToString();
                                        chs.isFromInWeek = erp;

                                    }
                                    toChangeSlots.Add(chs);
                                }

                            }
                        }
                    }
                }
                if (toChangeSlots.Count > 0)
                {
                    //change schedule here
                    String schedule = sd.details;
                    char[] sr = schedule.ToCharArray();
                    foreach (var item in toChangeSlots)
                    {
                        if (item.isFromInWeek)
                        {
                            sr[int.Parse(item.froom)] = '3';
                        }
                        if (item.isToInWeek)
                        {
                            sr[int.Parse(item.to)] = '4';
                        }
                    }
                    String sn = "";
                    for (int i = 0; i < sr.Length; i++)
                    {
                        sn = String.Concat(sn, sr[i]);
                    }
                    String details = sn;
                    return details;
                }
                return sd.details;
            }
            else
                return sd.details;

        }
        [HttpGet]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public HttpResponseMessage GetTutorSchedule(String email)
        {
            try
            {
                Tutor u = db.Tutors.Where(s => s.email == email).FirstOrDefault();
                if (u.scheduleid == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, "Schedule Not Set");
                }
                //Task.Run(() => AutoCancelClass(email));
                StartCancelTimer(email);
                return Request.CreateResponse(HttpStatusCode.OK, GetTSchedule(email));
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, ex.Message);
            }
        }

        [HttpGet]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public HttpResponseMessage GetCoursesList(string email)
        {
            try
            {
                Tutor t = db.Tutors.Where(tt => tt.email == email).FirstOrDefault();
                if (t != null)
                {
                    int semesterNo = int.Parse(t.semester.ToString());
                    List<Course> semesterBelowCourses = new List<Course>();
                    for (int i = semesterNo - 1; i >= 1; i--)
                    {
                        var semesterCourses = db.SemesterCourses.Where(sc => sc.semesterNo == i).ToList();
                        foreach (var item in semesterCourses)
                        {
                            Course c = db.Courses.Where(cc => cc.courseid == item.courseid).FirstOrDefault();
                            semesterBelowCourses.Add(c);
                        }

                    }
                    bool check = false;
                    List<Course> listCourse = new List<Course>();
                    var enrolledCourses = from c in db.TutorCourseLists where c.tutoremail == email && c.type == 1 select new { c.courseid, c.tutoremail };
                    foreach (var c in semesterBelowCourses)
                    {
                        foreach (var item in enrolledCourses)
                        {
                            if (item.courseid == c.courseid)
                            {
                                check = true;
                            }
                        }
                        if (!check)
                        {
                            Course cs = new Course();
                            cs.coursecode = c.coursecode;
                            cs.coursefee = c.coursefee;
                            cs.courseid = c.courseid;
                            cs.coursename = c.coursename;
                            listCourse.Add(cs);
                        }
                        check = false;
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, listCourse.Select(s => new { s.courseid, s.coursename, s.coursefee, s.coursecode }).ToList());
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, "Tutor doesnot Exist");

                }

            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, e.Message);
            }
        }
        [HttpGet]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public HttpResponseMessage GetCourseGroup(string email, string coursename, int courseid)
        {
            try
            {
                Tutor t = db.Tutors.Where(tt => tt.email == email).FirstOrDefault();
                if (t != null)
                {
                    bool check = false;
                    int semesterNo = int.Parse(t.semester.ToString());
                    List<Course> semesterBelowCourses = new List<Course>();
                    for (int i = semesterNo - 1; i >= 1; i--)
                    {
                        var semesterCourses = db.SemesterCourses.Where(sc => sc.semesterNo == i).ToList();
                        foreach (var item in semesterCourses)
                        {
                            Course c = db.Courses.Where(cc => cc.courseid == item.courseid).FirstOrDefault();
                            semesterBelowCourses.Add(c);
                        }

                    }
                    List<Course> listCourse = new List<Course>();
                    var enrolledCourses = from c in db.TutorCourseLists where c.tutoremail == email && c.type == 1 select new { c.courseid, c.tutoremail };
                    foreach (var c in semesterBelowCourses)
                    {
                        foreach (var item in enrolledCourses)
                        {
                            if (item.courseid == c.courseid)
                            {
                                check = true;
                            }
                        }
                        if (!check)
                        {
                            Course cs = new Course();
                            cs.coursecode = c.coursecode;
                            cs.coursefee = c.coursefee;
                            cs.courseid = c.courseid;
                            cs.coursename = c.coursename;
                            listCourse.Add(cs);
                        }
                        check = false;
                    }
                    //listCourse all courses of tutor below his semester
                    SubjectGroup searchedGroupId = db.SubjectGroups.Where(s => s.coursename == coursename).FirstOrDefault();
                    List<Course> removed = new List<Course>();
                    foreach (var item in listCourse)
                    {
                        SubjectGroup sg = db.SubjectGroups.Where(sgg => sgg.courseid == item.courseid && sgg.groupid == searchedGroupId.groupid).FirstOrDefault();
                        if (sg != null)
                        {
                            removed.Add(item);
                        }
                    }
                    List<TutorCourseListCustom> courseList = new List<TutorCourseListCustom>();
                    List<TutorCourseListCustom> toSend = new List<TutorCourseListCustom>();

                    var group = db.SubjectGroups.Where(ss => ss.groupid == searchedGroupId.groupid).ToList();
                    var tlist = db.TutorCourseLists.Where(s => s.tutoremail == email).ToList();
                    foreach (var item in group)
                    {
                        foreach (var it in tlist)
                        {
                            if (item.courseid == it.courseid)
                            {
                                if (it.courseid == courseid)
                                {
                                    TutorCourseListCustom tt = new TutorCourseListCustom();
                                    tt.tutoremail = email;
                                    tt.courseid = courseid;
                                    tt.type = 1;
                                    tt.isSelected = true;
                                    tt.coursename = item.coursename;
                                    courseList.Add(tt);
                                }
                                else
                                {
                                    TutorCourseListCustom tt = new TutorCourseListCustom();
                                    tt.tutoremail = it.tutoremail;
                                    tt.courseid = int.Parse(it.courseid.ToString());
                                    tt.type = int.Parse(it.type.ToString());
                                    tt.isSelected = bool.Parse(it.isSelected);
                                    tt.coursename = item.coursename;
                                    courseList.Add(tt);
                                }
                                check = true;
                            }
                        }
                        if (!check)
                        {
                            TutorCourseListCustom tc = new TutorCourseListCustom();
                            tc.courseid = int.Parse(item.courseid.ToString());
                            tc.tutoremail = email;
                            tc.coursename = item.coursename;
                            if (item.courseid == courseid)
                            {
                                tc.type = 1;
                                tc.isSelected = true;

                            }
                            else
                            {
                                tc.type = 0;
                                tc.isSelected = false;

                            }
                            courseList.Add(tc);
                        }
                        check = false;
                    }
                    bool ischeck = false;
                    foreach (var item in courseList)
                    {
                        foreach (var rmitem in removed)
                        {
                            if (rmitem.courseid == item.courseid)
                            {
                                ischeck = true;
                            }
                        }
                        if (ischeck)
                        {
                            toSend.Add(item);
                        }
                        ischeck = false;
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, toSend.Select(s => new { s.courseid, s.isSelected, s.tutoremail, s.type, s.coursename }).ToList());

                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, "Tutor Doesnot Exist");

                }
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, e.Message);
            }
        }
        [HttpPost]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public HttpResponseMessage SaveCourseGroup(TutorCourseListCustom course)
        {
            try
            {


                TutorCourseList t = db.TutorCourseLists.Where(s => s.courseid == course.courseid && s.tutoremail == course.tutoremail).FirstOrDefault();
                if (t == null)
                {
                    TutorCourseList addCourse = new TutorCourseList();
                    addCourse.courseid = course.courseid;
                    addCourse.tutoremail = course.tutoremail;
                    addCourse.type = course.type;
                    addCourse.isSelected = course.isSelected.ToString();
                    db.TutorCourseLists.Add(addCourse);
                    db.SaveChanges();
                }
                else
                {
                    t.type = course.type;
                    t.isSelected = course.isSelected.ToString();
                    db.SaveChanges();
                }
                return Request.CreateResponse(HttpStatusCode.OK, "Course Enlisted Successfully");
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, ex.Message);
            }
        }
        //[HttpPost]
        //[EnableCors(origins: "*", headers: "*", methods: "*")]
        //public HttpResponseMessage TutorCourseEnlist(String email, int cid)
        //{
        //    try
        //    {
        //        Tutor st = db.Tutors.Where(s => s.email == email).FirstOrDefault();
        //        if (st != null)
        //        {
        //            var o = db.TutorCourseLists.Where(q => q.tutoremail == email && q.courseid == cid).FirstOrDefault();
        //            if (o == null)
        //            {
        //                TutorCourseList s = new TutorCourseList();
        //                s.courseid = cid;
        //                s.tutoremail = email;
        //                s.isSelected = grade;
        //                db.TutorCourseLists.Add(s);
        //                db.SaveChanges();
        //                return Request.CreateResponse(HttpStatusCode.OK, "Course Enlisted Successfully");
        //            }
        //            else
        //            {
        //                return Request.CreateResponse(HttpStatusCode.OK, "Course Already Enlisted");

        //            }
        //        }
        //        else
        //        {
        //            return Request.CreateResponse(HttpStatusCode.OK, "Student doesnot exist");

        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.OK, ex.Message);
        //    }
        //}

        [HttpGet]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public HttpResponseMessage GetTutorEnlistedCourses(String email)
        {
            try
            {
                List<Course> courseList = new List<Course>();
                Tutor st = db.Tutors.Where(s => s.email == email).FirstOrDefault();
                if (st != null)
                {
                    var cl = from c in db.TutorCourseLists where c.tutoremail == email && c.type == 1 select new { c.courseid };
                    foreach (var item in cl)
                    {

                        var courseL = db.Courses.Where(c => c.courseid == item.courseid).FirstOrDefault();
                        courseList.Add(courseL);

                    }
                    return Request.CreateResponse(HttpStatusCode.OK, courseList.Select(e => new { e.courseid, e.coursename, e.coursefee }).ToList());
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, "No Course Enlisted");

                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, ex.Message);
            }
        }
        [HttpGet]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public HttpResponseMessage GetStudentRequests(String email)
        {
            try
            {
                var requestList = db.StudentRequests.Where(sr => sr.tutoremail == email && sr.studentrequeststatus == 0).ToList();
                if (requestList != null)
                {
                    var courses = requestList.Select(c => c.courseid).Distinct().ToList();
                    var allCourses = db.Courses.Select(s => s).ToList();
                    List<StudentRequestCustom> stReq = new List<StudentRequestCustom>();
                    foreach (var item in requestList)
                    {
                        foreach (var alItem in allCourses)
                        {
                            if (alItem.courseid == item.courseid)
                            {
                                Student s = db.Students.Where(ss => ss.email == item.studentemail).FirstOrDefault();
                                StudentRequestCustom sr = new StudentRequestCustom();
                                sr.courseid = item.courseid;
                                sr.studentemail = item.studentemail;
                                sr.tutoremail = item.tutoremail;
                                sr.coursename = alItem.coursename;
                                sr.slot = item.slot;
                                sr.studentname = s.name;
                                sr.enrollDate = item.dateToBeEnrolled;
                                stReq.Add(sr);
                            }
                        }
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, stReq);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, "No Requests");

                }

            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

        }
        //getting request to change enrolled schedule
        [HttpGet]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public HttpResponseMessage AcceptEnrollScheduleChangeRequest(String tutoremail, String studentemail, int courseid, String slotFrom, String slotTo, String enrollDate)
        {
            try
            {
                DateTime eDate = DateTime.ParseExact(enrollDate, "MM/dd/yyyy", null);
                DateTime currentDate = DateTime.ParseExact(GetCurrentDate(), "MM/dd/yyyy", null);
                Student s = db.Students.Where(ss => ss.email == studentemail).FirstOrDefault();
                if (currentDate >= eDate)
                {
                    var scr = db.ScheduleChangeRequests.Where(ssr => ssr.tutoremail == tutoremail && ssr.slotTo == slotTo && ssr.status==1 || ssr.status==2).ToList();
                    if (scr != null && scr.Count>0)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, "Cannot change schedule.");
                    }
                    else
                    {
                        ChangingEnroll(tutoremail, studentemail, courseid, slotFrom, slotTo, enrollDate);
                        ScheduleChangeRequest sr = new ScheduleChangeRequest();
                        sr.studentemail = studentemail;
                        sr.tutoremail = tutoremail;
                        sr.courseid = courseid;
                        sr.slotFrom = slotFrom;
                        sr.slotTo = slotTo;
                        sr.enrollDate = enrollDate;
                        sr.status = 2;
                        db.ScheduleChangeRequests.Add(sr);
                        db.SaveChanges();
                        tutorNotification t = new tutorNotification();
                        t.email = tutoremail;
                        t.notificationType = 0;
                        t.isRead = 0;
                        t.notificationMsg = "Your request for swapping schedule has been accepted by" + s.name;
                        t.notificationReply = "";
                        db.tutorNotifications.Add(t);
                        db.SaveChanges();
                        return Request.CreateResponse(HttpStatusCode.OK, "Course Schedule Updated");
                    }
                }
                else
                {
                    
                    ScheduleChangeRequest sr = new ScheduleChangeRequest();
                    sr.studentemail = studentemail;
                    sr.tutoremail = tutoremail;
                    sr.courseid = courseid;
                    sr.slotFrom = slotFrom;
                    sr.slotTo = slotTo;
                    sr.enrollDate = enrollDate;
                    sr.status = 1;
                    db.ScheduleChangeRequests.Add(sr);
                    db.SaveChanges();
                    tutorNotification t = new tutorNotification();
                    t.email = tutoremail;
                    t.notificationType = 0;
                    t.isRead = 0;
                    t.notificationMsg = "Your request for swapping schedule has been accepted by" + s.name;
                    t.notificationReply = "";
                    db.tutorNotifications.Add(t);
                    db.SaveChanges();
                    return Request.CreateResponse(HttpStatusCode.OK, "Request accepted successfully. Your schedule will change on " + enrollDate);

                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, ex.Message);
            }
        }
        //changing enrooll schedule
        public void ChangingEnroll(String tutoremail, String studentemail, int courseid, String slotFrom, String slotTo, String enrollDate)
        {
            try
            {
                string[] toArr = slotTo.Split(',');
                string[] fromArr = slotFrom.Split(',');
                //getting schedule
                Student std = db.Students.Where(st => st.email == studentemail).FirstOrDefault();
                Tutor tt = db.Tutors.Where(t => t.email == tutoremail).FirstOrDefault();
                Schedule sSchedule = db.Schedules.Where(ss => ss.scheduleid == std.scheduleid).FirstOrDefault();
                Schedule tSchedule = db.Schedules.Where(ss => ss.scheduleid == tt.scheduleid).FirstOrDefault();

                //updating student schedule
                String s = sSchedule.details;
                char[] sr = s.ToCharArray();
                int ival = 0;
                int toval = 0;
                for (int i = 0; i < toArr.Length - 1; i++)
                {
                    ival = int.Parse(toArr[i].ToString());
                    toval = int.Parse(fromArr[i].ToString());
                    sr[ival] = '2';
                    sr[toval] = '1';
                }
                String sn = "";
                for (int i = 0; i < s.Length; i++)
                {
                    sn = String.Concat(sn, sr[i]);
                }
                sSchedule.details = sn;
                db.SaveChanges();

                ////updating tutor schedule
                String ts = tSchedule.details;
                char[] tsr = ts.ToCharArray();
                int tval = 0;
                int fval = 0;
                for (int i = 0; i < toArr.Length - 1; i++)
                {
                    tval = int.Parse(toArr[i].ToString());
                    fval = int.Parse(fromArr[i].ToString());
                    tsr[tval] = '2';
                    tsr[fval] = '1';
                }
                String tsn = "";
                for (int i = 0; i < s.Length; i++)
                {
                    tsn = String.Concat(tsn, tsr[i]);
                }
                tSchedule.details = tsn;
                db.SaveChanges();
                ////saving to enrolled table schedule
                CourseEnrolled ce = db.CourseEnrolleds.Where(cee => cee.tutoremail == tutoremail && cee.studentemail == studentemail && cee.courseid == courseid && cee.coursestatus == 0).FirstOrDefault();
                if (ce != null)
                {
                    String se = ce.schedule;
                    char[] esr = se.ToCharArray();
                    int ffval = 0;
                    int ttval = 0;
                    for (int i = 0; i < toArr.Length - 1; i++)
                    {
                        ttval = int.Parse(toArr[i].ToString());
                        ffval = int.Parse(fromArr[i].ToString());
                        esr[ttval] = '2';
                        esr[ffval] = '0';
                    }
                    String esn = "";
                    for (int i = 0; i < s.Length; i++)
                    {
                        esn = String.Concat(esn, esr[i]);
                    }
                    ce.schedule = esn;
                    db.SaveChanges();
                }
                CourseEnrolled edb = db.CourseEnrolleds.Where(ee => ee.studentemail == studentemail && ee.tutoremail == tutoremail && ee.courseid == courseid && ee.coursestatus != 1).FirstOrDefault();
                Course ccc = db.Courses.Where(css => css.courseid == courseid).FirstOrDefault();
                studentNotification sno = new studentNotification();
                sno.email = studentemail;
                sno.notificationType = 0;
                sno.isRead = 0;
                sno.notificationMsg = "Your schedule has been changed for course "+ ccc.coursename + ".";
                sno.notificationReply = "";
                db.studentNotifications.Add(sno);
                db.SaveChanges();
                tutorNotification tn = new tutorNotification();
                tn.email = tutoremail;
                tn.notificationType = 0;
                tn.isRead = 0;
                tn.notificationMsg = "Your schedule has been changed for course " + ccc.coursename + ".";
                tn.notificationReply = "";
                db.tutorNotifications.Add(tn);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
            }

        }

        [HttpPost]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public HttpResponseMessage AcceptStudentRequest(String tutoremail, String studentemail, int courseid, String slot, String enrollDate)
        {
            try
            {
                var req = db.StudentRequests.Where(s => s.studentemail == studentemail && s.tutoremail == tutoremail && s.courseid == courseid).FirstOrDefault();
                if (req != null && req.studentrequeststatus != 1)
                {
                    req.studentrequeststatus = 1;
                    db.SaveChanges();
                    var todismiss = db.StudentRequests.Where(ss => ss.tutoremail == tutoremail && ss.courseid == courseid && ss.slot == slot && ss.studentrequeststatus != 1).ToList();
                    foreach (var item in todismiss)
                    {
                        db.StudentRequests.Remove(item);
                    }
                    db.SaveChanges();
                    string[] alotArr = slot.Split(',');

                    Student std = db.Students.Where(st => st.email == studentemail).FirstOrDefault();
                    Tutor tt = db.Tutors.Where(t => t.email == tutoremail).FirstOrDefault();
                    Schedule sSchedule = db.Schedules.Where(ss => ss.scheduleid == std.scheduleid).FirstOrDefault();
                    Schedule tSchedule = db.Schedules.Where(ss => ss.scheduleid == tt.scheduleid).FirstOrDefault();

                    //updating student schedule
                    String s = sSchedule.details;
                    char[] sr = s.ToCharArray();
                    int ival = 0;
                    foreach (var item in alotArr)
                    {
                        ival = int.Parse(item.ToString());
                        sr[ival - 1] = '2';
                    }
                    String sn = "";
                    for (int i = 0; i < s.Length; i++)
                    {
                        sn = String.Concat(sn, sr[i]);
                    }
                    sSchedule.details = sn;
                    db.SaveChanges();

                    ////updating tutor schedule
                    String ts = tSchedule.details;
                    char[] tsr = ts.ToCharArray();
                    int tval = 0;
                    foreach (var item in alotArr)
                    {
                        tval = int.Parse(item.ToString());
                        tsr[tval - 1] = '2';
                    }
                    String tsn = "";
                    for (int i = 0; i < s.Length; i++)
                    {
                        tsn = String.Concat(tsn, tsr[i]);
                    }
                    tSchedule.details = tsn;
                    db.SaveChanges();
                    ////saving to enrolled table schedule
                    String se = "0000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000";
                    char[] esr = se.ToCharArray();
                    int eval = 0;
                    foreach (var item in alotArr)
                    {
                        eval = int.Parse(item.ToString());
                        esr[eval - 1] = '2';
                    }
                    String esn = "";
                    for (int i = 0; i < s.Length; i++)
                    {
                        esn = String.Concat(esn, esr[i]);
                    }

                    ////Adding to Course Enrolled

                    CourseEnrolled e = new CourseEnrolled();
                    e.tutoremail = tutoremail;
                    e.studentemail = studentemail;
                    e.courseid = courseid;
                    e.schedule = esn;
                    e.coursestatus = 0;
                    if (enrollDate != null)
                    {
                        DateTime date = DateTime.ParseExact(enrollDate, "MM/dd/yyyy", null);
                        date = date.AddDays(1);
                        string newDateString = date.ToString("MM/dd/yyyy");
                        e.date = newDateString;
                    }
                    else
                    {
                        e.date = GetCurrentDate();
                    }
                    db.CourseEnrolleds.Add(e);
                    db.SaveChanges();
                    CourseEnrolled edb = db.CourseEnrolleds.Where(ee => ee.studentemail == studentemail && ee.tutoremail == tutoremail && ee.courseid == courseid && ee.coursestatus != 1).FirstOrDefault();
                    Course ccc = db.Courses.Where(css => css.courseid == courseid).FirstOrDefault();
                    studentNotification sno = new studentNotification();
                    sno.email = studentemail;
                    sno.notificationType = 0;
                    sno.isRead = 0;
                    sno.notificationMsg = "Your Request is Accepted by tutor " + tt.name + " for course " + ccc.coursename + ".";
                    sno.notificationReply = "";
                    db.studentNotifications.Add(sno);
                    db.SaveChanges();
                    return Request.CreateResponse(HttpStatusCode.OK, "Request Accepted Successfully");
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, "No Requests");

                }

            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

        }
        //task
        [HttpPost]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public HttpResponseMessage AcceptStudentRequestTemp(String tutoremail, String studentemail, int courseid, String slot, String enrollDate,String endDate)
        {
            try
            {
                var req = db.StudentRequestTemporaryEnrollments.Where(s => s.studentemail == studentemail && s.tutoremail == tutoremail && s.courseid == courseid).FirstOrDefault();
                if (req != null && req.studentrequeststatus != 1)
                {
                    req.studentrequeststatus = 1;
                    db.SaveChanges();
                    var todismiss = db.StudentRequestTemporaryEnrollments.Where(ss => ss.tutoremail == tutoremail && ss.courseid == courseid && ss.slot == slot && ss.studentrequeststatus != 1).ToList();
                    foreach (var item in todismiss)
                    {
                        db.StudentRequestTemporaryEnrollments.Remove(item);
                    }
                    db.SaveChanges();
                    string[] alotArr = slot.Split(',');

                    Student std = db.Students.Where(st => st.email == studentemail).FirstOrDefault();
                    Tutor tt = db.Tutors.Where(t => t.email == tutoremail).FirstOrDefault();
                    Schedule sSchedule = db.Schedules.Where(ss => ss.scheduleid == std.scheduleid).FirstOrDefault();
                    Schedule tSchedule = db.Schedules.Where(ss => ss.scheduleid == tt.scheduleid).FirstOrDefault();

                    //updating student schedule
                    String s = sSchedule.details;
                    char[] sr = s.ToCharArray();
                    int ival = 0;
                    foreach (var item in alotArr)
                    {
                        ival = int.Parse(item.ToString());
                        sr[ival - 1] = '9';
                    }
                    String sn = "";
                    for (int i = 0; i < s.Length; i++)
                    {
                        sn = String.Concat(sn, sr[i]);
                    }
                    sSchedule.details = sn;
                    db.SaveChanges();

                    ////updating tutor schedule
                    String ts = tSchedule.details;
                    char[] tsr = ts.ToCharArray();
                    int tval = 0;
                    foreach (var item in alotArr)
                    {
                        tval = int.Parse(item.ToString());
                        tsr[tval - 1] = '9';
                    }
                    String tsn = "";
                    for (int i = 0; i < s.Length; i++)
                    {
                        tsn = String.Concat(tsn, tsr[i]);
                    }
                    tSchedule.details = tsn;
                    db.SaveChanges();
                    ////saving to enrolled table schedule
                    String se = "0000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000";
                    char[] esr = se.ToCharArray();
                    int eval = 0;
                    foreach (var item in alotArr)
                    {
                        eval = int.Parse(item.ToString());
                        esr[eval - 1] = '9';
                    }
                    String esn = "";
                    for (int i = 0; i < s.Length; i++)
                    {
                        esn = String.Concat(esn, esr[i]);
                    }

                    ////Adding to Course Enrolled

                    TemporaryEnrolled e = new TemporaryEnrolled();
                    e.tutoremail = tutoremail;
                    e.studentemail = studentemail;
                    e.courseid = courseid;
                    e.schedule = esn;
                    e.coursestatus = 0;
                    if (enrollDate != null)
                    {
                        DateTime date = DateTime.ParseExact(enrollDate, "MM/dd/yyyy", null);
                        string newDateString = date.ToString("MM/dd/yyyy");
                        e.date = newDateString;
                    }
                    else
                    {
                        e.date = GetCurrentDate();
                    }
                    e.dateToEnd = endDate;
                    db.TemporaryEnrolleds.Add(e);
                    db.SaveChanges();
                    TemporaryEnrolled edb = db.TemporaryEnrolleds.Where(ee => ee.studentemail == studentemail && ee.tutoremail == tutoremail && ee.courseid == courseid && ee.coursestatus != 1).FirstOrDefault();
                    Course ccc = db.Courses.Where(css => css.courseid == courseid).FirstOrDefault();
                    studentNotification sno = new studentNotification();
                    sno.email = studentemail;
                    sno.notificationType = 0;
                    sno.isRead = 0;
                    sno.notificationMsg = "Your Request for primary tuition is Accepted by tutor " + tt.name + " for course " + ccc.coursename + ".";
                    sno.notificationReply = "";
                    db.studentNotifications.Add(sno);
                    db.SaveChanges();
                    return Request.CreateResponse(HttpStatusCode.OK, "Request Accepted Successfully");
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, "No Requests");

                }

            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

        }
        [HttpPost]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public HttpResponseMessage RejectStudentRequestTemp(String tutoremail, String studentemail, int courseid, String slot, String enrollDate, String endDate)
        {
            try
            {

                StudentRequestTemporaryEnrollment strequest = db.StudentRequestTemporaryEnrollments.Where(s => s.tutoremail == tutoremail && s.studentemail == studentemail && s.courseid == courseid).FirstOrDefault();
                if (strequest != null)
                {
                    strequest.studentrequeststatus = 2;
                    db.SaveChanges();
                    Course ccc = db.Courses.Where(css => css.courseid == courseid).FirstOrDefault();
                    Tutor tt = db.Tutors.Where(t => t.email == tutoremail).FirstOrDefault();
                    studentNotification sno = new studentNotification();
                    sno.email = studentemail;
                    sno.notificationType = 0;
                    sno.isRead = 0;
                    sno.notificationMsg = "Your Request for primary tuition is Rejected by tutor " + tt.name + " for course " + ccc.coursename + ".";
                    sno.notificationReply = "";
                    db.studentNotifications.Add(sno);
                    db.SaveChanges();
                    return Request.CreateResponse(HttpStatusCode.OK, "Request Rejected Successfully");
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, "No Course Enlisted");

                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, ex.Message);
            }
        }
        //
        //
        [HttpPost]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public HttpResponseMessage RejectStudentRequest(String tutoremail, String studentemail, int courseid, String slot)
        {
            try
            {

                StudentRequest strequest = db.StudentRequests.Where(s => s.tutoremail == tutoremail && s.studentemail == studentemail && s.courseid == courseid).FirstOrDefault();
                if (strequest != null)
                {
                    strequest.studentrequeststatus = 2;
                    db.SaveChanges();
                    Course ccc = db.Courses.Where(css => css.courseid == courseid).FirstOrDefault();
                    Tutor tt = db.Tutors.Where(t => t.email == tutoremail).FirstOrDefault();
                    studentNotification sno = new studentNotification();
                    sno.email = studentemail;
                    sno.notificationType = 0;
                    sno.isRead = 0;
                    sno.notificationMsg = "Your Request is Rejected by tutor " + tt.name + " for course " + ccc.coursename + ".";
                    sno.notificationReply = "";
                    db.studentNotifications.Add(sno);
                    db.SaveChanges();
                    return Request.CreateResponse(HttpStatusCode.OK, "Request Rejected Successfully");
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, "No Course Enlisted");

                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, ex.Message);
            }
        }

        [HttpGet]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public HttpResponseMessage TodayClasses(String email)
        {
            try
            {
                var cList = db.CourseEnrolleds.Where(s => s.tutoremail == email && s.coursestatus == 0).ToList();
                var tempList = db.TemporaryEnrolleds.Where(t => t.tutoremail == email && t.coursestatus == 0).ToList();
                if (cList != null || tempList!=null)
                {
                    String currentDay = DateTime.Now.DayOfWeek.ToString();
                    List<String> classes = new List<string>();
                    List<TodayClasses> tclass = new List<TodayClasses>();
                    if(cList!=null && cList.Count > 0)
                    {
                        foreach (var item in cList)
                        {
                            DateTime dateee = DateTime.ParseExact(item.date, "MM/dd/yyyy", null);
                            DateTime todayDate = DateTime.ParseExact(GetCurrentDate(), "MM/dd/yyyy", null);
                            if (todayDate >= dateee)
                            {
                                List<String> slots = new List<string>();
                                List<int> newList = new List<int>();

                                String schedule = item.schedule;
                                char[] sr = schedule.ToCharArray();
                                for (int i = 0; i < sr.Length; i++)
                                {
                                    if (sr[i] == '2')
                                    {
                                        newList.Add(i);
                                    }
                                }
                                if (newList != null)
                                {
                                    foreach (var Citem in newList)
                                    {
                                        String data = slotCondition(Citem);
                                        slots.Add(data);
                                    }
                                    foreach (var dateitem in slots)
                                    {
                                        String currentDate = GetCurrentDate();
                                        String[] date = dateitem.Split(' ');
                                        var ccc = db.ClassReports.Where(crdb => crdb.classslot == dateitem && crdb.courseenrolledid == item.id && crdb.classesstatus == 2).ToList();
                                        if (ccc != null && ccc.Count > 0)
                                        {
                                            foreach (var ccitem in ccc)
                                            {
                                                NewReschedule res = db.NewReschedules.Where(r => r.classreportid == ccitem.id && r.rescheduledclassstatus == 0 && r.rescheduleclassTo == currentDate).FirstOrDefault();
                                                if (res != null)
                                                {
                                                    TodayClasses t = new TodayClasses();
                                                    t.slot = dateitem;
                                                    Student std = db.Students.Where(ss => ss.email == item.studentemail).FirstOrDefault();
                                                    t.name = std.name;
                                                    Course cc = db.Courses.Where(sc => sc.courseid == item.courseid).FirstOrDefault();
                                                    t.coursename = cc.coursename;
                                                    if (res.classStatus == 0)
                                                    {
                                                        t.isReschedule = true;
                                                        t.isPreSchedule = false;
                                                        t.isStudent = false;
                                                    }
                                                    else if (res.classStatus == 2)
                                                    {
                                                        t.isReschedule = false;
                                                        t.isPreSchedule = true;
                                                        t.isStudent = false;
                                                    }
                                                    else if (res.classStatus == 7)
                                                    {
                                                        t.isReschedule = false;
                                                        t.isPreSchedule = false;
                                                        t.isStudent = true;
                                                    }
                                                    t.classDate = currentDate;
                                                    t.semail = item.studentemail;
                                                    t.temail = item.tutoremail;
                                                    t.isTemp = false;
                                                    tclass.Add(t);
                                                }
                                            }
                                        }
                                        if (date[3] == currentDay)
                                        {
                                            int enrollId = item.id;
                                            ClassReport cReport = db.ClassReports.Where(cr => cr.courseenrolledid == enrollId && cr.classdate == currentDate && cr.classslot == dateitem).FirstOrDefault();
                                            if (cReport == null)
                                            {
                                                TodayClasses t = new TodayClasses();
                                                t.slot = dateitem;
                                                Student std = db.Students.Where(ss => ss.email == item.studentemail).FirstOrDefault();
                                                t.name = std.name;
                                                Course cc = db.Courses.Where(sc => sc.courseid == item.courseid).FirstOrDefault();
                                                t.coursename = cc.coursename;
                                                t.isPreSchedule = false;
                                                t.isReschedule = false;
                                                t.isStudent = false;
                                                t.classDate = currentDate;
                                                t.semail = item.studentemail;
                                                t.temail = item.tutoremail;
                                                t.isTemp = false;
                                                tclass.Add(t);
                                            }
                                            else
                                            {
                                                if (cReport.classesstatus == 2)
                                                {
                                                    NewReschedule rschedule = db.NewReschedules.Where(rs => rs.classreportid == cReport.id && rs.rescheduleclassTo == currentDate && rs.rescheduledclassstatus != 1).FirstOrDefault();
                                                    if (rschedule != null)
                                                    {
                                                        TodayClasses t = new TodayClasses();
                                                        t.slot = dateitem;
                                                        Student std = db.Students.Where(ss => ss.email == item.studentemail).FirstOrDefault();
                                                        t.name = std.name;
                                                        Course cc = db.Courses.Where(sc => sc.courseid == item.courseid).FirstOrDefault();
                                                        t.coursename = cc.coursename;
                                                        if (rschedule.classStatus == 0)
                                                        {
                                                            t.isReschedule = true;
                                                            t.isPreSchedule = false;
                                                            t.isStudent = false;

                                                        }
                                                        else if (rschedule.classStatus == 2)
                                                        {
                                                            t.isReschedule = false;
                                                            t.isPreSchedule = true;
                                                            t.isStudent = false;
                                                        }
                                                        else if (rschedule.classStatus == 7)
                                                        {
                                                            t.isReschedule = false;
                                                            t.isPreSchedule = false;
                                                            t.isStudent = true;
                                                        }
                                                        t.classDate = currentDate;
                                                        t.semail = item.studentemail;
                                                        t.temail = item.tutoremail;
                                                        t.isTemp = false;
                                                        tclass.Add(t);
                                                    }
                                                }
                                            }

                                        }

                                    }

                                }
                            }


                        }
                    }
                    if(tempList!=null || tempList.Count > 0)
                    {
                        foreach (var item in tempList)
                        {
                            DateTime dateee = DateTime.ParseExact(item.date, "MM/dd/yyyy", null);
                            DateTime todayDate = DateTime.ParseExact(GetCurrentDate(), "MM/dd/yyyy", null);
                            if (todayDate >= dateee)
                            {
                                List<String> slots = new List<string>();
                                List<int> newList = new List<int>();

                                String schedule = item.schedule;
                                char[] sr = schedule.ToCharArray();
                                for (int i = 0; i < sr.Length; i++)
                                {
                                    if (sr[i] == '2')
                                    {
                                        newList.Add(i);
                                    }
                                }
                                if (newList != null)
                                {
                                    foreach (var Citem in newList)
                                    {
                                        String data = slotCondition(Citem);
                                        slots.Add(data);
                                    }
                                    foreach (var dateitem in slots)
                                    {
                                        String currentDate = GetCurrentDate();
                                        String[] date = dateitem.Split(' ');
                                        if (date[3] == currentDay)
                                        {
                                            int enrollId = item.id;
                                            TempTakeClass1 cReport = db.TempTakeClass1.Where(cr => cr.tempId == enrollId && cr.classdate == currentDate && cr.classslot == dateitem).FirstOrDefault();
                                            if (cReport == null)
                                            {
                                                TodayClasses t = new TodayClasses();
                                                t.slot = dateitem;
                                                Student std = db.Students.Where(ss => ss.email == item.studentemail).FirstOrDefault();
                                                t.name = std.name;
                                                Course cc = db.Courses.Where(sc => sc.courseid == item.courseid).FirstOrDefault();
                                                t.coursename = cc.coursename;
                                                t.isPreSchedule = false;
                                                t.isReschedule = false;
                                                t.isStudent = false;
                                                t.classDate = currentDate;
                                                t.semail = item.studentemail;
                                                t.temail = item.tutoremail;
                                                t.isTemp = true;
                                                tclass.Add(t);
                                            }


                                        }

                                    }

                                }
                            }


                        }
                    }
                    //getting temp classes
                    if (tclass != null)
                    {

                        return Request.CreateResponse(HttpStatusCode.OK, tclass);

                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, "No Classes Today");

                    }
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, "No Course Enrolled");

                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, ex.Message);
            }
        }
        public bool GetTimeNow(String timeFrom)
        {
            String[] tsplit = timeFrom.Split(':');
            int thour = int.Parse(tsplit[0]);
            int hoursNow = int.Parse(DateTime.Now.TimeOfDay.Hours.ToString());
            if (thour <= hoursNow)
            {
                return true;
            }
            else
            {
                return false;
            }


        }
        public String GetCurrentDate()
        {
            String day = DateTime.Now.Date.Day.ToString();
            String month = DateTime.Now.Date.Month.ToString();
            String year = DateTime.Now.Date.Year.ToString();
            if (int.Parse(month) < 10)
            {
                month = "0" + month;
            }
            if (int.Parse(day) < 10)
            {
                day = "0" + day;
            }
            String date = month + "/" + day + "/" + year;
            return date;
        }

        [HttpPost]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public HttpResponseMessage ClassReportTake(String email, String coursename, String semail, String slot, bool isReschedule, bool isPreSchedule, bool isStudent, bool isTemp,String classDate)
        {
            try
            {
                Course course = db.Courses.Where(c => c.coursename == coursename).FirstOrDefault();
                CourseEnrolled courseenrolled = db.CourseEnrolleds.Where(ce => ce.tutoremail == email && ce.studentemail == semail && ce.courseid == course.courseid && ce.coursestatus == 0).FirstOrDefault();
                if (courseenrolled != null)
                {
                    String[] slotSplit = slot.Split(' ');
                    bool isGreater = GetTimeNow(slotSplit[0]);
                    if (isGreater)
                    {
                        if (!isReschedule && !isPreSchedule && !isStudent)
                        {
                            ClassReport cr = new ClassReport();
                            cr.courseenrolledid = courseenrolled.id;
                            cr.classesstatus = 1;
                            cr.classslot = slot;
                            cr.classdate = classDate;
                            cr.classTakenDate = GetCurrentDate();
                            String hour = DateTime.Now.TimeOfDay.Hours.ToString();
                            String minute = DateTime.Now.TimeOfDay.Minutes.ToString();
                            String stamp = "AM";
                            if (int.Parse(hour) <= 12)
                            {
                                stamp = "AM";
                            }
                            else
                            {
                                stamp = "PM";
                            }
                            if (int.Parse(hour) < 10)
                            {
                                hour = "0" + hour;
                            }
                            cr.classtime = hour + ":" + minute + " " + stamp;
                            db.ClassReports.Add(cr);
                            db.SaveChanges();
                            Feenew f = db.Feenews.Where(ff => ff.courseenrolledid == courseenrolled.id).FirstOrDefault();
                            Tutor tt = db.Tutors.Where(t => t.email == email).FirstOrDefault();
                            String sems = "Semester " + tt.semester.ToString();
                            SubjectFeeGroupName sfg = db.SubjectFeeGroupNames.Where(sf => sf.groupName == sems).FirstOrDefault();
                            if (f == null)
                            {
                                Feenew ff = new Feenew();
                                ff.courseenrolledid = courseenrolled.id;
                                ff.totalamount = sfg.fee;
                                ff.remainingamount = sfg.fee;
                                ff.paidamount = 0;
                                ff.status = "Unpaid";
                                db.Feenews.Add(ff);
                                db.SaveChanges();
                            }
                            else
                            {
                                f.totalamount = int.Parse(f.totalamount.ToString()) + sfg.fee;
                                f.remainingamount = int.Parse(f.remainingamount.ToString()) + sfg.fee;

                            }
                            db.SaveChanges();
                            return Request.CreateResponse(HttpStatusCode.OK, "Class Taken Successfully");
                        }
                        else
                        {
                            var cr = db.ClassReports.Where(c => c.courseenrolledid == courseenrolled.id && c.classesstatus == 2 && c.classslot == slot).ToList();
                            if (cr != null && cr.Count > 0)
                            {
                                string datee = GetCurrentDate();
                                foreach (var item in cr)
                                {
                                    NewReschedule rr = db.NewReschedules.Where(res => res.classreportid == item.id && res.rescheduleclassTo == datee && res.rescheduledclassstatus == 0).FirstOrDefault();
                                    if (rr != null)
                                    {
                                        if (isReschedule)
                                        {
                                            rr.rescheduledclassstatus = 1;
                                            rr.classStatus = 5;
                                        }
                                        else if (isPreSchedule)
                                        {
                                            rr.rescheduledclassstatus = 1;
                                            rr.classStatus = 3;
                                        }
                                        else
                                        {
                                            rr.rescheduledclassstatus = 1;
                                            rr.classStatus = 8;
                                        }
                                        Feenew f = db.Feenews.Where(ff => ff.courseenrolledid == courseenrolled.id).FirstOrDefault();
                                        Tutor tt = db.Tutors.Where(t => t.email == email).FirstOrDefault();
                                        String sems = "Semester " + tt.semester.ToString();
                                        SubjectFeeGroupName sfg = db.SubjectFeeGroupNames.Where(sf => sf.groupName == sems).FirstOrDefault();
                                        if (f == null)
                                        {
                                            Feenew ff = new Feenew();
                                            ff.courseenrolledid = courseenrolled.id;
                                            ff.totalamount = sfg.fee;
                                            ff.remainingamount = sfg.fee;
                                            ff.paidamount = 0;
                                            ff.status = "Unpaid";
                                            db.Feenews.Add(ff);
                                            db.SaveChanges();
                                        }
                                        else
                                        {
                                            f.totalamount = int.Parse(f.totalamount.ToString()) + sfg.fee;
                                            f.remainingamount = int.Parse(f.remainingamount.ToString()) + sfg.fee;
                                        }
                                        db.SaveChanges();
                                    }
                                }
                            }
                            return Request.CreateResponse(HttpStatusCode.OK, "Class Taken Successfully");

                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, "Class cannot be taken before its time");

                    }
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, "No Course Enrolled");
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, ex.Message);
            }
        }

        //

        [HttpPost]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public HttpResponseMessage ClassReportTakeTemp(String email, String coursename, String semail, String slot, bool isReschedule, bool isPreSchedule, bool isStudent,bool isTemp,String classDate)
        {
            try
            {
                Course course = db.Courses.Where(c => c.coursename == coursename).FirstOrDefault();
                TemporaryEnrolled courseenrolled = db.TemporaryEnrolleds.Where(ce => ce.tutoremail == email && ce.studentemail == semail && ce.courseid == course.courseid && ce.coursestatus == 0).FirstOrDefault();
                if (courseenrolled != null)
                {
                    String[] slotSplit = slot.Split(' ');
                    bool isGreater = GetTimeNow(slotSplit[0]);
                    if (isGreater)
                    {
                            TempTakeClass1 cr = new TempTakeClass1();
                            cr.tempId = courseenrolled.id;
                            cr.classesstatus = 1;
                            cr.classslot = slot;
                            cr.classdate = classDate;
                            cr.classTakenDate = GetCurrentDate();
                            String hour = DateTime.Now.TimeOfDay.Hours.ToString();
                            String minute = DateTime.Now.TimeOfDay.Minutes.ToString();
                            String stamp = "AM";
                            if (int.Parse(hour) <= 12)
                            {
                                stamp = "AM";
                            }
                            else
                            {
                                stamp = "PM";
                            }
                            if (int.Parse(hour) < 10)
                            {
                                hour = "0" + hour;
                            }
                            cr.classtime = hour + ":" + minute + " " + stamp;
                            db.TempTakeClass1.Add(cr);
                            db.SaveChanges();
                            return Request.CreateResponse(HttpStatusCode.OK, "Class Taken Successfully");
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, "Class cannot be taken before its time");

                    }
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, "No Course Enrolled");
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, ex.Message);
            }
        }


        //
        [HttpGet]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public HttpResponseMessage GetTeachingStudents(String temail)
        {
            try
            {
                var enrolled = db.CourseEnrolleds.Where(t => t.tutoremail == temail && t.coursestatus == 0).ToList();
                if (enrolled != null && enrolled.Count > 0)
                {
                    List<TeachingCustom> teaching = new List<TeachingCustom>();
                    foreach (var item in enrolled)
                    {
                        TeachingCustom c = new TeachingCustom();
                        Course co = db.Courses.Where(cc => cc.courseid == item.courseid).FirstOrDefault();
                        Student s = db.Students.Where(ss => ss.email == item.studentemail).FirstOrDefault();
                        c.studentemail = item.studentemail;
                        c.tutoremail = item.tutoremail;
                        c.courseid = int.Parse(item.courseid.ToString());
                        c.coursestatus = int.Parse(item.coursestatus.ToString());
                        c.coursename = co.coursename;
                        c.studentname = s.name;
                        c.courseEnrollId = item.id;
                        teaching.Add(c);
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, teaching);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, "No Course Enrolled");

                }

            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, ex.Message);
            }
        }
        //Tutor FEe
        [HttpGet]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public HttpResponseMessage TutorFeeRecord(String email)
        {
            try
            {
                var stdEnrolls = db.CourseEnrolleds.Where(s => s.tutoremail == email).ToList();
                if (stdEnrolls != null)
                {
                    List<FeeReport> cc = new List<FeeReport>();
                    foreach (var item in stdEnrolls)
                    {
                        FeeReport fr = new FeeReport();
                        fr.courseid = int.Parse(item.courseid.ToString());
                        Course c = db.Courses.Where(ss => ss.courseid == item.courseid).FirstOrDefault();
                        fr.coursename = c.coursename;
                        fr.studentemail = item.studentemail.ToString();
                        fr.tutoremail = email;
                        Student t = db.Students.Where(ttt => ttt.email == item.studentemail).FirstOrDefault();
                        Tutor tt = db.Tutors.Where(tu => tu.email == email).FirstOrDefault();
                        fr.name = t.name;
                        List<ClassReportCustom> crc = new List<ClassReportCustom>();
                        var report = db.ClassReports.Where(ss => ss.courseenrolledid == item.id).ToList();
                        if (report != null)
                        {
                            String sems = "Semester " + tt.semester.ToString();
                            SubjectFeeGroupName sfg = db.SubjectFeeGroupNames.Where(sf => sf.groupName == sems).FirstOrDefault();
                            int classCount = 0;
                            foreach (var reportitem in report)
                            {
                                if (reportitem.classesstatus == 1)
                                {
                                    ClassReportCustom crcc = new ClassReportCustom();
                                    crcc.courseenrolledid = int.Parse(reportitem.courseenrolledid.ToString());
                                    crcc.classslot = reportitem.classslot.ToString();
                                    crcc.classesstatus = int.Parse(reportitem.classesstatus.ToString());
                                    crcc.classdate = reportitem.classdate.ToString();
                                    crc.Add(crcc);
                                    classCount++;
                                }
                                else
                                {
                                    NewReschedule nr = db.NewReschedules.Where(nrr => nrr.rescheduleclassFrom == reportitem.classdate).FirstOrDefault();
                                    if (nr != null && nr.rescheduledclassstatus == 1)
                                    {
                                        ClassReportCustom crcc = new ClassReportCustom();
                                        crcc.courseenrolledid = int.Parse(reportitem.courseenrolledid.ToString());
                                        crcc.classslot = slotCondition(int.Parse(nr.slotTo.ToString()));
                                        crcc.classesstatus = int.Parse(nr.classStatus.ToString());
                                        crcc.classdate = nr.rescheduleclassTo;
                                        crc.Add(crcc);
                                        classCount++;
                                    }
                                }
                            }
                            var fee = db.Feenews.Where(f => f.courseenrolledid == item.id).FirstOrDefault();
                            if (fee != null)
                            {
                                fr.totalFee = int.Parse(fee.remainingamount.ToString());
                            }
                            fr.noOfLectures = classCount;
                            fr.reportList = crc;
                        }
                        else
                        {
                            fr.totalFee = 0;
                            fr.reportList = new List<ClassReportCustom>();
                        }
                        cc.Add(fr);
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, cc.ToList());
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, "Student Has Not Enrolled Any course");
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

        }
        [HttpGet]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        //Date 5/8/2023 day="Monday"
        public HttpResponseMessage GetTutorClassesForRescheduling(String email, String date, String day)
        {
            try
            {
                var enrolled = db.CourseEnrolleds.Where(t => t.tutoremail == email && t.coursestatus == 0).ToList();
                if (enrolled != null && enrolled.Count > 0)
                {
                    List<TutorRescheduleClass> tclass = new List<TutorRescheduleClass>();
                    foreach (var item in enrolled)
                    {
                        List<RescheduleSlots> slots = new List<RescheduleSlots>();
                        List<int> newList = new List<int>();

                        String schedule = item.schedule;
                        char[] sr = schedule.ToCharArray();
                        for (int i = 0; i < sr.Length; i++)
                        {
                            if (sr[i] == '2')
                            {
                                newList.Add(i);
                            }
                        }
                        if (newList != null)
                        {
                            foreach (var Citem in newList)
                            {
                                String data = slotCondition(Citem);
                                RescheduleSlots a = new RescheduleSlots();
                                a.slot = data;
                                a.slotno = Citem;
                                slots.Add(a);
                            }
                            foreach (var dateitem in slots)
                            {
                                String[] datep = dateitem.slot.Split(' ');
                                var crList = db.ClassReports.Where(s => s.classdate == date && s.classslot == dateitem.slot && s.courseenrolledid == item.id).ToList();

                                if (datep[3] == day && crList.Count < 1)
                                {
                                    if (crList.Count == 0)
                                    {
                                        TutorRescheduleClass t = new TutorRescheduleClass();
                                        t.slot = dateitem.slot;
                                        Student std = db.Students.Where(ss => ss.email == item.studentemail).FirstOrDefault();
                                        t.name = std.name;
                                        t.email = std.email;
                                        Course cc = db.Courses.Where(sc => sc.courseid == item.courseid).FirstOrDefault();
                                        t.coursename = cc.coursename;
                                        CourseEnrolled courseenrolled = db.CourseEnrolleds.Where(ce => ce.studentemail == std.email && ce.tutoremail == email && ce.courseid == cc.courseid && ce.coursestatus == 0).FirstOrDefault();
                                        String dateNow = DateTime.Now.ToString().Split(' ')[0];
                                        t.slotno = dateitem.slotno;
                                        tclass.Add(t);
                                    }
                                }
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, "No Classes To Reschedule");
                        }
                    }
                    if (tclass != null)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, tclass);
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, "No Classes To Reschedule ");
                    }
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, "No Course Enrolled");

                }

            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, ex.Message);
            }
        }

        [HttpGet]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public HttpResponseMessage GetSuggestedClassesToRescheduleTo(String temail, String semail, String date, String day, String coursename)
        {
            try
            {
                DateTime dateEE = DateTime.ParseExact(date, "MM/dd/yyyy", null);
                DateTime lastDayOfWeek = GetLastDayOfWeek(dateEE);
                Course c = db.Courses.Where(cc => cc.coursename == coursename).FirstOrDefault();
                var ce = db.CourseEnrolleds.Where(cee => cee.studentemail == semail && cee.tutoremail == temail && cee.courseid == c.courseid && cee.coursestatus == 0).FirstOrDefault();
                String std = GetStudentSchedule(semail);
                String tSc = GetTSchedule(temail);
                char[] scheduleArr = std.ToCharArray();
                char[] tScheduleArr = tSc.ToCharArray();
                List<TutorScheduleCustom> daysData = new List<TutorScheduleCustom>();
                List<TutorRescheduleClass> classes = new List<TutorRescheduleClass>();
                String fromDay = dateEE.Date.DayOfWeek.ToString();
                int from = DayToInt(fromDay);
                int to = DayToInt(lastDayOfWeek.DayOfWeek.ToString());
                for (int key = from; key <= to; key++)
                {
                    for (int i = 0; i < scheduleArr.Length; i++)
                    {
                        String d = findDay(i);
                        String da = IntToDay(key);
                        if (d == da)
                        {
                            TutorScheduleCustom tsc = new TutorScheduleCustom();
                            tsc.slotno = i;
                            tsc.slotvalue = scheduleArr[i];
                            tsc.tslotvalue = tScheduleArr[i];
                            daysData.Add(tsc);
                        }
                    }
                }
                foreach (var item in daysData)
                {
                    if (item.slotvalue == '1' && item.tslotvalue == '1')
                    {
                        TutorRescheduleClass trc = new TutorRescheduleClass();
                        Course cour = db.Courses.Where(ss => ss.coursename == coursename).FirstOrDefault();
                        CourseEnrolled ceee = db.CourseEnrolleds.Where(cee => cee.tutoremail == temail && cee.studentemail == semail && cee.courseid == cour.courseid).FirstOrDefault();
                        trc.coursename = coursename;
                        trc.email = semail;
                        Student s = db.Students.Where(st => st.email == semail).FirstOrDefault();
                        trc.name = s.name;
                        trc.slot = slotCondition(item.slotno);
                        trc.slotno = item.slotno;
                        String[] slotSplit = trc.slot.Split(' ');
                        String dateToSend = gettingDateForDay(date, day, slotSplit[3]);
                        trc.classDate = dateToSend;
                        trc.ClassDay = slotSplit[3];
                        classes.Add(trc);
                    }
                }
                return Request.CreateResponse(HttpStatusCode.OK, classes);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, ex.Message);
            }
        }

        [HttpGet]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public HttpResponseMessage GetClassesToRescheduleTo(String temail, String semail, String date, String day, String coursename)
        {
            try
            {
                Student std = db.Students.Where(s => s.email == semail).FirstOrDefault();
                if (std != null)
                {
                    Tutor t = db.Tutors.Where(tt => tt.email == temail).FirstOrDefault();
                    Schedule stdSchedule = db.Schedules.Where(sc => sc.scheduleid == std.scheduleid).FirstOrDefault();
                    Schedule ttSchedule = db.Schedules.Where(tc => tc.scheduleid == t.scheduleid).FirstOrDefault();
                    String sSchedule = stdSchedule.details;
                    String tSchedule = ttSchedule.details;
                    char[] scheduleArr = sSchedule.ToCharArray();
                    char[] tScheduleArr = tSchedule.ToCharArray();
                    List<TutorScheduleCustom> daysData = new List<TutorScheduleCustom>();
                    List<TutorRescheduleClass> classes = new List<TutorRescheduleClass>();

                    for (int i = 0; i < scheduleArr.Length; i++)
                    {
                        String d = findDay(i);
                        if (d == day)
                        {
                            TutorScheduleCustom tsc = new TutorScheduleCustom();
                            tsc.slotno = i;
                            tsc.slotvalue = scheduleArr[i];
                            tsc.tslotvalue = tScheduleArr[i];
                            daysData.Add(tsc);
                        }
                    }
                    foreach (var item in daysData)
                    {
                        if (item.slotvalue == '1' && item.tslotvalue == '1')
                        {
                            TutorRescheduleClass trc = new TutorRescheduleClass();
                            Course c = db.Courses.Where(ss => ss.coursename == coursename).FirstOrDefault();
                            CourseEnrolled ce = db.CourseEnrolleds.Where(cee => cee.tutoremail == temail && cee.studentemail == semail && cee.courseid == c.courseid).FirstOrDefault();
                            trc.coursename = coursename;
                            trc.email = semail;
                            trc.name = std.name;
                            trc.slot = slotCondition(item.slotno);
                            trc.slotno = item.slotno;
                            String[] slotSplit = trc.slot.Split(' ');
                            trc.classDate = date;
                            trc.ClassDay = slotSplit[3];
                            classes.Add(trc);
                        }
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, classes);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, "No Student Found");

                }

            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, ex.Message);
            }
        }
        [HttpPost]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public HttpResponseMessage reschedule(Reschedule re)
        {
            try
            {
                Course c = db.Courses.Where(cc => cc.coursename == re.coursename).FirstOrDefault();
                var enrolledData = db.CourseEnrolleds.Where(se => se.tutoremail == re.temail && se.studentemail == re.semail && c.courseid == se.courseid && se.coursestatus == 0).FirstOrDefault();
                ClassReport crs = new ClassReport();
                crs.courseenrolledid = enrolledData.id;
                crs.classesstatus = 2;
                crs.classdate = re.date;
                crs.classTakenDate = GetCurrentDate();
                crs.classtime = DateTime.Now.Date.ToShortTimeString();
                crs.classslot = slotCondition(re.slotno);
                db.ClassReports.Add(crs);
                db.SaveChanges();
                //getting rescheduled class id from db
                ClassReport crdb = db.ClassReports.Where(sc => sc.courseenrolledid == enrolledData.id && sc.classslot == crs.classslot && sc.classdate == crs.classdate && crs.classtime == sc.classtime).FirstOrDefault();
                //rescheduling class
                NewReschedule res = new NewReschedule();
                res.classreportid = crdb.id;
                res.rescheduledclassstatus = 0;
                res.rescheduleclassFrom = re.date;
                res.rescheduleclassTo = re.tdate;
                res.slotFrom = re.slotno;
                res.slotTo = re.tslotno;
                res.classStatus = 0;
                db.NewReschedules.Add(res);
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, "Class is Rescheduled");
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, ex.Message);
            }
        }
        [HttpPost]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public HttpResponseMessage rescheduleByStudent(Reschedule re)
        {
            try
            {
                Course c = db.Courses.Where(cc => cc.coursename == re.coursename).FirstOrDefault();
                var enrolledData = db.CourseEnrolleds.Where(se => se.tutoremail == re.temail && se.studentemail == re.semail && c.courseid == se.courseid && se.coursestatus == 0).FirstOrDefault();
                ClassReport crs = new ClassReport();
                crs.courseenrolledid = enrolledData.id;
                crs.classesstatus = 2;
                crs.classdate = re.date;
                crs.classTakenDate = GetCurrentDate();
                crs.classtime = DateTime.Now.Date.ToShortTimeString();
                crs.classslot = slotCondition(re.slotno);
                db.ClassReports.Add(crs);
                db.SaveChanges();
                //getting rescheduled class id from db
                ClassReport crdb = db.ClassReports.Where(sc => sc.courseenrolledid == enrolledData.id && sc.classslot == crs.classslot && sc.classdate == crs.classdate && crs.classtime == sc.classtime).FirstOrDefault();
                //rescheduling class
                NewReschedule res = new NewReschedule();
                res.classreportid = crdb.id;
                res.rescheduledclassstatus = 0;
                res.rescheduleclassFrom = re.date;
                res.rescheduleclassTo = re.tdate;
                res.slotFrom = re.slotno;
                res.slotTo = re.tslotno;
                res.classStatus = 7;
                db.NewReschedules.Add(res);
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, "Class is Rescheduled");
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, ex.Message);
            }
        }
        //Preschedule
        [HttpPost]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public HttpResponseMessage PreSchedule(Reschedule re)
        {
            try
            {
                Course c = db.Courses.Where(cc => cc.coursename == re.coursename).FirstOrDefault();
                var enrolledData = db.CourseEnrolleds.Where(se => se.tutoremail == re.temail && se.studentemail == re.semail && c.courseid == se.courseid && se.coursestatus == 0).FirstOrDefault();
                ClassReport crs = new ClassReport();
                crs.courseenrolledid = enrolledData.id;
                crs.classesstatus = 2;
                crs.classdate = re.date;
                crs.classTakenDate = GetCurrentDate();
                crs.classtime = DateTime.Now.Date.ToShortTimeString();
                crs.classslot = slotCondition(re.slotno);
                db.ClassReports.Add(crs);
                db.SaveChanges();
                //getting rescheduled class id from db
                ClassReport crdb = db.ClassReports.Where(sc => sc.courseenrolledid == enrolledData.id && sc.classslot == crs.classslot && sc.classdate == crs.classdate && crs.classtime == sc.classtime).FirstOrDefault();
                //rescheduling class
                NewReschedule res = new NewReschedule();
                res.classreportid = crdb.id;
                res.rescheduledclassstatus = 0;
                res.rescheduleclassFrom = re.date;
                res.rescheduleclassTo = re.tdate;
                res.slotFrom = re.slotno;
                res.slotTo = re.tslotno;
                res.classStatus = 2;
                db.NewReschedules.Add(res);
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, "Class is Prescheduled");
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, ex.Message);
            }
        }

        // Muliple prescheduling
        [HttpGet]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        //Date 5/8/2023 day="Monday"
        public HttpResponseMessage GetTutorClassesForReschedulingMultiple(String email, String startDate, String startDay, String endDate, String endDay)
        {
            try
            {
                var enrolled = db.CourseEnrolleds.Where(t => t.tutoremail == email && t.coursestatus == 0).ToList();

                if (enrolled != null && enrolled.Count > 0)
                {
                    List<TutorRescheduleClass> tclass = new List<TutorRescheduleClass>();
                    DateTime startDateTime = DateTime.ParseExact(startDate, "MM/dd/yyyy", null);
                    DateTime endDateTime = DateTime.ParseExact(endDate, "MM/dd/yyyy", null);
                    List<DateTime> dates = new List<DateTime>();

                    // Add the start date to the list
                    dates.Add(startDateTime);

                    // Loop through the dates from start date to end date
                    DateTime currentDate = startDateTime.AddDays(1); // Start with the day after start date
                    while (currentDate <= endDateTime)
                    {
                        dates.Add(currentDate);
                        currentDate = currentDate.AddDays(1);
                    }
                    foreach (var item in enrolled)
                    {
                        List<RescheduleSlots> slots = new List<RescheduleSlots>();
                        List<int> newList = new List<int>();

                        String schedule = item.schedule;
                        char[] sr = schedule.ToCharArray();
                        for (int i = 0; i < sr.Length; i++)
                        {
                            if (sr[i] == '2')
                            {
                                newList.Add(i);
                            }
                        }
                        if (newList != null)
                        {
                            foreach (var Citem in newList)
                            {
                                String data = slotCondition(Citem);
                                RescheduleSlots a = new RescheduleSlots();
                                a.slot = data;
                                a.slotno = Citem;
                                slots.Add(a);
                            }
                            foreach (var dateMap in dates)
                            {
                                String dateToSearch = ChangeDateFormatToMatch(dateMap);
                                DayOfWeek dayToSearch = dateMap.DayOfWeek;
                                foreach (var dateitem in slots)
                                {
                                    String[] datep = dateitem.slot.Split(' ');
                                    var crList = db.ClassReports.Where(s => s.classdate == dateToSearch && s.classslot == dateitem.slot).ToList();

                                    if (datep[3] == dayToSearch.ToString() && crList.Count < 1)
                                    {
                                        if (crList.Count == 0)
                                        {
                                            TutorRescheduleClass t = new TutorRescheduleClass();
                                            t.slot = dateitem.slot;
                                            Student std = db.Students.Where(ss => ss.email == item.studentemail).FirstOrDefault();
                                            t.name = std.name;
                                            t.email = std.email;
                                            Course cc = db.Courses.Where(sc => sc.courseid == item.courseid).FirstOrDefault();
                                            t.coursename = cc.coursename;
                                            CourseEnrolled courseenrolled = db.CourseEnrolleds.Where(ce => ce.studentemail == std.email && ce.tutoremail == email && ce.courseid == cc.courseid && ce.coursestatus == 0).FirstOrDefault();
                                            t.classDate = dateToSearch;
                                            t.ClassDay = dayToSearch.ToString();
                                            t.slotno = dateitem.slotno;
                                            tclass.Add(t);
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, "No Classes To Reschedule");
                        }
                    }
                    if (tclass != null && tclass.Count > 0)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, tclass.OrderBy(s => s.classDate).ToList());
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, "No Classes To Reschedule ");
                    }
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, "No Course Enrolled");

                }

            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, ex.Message);
            }
        }
        //SUGGESTED classses
        [HttpGet]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public HttpResponseMessage GetSuggestedClassesToRescheduleToMultiple(String temail, String semail, String startDate, String startDay, String endDate, String endDay, String classDate, String coursename)
        {
            try
            {
                DateTime startDT = DateTime.ParseExact(startDate, "MM/dd/yyyy", null);
                DateTime endDT = DateTime.ParseExact(endDate, "MM/dd/yyyy", null);
                List<DateTime> datesDT = new List<DateTime>();
                // Add the start date to the list
                datesDT.Add(startDT);
                // Loop through the dates from start date to end date
                DateTime currentDate = startDT.AddDays(1); // Start with the day after start date
                while (currentDate <= endDT)
                {
                    datesDT.Add(currentDate);
                    currentDate = currentDate.AddDays(1);
                }
                int j = 1, index = datesDT.Count;
                while (j <= index)
                {
                    currentDate = currentDate.AddDays(1);
                    j++;
                }
                List<DateTime> dates = GetDatesBetween(endDT, currentDate);
                List<TutorRescheduleClass> classes = new List<TutorRescheduleClass>();

                //String dateToSearch = ChangeDateFormatToMatch(dateMap);
                //DayOfWeek dayToSearch = dateMap.DayOfWeek;
                foreach (var dateItem in dates)
                {
                    //Function 
                    List<TutorScheduleCustom> daysData = new List<TutorScheduleCustom>();
                    String dateForSchedule = ChangeDateFormatToMatch(dateItem);
                    DayOfWeek dayToSearch = dateItem.DayOfWeek;
                    Course c = db.Courses.Where(cc => cc.coursename == coursename).FirstOrDefault();
                    String std = GetStudentScheduleofWeek(semail, dateForSchedule);
                    String tSc = GetTutorScheduleofWeek(temail, dateForSchedule);
                    char[] scheduleArr = std.ToCharArray();
                    char[] tScheduleArr = tSc.ToCharArray();
                    for (int i = 0; i < scheduleArr.Length; i++)
                    {
                        if (scheduleArr[i] == '1' && tScheduleArr[i] == '1')
                        {
                            TutorScheduleCustom tsc = new TutorScheduleCustom();
                            tsc.slotno = i;
                            tsc.slotvalue = scheduleArr[i];
                            tsc.tslotvalue = tScheduleArr[i];
                            daysData.Add(tsc);
                        }
                    }
                    foreach (var item in daysData)
                    {
                        if (item.slotvalue == '1' && item.tslotvalue == '1')
                        {
                            TutorRescheduleClass trc = new TutorRescheduleClass();
                            Course cour = db.Courses.Where(ss => ss.coursename == coursename).FirstOrDefault();
                            CourseEnrolled ceee = db.CourseEnrolleds.Where(cee => cee.tutoremail == temail && cee.studentemail == semail && cee.courseid == cour.courseid).FirstOrDefault();
                            trc.coursename = coursename;
                            trc.email = semail;
                            Student s = db.Students.Where(st => st.email == semail).FirstOrDefault();
                            trc.name = s.name;
                            trc.slot = slotCondition(item.slotno);
                            trc.slotno = item.slotno;
                            String[] slotSplit = trc.slot.Split(' ');
                            String dateToSend = gettingDateForDayMultiple(ChangeDateFormatToMatch(dateItem), dateItem.DayOfWeek.ToString(), slotSplit[3]);
                            trc.classDate = dateToSend;
                            DateTime dateSend = DateTime.ParseExact(dateToSend, "MM/dd/yyyy", null);
                            trc.ClassDay = dateSend.DayOfWeek.ToString();
                            classes.Add(trc);
                        }
                    }
                }
                return Request.CreateResponse(HttpStatusCode.OK, classes.OrderBy(s => s.classDate).ToList());
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, ex.Message);
            }
        }
        //Get Tutor free classes
        [HttpGet]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public HttpResponseMessage GetTutorFreeClasses(String temail)
        {
            try
            {
                Tutor t = db.Tutors.Where(tt => tt.email == temail).FirstOrDefault();
                if (t != null)
                {
                    String tschedule = GetTSchedule(temail);
                    String slotMatched = "";
                    int j = 0;
                    for (int i = 0; i < tschedule.Length; i++)
                    {
                        if (tschedule[i].Equals('1'))
                        {
                            j++;
                            slotMatched += (i + 1) + ",";
                        }

                    }
                    return Request.CreateResponse(HttpStatusCode.OK, slotMatched);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, "Tutor not found or doesnot exist.");
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, ex.Message);
            }
        }
        [HttpGet]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public HttpResponseMessage GetUpdatedMatchedSlots(int enrollId)
        {
            try
            {
                CourseEnrolled c = db.CourseEnrolleds.Where(tt => tt.id == enrollId).FirstOrDefault();
                if (c != null)
                {
                    String studentLastdate = "1";
                    var studentEnrolled = db.CourseEnrolleds.Where(ce => ce.studentemail == c.studentemail && ce.coursestatus == 0).ToList();
                    foreach (var item in studentEnrolled)
                    {
                        var classReports = db.ClassReports.Where(s => s.classesstatus == 2 && s.courseenrolledid == item.id).ToList();
                        if (classReports != null)
                        {
                            foreach (var crItem in classReports)
                            {
                                NewReschedule rescheduled = db.NewReschedules.Where(r => r.classreportid == crItem.id && r.rescheduledclassstatus == 0).FirstOrDefault();
                                if (rescheduled != null)
                                {
                                    studentLastdate = rescheduled.rescheduleclassTo;
                                }
                            }
                        }
                    }
                    //for tutor
                    String tutorLastdate = "1";
                    var tutorEnrolled = db.CourseEnrolleds.Where(ce => ce.tutoremail == c.tutoremail && ce.coursestatus == 0).ToList();
                    foreach (var item in tutorEnrolled)
                    {
                        var classReports = db.ClassReports.Where(s => s.classesstatus == 2 && s.courseenrolledid == item.id).ToList();
                        if (classReports != null)
                        {
                            foreach (var crItem in classReports)
                            {
                                NewReschedule rescheduled = db.NewReschedules.Where(r => r.classreportid == crItem.id && r.rescheduledclassstatus == 0).FirstOrDefault();
                                if (rescheduled != null)
                                {
                                    tutorLastdate = rescheduled.rescheduleclassTo;
                                }
                            }
                        }
                    }
                    string studentSchedule = "";
                    string tutorschedule = "";
                    if (studentLastdate == "1")
                    {
                        studentSchedule = GetStudentSchedule(c.studentemail);
                    }
                    else
                    {
                        studentSchedule = GetStudentScheduleofWeek(c.studentemail, studentLastdate);
                    }
                    if (tutorLastdate == "1")
                    {
                        tutorschedule = GetTSchedule(c.tutoremail);
                    }
                    else
                    {
                        tutorschedule = GetTutorScheduleofWeek(c.tutoremail, tutorLastdate);
                    }
                    char[] studentArr = studentSchedule.ToCharArray();
                    char[] tutorArr = tutorschedule.ToCharArray();
                    List<int> slots = new List<int>();
                    List<RescheduleSlots> slotsMatched = new List<RescheduleSlots>();
                    for (int i = 0; i < studentArr.Length; i++)
                    {
                        if (studentArr[i] == '1' && tutorArr[i] == '1')
                        {
                            slots.Add(i);
                        }
                    }
                    foreach (var item in slots)
                    {
                        RescheduleSlots rr = new RescheduleSlots();
                        rr.slotno = item;
                        rr.slot = slotCondition(item);
                        slotsMatched.Add(rr);
                    }
                    string dateToSend = GetCurrentDate();
                    if (studentLastdate != "1" && tutorLastdate != "1")
                    {
                        DateTime sdate = DateTime.ParseExact(studentLastdate, "MM/dd/yyyy", null);
                        DateTime tdate = DateTime.ParseExact(tutorLastdate, "MM/dd/yyyy", null);
                        if (sdate >= tdate)
                        {
                            dateToSend = studentLastdate;
                        }
                        else
                        {
                            dateToSend = tutorLastdate;
                        }
                    }
                    else if (studentLastdate != "1")
                    {
                        dateToSend = studentLastdate;
                    }
                    else if (tutorLastdate != "1")
                    {
                        dateToSend = tutorLastdate;
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { date = dateToSend, slots = slotsMatched });
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, "No Course Enrolled");
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, ex.Message);
            }
        }

        [HttpGet]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public HttpResponseMessage GetEnrolledClasses(int enrollId)
        {
            try
            {
                CourseEnrolled c = db.CourseEnrolleds.Where(tt => tt.id == enrollId).FirstOrDefault();
                if (c != null)
                {
                    //updating student schedule
                    String s = c.schedule;
                    char[] sr = s.ToCharArray();
                    List<int> slots = new List<int>();
                    List<RescheduleSlots> slotsMatched = new List<RescheduleSlots>();
                    for (int i = 0; i < sr.Length; i++)
                    {
                        if (sr[i] == '2')
                        {
                            slots.Add(i);
                        }
                    }
                    foreach (var item in slots)
                    {
                        RescheduleSlots rr = new RescheduleSlots();
                        rr.slotno = item;
                        rr.slot = slotCondition(item);
                        slotsMatched.Add(rr);
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, slotsMatched);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, "No Enrolled Classes Found");
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, ex.Message);
            }
        }












        // functions
        public String slotCondition(int value)
        {
            value += 1;
            String mon = "Monday";
            String tue = "Tuesday";
            String wed = "Wednesday";
            String thr = "Thursday";
            String fri = "Friday";
            String sat = "Saturday";
            String sun = "Sunday";
            String s1 = "08:00 - 09:00 ";
            String s2 = "09:00 - 10:00 ";
            String s3 = "10:00 - 11:00 ";
            String s4 = "11:00 - 12:00 ";
            String s5 = "12:00 - 13:00 ";
            String s6 = "13:00 - 14:00 ";
            String s7 = "14:00 - 15:00 ";
            String s8 = "15:00 - 16:00 ";
            String s9 = "16:00 - 17:00 ";
            String s10 = "17:00 - 18:00 ";
            String s11 = "18:00 - 19:00 ";
            String s12 = "19:00 - 20:00 ";
            String s13 = "20:00 - 21:00 ";
            String s14 = "21:00 - 22:00 ";
            String s15 = "22:00 - 23:00 ";
            String s16 = "23:00 - 00:00 ";
            String slot = "";
            double Val = (value / 7.0);
            if (Val <= 1.0)
            {
                slot = s1;
            }
            else if (Val > 1.0 && Val <= 2.0)
            {
                slot = s2;
            }
            else if (Val > 2.0 && Val <= 3.0)
            {
                slot = s3;
            }
            else if (Val > 3.0 && Val <= 4.0)
            {
                slot = s4;
            }
            else if (Val > 4.0 && Val <= 5.0)
            {
                slot = s5;
            }
            else if (Val > 5.0 && Val <= 6.0)
            {
                slot = s6;
            }
            else if (Val > 6.0 && Val <= 7.0)
            {
                slot = s7;
            }
            else if (Val > 7.0 && Val <= 8.0)
            {
                slot = s8;
            }
            else if (Val > 8.0 && Val <= 9.0)
            {
                slot = s9;
            }
            else if (Val > 9.0 && Val <= 10.0)
            {
                slot = s10;
            }
            else if (Val > 10.0 && Val <= 11.0)
            {
                slot = s11;
            }
            else if (Val > 11.0 && Val <= 12.0)
            {
                slot = s12;
            }
            else if (Val > 12.0 && Val <= 13.0)
            {
                slot = s13;
            }
            else if (Val > 13.0 && Val <= 14.0)
            {
                slot = s14;
            }
            else if (Val > 14.0 && Val <= 15.0)
            {
                slot = s15;
            }
            else if (Val > 15.0 && Val <= 16.0)
            {
                slot = s16;
            }

            if (value % 7 == 0)
            {
                slot += sun;
            }
            else if (value % 7 == 1)
            {
                slot += mon;
            }
            else if (value % 7 == 2)
            {
                slot += tue;
            }
            else if (value % 7 == 3)
            {
                slot += wed;
            }
            else if (value % 7 == 4)
            {
                slot += thr;
            }
            else if (value % 7 == 5)
            {
                slot += fri;
            }
            else if (value % 7 == 6)
            {
                slot += sat;
            }

            return slot;
        }
        public String findDay(int value)
        {
            value += 1;
            if (value % 7 == 0)
            {
                return "Sunday";
            }
            else if (value % 7 == 1)
            {
                return "Monday";

            }
            else if (value % 7 == 2)
            {
                return "Tuesday";

            }
            else if (value % 7 == 3)
            {
                return "Wednesday";

            }
            else if (value % 7 == 4)
            {
                return "Thursday";

            }
            else if (value % 7 == 5)
            {
                return "Friday";

            }
            else
            {
                return "Saturday";

            }
        }
        public int DayToInt(String day)
        {
            if (day == "Monday")
            {
                return 1;
            }
            else if (day == "Tuesday")
            {
                return 2;
            }
            else if (day == "Wednesday")
            {
                return 3;
            }
            else if (day == "Thursday")
            {
                return 4;
            }
            else if (day == "Friday")
            {
                return 5;
            }
            else if (day == "Saturday")
            {
                return 6;
            }
            else
            {
                return 7;
            }
        }

        public String IntToDay(int day)
        {
            if (day == 1)
            {
                return "Monday";
            }
            else if (day == 2)
            {
                return "Tuesday";
            }
            else if (day == 3)
            {
                return "Wednesday";
            }
            else if (day == 4)
            {
                return "Thursday";
            }
            else if (day == 5)
            {
                return "Friday";
            }
            else if (day == 6)
            {
                return "Saturday";
            }
            else
            {
                return "Sunday";
            }
        }
        static DateTime GetLastDayOfWeek(DateTime date)
        {
            DayOfWeek currentDayOfWeek = date.DayOfWeek;
            int daysUntilLastDayOfWeek = (int)DayOfWeek.Sunday - (int)currentDayOfWeek;

            if (daysUntilLastDayOfWeek < 0)
                daysUntilLastDayOfWeek += 7;

            return date.AddDays(daysUntilLastDayOfWeek);
        }
        public static DayOfWeek ConvertStringToDayOfWeek(string day)
        {
            // Parse the string representation of the day and convert it to DayOfWeek enum
            return (DayOfWeek)Enum.Parse(typeof(DayOfWeek), day, true);
        }
        public String ChangeDateFormatToMatch(DateTime toChangeDate)
        {
            String day = toChangeDate.Day.ToString();
            String month = toChangeDate.Month.ToString();
            String year = toChangeDate.Year.ToString();
            if (int.Parse(month) < 10)
            {
                month = "0" + month;
            }
            if (int.Parse(day) < 10)
            {
                day = "0" + day;
            }
            String date = month + "/" + day + "/" + year;
            return date;
        }
        public string gettingDateForDay(String date, String dayGiven, String dayFind)
        {
            DateTime startDate = DateTime.ParseExact(date, "MM/dd/yyyy", null);
            int givenDay = DayToInt(dayGiven);
            int findDay = DayToInt(dayFind);
            DayOfWeek targetDayOfWeek = ConvertStringToDayOfWeek(dayFind);
            if (findDay == givenDay)
            {
                int numberOfDays = findDay;
                int direction = numberOfDays >= 0 ? 1 : -1;
                int daysToAdd = Math.Abs(numberOfDays);
                while (daysToAdd > 0)
                {
                    startDate = startDate.AddDays(direction);
                    if (startDate.DayOfWeek == targetDayOfWeek)
                        break;
                }
                String stringReturnDate = ChangeDateFormatToMatch(startDate);
                return stringReturnDate;
            }
            if (findDay > givenDay)
            {
                int numberOfDays = findDay - givenDay;
                int direction = numberOfDays >= 0 ? 1 : -1;
                int daysToAdd = Math.Abs(numberOfDays);
                while (daysToAdd > 0)
                {
                    startDate = startDate.AddDays(direction);
                    if (startDate.DayOfWeek == targetDayOfWeek)
                        break;
                }
                String stringReturnDate = ChangeDateFormatToMatch(startDate);
                return stringReturnDate;
            }
            else
            {
                int remDays = 7 - givenDay;
                int numberOfDays = findDay + remDays;
                int direction = numberOfDays >= 0 ? 1 : -1;
                int daysToAdd = Math.Abs(numberOfDays);
                while (daysToAdd > 0)
                {
                    startDate = startDate.AddDays(direction);
                    if (startDate.DayOfWeek == targetDayOfWeek)
                        break;
                }
                String stringReturnDate = ChangeDateFormatToMatch(startDate);
                return stringReturnDate;
            }

        }
        bool AreFallingInSameWeek(DateTime date1, DateTime date2, DayOfWeek weekStartsOn)
        {
            return date1.AddDays(-GetOffsetedDayofWeek(date1.DayOfWeek, (int)weekStartsOn)) == date2.AddDays(-GetOffsetedDayofWeek(date2.DayOfWeek, (int)weekStartsOn));
        }

        int GetOffsetedDayofWeek(DayOfWeek dayOfWeek, int offsetBy)
        {
            return (((int)dayOfWeek - offsetBy + 7) % 7);
        }
        public string GetTutorScheduleofWeek(String email, String dateStart)
        {
            Tutor t = db.Tutors.Where(s => s.email == email).FirstOrDefault();
            Schedule sd = db.Schedules.Where(ss => ss.scheduleid == t.scheduleid).FirstOrDefault();
            var tutorEnrolled = db.CourseEnrolleds.Where(s => s.tutoremail == email).ToList();
            List<ChangeSchedule> toChangeSlots = new List<ChangeSchedule>();
            if (tutorEnrolled != null && tutorEnrolled.Count > 0)
            {
                foreach (var item in tutorEnrolled)
                {
                    var classReports = db.ClassReports.Where(cr => cr.courseenrolledid == item.id && cr.classesstatus == 2).ToList();
                    if (classReports != null && classReports.Count > 0)
                    {
                        foreach (var crItem in classReports)
                        {
                            NewReschedule r = db.NewReschedules.Where(rr => rr.classreportid == crItem.id && rr.rescheduledclassstatus == 0).FirstOrDefault();
                            if (r != null)
                            {
                                String cdate = dateStart;
                                String date = r.rescheduleclassTo;
                                String fromDate = r.rescheduleclassFrom;
                                String currentDate = cdate;
                                DateTime myDateTime2 = DateTime.ParseExact(date, "MM/dd/yyyy", null);
                                DateTime myDateTime = DateTime.ParseExact(currentDate, "MM/dd/yyyy", null);
                                DateTime myDateFrom = DateTime.ParseExact(fromDate, "MM/dd/yyyy", null);
                                DayOfWeek day = System.DayOfWeek.Monday;
                                bool ans = AreFallingInSameWeek(myDateTime, myDateTime2, day);
                                bool erp = AreFallingInSameWeek(myDateTime, myDateFrom, day);
                                if (ans || erp)
                                {
                                    ChangeSchedule chs = new ChangeSchedule();
                                    chs.isToInWeek = false;
                                    chs.isFromInWeek = false;
                                    if (ans)
                                    {
                                        chs.isToInWeek = ans;
                                        chs.to = r.slotTo.ToString();
                                    }
                                    if (erp)
                                    {
                                        chs.froom = r.slotFrom.ToString();
                                        chs.isFromInWeek = erp;

                                    }
                                    toChangeSlots.Add(chs);
                                }

                            }
                        }
                    }
                }
                if (toChangeSlots.Count > 0)
                {
                    //change schedule here
                    String schedule = sd.details;
                    char[] sr = schedule.ToCharArray();
                    foreach (var item in toChangeSlots)
                    {
                        if (item.isFromInWeek)
                        {
                            sr[int.Parse(item.froom)] = '3';
                        }
                        if (item.isToInWeek)
                        {
                            sr[int.Parse(item.to)] = '4';
                        }
                    }
                    String sn = "";
                    for (int i = 0; i < sr.Length; i++)
                    {
                        sn = String.Concat(sn, sr[i]);
                    }
                    String details = sn;
                    return details;
                }
                return sd.details;
            }
            else
                return sd.details;
        }
        //Student Schedule
        public string GetStudentScheduleofWeek(String email, String dateStart)
        {
            Student u = db.Students.Where(s => s.email == email).FirstOrDefault();
            Schedule sd = db.Schedules.Where(ss => ss.scheduleid == u.scheduleid).FirstOrDefault();
            var tutorEnrolled = db.CourseEnrolleds.Where(s => s.studentemail == email).ToList();
            List<ChangeSchedule> toChangeSlots = new List<ChangeSchedule>();
            if (tutorEnrolled != null && tutorEnrolled.Count > 0)
            {
                foreach (var item in tutorEnrolled)
                {
                    var classReports = db.ClassReports.Where(cr => cr.courseenrolledid == item.id && cr.classesstatus == 2).ToList();
                    if (classReports != null && classReports.Count > 0)
                    {
                        foreach (var crItem in classReports)
                        {
                            NewReschedule r = db.NewReschedules.Where(rr => rr.classreportid == crItem.id && rr.rescheduledclassstatus == 0).FirstOrDefault();
                            if (r != null)
                            {
                                String cdate = dateStart;
                                String date = r.rescheduleclassTo;
                                String fromDate = r.rescheduleclassFrom;
                                String currentDate = cdate;
                                DateTime myDateTime = DateTime.ParseExact(currentDate, "MM/dd/yyyy", null);
                                DateTime myDateFrom = DateTime.ParseExact(fromDate, "MM/dd/yyyy", null);
                                DateTime myDateTime2 = DateTime.ParseExact(date, "MM/dd/yyyy", null);

                                DayOfWeek day = System.DayOfWeek.Monday;
                                bool ans = AreFallingInSameWeek(myDateTime, myDateTime2, day);
                                bool erp = AreFallingInSameWeek(myDateTime, myDateFrom, day);
                                if (ans || erp)
                                {
                                    ChangeSchedule chs = new ChangeSchedule();
                                    chs.isToInWeek = false;
                                    chs.isFromInWeek = false;
                                    if (ans)
                                    {
                                        chs.isToInWeek = ans;
                                        chs.to = r.slotTo.ToString();
                                    }
                                    if (erp)
                                    {
                                        chs.froom = r.slotFrom.ToString();
                                        chs.isFromInWeek = erp;

                                    }
                                    toChangeSlots.Add(chs);
                                }

                            }
                        }
                    }
                }
                if (toChangeSlots.Count > 0)
                {
                    //change schedule here
                    String schedule = sd.details;
                    char[] sr = schedule.ToCharArray();
                    foreach (var item in toChangeSlots)
                    {
                        if (item.isFromInWeek)
                        {
                            sr[int.Parse(item.froom)] = '3';
                        }
                        if (item.isToInWeek)
                        {
                            sr[int.Parse(item.to)] = '4';
                        }
                    }
                    String sn = "";
                    for (int i = 0; i < sr.Length; i++)
                    {
                        sn = String.Concat(sn, sr[i]);
                    }
                    String details = sn;
                    return details;
                }
                return sd.details;
            }
            else
                return sd.details;

        }
        //Get Schedule of a particular day and date
        public String getParticularSchedule(String email, String date, String day)
        {
            Student u = db.Students.Where(s => s.email == email).FirstOrDefault();
            Schedule sd = db.Schedules.Where(ss => ss.scheduleid == u.scheduleid).FirstOrDefault();
            var tutorEnrolled = db.CourseEnrolleds.Where(s => s.studentemail == email).ToList();
            List<ChangeSchedule> toChangeSlots = new List<ChangeSchedule>();
            char[] scheduleArr = sd.details.ToCharArray();
            for (int i = 0; i < scheduleArr.Length; i++)
            {
                String daySchedule = getSlotDay(scheduleArr[i]);
                if (daySchedule == day)
                {

                }
            }
            if (tutorEnrolled != null && tutorEnrolled.Count > 0)
            {
                foreach (var item in tutorEnrolled)
                {
                }
            }
            return "";
        }
        //First day of each week lying in given dates
        public List<DateTime> GetDatesBetween(DateTime startDate, DateTime endDate)
        {
            List<DateTime> allDates = new List<DateTime>();
            // Calculate all dates between the given dates
            for (DateTime date = startDate; date <= endDate; date = date.AddDays(1))
            {
                allDates.Add(date);
            }
            // Remove duplicates within the same week
            List<DateTime> filteredDates = allDates.GroupBy(d => new { Week = GetWeekOfYear(d, CalendarWeekRule.FirstDay, DayOfWeek.Sunday), Year = d.Year })
                                                   .Select(g => g.First())
                                                   .ToList();
            filteredDates.RemoveAt(0);
            return filteredDates;
        }
        private int GetWeekOfYear(DateTime date, CalendarWeekRule weekRule, DayOfWeek firstDayOfWeek)
        {
            Calendar cal = CultureInfo.CurrentCulture.Calendar;
            return cal.GetWeekOfYear(date, weekRule, firstDayOfWeek);
        }

        private int GetWeekOfYear(DateTime date)
        {
            return System.Globalization.CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(date,
                System.Globalization.CalendarWeekRule.FirstDay, DayOfWeek.Monday);
        }
        private int GetIso8601WeekOfYear(DateTime date)
        {
            DateTimeFormatInfo dfi = DateTimeFormatInfo.CurrentInfo;
            Calendar cal = dfi.Calendar;

            return cal.GetWeekOfYear(date, dfi.CalendarWeekRule, dfi.FirstDayOfWeek);
        }
        //Date for day for multiple 
        public string gettingDateForDayMultiple(String date, String dayGiven, String dayFind)
        {
            DateTime startDate = DateTime.ParseExact(date, "MM/dd/yyyy", null);
            int givenDay = DayToInt(dayGiven);
            int findDay = DayToInt(dayFind);
            DayOfWeek targetDayOfWeek = ConvertStringToDayOfWeek(dayFind);
            if (findDay == givenDay)
            {
                return date;
            }
            if (findDay > givenDay)
            {
                int numberOfDays = findDay - givenDay;
                int direction = numberOfDays >= 0 ? 1 : -1;
                int daysToAdd = Math.Abs(numberOfDays);
                while (daysToAdd > 0)
                {
                    startDate = startDate.AddDays(direction);
                    if (startDate.DayOfWeek == targetDayOfWeek)
                        break;
                }
                String stringReturnDate = ChangeDateFormatToMatch(startDate);
                return stringReturnDate;
            }
            else
            {
                int remDays = 7 - givenDay;
                int numberOfDays = findDay + remDays;
                int direction = numberOfDays >= 0 ? 1 : -1;
                int daysToAdd = Math.Abs(numberOfDays);
                while (daysToAdd > 0)
                {
                    startDate = startDate.AddDays(direction);
                    if (startDate.DayOfWeek == targetDayOfWeek)
                        break;
                }
                String stringReturnDate = ChangeDateFormatToMatch(startDate);
                return stringReturnDate;
            }

        }

        //Get day from schedule 
        public String getSlotDay(int value)
        {
            value += 1;
            String mon = "Monday";
            String tue = "Tuesday";
            String wed = "Wednesday";
            String thr = "Thursday";
            String fri = "Friday";
            String sat = "Saturday";
            String sun = "Sunday";
            String slot = "";
            if (value % 7 == 0)
            {
                slot += sun;
            }
            else if (value % 7 == 1)
            {
                slot += mon;
            }
            else if (value % 7 == 2)
            {
                slot += tue;
            }
            else if (value % 7 == 3)
            {
                slot += wed;
            }
            else if (value % 7 == 4)
            {
                slot += thr;
            }
            else if (value % 7 == 5)
            {
                slot += fri;
            }
            else if (value % 7 == 6)
            {
                slot += sat;
            }
            return slot;
        }

        //Today classes
        public List<TodayClasses> GetTodayClasses(String email)
        {
            var cList = db.CourseEnrolleds.Where(s => s.tutoremail == email && s.coursestatus == 0).ToList();
            List<String> classes = new List<string>();
            List<TodayClasses> tclass = new List<TodayClasses>();

            if (cList != null)
            {
                String currentDay = DateTime.Now.DayOfWeek.ToString();
                foreach (var item in cList)
                {
                    DateTime dateee = DateTime.ParseExact(item.date, "MM/dd/yyyy", null);
                    DateTime todayDate = DateTime.ParseExact(GetCurrentDate(), "MM/dd/yyyy", null);
                    if (todayDate >= dateee)
                    {
                        List<String> slots = new List<string>();
                        List<int> newList = new List<int>();

                        String schedule = item.schedule;
                        char[] sr = schedule.ToCharArray();
                        for (int i = 0; i < sr.Length; i++)
                        {
                            if (sr[i] == '2')
                            {
                                newList.Add(i);
                            }
                        }
                        if (newList != null)
                        {
                            foreach (var Citem in newList)
                            {
                                String data = slotCondition(Citem);
                                slots.Add(data);
                            }
                            foreach (var dateitem in slots)
                            {
                                String currentDate = GetCurrentDate();
                                String[] date = dateitem.Split(' ');
                                var ccc = db.ClassReports.Where(crdb => crdb.classslot == dateitem && crdb.courseenrolledid == item.id && crdb.classesstatus == 2).ToList();
                                if (ccc != null && ccc.Count > 0)
                                {
                                    foreach (var ccitem in ccc)
                                    {
                                        NewReschedule res = db.NewReschedules.Where(r => r.classreportid == ccitem.id && r.rescheduledclassstatus == 0 && r.rescheduleclassTo == currentDate).FirstOrDefault();
                                        if (res != null)
                                        {
                                            TodayClasses t = new TodayClasses();
                                            t.slot = dateitem;
                                            Student std = db.Students.Where(ss => ss.email == item.studentemail).FirstOrDefault();
                                            t.name = std.name;
                                            Course cc = db.Courses.Where(sc => sc.courseid == item.courseid).FirstOrDefault();
                                            t.coursename = cc.coursename;
                                            if (res.classStatus == 0)
                                            {
                                                t.isReschedule = true;
                                                t.isPreSchedule = false;
                                                t.isStudent = false;
                                            }
                                            else if (res.classStatus == 2)
                                            {
                                                t.isReschedule = false;
                                                t.isPreSchedule = true;
                                                t.isStudent = false;
                                            }
                                            else if (res.classStatus == 7)
                                            {
                                                t.isReschedule = false;
                                                t.isPreSchedule = false;
                                                t.isStudent = true;
                                            }
                                            t.classDate = currentDate;
                                            t.semail = item.studentemail;
                                            t.temail = item.tutoremail;
                                            tclass.Add(t);
                                        }
                                    }
                                }
                                if (date[3] == currentDay)
                                {
                                    int enrollId = item.id;
                                    ClassReport cReport = db.ClassReports.Where(cr => cr.courseenrolledid == enrollId && cr.classdate == currentDate && cr.classslot == dateitem).FirstOrDefault();
                                    if (cReport == null)
                                    {
                                        TodayClasses t = new TodayClasses();
                                        t.slot = dateitem;
                                        Student std = db.Students.Where(ss => ss.email == item.studentemail).FirstOrDefault();
                                        t.name = std.name;
                                        Course cc = db.Courses.Where(sc => sc.courseid == item.courseid).FirstOrDefault();
                                        t.coursename = cc.coursename;
                                        t.isPreSchedule = false;
                                        t.isReschedule = false;
                                        t.isStudent = false;
                                        t.classDate = currentDate;
                                        t.semail = item.studentemail;
                                        t.temail = item.tutoremail;
                                        tclass.Add(t);
                                    }
                                    else
                                    {
                                        if (cReport.classesstatus == 2)
                                        {
                                            NewReschedule rschedule = db.NewReschedules.Where(rs => rs.classreportid == cReport.id && rs.rescheduleclassTo == currentDate && rs.rescheduledclassstatus != 1).FirstOrDefault();
                                            if (rschedule != null)
                                            {
                                                TodayClasses t = new TodayClasses();
                                                t.slot = dateitem;
                                                Student std = db.Students.Where(ss => ss.email == item.studentemail).FirstOrDefault();
                                                t.name = std.name;
                                                Course cc = db.Courses.Where(sc => sc.courseid == item.courseid).FirstOrDefault();
                                                t.coursename = cc.coursename;
                                                if (rschedule.classStatus == 0)
                                                {
                                                    t.isReschedule = true;
                                                    t.isPreSchedule = false;
                                                    t.isStudent = false;

                                                }
                                                else if (rschedule.classStatus == 2)
                                                {
                                                    t.isReschedule = false;
                                                    t.isPreSchedule = true;
                                                    t.isStudent = false;
                                                }
                                                else if (rschedule.classStatus == 7)
                                                {
                                                    t.isReschedule = false;
                                                    t.isPreSchedule = false;
                                                    t.isStudent = true;
                                                }
                                                t.classDate = currentDate;
                                                t.semail = item.studentemail;
                                                t.temail = item.tutoremail;
                                                tclass.Add(t);
                                            }
                                        }
                                    }

                                }

                            }

                        }
                    }
                }
                return tclass;

            }
            return tclass;
        }
        //Autocancel Class
        private void StartCancelTimer(string email)
        {
            // Cancel any previous timer if it exists
            //StopCancelTimer();
            int minutes = 100;
            int millisecondsInMinute = (int)TimeSpan.FromMinutes(minutes).TotalMilliseconds;
            // Create a new timer
            cancelTimer = new Timer();
            cancelTimer.Interval = millisecondsInMinute;
            cancelTimer.Elapsed += delegate { AutocancelClass(email); };
            cancelTimer.AutoReset = false; // Only trigger once
            // Start the timer
            cancelTimer.Start();
        }

        private void StopCancelTimer()
        {
            if (cancelTimer != null)
            {
                cancelTimer.Stop();
                cancelTimer.Elapsed -= delegate { AutocancelClass(""); }; // Remove the anonymous method
                cancelTimer.Dispose();
                cancelTimer = null;
            }
        }

        private void AutocancelClass(string email)
        {
            // Perform autocancelClass logic using the email parameter
            var classes = GetTodayClasses(email);
            if (classes != null)
            {
                foreach (var item in classes)
                {
                    if (!item.isPreSchedule && !item.isPreSchedule && !item.isStudent)
                    {
                        Course co = db.Courses.Where(ca => ca.coursename == item.coursename).FirstOrDefault();
                        CourseEnrolled ce = db.CourseEnrolleds.Where(cc => cc.studentemail == item.semail && cc.tutoremail == item.temail && cc.courseid == co.courseid && cc.coursestatus == 0).FirstOrDefault();
                        ClassReport cr = db.ClassReports.Where(c => c.classdate == item.classDate && c.classslot == item.slot && c.courseenrolledid == ce.id).FirstOrDefault();
                        if (cr == null)
                        {
                            ClassReport crAdd = new ClassReport();
                            crAdd.courseenrolledid = ce.id;
                            crAdd.classesstatus = 0;
                            crAdd.classdate = item.classDate;
                            crAdd.classslot = item.slot;
                            String hour = DateTime.Now.TimeOfDay.Hours.ToString();
                            String minute = DateTime.Now.TimeOfDay.Minutes.ToString();
                            String stamp = "AM";
                            if (int.Parse(hour) <= 12)
                            {
                                stamp = "AM";
                            }
                            else
                            {
                                stamp = "PM";
                            }
                            if (int.Parse(hour) < 10)
                            {
                                hour = "0" + hour;
                            }

                            crAdd.classtime = hour + ":" + minute + " " + stamp;
                            crAdd.classTakenDate = GetCurrentDate();
                            db.ClassReports.Add(crAdd);
                            db.SaveChanges();
                        }
                    }
                }
                // Stop the cancel timer
            }
            StopCancelTimer();

        }
    }
}
