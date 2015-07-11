using System.Web.Mvc;

namespace Reporting.Models.LinqCategories
{
    public class LinqCategoriesController : Controller
    {
        // GET: LinqCategories
        public ViewResult Index()
        {
            ReportUsingLinq report = new ReportUsingLinq("Category Report using Linq");
            return View(report);
        }
    }
}