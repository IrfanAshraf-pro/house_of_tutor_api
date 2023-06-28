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
    public class NotificationController : ApiController
    {
        houseoftutorEntities db = new houseoftutorEntities();
        [HttpGet]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public HttpResponseMessage StudentNotification(string email)
        {
            try
            {
                Student std = db.Students.Where(s => s.email == email).FirstOrDefault();
                if (std != null)
                {
                    var notifications = db.studentNotifications.Where(sn => sn.email == std.email && sn.isRead == 0).ToList();
                    return Request.CreateResponse(HttpStatusCode.OK, notifications.Select(s => new { s.email, s.id, s.notificationType, s.isRead, s.notificationMsg, s.notificationReply }).ToList());
                }
                return Request.CreateResponse(HttpStatusCode.OK, "No Notifications at the Moment");
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, e.Message);
            }
        }

        [HttpGet]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public HttpResponseMessage TutorNotification(string email)
        {
            try
            {
                Tutor std = db.Tutors.Where(s => s.email == email).FirstOrDefault();
                if (std != null)
                {
                    var notifications = db.tutorNotifications.Where(sn => sn.email == std.email && sn.isRead == 0).ToList();
                    List<StudentRequestCustom> stReq = new List<StudentRequestCustom>();

                    //getting student requests
                    var requestList = db.StudentRequests.Where(sr => sr.tutoremail == email && sr.studentrequeststatus == 0).ToList();
                    if (requestList != null)
                    {
                        var courses = requestList.Select(c => c.courseid).Distinct().ToList();
                        var allCourses = db.Courses.Select(s => s).ToList();
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
                    }
                    //getting temporary requests
                    List<StudentRequestCustomTemp> stReqTemp = new List<StudentRequestCustomTemp>();

                    var requestListTemp = db.StudentRequestTemporaryEnrollments.Where(sr => sr.tutoremail == email && sr.studentrequeststatus == 0).ToList();
                    if (requestListTemp != null)
                    {
                        var courses = requestListTemp.Select(c => c.courseid).Distinct().ToList();
                        var allCourses = db.Courses.Select(s => s).ToList();
                        foreach (var item in requestListTemp)
                        {
                            foreach (var alItem in allCourses)
                            {
                                if (alItem.courseid == item.courseid)
                                {
                                    Student s = db.Students.Where(ss => ss.email == item.studentemail).FirstOrDefault();
                                    StudentRequestCustomTemp sr = new StudentRequestCustomTemp();
                                    sr.courseid =int.Parse(item.courseid.ToString());
                                    sr.studentemail = item.studentemail;
                                    sr.tutoremail = item.tutoremail;
                                    sr.coursename = alItem.coursename;
                                    sr.slot = item.slot;
                                    sr.studentname = s.name;
                                    sr.enrollDate = item.dateToBeEnrolled;
                                    sr.endDate = item.dateToEnd;
                                    stReqTemp.Add(sr);
                                }
                            }
                        }
                    }
                    return Request.CreateResponse(HttpStatusCode.OK,new { notifications=notifications.Select(s => new { s.email, s.id, s.notificationType, s.isRead, s.notificationMsg, s.notificationReply }).ToList(),request= stReq,tempRequest= stReqTemp } );
                }
                return Request.CreateResponse(HttpStatusCode.OK, "No Notifications at the Moment");
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, e.Message);
            }
        }

        //Post notification
        [HttpPost]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public HttpResponseMessage AddStudentNotification(studentNotification snot)
        {
            try
            {
                Student std = db.Students.Where(s => s.email == snot.email).FirstOrDefault();
                if (std != null)
                {
                    db.studentNotifications.Add(snot);
                    db.SaveChanges();
                    return Request.CreateResponse(HttpStatusCode.OK, "Notification Sent");
                }
                return Request.CreateResponse(HttpStatusCode.OK, "Student Doesnot Exist");
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, e.Message);
            }
        }
        [HttpPost]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public HttpResponseMessage AddTutorNotification(tutorNotification snot)
        {
            try
            {
                Tutor std = db.Tutors.Where(s => s.email == snot.email).FirstOrDefault();
                if (std != null)
                {
                    db.tutorNotifications.Add(snot);
                    db.SaveChanges();
                    return Request.CreateResponse(HttpStatusCode.OK, "Notification Sent");
                }
                return Request.CreateResponse(HttpStatusCode.OK, "Tutor Doesnot Exist");
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, e.Message);
            }
        }
        [HttpPost]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public HttpResponseMessage MarkStudentNotificationAsRead(tutorNotification snot)
        {
            try
            {
                studentNotification sn = db.studentNotifications.Where(n => n.id == snot.id).FirstOrDefault();
                if (sn.notificationType == 0 || sn.notificationType==2)
                {
                    sn.isRead = 1;
                    db.SaveChanges();
                }
                else
                {
                    if (snot.notificationReply.Length > 0)
                    {
                        sn.isRead = 1;
                        sn.notificationReply = snot.notificationReply;
                        db.SaveChanges();
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, "Please reply to the notification");

                    }
                }
                return Request.CreateResponse(HttpStatusCode.OK, "Notification Read");
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, e.Message);
            }
        }
        [HttpPost]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public HttpResponseMessage MarkTutorNotificationAsRead(tutorNotification snot)
        {
            try
            {
                tutorNotification sn = db.tutorNotifications.Where(n => n.id == snot.id).FirstOrDefault();
                if (sn.notificationType == 0)
                {
                    sn.isRead = 1;
                    db.SaveChanges();
                }
                else
                {
                    if (snot.notificationReply.Length > 0)
                    {
                        sn.isRead = 1;
                        sn.notificationReply = snot.notificationReply;
                        db.SaveChanges();
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, "Please reply to the notification");
                    }
                }
                return Request.CreateResponse(HttpStatusCode.OK, "Notification Read");
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, e.Message);
            }
        }
        [HttpPost]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public HttpResponseMessage FeeAcknowledgement(tutorNotification snot)
        {
            try
            {
                tutorNotification sn = db.tutorNotifications.Where(n => n.id == snot.id).FirstOrDefault();
                sn.isRead = 1;
                sn.notificationReply = snot.notificationReply;
                string reply = sn.notificationReply;
                string idstring = snot.notificationMsg.Split(';')[1];
                int id = int.Parse(idstring);
                Feenew f = db.Feenews.Where(ff => ff.id == id).FirstOrDefault();
                if (reply== "Yes")
                {
                    int paid = int.Parse(f.paid.ToString());
                    f.paid = 0;
                    f.paidamount += paid;
                    f.remainingamount -= paid;
                    if (f.remainingamount == 0)
                    {
                        f.status = "Paid";
                    }
                }
                else
                {
                    f.paid = 0;  
                }
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, "Notification Read");
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, e.Message);
            }
        }
    }
}
