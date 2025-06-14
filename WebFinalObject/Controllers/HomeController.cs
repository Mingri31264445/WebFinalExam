using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WebFinalExam.Models;

namespace WebFinalExam.Controllers
{
    public class HomeController : Controller
    {
        private readonly ProductsDB _context;
        public HomeController(ProductsDB context)
        {
            _context = context;
        }
        

        public IActionResult Index()
        {
            var products = _context.Product
                           .Where(p => p.IsActive)// �u���W�[���ӫ~
                           .OrderByDescending(p => p.Id)
                           .ToList();
            return View(products);
        }

        public IActionResult Privacy()
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
