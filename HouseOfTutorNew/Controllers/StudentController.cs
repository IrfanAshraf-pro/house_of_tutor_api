using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;
using HouseOfTutorNew.Models.CustomClasses;
using HouseOfTutorNew.Models;

namespace HouseOfTutorNew.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class StudentController : ApiController
    {
        houseoftutorEntities db = new houseoftutorEntities();
        [HttpPost]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public HttpResponseMessage StudentSignup(Student s)
        {
            try
            {
                var v = db.Students.Where(i => i.email.ToLower() == s.email.ToLower()).FirstOrDefault();
                var t = db.Tutors.Where(i => i.email.ToLower() == s.email.ToLower()).FirstOrDefault();
                if (t == null)
                {
                    if (v == null)
                    {
                        Student std = new Student();
                        std.name = s.name;
                        std.email = s.email;
                        std.password = s.password;
                        std.semester = s.semester;
                        std.contact = s.contact;
                        std.cgpa = s.cgpa;
                        std.fathercnic = s.fathercnic;
                        std.gender = s.gender;
                        db.Students.Add(std);
                        db.SaveChanges();
                        String details = "0000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000";
                        StudentSchedule(s.email, details);
                        db.SaveChanges();
                        setting se = new setting();
                        se.email = s.email;
                        se.showAbsents = 1;
                        se.showClassReport = 1;
                        se.showRescheduleStudent = 1;
                        se.showRescheduleTutor = 1;
                        db.settings.Add(se);
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
                    return Request.CreateResponse(HttpStatusCode.OK, "You Can't Sign Up as Student Because You're already Tutor");
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
                var user = db.Students.Where(x => x.email == email).FirstOrDefault();
                user.image = filename;
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, "Image Uploaded Successfuly");
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, e.Message);
            }
        }
        //student Settings
        [HttpGet]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public HttpResponseMessage GetStudentSettings(String email)
        {
            try
            {
                Student s = db.Students.Where(ss => ss.email == email).FirstOrDefault();
                if (s != null)
                {
                    setting se = db.settings.Where(set => set.email == email).FirstOrDefault();
                    return Request.CreateResponse(HttpStatusCode.OK, new { se.id, se.email, se.showAbsents, se.showClassReport, se.showRescheduleStudent, se.showRescheduleTutor });
                }
                else
                    return Request.CreateResponse(HttpStatusCode.OK, "Student Doesnot Exist");

            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, ex.Message);
            }
        }
        [HttpPost]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public HttpResponseMessage updateStudentSettings(setting s)
        {
            try
            {
                Student std = db.Students.Where(ss => ss.email == s.email).FirstOrDefault();
                if (std != null)
                {

                    setting se = db.settings.Where(set => set.id == s.id).FirstOrDefault();
                    if (se != null)
                    {
                        se.id = s.id;
                        se.email = s.email;
                        se.showAbsents = s.showAbsents;
                        se.showClassReport = s.showClassReport;
                        se.showRescheduleStudent = s.showRescheduleStudent;
                        se.showRescheduleTutor = s.showRescheduleTutor;
                        db.SaveChanges();
                        return Request.CreateResponse(HttpStatusCode.OK, "Setting update Successfully");

                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, "Setting Doesnot Exist");
                    }
                }
                else
                    return Request.CreateResponse(HttpStatusCode.OK, "Student Doesnot Exist");

            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, ex.Message);
            }
        }
        [HttpPost]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public HttpResponseMessage StudentSchedule(String email, String details)
        {
            try
            {
                Student u = db.Students.Where(s => s.email == email).FirstOrDefault();
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
        //updating single slot in schedule
        [HttpPost]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public HttpResponseMessage UpdateStudentSlot(String email, String slots)
        {
            try
            {
                Student u = db.Students.Where(s => s.email == email).FirstOrDefault();
                if (u.scheduleid == null)
                {

                    return Request.CreateResponse(HttpStatusCode.OK, "Schedule Doesnot Exist");
                }
                else
                {
                    Schedule sd = db.Schedules.Where(ss => ss.scheduleid == u.scheduleid).FirstOrDefault();
                    //updating student schedule
                    string[] alotArr = slots.Split(',');
                    String s = sd.details;
                    char[] sr = s.ToCharArray();
                    int ival = 0;
                    foreach (var item in alotArr)
                    {
                        ival = int.Parse(item.ToString());
                        sr[ival - 1] = '1';
                    }
                    String sn = "";
                    for (int i = 0; i < s.Length; i++)
                    {
                        sn = String.Concat(sn, sr[i]);
                    }
                    sd.details = sn;
                    db.SaveChanges();
                    return Request.CreateResponse(HttpStatusCode.OK, "Schedule Updated successfully");

                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, ex.Message);
            }
        }

        [HttpGet]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public HttpResponseMessage GetStudentSchedule(String email)
        {
            try
            {
                Student u = db.Students.Where(s => s.email == email).FirstOrDefault();
                if (u.scheduleid == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, "Schedule not set");
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, GetSSchedule(email));
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, ex.Message);
            }
        }
        //added relevenat courses
        [HttpGet]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public HttpResponseMessage GetCoursesList(string email)
        {
            try
            {
                Student std = db.Students.Where(st => st.email == email).FirstOrDefault();
                if (std != null)
                {
                    var semesterCourses = db.SemesterCourses.Where(sc => sc.semesterNo == std.semester).ToList();
                    bool check = false;
                    List<Course> listCourse = new List<Course>();
                    var enrolledCourses = from c in db.StudentCourseLists where c.studentemail == email select new { c.courseid, c.studentemail };
                    foreach (var c in semesterCourses)
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
                            Course course = db.Courses.Where(cc => cc.courseid == c.courseid).FirstOrDefault();
                            Course cs = new Course();
                            cs.coursecode = course.coursecode;
                            cs.coursefee = course.coursefee;
                            cs.courseid = course.courseid;
                            cs.coursename = course.coursename;
                            listCourse.Add(cs);
                        }
                        check = false;
                    }

                    return Request.CreateResponse(HttpStatusCode.OK, listCourse.Select(s => new { s.courseid, s.coursename, s.coursefee, s.coursecode }).ToList());
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, "Student Doesnot Exist");

                }
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, e.Message);
            }
        }
        //getting failed Courses
        [HttpGet]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public HttpResponseMessage GettingFailedCoursesAll(string email)
        {
            try
            {
                Student std = db.Students.Where(st => st.email == email).FirstOrDefault();
                if (std != null)
                {
                    var semesterCourses = db.SemesterCourses.Where(sc => sc.semesterNo < std.semester).ToList();
                    bool check = false;
                    List<Course> listCourse = new List<Course>();
                    var enrolledCourses = from c in db.StudentCourseLists where c.studentemail == email select new { c.courseid, c.studentemail };
                    foreach (var c in semesterCourses)
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
                            Course course = db.Courses.Where(cc => cc.courseid == c.courseid).FirstOrDefault();
                            Course cs = new Course();
                            cs.coursecode = course.coursecode;
                            cs.coursefee = course.coursefee;
                            cs.courseid = course.courseid;
                            cs.coursename = course.coursename;
                            listCourse.Add(cs);
                        }
                        check = false;
                    }

                    return Request.CreateResponse(HttpStatusCode.OK, listCourse.Select(s => new { s.courseid, s.coursename, s.coursefee, s.coursecode }).ToList());
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, "Student Doesnot Exist");

                }
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, e.Message);
            }
        }
        [HttpPost]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public HttpResponseMessage StudentCourseEnlist(String semail, int cid)
        {
            try
            {
                Student st = db.Students.Where(s => s.email == semail).FirstOrDefault();
                if (st != null)
                {
                    var o = db.StudentCourseLists.Where(q => q.studentemail == semail && q.courseid == cid).FirstOrDefault();
                    if (o == null)
                    {
                        StudentCourseList s = new StudentCourseList();
                        s.courseid = cid;
                        s.studentemail = semail;
                        db.StudentCourseLists.Add(s);
                        db.SaveChanges();
                        return Request.CreateResponse(HttpStatusCode.OK, "Course Enlisted Successfully");
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, "Course Already Enlisted");

                    }
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, "Student doesnot exist");

                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, ex.Message);
            }
        }
        [HttpGet]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public HttpResponseMessage GetStudentEnlistedCourses(String semail)
        {
            try
            {
                List<StudentEnlistedCustom> courseList = new List<StudentEnlistedCustom>();
                Student st = db.Students.Where(s => s.email == semail).FirstOrDefault();
                if (st != null)
                {
                    var cl = from c in db.StudentCourseLists where c.studentemail == semail select new { c.courseid };
                    foreach (var item in cl)
                    {
                        StudentEnlistedCustom cc = new StudentEnlistedCustom();
                        var courseL = db.Courses.Where(c => c.courseid == item.courseid).FirstOrDefault();
                        cc.coursecode = courseL.coursecode;
                        cc.coursefee = int.Parse(courseL.coursefee.ToString());
                        cc.courseid = courseL.courseid;
                        cc.coursename = courseL.coursename;
                        var enrolled = db.CourseEnrolleds.Where(s => s.studentemail == semail && s.courseid == item.courseid && s.coursestatus == 0).FirstOrDefault();
                        if (enrolled == null)
                        {
                            cc.isLearning = 0;
                        }
                        else
                        {
                            cc.isLearning = 1;
                        }
                        courseList.Add(cc);
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, courseList);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, "No course enlisted");

                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, ex.Message);
            }
        }

        [HttpGet]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public HttpResponseMessage FindTutor(String semail, int cid, String noOfSlots)
        {
            try
            {
                Student st = db.Students.Where(s => s.email == semail).FirstOrDefault();

                if (st != null)
                {
                    String s = GetSSchedule(semail);
                    if (s != null)
                    {
                        List<Tutor> tutorlist = new List<Tutor>();
                        List<Tutor> matchedTutor = new List<Tutor>();
                        List<TutorMatched> rList = new List<TutorMatched>();

                        var tlist = db.TutorCourseLists.Where(tc => tc.courseid == cid && tc.type != 0).ToList();
                        if (tlist != null && tlist.Count > 0)
                        {
                            foreach (var item in tlist)
                            {
                                Tutor t = db.Tutors.Where(tt => tt.email == item.tutoremail).FirstOrDefault();
                                tutorlist.Add(t);
                            }
                            foreach (var item in tutorlist)
                            {
                                String tschedule = GetTTSchedule(item.email);
                                String slotMatched = "";
                                int j = 0;
                                for (int i = 0; i < tschedule.Length; i++)
                                {
                                    if (tschedule[i].Equals('1') && s[i].Equals('1'))
                                    {
                                        j++;
                                        slotMatched += (i + 1) + ",";
                                    }

                                }
                                //if (j >= int.Parse(noOfSlots))
                                {
                                    List<TutorCourseList> lt = db.TutorCourseLists.Where(ss => ss.tutoremail == item.email && ss.isSelected == "true").ToList();
                                    SubjectGroup selected = db.SubjectGroups.Where(sb => sb.courseid == cid).FirstOrDefault();
                                    List<SubjectGroup> grouplist = db.SubjectGroups.Where(gp => gp.groupid == selected.groupid).ToList();
                                    List<String> tutorgroup = new List<String>();

                                    foreach (var gitem in grouplist)
                                    {
                                        foreach (var tutoritem in lt)
                                        {
                                            if (gitem.courseid == tutoritem.courseid && tutoritem.isSelected != "False")
                                            {
                                                Course c = db.Courses.Where(cc => cc.courseid == tutoritem.courseid).FirstOrDefault();
                                                SemesterCourse sr = db.SemesterCourses.Where(ss => ss.courseid == tutoritem.courseid).FirstOrDefault();
                                                var result = db.Results.Where(r => r.email == item.email && r.semesterId == sr.id).FirstOrDefault();
                                                String data = c.coursename + "," + result.grade;
                                                tutorgroup.Add(data);
                                            }
                                        }
                                    }

                                    TutorMatched tt = new TutorMatched();
                                    tt.email = item.email;
                                    tt.name = item.name;
                                    tt.cgpa = item.cgpa.ToString();
                                    tt.semester = item.semester.ToString();
                                    tt.slotMatched = slotMatched;
                                    tt.noOfSlots = int.Parse(noOfSlots);
                                    tt.coursegroup = tutorgroup;
                                    var tutorRating = db.Rates.Where(ss => ss.courseid == cid && ss.tutoremail == item.email).ToList();
                                    if (tutorRating != null && tutorRating.Count > 0)
                                    {
                                        float r = 0;
                                        foreach (var ratingItem in tutorRating)
                                        {
                                            r += float.Parse(ratingItem.rating.ToString());
                                        }
                                        r = r / tutorRating.Count;
                                        tt.rating = r.ToString();
                                        tt.ratingCount = tutorRating.Count;
                                    }
                                    else
                                    {
                                        tt.rating = "NA";
                                        tt.ratingCount = 0;
                                    }
                                    rList.Add(tt);
                                    matchedTutor.Add(item);
                                }

                            }
                            if (matchedTutor != null)
                            {
                                List<TutorMatched> returnList = new List<TutorMatched>();
                                foreach (var item in rList)
                                {
                                    foreach (var tcourse in tlist)
                                    {
                                        if (item.email == tcourse.tutoremail)
                                        {
                                            BlockedTutor bl = db.BlockedTutors.Where(b => b.email == item.email).FirstOrDefault();
                                            var tutorEnrolled = db.CourseEnrolleds.Where(c => c.tutoremail == item.email && c.coursestatus == 0).ToList();
                                            if (tutorEnrolled.Count > 0)
                                            {
                                                List<TutorMatchedMessage> msgList = new List<TutorMatchedMessage>();
                                                foreach (var eritem in tutorEnrolled)
                                                {
                                                    var reportItems = db.ClassReports.Where(r => r.courseenrolledid == eritem.id && r.classesstatus == 2).ToList();
                                                    if (reportItems.Count > 0)
                                                    {
                                                        foreach (var reitem in reportItems)
                                                        {
                                                            NewReschedule nr = db.NewReschedules.Where(nre => nre.classreportid == reitem.id && nre.rescheduledclassstatus == 0).FirstOrDefault();
                                                            if (nr != null)
                                                            {
                                                                string[] matchedslots = item.slotMatched.Split(',');
                                                                for (int i = 0; i < matchedslots.Length - 1; i++)
                                                                {
                                                                    if ((nr.slotTo + 1) == int.Parse(matchedslots[i]))
                                                                    {
                                                                        TutorMatchedMessage mssg = new TutorMatchedMessage();
                                                                        mssg.message = "The tutor is not available till date " + nr.rescheduleclassTo + " for slot " + slotCondition(int.Parse(nr.slotTo.ToString()));
                                                                        mssg.date = nr.rescheduleclassTo;
                                                                        mssg.slot = int.Parse((nr.slotTo + 1).ToString());
                                                                        msgList.Add(mssg);
                                                                    }
                                                                }

                                                            }
                                                        }
                                                    }
                                                }
                                                if (bl == null)
                                                {
                                                    TutorMatched tr = new TutorMatched();
                                                    tr = item;
                                                    int semesterNo = int.Parse(item.semester);
                                                    SemesterCourse sr = db.SemesterCourses.Where(ss => ss.courseid == cid).FirstOrDefault();
                                                    var result = db.Results.Where(r => r.email == item.email && r.semesterId == sr.id).FirstOrDefault();
                                                    tr.grade = result.grade.Length > 0 ? result.grade : "";
                                                    tr.message = msgList;
                                                    returnList.Add(tr);
                                                }
                                                else if (bl.isBlock == 0)
                                                {
                                                    TutorMatched tr = new TutorMatched();
                                                    tr = item;
                                                    int semesterNo = int.Parse(item.semester);
                                                    SemesterCourse sr = db.SemesterCourses.Where(ss => ss.courseid == cid).FirstOrDefault();
                                                    var result = db.Results.Where(r => r.email == item.email && r.semesterId == sr.id).FirstOrDefault();
                                                    tr.grade = result.grade.Length > 0 ? result.grade : "";
                                                    tr.message = msgList;
                                                    returnList.Add(tr);
                                                }
                                            }
                                            else
                                            {
                                                if (bl == null)
                                                {
                                                    TutorMatched tr = new TutorMatched();
                                                    tr = item;
                                                    int semesterNo = int.Parse(item.semester);
                                                    SemesterCourse sr = db.SemesterCourses.Where(ss => ss.courseid == cid).FirstOrDefault();
                                                    var result = db.Results.Where(r => r.email == item.email && r.semesterId == sr.id).FirstOrDefault();
                                                    tr.grade = result.grade.Length > 0 ? result.grade : "";
                                                    tr.message = null;
                                                    returnList.Add(tr);
                                                }
                                                else if (bl.isBlock == 0)
                                                {
                                                    TutorMatched tr = new TutorMatched();
                                                    tr = item;
                                                    int semesterNo = int.Parse(item.semester);
                                                    SemesterCourse sr = db.SemesterCourses.Where(ss => ss.courseid == cid).FirstOrDefault();
                                                    var result = db.Results.Where(r => r.email == item.email && r.semesterId == sr.id).FirstOrDefault();
                                                    tr.grade = result.grade.Length > 0 ? result.grade : "";
                                                    tr.message = null;
                                                    returnList.Add(tr);
                                                }
                                            }

                                        }
                                    }
                                }
                                return Request.CreateResponse(HttpStatusCode.OK, returnList.OrderBy(ss => ss.rating).ThenBy(tb => tb.grade).ToList());

                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.OK, "No tutor available");

                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, "No tutor available");

                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, "Schedule not set");

                    }

                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, "Student not found");

                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, ex.Message);
            }
        }

        //Task Api
        [HttpGet]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public HttpResponseMessage FindTutorParent(int cid)
        {
            try
            {


                List<Tutor> tutorlist = new List<Tutor>();
                List<Tutor> matchedTutor = new List<Tutor>();
                List<ParentTutorMatched> rList = new List<ParentTutorMatched>();

                var tlist = db.TutorCourseLists.Where(tc => tc.courseid == cid && tc.type != 0).ToList();
                if (tlist != null && tlist.Count > 0)
                {
                    foreach (var item in tlist)
                    {
                        Tutor t = db.Tutors.Where(tt => tt.email == item.tutoremail).FirstOrDefault();
                        tutorlist.Add(t);
                    }
                    foreach (var item in tutorlist)
                    {
                        String tschedule = GetTTSchedule(item.email);
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
                        //if (j >= int.Parse(noOfSlots))
                        {
                            List<TutorCourseList> lt = db.TutorCourseLists.Where(ss => ss.tutoremail == item.email && ss.isSelected == "true").ToList();
                            SubjectGroup selected = db.SubjectGroups.Where(sb => sb.courseid == cid).FirstOrDefault();
                            List<SubjectGroup> grouplist = db.SubjectGroups.Where(gp => gp.groupid == selected.groupid).ToList();
                            List<String> tutorgroup = new List<String>();

                            foreach (var gitem in grouplist)
                            {
                                foreach (var tutoritem in lt)
                                {
                                    if (gitem.courseid == tutoritem.courseid && tutoritem.isSelected != "False")
                                    {
                                        Course c = db.Courses.Where(cc => cc.courseid == tutoritem.courseid).FirstOrDefault();
                                        SemesterCourse sr = db.SemesterCourses.Where(ss => ss.courseid == tutoritem.courseid).FirstOrDefault();
                                        var result = db.Results.Where(r => r.email == item.email && r.semesterId == sr.id).FirstOrDefault();
                                        String data = c.coursename + "," + result.grade;
                                        tutorgroup.Add(data);
                                    }
                                }
                            }

                            ParentTutorMatched tt = new ParentTutorMatched();
                            tt.email = item.email;
                            tt.name = item.name;
                            tt.cgpa = item.cgpa.ToString();
                            tt.semester = item.semester.ToString();
                            tt.slotMatched = slotMatched;
                            tt.coursegroup = tutorgroup;
                            var tutorRating = db.Rates.Where(ss => ss.courseid == cid && ss.tutoremail == item.email).ToList();
                            if (tutorRating != null && tutorRating.Count > 0)
                            {
                                float r = 0;
                                foreach (var ratingItem in tutorRating)
                                {
                                    r += float.Parse(ratingItem.rating.ToString());
                                }
                                r = r / tutorRating.Count;
                                tt.rating = r.ToString();
                                tt.ratingCount = tutorRating.Count;
                            }
                            else
                            {
                                tt.rating = "NA";
                                tt.ratingCount = 0;
                            }
                            rList.Add(tt);
                            matchedTutor.Add(item);
                        }

                    }
                    if (matchedTutor != null)
                    {
                        List<ParentTutorMatched> returnList = new List<ParentTutorMatched>();
                        foreach (var item in rList)
                        {
                            foreach (var tcourse in tlist)
                            {
                                if (item.email == tcourse.tutoremail)
                                {
                                    var tutorEnrolled = db.CourseEnrolleds.Where(c => c.tutoremail == item.email && c.coursestatus == 0).ToList();
                                    if (tutorEnrolled.Count > 0)
                                    {
                                        List<TutorMatchedMessage> msgList = new List<TutorMatchedMessage>();
                                        foreach (var eritem in tutorEnrolled)
                                        {
                                            var reportItems = db.ClassReports.Where(r => r.courseenrolledid == eritem.id && r.classesstatus == 2).ToList();
                                            if (reportItems.Count > 0)
                                            {
                                                foreach (var reitem in reportItems)
                                                {
                                                    NewReschedule nr = db.NewReschedules.Where(nre => nre.classreportid == reitem.id && nre.rescheduledclassstatus == 0).FirstOrDefault();
                                                    if (nr != null)
                                                    {
                                                        string[] matchedslots = item.slotMatched.Split(',');
                                                        for (int i = 0; i < matchedslots.Length - 1; i++)
                                                        {
                                                            if ((nr.slotTo + 1) == int.Parse(matchedslots[i]))
                                                            {
                                                                TutorMatchedMessage mssg = new TutorMatchedMessage();
                                                                mssg.message = "The tutor is not available till date " + nr.rescheduleclassTo + " for slot " + slotCondition(int.Parse(nr.slotTo.ToString()));
                                                                mssg.date = nr.rescheduleclassTo;
                                                                mssg.slot = int.Parse((nr.slotTo + 1).ToString());
                                                                msgList.Add(mssg);
                                                            }
                                                        }

                                                    }
                                                }
                                            }
                                        }

                                        ParentTutorMatched tr = new ParentTutorMatched();
                                        tr = item;
                                        int semesterNo = int.Parse(item.semester);
                                        SemesterCourse sr = db.SemesterCourses.Where(ss => ss.courseid == cid).FirstOrDefault();
                                        var result = db.Results.Where(r => r.email == item.email && r.semesterId == sr.id).FirstOrDefault();
                                        tr.grade = result.grade.Length > 0 ? result.grade : "";
                                        tr.message = msgList;
                                        returnList.Add(tr);
                                    }
                                    else
                                    {
                                        ParentTutorMatched tr = new ParentTutorMatched();
                                        tr = item;
                                        int semesterNo = int.Parse(item.semester);
                                        SemesterCourse sr = db.SemesterCourses.Where(ss => ss.courseid == cid).FirstOrDefault();
                                        var result = db.Results.Where(r => r.email == item.email && r.semesterId == sr.id).FirstOrDefault();
                                        tr.grade = result.grade.Length > 0 ? result.grade : "";
                                        tr.message = new List<TutorMatchedMessage>();
                                        returnList.Add(tr);
                                    }
                                }
                            }
                        }
                        return Request.CreateResponse(HttpStatusCode.OK, returnList.OrderBy(ss => ss.rating).ThenBy(tb => tb.grade).ToList());
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, "No tutor available");
                    }
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, "No tutor available");
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, ex.Message);
            }
        }

        [HttpGet]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public HttpResponseMessage GetCourses()
        {
            try
            {
                var courses = from c in db.Courses select new { c.courseid, c.coursecode, c.coursename, c.coursefee, c.coursefullname };
                return Request.CreateResponse(HttpStatusCode.OK, courses.ToList());
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, e.Message);
            }
        }

        [HttpGet]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public HttpResponseMessage GetTutorsLearning(String semail)
        {
            try
            {
                var enrolled = db.CourseEnrolleds.Where(t => t.studentemail == semail && t.coursestatus == 0).ToList();
                if (enrolled != null && enrolled.Count > 0)
                {
                    List<TeachingCustom> teaching = new List<TeachingCustom>();
                    foreach (var item in enrolled)
                    {
                        TeachingCustom c = new TeachingCustom();
                        Course co = db.Courses.Where(cc => cc.courseid == item.courseid).FirstOrDefault();
                        //Student s = db.Students.Where(ss => ss.email == item.studentemail).FirstOrDefault();
                        Tutor t = db.Tutors.Where(tt => tt.email == item.tutoremail).FirstOrDefault();
                        c.studentemail = item.studentemail;
                        c.tutoremail = item.tutoremail;
                        c.courseid = int.Parse(item.courseid.ToString());
                        c.coursestatus = int.Parse(item.coursestatus.ToString());
                        c.coursename = co.coursename;
                        c.studentname = t.name;
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
        [HttpGet]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public HttpResponseMessage GetTutorsForExtraSession(int courseenrollid, int courseid)
        {
            try
            {
                CourseEnrolled ce = db.CourseEnrolleds.Where(ss => ss.id == courseenrollid && ss.coursestatus == 0).FirstOrDefault();
                if (ce != null)
                {
                    Tutor t = db.Tutors.Where(tt => tt.email == ce.tutoremail).FirstOrDefault();
                    Student ss = db.Students.Where(sss => sss.email == ce.studentemail).FirstOrDefault();
                    //Schedule stdSchedule = db.Schedules.Where(sc => sc.scheduleid == s.scheduleid).FirstOrDefault();
                    //Schedule tSchedule = db.Schedules.Where(ts => ts.scheduleid == t.scheduleid).FirstOrDefault();
                    String sScheudle = GetSSchedule(ss.email);
                    String tSchedule = GetTTSchedule(t.email);
                    String slotMatched = "";
                    int j = 0;
                    for (int i = 0; i < tSchedule.Length; i++)
                    {
                        if (tSchedule[i].Equals('1') && sScheudle[i].Equals('1'))
                        {
                            j++;
                            slotMatched += (i + 1) + ",";
                        }
                    }
                    if (j > 0)
                    {
                        TutorMatched ptm = new TutorMatched();
                        ptm.name = t.name;
                        ptm.email = t.email;
                        ptm.cgpa = t.cgpa.ToString();
                        ptm.semester = t.semester.ToString();
                        SemesterCourse sr = db.SemesterCourses.Where(tss => tss.courseid == courseid).FirstOrDefault();
                        var result = db.Results.Where(r => r.email == ce.tutoremail && r.semesterId == sr.id).FirstOrDefault();
                        ptm.grade = result.grade.Length > 0 ? result.grade : "";
                        ptm.slotMatched = slotMatched;
                        var tutorRating = db.Rates.Where(rss => rss.courseid == courseid && rss.tutoremail == ce.tutoremail).ToList();
                        if (tutorRating != null && tutorRating.Count > 0)
                        {
                            float r = 0;
                            foreach (var ratingItem in tutorRating)
                            {
                                r += float.Parse(ratingItem.rating.ToString());
                            }
                            r = r / tutorRating.Count;
                            ptm.rating = r.ToString();
                            ptm.ratingCount = tutorRating.Count;
                        }
                        else
                        {
                            ptm.rating = "NA";
                            ptm.ratingCount = 0;
                        }
                        ptm.message = new List<TutorMatchedMessage>();
                        ptm.coursegroup = new List<string>();
                        return Request.CreateResponse(HttpStatusCode.OK, ptm);
                        //Send back this tutor
                    }
                    else
                    {
                        String s = GetSSchedule(ss.email);
                        List<Tutor> tutorlist = new List<Tutor>();
                        List<Tutor> matchedTutor = new List<Tutor>();
                        List<TutorMatched> rList = new List<TutorMatched>();
                        var tlist = db.TutorCourseLists.Where(tc => tc.courseid == courseid && tc.type != 0).ToList();
                        if (tlist != null && tlist.Count > 0)
                        {
                            foreach (var item in tlist)
                            {
                                Tutor ttt = db.Tutors.Where(tt => tt.email == item.tutoremail).FirstOrDefault();
                                tutorlist.Add(ttt);
                            }
                            foreach (var item in tutorlist)
                            {
                                String tschedule = GetTTSchedule(item.email);
                                String slotMatchedd = "";
                                int jj = 0;
                                for (int i = 0; i < tschedule.Length; i++)
                                {
                                    if (tschedule[i].Equals('1') && s[i].Equals('1'))
                                    {
                                        jj++;
                                        slotMatchedd += (i + 1) + ",";
                                    }

                                }
                                if (jj > 0)
                                {
                                    List<TutorCourseList> lt = db.TutorCourseLists.Where(sds => sds.tutoremail == item.email && sds.isSelected == "true").ToList();
                                    SubjectGroup selected = db.SubjectGroups.Where(sb => sb.courseid == courseid).FirstOrDefault();
                                    List<SubjectGroup> grouplist = db.SubjectGroups.Where(gp => gp.groupid == selected.groupid).ToList();
                                    List<String> tutorgroup = new List<String>();

                                    foreach (var gitem in grouplist)
                                    {
                                        foreach (var tutoritem in lt)
                                        {
                                            if (gitem.courseid == tutoritem.courseid && tutoritem.isSelected != "False")
                                            {
                                                Course c = db.Courses.Where(cc => cc.courseid == tutoritem.courseid).FirstOrDefault();
                                                SemesterCourse sr = db.SemesterCourses.Where(ssr => ssr.courseid == tutoritem.courseid).FirstOrDefault();
                                                var result = db.Results.Where(r => r.email == item.email && r.semesterId == sr.id).FirstOrDefault();
                                                String data = c.coursename + "," + result.grade;
                                                tutorgroup.Add(data);
                                            }
                                        }
                                    }

                                    TutorMatched tt = new TutorMatched();
                                    tt.email = item.email;
                                    tt.name = item.name;
                                    tt.cgpa = item.cgpa.ToString();
                                    tt.semester = item.semester.ToString();
                                    tt.slotMatched = slotMatchedd;
                                    tt.noOfSlots = 0;
                                    tt.coursegroup = tutorgroup;
                                    SemesterCourse srf = db.SemesterCourses.Where(tss => tss.courseid == courseid).FirstOrDefault();
                                    var resultt = db.Results.Where(r => r.email == item.email && r.semesterId == srf.id).FirstOrDefault();
                                    tt.grade = resultt.grade.Length > 0 ? resultt.grade : "";
                                    var tutorRating = db.Rates.Where(ssrt => ssrt.courseid == courseid && ssrt.tutoremail == item.email).ToList();
                                    if (tutorRating != null && tutorRating.Count > 0)
                                    {
                                        float r = 0;
                                        foreach (var ratingItem in tutorRating)
                                        {
                                            r += float.Parse(ratingItem.rating.ToString());
                                        }
                                        r = r / tutorRating.Count;
                                        tt.rating = r.ToString();
                                        tt.ratingCount = tutorRating.Count;
                                    }
                                    else
                                    {
                                        tt.rating = "NA";
                                        tt.ratingCount = 0;
                                    }
                                    tt.message = new List<TutorMatchedMessage>();
                                    rList.Add(tt);
                                    matchedTutor.Add(item);
                                }
                            }
                            if (rList != null && rList.Count>0)
                            {

                                return Request.CreateResponse(HttpStatusCode.OK, rList.OrderBy(sws => sws.rating).ThenBy(tb => tb.grade).ToList());
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.OK, "No tutor available");
                            }

                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, "No tutor available for this course");
                        }
                    }
                }
                else
                {

                    return Request.CreateResponse(HttpStatusCode.OK, "No tutor available for this course");
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, ex.Message);
            }
        }
        [HttpPost]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public HttpResponseMessage SendRequestToTutorTemporary(String semail, String temail, int cid, String slot, int noOfWeek)
        {
            try
            {
                var sres = db.StudentRequestTemporaryEnrollments.Where(sr => sr.studentemail == semail && sr.tutoremail == temail && sr.courseid == cid).FirstOrDefault();
                if (sres == null)
                {
                    StudentRequestTemporaryEnrollment s = new StudentRequestTemporaryEnrollment();
                    s.studentemail = semail;
                    s.tutoremail = temail;
                    s.courseid = cid;
                    s.studentrequeststatus = 0;
                    s.slot = slot;
                    s.requestDate = GetCurrentDate();
                    s.requestTime = getCurrentTime();
                    s.dateToBeEnrolled = GetCurrentDate();
                    DateTime dateEE = DateTime.Now;
                    DateTime newDate = new DateTime();
                    if (noOfWeek == 1)
                    {
                        newDate=dateEE.AddDays(7);

                    }else if (noOfWeek == 2)
                    {
                        newDate = dateEE.AddDays(7);
                    }
                    else
                    {
                        newDate = dateEE.AddDays(7);
                    }
                    s.dateToEnd = GetCurrentDateFromDate(newDate);
                    db.StudentRequestTemporaryEnrollments.Add(s);
                    db.SaveChanges();
                    //Task.Run(() => AutoRejectStudentRequest(semail, temail, cid, slot, s.requestDate));
                    return Request.CreateResponse(HttpStatusCode.OK, "Request sent Successfully");
                }
                else
                {
                    if (sres.studentrequeststatus == 2 || sres.studentrequeststatus == 1)
                    {
                        sres.studentemail = semail;
                        sres.tutoremail = temail;
                        sres.courseid = cid;
                        sres.studentrequeststatus = 0;
                        sres.slot = slot;
                        sres.requestDate = GetCurrentDate();
                        sres.requestTime = getCurrentTime();
                        sres.dateToBeEnrolled = GetCurrentDate();
                        DateTime dateEE = DateTime.Now;
                        DateTime newDate = new DateTime();
                        if (noOfWeek == 1)
                        {
                            newDate = dateEE.AddDays(7);

                        }
                        else if (noOfWeek == 2)
                        {
                            newDate = dateEE.AddDays(7);
                        }
                        else
                        {
                            newDate = dateEE.AddDays(7);
                        }
                        sres.dateToEnd = GetCurrentDateFromDate(newDate);
                        db.SaveChanges();
                        //Task.Run(() => AutoRejectStudentRequest(semail, temail, cid, slot, sres.requestDate));
                        return Request.CreateResponse(HttpStatusCode.OK, "Request sent Successfully");
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, "Already Requested");
                    }
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

        }

        public String GetCurrentDateFromDate(DateTime dd)
        {
            String day = dd.Date.Day.ToString();
            String month = dd.Date.Month.ToString();
            String year = dd.Date.Year.ToString();
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


        //
        [HttpPost]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public HttpResponseMessage SendRequestToTutor(String semail, String temail, int cid, String slot, string dateToBeEnrolled)
        {
            try
            {
                var sres = db.StudentRequests.Where(sr => sr.studentemail == semail && sr.tutoremail == temail && sr.courseid == cid).FirstOrDefault();
                if (sres == null)
                {
                    StudentRequest s = new StudentRequest();
                    s.studentemail = semail;
                    s.tutoremail = temail;
                    s.courseid = cid;
                    s.studentrequeststatus = 0;
                    s.slot = slot;
                    s.requestDate = GetCurrentDate();
                    s.requestTime = getCurrentTime();
                    if (dateToBeEnrolled == null)
                    {
                        s.dateToBeEnrolled = "";
                    }
                    else
                    {
                        s.dateToBeEnrolled = dateToBeEnrolled;
                    }
                    db.StudentRequests.Add(s);
                    db.SaveChanges();
                    Task.Run(() => AutoRejectStudentRequest(semail, temail, cid, slot, s.requestDate));
                    return Request.CreateResponse(HttpStatusCode.OK, "Request sent Successfully");
                }
                else
                {
                    if (sres.studentrequeststatus == 2 || sres.studentrequeststatus == 1)
                    {
                        sres.studentemail = semail;
                        sres.tutoremail = temail;
                        sres.courseid = cid;
                        sres.studentrequeststatus = 0;
                        sres.slot = slot;
                        sres.requestDate = GetCurrentDate();
                        sres.requestTime = getCurrentTime();
                        if (dateToBeEnrolled == null)
                        {
                            sres.dateToBeEnrolled = "";
                        }
                        else
                        {
                            sres.dateToBeEnrolled = dateToBeEnrolled;
                        }
                        db.SaveChanges();
                        Task.Run(() => AutoRejectStudentRequest(semail, temail, cid, slot, sres.requestDate));
                        return Request.CreateResponse(HttpStatusCode.OK, "Request sent Successfully");
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, "Already Requested");
                    }
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

        }

        [HttpGet]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public HttpResponseMessage TodayClasses(String email)
        {
            try
            {
                var cList = db.CourseEnrolleds.Where(s => s.studentemail == email && s.coursestatus == 0).ToList();
                var tempList = db.TemporaryEnrolleds.Where(t => t.tutoremail == email && t.coursestatus == 0).ToList();

                if (cList != null || tempList != null)
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
                                                    Tutor td = db.Tutors.Where(ss => ss.email == item.tutoremail).FirstOrDefault();
                                                    t.name = td.name;
                                                    Course cc = db.Courses.Where(sc => sc.courseid == item.courseid).FirstOrDefault();
                                                    t.coursename = cc.coursename;
                                                    t.isReschedule = true;
                                                    t.classDate = GetCurrentDate();
                                                    t.temail = item.tutoremail;
                                                    t.semail = email;
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
                                                Tutor td = db.Tutors.Where(ss => ss.email == item.tutoremail).FirstOrDefault();
                                                t.name = td.name;
                                                Course cc = db.Courses.Where(sc => sc.courseid == item.courseid).FirstOrDefault();
                                                t.coursename = cc.coursename;
                                                t.isReschedule = false;
                                                t.classDate = GetCurrentDate();
                                                t.temail = item.tutoremail;
                                                t.semail = email;

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
                                                        Tutor td = db.Tutors.Where(ss => ss.email == item.tutoremail).FirstOrDefault();
                                                        t.name = td.name;
                                                        Course cc = db.Courses.Where(sc => sc.courseid == item.courseid).FirstOrDefault();
                                                        t.coursename = cc.coursename;
                                                        t.isReschedule = true;
                                                        t.classDate = GetCurrentDate();
                                                        t.temail = item.tutoremail;
                                                        t.semail = email;
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
                    if (tempList != null || tempList.Count > 0)
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
                //var cList = db.CourseEnrolleds.Where(s => s.studentemail == email).ToList();
                //if (cList != null)
                //{

                //    List<int> newList = new List<int>();
                //    String currentDay = DateTime.Now.DayOfWeek.ToString();
                //    List<String> slots = new List<string>();
                //    List<String> classes = new List<string>();
                //    List<TodayClasses> tclass = new List<TodayClasses>();
                //    foreach (var item in cList)
                //    {
                //        String schedule = item.schedule;
                //        char[] sr = schedule.ToCharArray();
                //        for (int i = 0; i < sr.Length; i++)
                //        {
                //            if (sr[i] == '2')
                //            {
                //                newList.Add(i);
                //            }
                //        }
                //        if (newList != null)
                //        {
                //            foreach (var Citem in newList)
                //            {
                //                String data = slotCondition(Citem);
                //                slots.Add(data);
                //            }
                //            foreach (var dateitem in slots)
                //            {
                //                String[] date = dateitem.Split(' ');
                //                if (date[3] == currentDay)
                //                {
                //                    TodayClasses t = new TodayClasses();
                //                    t.slot = dateitem;
                //                    Tutor std = db.Tutors.Where(ss => ss.email == item.tutoremail).FirstOrDefault();
                //                    t.name = std.name;
                //                    Course cc = db.Courses.Where(sc => sc.courseid == item.courseid).FirstOrDefault();
                //                    t.coursename = cc.coursename;
                //                    tclass.Add(t);
                //                    classes.Add(dateitem);
                //                }
                //            }
                //        }
                //        else
                //        {
                //            return Request.CreateResponse(HttpStatusCode.OK, "No Classes Today");

                //        }

                //    }
                //    if (tclass != null)
                //    {

                //        return Request.CreateResponse(HttpStatusCode.OK, tclass);

                //    }
                //    else
                //    {
                //        return Request.CreateResponse(HttpStatusCode.OK, "No Classes Today");

                //    }
                //}
                //else
                //{
                //    return Request.CreateResponse(HttpStatusCode.OK, "No Course Enrolled");

                //}
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, ex.Message);
            }
        }

        [HttpGet]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public HttpResponseMessage StudentFeeRecord(String semail)
        {
            try
            {
                var stdEnrolls = db.CourseEnrolleds.Where(s => s.studentemail == semail).ToList();
                if (stdEnrolls != null)
                {
                    List<FeeReport> cc = new List<FeeReport>();
                    foreach (var item in stdEnrolls)
                    {
                        FeeReport fr = new FeeReport();
                        fr.courseid = int.Parse(item.courseid.ToString());
                        Course c = db.Courses.Where(ss => ss.courseid == item.courseid).FirstOrDefault();
                        fr.coursename = c.coursename;
                        fr.studentemail = semail;
                        fr.tutoremail = item.tutoremail.ToString();
                        Tutor t = db.Tutors.Where(tt => tt.email == item.tutoremail).FirstOrDefault();
                        fr.name = t.name;
                        List<ClassReportCustom> crc = new List<ClassReportCustom>();
                        var report = db.ClassReports.Where(ss => ss.courseenrolledid == item.id).ToList();
                        if (report != null)
                        {
                            String sems = "Semester " + t.semester.ToString();
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
        public HttpResponseMessage GetStudentLearningCourses(String semail)
        {
            try
            {
                var enrolled = db.CourseEnrolleds.Where(s => s.studentemail == semail && s.coursestatus < 2).ToList();
                if (enrolled != null && enrolled.Count > 0)
                {
                    List<LearningCustom> learning = new List<LearningCustom>();
                    foreach (var item in enrolled)
                    {
                        LearningCustom c = new LearningCustom();
                        Course co = db.Courses.Where(cc => cc.courseid == item.courseid).FirstOrDefault();
                        c.studentemail = item.studentemail;
                        c.tutoremail = item.tutoremail;
                        c.courseid = int.Parse(item.courseid.ToString());
                        c.coursestatus = int.Parse(item.coursestatus.ToString());
                        c.coursename = co.coursename;
                        learning.Add(c);
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, learning);
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

        //Finishing Course
        [HttpPost]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public HttpResponseMessage FinishCourse(LearningCustom course)
        {
            try
            {
                var enrolled = db.CourseEnrolleds.Where(s => s.studentemail == course.studentemail && s.tutoremail == course.tutoremail && s.courseid == course.courseid && s.coursestatus < 2).FirstOrDefault();
                if (enrolled != null)
                {

                    enrolled.coursestatus = 1;
                    String eSchedule = enrolled.schedule;
                    char[] eScheduleArr = eSchedule.ToCharArray();
                    List<int> enrolledClasses = new List<int>();
                    for (int i = 0; i < eSchedule.Length; i++)
                    {
                        if (eSchedule[i] == '2')
                        {
                            enrolledClasses.Add(i);
                        }
                    }
                    Student std = db.Students.Where(s => s.email == enrolled.studentemail).FirstOrDefault();
                    Schedule stdSchedule = db.Schedules.Where(st => st.scheduleid == std.scheduleid).FirstOrDefault();
                    Tutor t = db.Tutors.Where(tt => tt.email == enrolled.tutoremail).FirstOrDefault();
                    Schedule tSchedule = db.Schedules.Where(ts => ts.scheduleid == t.scheduleid).FirstOrDefault();
                    char[] stdScheduleArr = stdSchedule.details.ToCharArray();
                    char[] tScheduleArr = tSchedule.details.ToCharArray();
                    foreach (var item in enrolledClasses)
                    {
                        stdScheduleArr[item] = '1';
                        tScheduleArr[item] = '1';
                    }
                    String stdString = "";
                    String tString = "";
                    for (int i = 0; i < stdScheduleArr.Length; i++)
                    {
                        stdString = String.Concat(stdString, stdScheduleArr[i]);
                        tString = String.Concat(tString, tScheduleArr[i]);
                    }
                    stdSchedule.details = stdString;
                    tSchedule.details = tString;
                    db.SaveChanges();
                    return Request.CreateResponse(HttpStatusCode.OK, "Course Finished Please Rate Tutor");
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, "No Course Enrolled To Finish");

                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, ex.Message);
            }
        }

        //Rate Tutor
        [HttpPost]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public HttpResponseMessage RateTutor(String tutoremail, String studentemail, int courseid, float rating)
        {
            try
            {
                var enrolled = db.CourseEnrolleds.Where(s => s.studentemail == studentemail && s.tutoremail == tutoremail && s.coursestatus < 2).FirstOrDefault();
                if (enrolled != null)
                {
                    Rate re = db.Rates.Where(rr => rr.studentemail == studentemail && rr.tutoremail == tutoremail && rr.courseid == courseid).FirstOrDefault();
                    if (re == null)
                    {
                        Rate r = new Rate();
                        r.courseid = courseid;
                        r.studentemail = studentemail;
                        r.tutoremail = tutoremail;
                        r.rating = rating;
                        db.Rates.Add(r);
                    }
                    else
                    {
                        re.rating = rating;
                        db.SaveChanges();
                    }

                    enrolled.coursestatus = 2;
                    db.SaveChanges();
                    return Request.CreateResponse(HttpStatusCode.OK, "Tutor Rated");
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, "Course Not Found");

                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, ex.Message);
            }
        }

        //Parent get details of each student
        [HttpGet]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public HttpResponseMessage GetFeeDetailOfStudent(String semail)
        {
            try
            {
                var stdEnrolls = db.CourseEnrolleds.Where(s => s.studentemail == semail).ToList();
                if (stdEnrolls != null)
                {
                    List<FeeReport> cc = new List<FeeReport>();
                    foreach (var item in stdEnrolls)
                    {
                        FeeReport fr = new FeeReport();
                        fr.courseid = int.Parse(item.courseid.ToString());
                        Course c = db.Courses.Where(ss => ss.courseid == item.courseid).FirstOrDefault();
                        fr.coursename = c.coursename;
                        fr.studentemail = semail;
                        fr.tutoremail = item.tutoremail.ToString();
                        Tutor t = db.Tutors.Where(tt => tt.email == item.tutoremail).FirstOrDefault();
                        fr.name = t.name;
                        setting setting = db.settings.Where(ste => ste.email == item.studentemail).FirstOrDefault();
                        List<ClassReportCustom> crc = new List<ClassReportCustom>();
                        var report = db.ClassReports.Where(ss => ss.courseenrolledid == item.id).ToList();
                        if (report != null)
                        {
                            String sems = "Semester " + t.semester.ToString();
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
                                else if (reportitem.classesstatus == 0)
                                {
                                    if (setting.showAbsents == 1)
                                    {
                                        ClassReportCustom crcc = new ClassReportCustom();
                                        crcc.courseenrolledid = int.Parse(reportitem.courseenrolledid.ToString());
                                        crcc.classslot = reportitem.classslot.ToString();
                                        crcc.classesstatus = int.Parse(reportitem.classesstatus.ToString());
                                        crcc.classdate = reportitem.classdate.ToString();
                                        crc.Add(crcc);
                                    }
                                }
                                else
                                {

                                    NewReschedule nr = db.NewReschedules.Where(nrr => nrr.rescheduleclassFrom == reportitem.classdate).FirstOrDefault();
                                    if (nr != null && nr.rescheduledclassstatus == 1)
                                    {
                                        if (setting.showRescheduleTutor == 1 && nr.classStatus != 7 && nr.classStatus != 8)
                                        {
                                            ClassReportCustom crcc = new ClassReportCustom();
                                            crcc.courseenrolledid = int.Parse(reportitem.courseenrolledid.ToString());
                                            crcc.classslot = slotCondition(int.Parse(nr.slotTo.ToString()));
                                            crcc.classesstatus = int.Parse(nr.classStatus.ToString());
                                            crcc.classdate = nr.rescheduleclassTo;
                                            crc.Add(crcc);
                                            classCount++;
                                        }
                                        else if (setting.showRescheduleStudent == 1 && nr.classStatus == 8)
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
                            }
                            Feenew f = db.Feenews.Where(ff => ff.courseenrolledid == item.id).FirstOrDefault();
                            if (f != null)
                            {
                                fr.totalFee = int.Parse(f.totalamount.ToString());
                                fr.remainingamount = int.Parse(f.remainingamount.ToString());
                                fr.paidamount = int.Parse(f.paidamount.ToString());
                            }
                            else
                            {
                                fr.totalFee = classCount * int.Parse(sfg.fee.ToString());
                                fr.remainingamount = 0;
                                fr.paidamount = 0;
                            }

                            fr.noOfLectures = classCount;
                            if (setting.showClassReport == 1)
                            {
                                fr.reportList = crc;
                            }
                            else
                            {
                                fr.reportList = new List<ClassReportCustom>();

                            }
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
        //Parent pay fee
        [HttpPost]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public HttpResponseMessage PayFee(String semail, String temail, int courseid, int amount)
        {
            try
            {
                CourseEnrolled cr = db.CourseEnrolleds.Where(cc => cc.studentemail == semail && cc.tutoremail == temail && cc.courseid == courseid).FirstOrDefault();
                if (cr != null)
                {
                    Feenew f = db.Feenews.Where(ff => ff.courseenrolledid == cr.id).FirstOrDefault();
                    if (f != null)
                    {
                        f.paid = amount;
                        if (f.remainingamount == 0)
                        {
                            f.status = "Paid";
                        }
                        else
                        {
                            f.status = "Unpaid";
                        }
                        Student s = db.Students.Where(ss => ss.email == semail).FirstOrDefault();
                        tutorNotification n = new tutorNotification();
                        n.email = temail;
                        n.notificationType = 2;
                        n.isRead = 0;
                        n.notificationMsg = s.name + " parents have paid " + amount + ". Did you receive the amount.;" + f.id;
                        n.notificationReply = "";
                        db.tutorNotifications.Add(n);
                        db.SaveChanges();
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, "No Fee to be paid");
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, "Fee paid");
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, "No course Found");
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

        }
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

            if ((value / 7) <= 1)
            {
                slot = s1;
            }
            else if ((value / 7) > 1 && (value / 7) <= 2)
            {
                slot = s2;
            }
            else if ((value / 7) > 2 && (value / 7) <= 3)
            {
                slot = s3;
            }
            else if ((value / 7) > 3 && (value / 7) <= 4)
            {
                slot = s4;
            }
            else if ((value / 7) > 4 && (value / 7) <= 5)
            {
                slot = s5;
            }
            else if ((value / 7) > 5 && (value / 7) <= 6)
            {
                slot = s6;
            }
            else if ((value / 7) > 6 && (value / 7) <= 7)
            {
                slot = s7;
            }
            else if ((value / 7) > 7 && (value / 7) <= 8)
            {
                slot = s8;
            }
            else if ((value / 7) > 8 && (value / 7) <= 9)
            {
                slot = s9;
            }
            else if ((value / 7) > 9 && (value / 7) <= 10)
            {
                slot = s10;
            }
            else if ((value / 7) > 10 && (value / 7) <= 11)
            {
                slot = s11;
            }
            else if ((value / 7) > 11 && (value / 7) <= 12)
            {
                slot = s12;
            }
            else if ((value / 7) > 12 && (value / 7) <= 13)
            {
                slot = s13;
            }
            else if ((value / 7) > 13 && (value / 7) <= 14)
            {
                slot = s14;
            }
            else if ((value / 7) > 14 && (value / 7) <= 15)
            {
                slot = s15;
            }
            else if ((value / 7) > 15 && (value / 7) <= 16)
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
        bool AreFallingInSameWeek(DateTime date1, DateTime date2, DayOfWeek weekStartsOn)
        {
            return date1.AddDays(-GetOffsetedDayofWeek(date1.DayOfWeek, (int)weekStartsOn)) == date2.AddDays(-GetOffsetedDayofWeek(date2.DayOfWeek, (int)weekStartsOn));
        }
        int GetOffsetedDayofWeek(DayOfWeek dayOfWeek, int offsetBy)
        {
            return (((int)dayOfWeek - offsetBy + 7) % 7);
        }
        //Student Schedule
        public string GetSSchedule(String email)
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
        //Tutor Schedule
        public string GetTTSchedule(String email)
        {
            Tutor t = db.Tutors.Where(s => s.email == email).FirstOrDefault();
            if (t != null)
            {
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
            else
            {
                return "";
            }
        }

        public String getCurrentTime()
        {
            TimeSpan hour = DateTime.Now.TimeOfDay;
            String[] timeSplit = hour.ToString().Split('.');
            return timeSplit[0];
        }
        private async Task AutoRejectStudentRequest(String studentemail, String tutoremail, int courseid, String slot, String requestDate)
        {
            // Run the autoReject function for one hour (3600 seconds)
            TimeSpan duration = TimeSpan.FromHours(1);
            TimeSpan duration2 = TimeSpan.FromMinutes(1);
            // Calculate the end time
            DateTime endTime = DateTime.Now.Add(duration2);

            // Loop until the end time
            while (DateTime.Now < endTime)
            {
                await Task.Delay(1000);
            }

            // Perform any cleanup or final operations after the autoReject function ends
            StudentRequest st = db.StudentRequests.Where(s => s.studentemail == studentemail && s.studentrequeststatus == 0 && s.tutoremail == tutoremail && s.courseid == courseid && s.slot == slot && s.requestDate == requestDate).FirstOrDefault();
            if (st != null)
            {
                st.studentrequeststatus = 2;
                db.SaveChanges();
                Course ccc = db.Courses.Where(css => css.courseid == courseid).FirstOrDefault();
                Tutor t = db.Tutors.Where(tt => tt.email == tutoremail).FirstOrDefault();
                studentNotification sn = new studentNotification();
                sn.email = studentemail;
                sn.notificationType = 0;
                sn.isRead = 0;
                sn.notificationMsg = "Your Request to tutor " + t.name + " has been auto rejected by the system " + " for course " + ccc.coursename + ".";
                sn.notificationReply = "";
                db.studentNotifications.Add(sn);
                db.SaveChanges();
            }
        }
    }
}
