using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using website.Models;

namespace website.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private readonly MyDBContext _context;

        public HomeController(ILogger<HomeController> logger, MyDBContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            
            return View(_context.Categories);
        }
        public IActionResult first()
        {

            return View();
        }
        public IActionResult address()
        {

            return View();
        }
        public IActionResult Register()
        {

            return View();
        }
        public IActionResult Spots()
        {

            return View();
        }


        public IActionResult JSONTest()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }
        public IActionResult callApi()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}