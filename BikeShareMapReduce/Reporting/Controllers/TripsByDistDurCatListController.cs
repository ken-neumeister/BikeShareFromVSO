using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Reporting.TripsByDurDistCatListBuilder;

namespace Reporting.Controllers
{
    public class TripsByDistDurCatListController : Controller
    {
        // GET: TripsByDistDurCatList
        public ActionResult Index()
        {
            return View();
        }

        public ViewResult TripsByDistDurCatList()
        {
            TripsByDistDurCatList list = new TripsByDistDurCatList();
            list.GetData();
            return View("TripsByDistDurCatList", list);
        }
    }
}