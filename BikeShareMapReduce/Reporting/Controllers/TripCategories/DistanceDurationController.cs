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

            ReportTripCategoriesBySubscriberHour report = new ReportTripCategoriesBySubscriberHour();
            // GetData may be overloaded to accept different parameters
            TableTripCategories model = report.GetData();
            return View("View", model);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="DistanceCategory"></param>
        /// <param name="DurationCategory"></param>
        /// <returns>
        /// View
        /// </returns>
        /// <remarks>
        /// There should be a separate class for sub-report because eventually it may arrive from other referrers
        /// The heirarchy list move to the view model itself
        /// Thus, the class should support the same hierarchy but may have multiple getdata for different parameters
        /// </remarks>
        [HttpPost]
        public ViewResult CategoryWeekdayDetails(string DistanceCategory, string DurationCategory)
        {
            ReportTripWeekdayBySubscriberHour report = new ReportTripWeekdayBySubscriberHour();
            ViewWeekofyearGivenDistanceDurationCategories model = report.GetData(DistanceCategory, DurationCategory);
            return View("CategoryWeekdayDetails", model);
        }
    }
}