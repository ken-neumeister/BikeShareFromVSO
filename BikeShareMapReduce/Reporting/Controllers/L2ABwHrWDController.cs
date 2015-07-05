using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Reporting.Models;

namespace Reporting.Controllers
{
    public class L2ABwHrWDController : Controller
    {
        // more notes: 
        // should be ViewResult rather than ActionResult (ViewResult is more precise, ActionResult is ancester abstract class
        // forms should send a full model instead of components
        //     [HttpPost]
        //     public ViewResult Report(L3PairDirParams params)
        // View would be
        //     @using (Html.BeginForm("L2ABwHrWDPost", "L2ABwHrWD")) {
        //        @Html.Hidden("Level3A","...")
        //        @Html.Hidden("Level3B","...")
        //        @Html.Hidden("Direction","...")
        // submission returns anonymous object with these fields, not isolated strings!
        [HttpPost]  // not sure this will work, may need special type with simple string members
        public ViewResult L2ABwHrWDPost(L3ABwHrWDParams L3params)  
        {
            L2ABwHrWDReport l2abwhrwdreport = new L2ABwHrWDReport(L3params);
            l2abwhrwdreport.GetData();

            return View(l2abwhrwdreport);
        }

        // Get: L2ABwHrWD
        [HttpGet] // works for /Reporting/L2ABwHrWD/Chevy%20Chase%2c%20Washington%2c%20DC%2c%20USA/Chevy%20Chase%2c%20Washington%2c%20DC%2c%20USA/A-B
        [Route("Reporting/L2ABwHrWD/{L3A:regex(^[a-zA-Z ,.0-9]+$)}/{L3B:regex(^[a-zA-Z ,.0-9]+$)}/{Dir:regex(^(A-B|B-A)$)}")]
        //[HttpPost] // does not work for Request POST /Reporting/L2ABwHrWD HTTP/1.1 L3A=Chevy+Chase%2C+Washington%2C+DC%2C+USA&L3B=Chevy+Chase%2C+Washington%2C+DC%2C+USA&Dir=A-B
        //[Route("Reporting/L2ABwHrWD")]  // can't get form to find route, period
        public ActionResult L2ABwHrWD(string L3A, string L3B, string Dir)
        {
            // Can't get the Route directive to work... ( try again with fresh project )
            // needed to modify RouteConfig.cs, now it is called but not populating parameters.
            // needed to modify RouteConfig.cs to enable attribute routing (undo above replace with routes.MapMvcAttributeRoutes())
            // working now, but some data has '&' in name that is causing exception outside of my code, perhaps better to avoid it


            // TBD more things to experiment
            // 1* use query strings to get paramaters, match exactly -- this works
            // 2. use run query to return results that equate to selected row add to model so it can populate the page title
            //     pass multiple models back to view: http://www.codeproject.com/Articles/687061/Multiple-Models-in-a-View-in-ASP-NET-MVC-MVC
            //     use the viewmodel approach: model containing two models with results for each query
            // 2*  Existing parameters already a model, just needed to attach it to the model results
            // 3. pass keys and then use results for parameterizing the query by name, covering matching names for different partitions

            // debug hard-code sample parameters
            //L3A = "NE Florida Av & NE 8th St, Gallaudet University, Washington, DC 20002, USA";
            //L3B = "Court House, Arlington, VA, USA";
            //Dir = "A-B";

            //construct unique names from passed parameters
            L3A = "[CentroidA].[Level3 Locality].&[" + L3A.Replace(".0a2n0d.", "&") + "]";
            L3B = "[CentroidB].[Level3 Locality].&[" + L3B.Replace(".0a2n0d.", "&") + "]";
            Dir = "[Direction].[Direction].&[" + Dir + "]";

            //Debug, just hard code it
            //string L3A = "[CentroidA].[Level3 Locality].&[NE Florida Av & NE 8th St, Gallaudet University, Washington, DC 20002, USA]";
            //string L3B = "[CentroidB].[Level3 Locality].&[Court House, Arlington, VA, USA]";
            //string Dir = "[Direction].[Direction].&[A-B]";

            //default constructor-less
            //L3ABwHrWDParams param = new L3ABwHrWDParams()
            //     {
            //       Direction = Dir,
            //       Level3A = L3A,
            //       Level3B = L3B
            //     };
            //L3ABwHrWDParams param = new L3ABwHrWDParams(L3A, L3B, Dir);
            L2ABwHrWDReport l2abwhrwdreport = new L2ABwHrWDReport(L3A, L3B, Dir);
            l2abwhrwdreport.GetData();

            return View(l2abwhrwdreport);
        }
    }
}