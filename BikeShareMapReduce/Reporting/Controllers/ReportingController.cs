using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Reporting.Data;

namespace Reporting.Controllers
{
    public class ReportingController : Controller
    {
        // GET: Reporting
        public ActionResult Index()
        {
            return View();
        }

        public JsonResult ReportInJson()
        {
            List<object> data = Reporting.Data.Reporting.FirstReport();
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ReportTable()
        {
           string command = @" 
           SELECT { 
                    ( 
                     [Direction].[Direction].[Direction].ALLMEMBERS *
                     [Measures].[Bikes]
                    )
                  } DIMENSION PROPERTIES MEMBER_CAPTION ON COLUMNS, 
                  { 
                    (
                     [CentroidA].[Level3 Locality].[Level3 Locality].ALLMEMBERS * 
                     [CentroidB].[Level3 Locality].[Level3 Locality].ALLMEMBERS
                    ) 
                  } DIMENSION PROPERTIES MEMBER_CAPTION ON ROWS 
           FROM [Bikeshare]";

            ActionResult qresult = View(Data.Reporting.GetData(command));
            return (qresult);
        }
    }
}