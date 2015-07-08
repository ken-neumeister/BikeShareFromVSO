using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Reporting.Models.TripCategories;
using Reporting.BikesBase;

namespace Reporting.Controllers.TripCategories
{
    public class DistanceDurationController : Controller
    {
        // GET: DistanceDuration
        public ActionResult Index()
        {
            // worth a second try, bug was in different spot
            //List<BikeTable> hierarchy = new List<BikeTable>();
            //TableTripCategories trips = new TableTripCategories();
            //hierarchy.Add(trips);
            //TableSubscriber subscribers = new TableSubscriber();
            //hierarchy.Add(subscribers);
            //TableHour2 hour2 = new TableHour2();
            //hierarchy.Add(hour2);

            ReportTripCategoriesBySubscriberHour report = new ReportTripCategoriesBySubscriberHour();
            TableTripCategories model = report.GetData(); // hard-code the hierarchy
            return View("View", model);
        }
    }
}