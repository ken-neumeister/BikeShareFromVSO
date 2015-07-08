using System.Collections.Generic;
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
            List<BikeTable> hierarchy = new List<BikeTable>();
            hierarchy.Add(new TableTripCategories()); // "Reporting.Models.TripCategories.TableTripCategory");
            hierarchy.Add(new TableSubscriber()); //"Reporting.Models.TripCategories.TableSubscriber");
            hierarchy.Add(new TableHour2()); // "Reporting.Models.TripCategories.TableHour2");

            ReportTripCategoriesBySubscriberHour report = new ReportTripCategoriesBySubscriberHour();
            TableTripCategories model = (TableTripCategories)report.GetData(hierarchy);
            return View("View", model);
        }
    }
}