using Microsoft.AspNetCore.Mvc;
using WebFinalExam.Models;

namespace WebFinalExam.Controllers
{
    public class ProductController : Controller
    {
        private readonly ProductsDB _context;

        public ProductController(ProductsDB context)
        {
            _context = context;
        }

        public IActionResult Details(int id)
        {
            var product = _context.Product.FirstOrDefault(p => p.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }
    }
}
