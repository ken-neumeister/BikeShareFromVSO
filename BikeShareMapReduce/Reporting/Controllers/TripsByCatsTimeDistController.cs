using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Reporting.Models;

namespace Reporting.Controllers
{
    public class TripsByCatsTimeDistController : Controller
    {
        // root table needs no parameters
        // GET: TripsByCatsTimeDist
        public ActionResult Index()
        {
            return View();
        }

        //[Route("Reporting/TripsByCatsTimeDist")]
        public ViewResult TripsByCatsTimeDist()
        {
            TripsByCatsTimeDist tripsreport = new TripsByCatsTimeDist();
            tripsreport.GetData();
            return View("TripsByCatsTimeDist", tripsreport);
        }

    }
}