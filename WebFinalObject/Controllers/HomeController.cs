using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WebFinalExam.Models;
using WebFinalObject.Data;
using Microsoft.EntityFrameworkCore;           // GroupBy

namespace WebFinalExam.Controllers
{
    public class HomeController : Controller
    {
        private readonly ProductsDB _context;
        private readonly ApplicationDbContext _app; //訂單資料庫
        public HomeController(ProductsDB context, ApplicationDbContext app)
        {
            _context = context;
            _app = app;
        }
        

        public IActionResult Index()
        {
            var products = _context.Product
                           .Where(p => p.IsActive)// 只取上架的商品
                           .OrderByDescending(p => p.Id)
                           .ToList();

            // 只抓「目前上架商品」的分類
            ViewBag.Categories = _context.Product
                                     .Where(p => p.IsActive)
                                     .Select(p => p.Category)
                                     .Distinct()
                                     .OrderBy(c => c)
                                     .ToList();

            var topIds = _app.OrderDetail
                         .GroupBy(d => d.ProductId)
                         .Select(g => new { g.Key, Sold = g.Sum(x => x.Quantity) })
                         .OrderByDescending(x => x.Sold)
                         .Take(3)
                         .Select(x => x.Key)
                         .ToList();

            var hotProducts = _context.Product
                                      .Where(p => p.IsActive && topIds.Contains(p.Id))
                                      .ToList();

            ViewBag.HotProducts = hotProducts;    // 傳到 View

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
