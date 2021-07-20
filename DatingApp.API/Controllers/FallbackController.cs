using System.IO;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.API.Controllers
{
    public class FallbackController : Controller
    {
        // To handle angular routing that API doesn't know
        public ActionResult Index()
        {
            return PhysicalFile(
                Path.Combine(
                    Directory.GetCurrentDirectory(), 
                    "wwwroot", 
                    "index.html"
                ), 
                "text/HTML"
            );
        }
    }
}