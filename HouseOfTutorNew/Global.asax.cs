using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace HouseOfTutorNew
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        //protected void Application_Start(object sender, EventArgs e)
        //{
        //    // Create and start a new thread
        //    Thread thread = new Thread(ContinuousThreadMethod);
        //    thread.Start();
        //}

        //private void ContinuousThreadMethod()
        //{
        //    while (true)
        //    {
        //        Console.WriteLine("aaa");
        //    }
        //}

    }
}
