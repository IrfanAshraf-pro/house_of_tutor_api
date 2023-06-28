using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;
using HouseOfTutorNew.Models;
using HouseOfTutorNew.Models.CustomClasses;


namespace HouseOfTutorNew.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class AdminController : ApiController
    {
        houseoftutorEntities db = new houseoftutorEntities();
        [HttpPost]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public HttpResponseMessage AddCourse(Course c)
        {
            try
            {
                var v = db.Courses.Where(i => i.coursename == c.coursename).FirstOrDefault();
                if (v == null)
                {
                    db.Courses.Add(c);
                    db.SaveChanges();
                    return Request.CreateResponse(HttpStatusCode.OK, "Course Inserted Successfully");
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, "Already Exist");
                }
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, e.Message);
            }
        }
        [HttpGet]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public HttpResponseMessage GetCourses()
        {
            try
            {
                var courses = from c in db.Courses select new { c.courseid, c.coursecode, c.coursename, c.coursefee };
                return Request.CreateResponse(HttpStatusCode.OK, courses.ToList());
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, e.Message);
            }
        }
        [HttpGet]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public HttpResponseMessage GetAllCourseGroup()
        {
            try
            {
                var groups = from c in db.SubjectGroups select new { c.courseid, c.coursename, c.groupid };
                var groupsNames = from gn in db.SubjectGroupNames select new { gn.groupid, gn.groupName };
                List<CourseGroupCustom> cgc = new List<CourseGroupCustom>();
                foreach (var item in groupsNames)
                {
                    CourseGroupCustom cg1 = new CourseGroupCustom();
                    cg1.groupName = item.groupName;
                    List<SubjectGroupCustom> sgcustom = new List<SubjectGroupCustom>();

                    foreach (var group in groups)
                    {
                        if (item.groupid == group.groupid)
                        {
                            SubjectGroupCustom sg = new SubjectGroupCustom();
                            sg.courseid = int.Parse(group.courseid.ToString());
                            sg.coursename = group.coursename;
                            sg.groupid = int.Parse(group.groupid.ToString());
                            sgcustom.Add(sg);
                        }
                    }
                    cg1.subjectGroup = sgcustom;
                    cgc.Add(cg1);
                }

                return Request.CreateResponse(HttpStatusCode.OK, cgc.ToList());
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, e.Message);
            }
        }
        [HttpGet]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public HttpResponseMessage GetCourseGroup(String groupname)
        {
            try
            {
                var groupsNames = db.SubjectGroupNames.Where(s => s.groupName == groupname).Select(ss => new { ss.groupid, ss.groupName }).FirstOrDefault();
                if (groupsNames != null)
                {
                    var groups = from c in db.SubjectGroups where c.groupid == groupsNames.groupid select new { c.courseid, c.coursename, c.groupid };
                    CourseGroupCustom cg1 = new CourseGroupCustom();
                    cg1.groupName = groupsNames.groupName;
                    List<SubjectGroupCustom> sgcustom = new List<SubjectGroupCustom>();
                    foreach (var group in groups)
                    {
                        SubjectGroupCustom sg = new SubjectGroupCustom();
                        sg.courseid = int.Parse(group.courseid.ToString());
                        sg.coursename = group.coursename;
                        sg.groupid = int.Parse(group.groupid.ToString());
                        sgcustom.Add(sg);
                    }
                    cg1.subjectGroup = sgcustom;
                    return Request.CreateResponse(HttpStatusCode.OK, cg1);
                }
                else
                    return Request.CreateResponse(HttpStatusCode.OK, "Subject Group DoesNot Exist");
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, e.Message);
            }
        }
        [HttpPost]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public HttpResponseMessage SaveCourseGroup(CourseGroupCustom group)
        {
            try
            {
                var groupname = group.groupName;
                var coursegroupname = db.SubjectGroupNames.Where(ss => ss.groupName == groupname).FirstOrDefault();
                if (coursegroupname != null)
                {
                    var groupid = coursegroupname.groupid;
                    var groups = db.SubjectGroups.Where(s => s.groupid == groupid).ToList();
                    var groupsToSave = group.subjectGroup;
                    foreach (var groupdb in groups)
                    {
                        SubjectGroupCustom cgcustom = new SubjectGroupCustom();
                        bool isThere = false;
                        foreach (var item in groupsToSave)
                        {
                            if (item.courseid == groupdb.courseid)
                            {
                                isThere = true;
                                cgcustom = item;
                                break;
                            }
                        }
                        if (!isThere)
                        {
                            db.SubjectGroups.Remove(groupdb);
                            db.SaveChanges();
                        }
                        else
                        {
                            groupsToSave.Remove(cgcustom);
                        }
                    }
                    foreach (var item in groupsToSave)
                    {
                        SubjectGroup cg = new SubjectGroup();
                        cg.courseid = item.courseid;
                        cg.coursename = item.coursename;
                        cg.groupid = groupid;
                        db.SubjectGroups.Add(cg);
                        db.SaveChanges();
                    }

                    return Request.CreateResponse(HttpStatusCode.OK, "Course Group Successfully updated");
                }
                else
                {
                    SubjectGroupName cg = new SubjectGroupName();
                    cg.groupName = groupname;
                    db.SubjectGroupNames.Add(cg);
                    db.SaveChanges();
                    if (group.subjectGroup != null)
                    {
                        var groupAdded = db.SubjectGroupNames.Where(s => s.groupName == groupname).FirstOrDefault();
                        foreach (var item in group.subjectGroup)
                        {
                            SubjectGroup sg = new SubjectGroup();
                            sg.courseid = int.Parse(item.courseid.ToString());
                            sg.coursename = item.coursename;
                            sg.groupid = groupAdded.groupid;
                            db.SubjectGroups.Add(sg);
                            db.SaveChanges();
                        }
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, "Course Group Added");
                }
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, e.Message);
            }
        }
        [HttpGet]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public HttpResponseMessage GetCoursesForGroup(String groupname)
        {
            try
            {
                var groupsNames = db.SubjectGroupNames.Where(s => s.groupName == groupname).Select(ss => new { ss.groupid, ss.groupName }).FirstOrDefault();
                if (groupsNames != null)
                {
                    var groups = from c in db.SubjectGroups where c.groupid == groupsNames.groupid select new { c.courseid, c.coursename };
                    var courses = db.Courses.Select(s => s).ToList();
                    foreach (var item in groups)
                    {
                        bool isFound = false;
                        Course cc = new Course();
                        foreach (var courseitem in courses)
                        {
                            if (courseitem.courseid == item.courseid && courseitem.coursename == item.coursename)
                            {
                                isFound = true;
                                cc = courseitem;
                            }
                        }
                        if (isFound)
                        {
                            courses.Remove(cc);
                        }
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, courses.Select(s => new { s.courseid, s.coursename }));
                }
                else
                    return Request.CreateResponse(HttpStatusCode.OK, "Subject Group DoesNot Exist");
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, e.Message);
            }
        }

        [HttpPost]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public HttpResponseMessage SaveSubjectFeeGroupName(SubjectFeeGroupName s)
        {
            try
            {
                var f = db.SubjectFeeGroupNames.Where(e => e.groupName.ToLower() == s.groupName.ToLower()).FirstOrDefault();
                if (f == null)
                {
                    SubjectFeeGroupName ss = new SubjectFeeGroupName();
                    ss.groupName = s.groupName;
                    ss.fee = s.fee;
                    db.SubjectFeeGroupNames.Add(ss);
                    db.SaveChanges();
                    return Request.CreateResponse(HttpStatusCode.OK, "Fee Group Added Successfully");
                }
                else
                {
                    f.groupName = s.groupName;
                    f.fee = s.fee;
                    db.SaveChanges();
                    return Request.CreateResponse(HttpStatusCode.OK, "Fee Group Updated Successfully");
                }
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, e.Message);

            }
        }
        [HttpGet]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public HttpResponseMessage GetSubjectGroupFeeName()
        {
            try
            {
                var feeList = db.SubjectFeeGroupNames.Select(s => s).ToList();
                return Request.CreateResponse(HttpStatusCode.OK,feeList);
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, e.Message);

            }
        }
        [HttpGet]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public HttpResponseMessage GetAllTutors()
        {
            try
            {
                var Tutors = db.Tutors.Select(u => new { u.name, u.email, u.password, u.cgpa, u.semester, u.image }).ToList();
                if(Tutors!=null && Tutors.Count > 0)
                {
                    List<TutorCustom> list = new List<TutorCustom>();
                    foreach (var item in Tutors)
                    {
                        TutorCustom t = new TutorCustom();
                        t.name = item.name;
                        t.email = item.email;
                        var r = db.Rates.Where(rr => rr.tutoremail == item.email).ToList();
                        double rating = 0;
                        int ratingCount = 0;
                        if (r.Count > 0)
                        {
                            foreach (var ritem in r)
                            {
                                rating += double.Parse(ritem.rating.ToString());
                                ratingCount++;
                            }
                            if (ratingCount > 0)
                            {
                                rating = rating / ratingCount;
                            }
                        }
                        t.rating = rating;
                        BlockedTutor b = db.BlockedTutors.Where(bb => bb.email == item.email).FirstOrDefault();
                        if (b != null)
                        {
                            t.isBlocked = int.Parse(b.isBlock.ToString());
                        }
                        else
                        {
                            t.isBlocked = 0;
                        }
                        list.Add(t);
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, list);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, "No Tutor Available");

                }
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, e.Message);

            }
        }
        [HttpPost]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public HttpResponseMessage BlockTutors(TutorCustom tutor)
        {
            try
            {
                BlockedTutor bt = db.BlockedTutors.Where(b => b.email == tutor.email).FirstOrDefault();
                if (bt != null)
                {
                    bt.isBlock = bt.isBlock==1?0:1;
                    db.SaveChanges();
                    return Request.CreateResponse(HttpStatusCode.OK, bt.isBlock == 1?"Tutor is Blocked":"Tutor is Unblocked");

                }
                else
                {
                    BlockedTutor t = new BlockedTutor();
                    t.email = tutor.email;
                    t.isBlock = 1;
                    db.BlockedTutors.Add(t);
                    db.SaveChanges();
                    return Request.CreateResponse(HttpStatusCode.OK, "Tutor is Blocked");

                }

            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, e.Message);

            }
        }
    }

}

