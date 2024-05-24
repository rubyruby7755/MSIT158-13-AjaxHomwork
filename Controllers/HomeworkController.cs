using Microsoft.AspNetCore.Mvc;

namespace website.Controllers
{
    public class HomeworkController : Controller
    {
        public IActionResult Homework1()
        {
            return View();
        }
    }
}
