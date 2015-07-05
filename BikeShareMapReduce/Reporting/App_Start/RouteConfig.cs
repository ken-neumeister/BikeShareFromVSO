using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Reporting
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
            //routes.MapRoute(
            //    name: "Parameters3",
            //    url: "Reporting/{controller}/{action}/{param1}/{param2}/{param3}",
            //    defaults: new
            //    {
            //        controller = "Home",
            //        action = "Index",
            //        param1 = UrlParameter.Optional,
            //        param2 = UrlParameter.Optional,
            //        param3 = UrlParameter.Optional
            //    });
            routes.MapMvcAttributeRoutes();
        }
    }
}
