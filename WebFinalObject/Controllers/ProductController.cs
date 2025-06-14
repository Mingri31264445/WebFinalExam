using Microsoft.AspNetCore.Mvc;
using WebFinalExam.Models;
using Microsoft.AspNetCore.Authorization;   // Authorize 讓只有「已登入」的人才能新增商品。
using System.Security.Claims;

namespace WebFinalExam.Controllers
{
    public class ProductController : Controller
    {
        private readonly ProductsDB _context;

        public ProductController(ProductsDB context)
        {
            _context = context;
        }

        [Authorize]// 只有登入者能看
        public IActionResult MyStore()
        {
            var uid = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var myProducts = _context.Product
                                     .Where(p => p.SellerId == uid)
                                     .ToList();

            return View(myProducts);      // 會找 Views/Product/MyStore.cshtml
        }

        [Authorize]
        public IActionResult Edit(int id)
        {
            var product = _context.Product.Find(id);
            if (product == null) return NotFound();

            // ⬇️ 身分驗證：不是自己的商品就 403
            if (product.SellerId != User.FindFirstValue(ClaimTypes.NameIdentifier))
                return Forbid();

            return View(product);
        }

        [Authorize, HttpPost, ValidateAntiForgeryToken]//// 只有登入的使用者，透過表單 POST 且有驗證碼，才能執行這段程式
        public IActionResult Edit(int id, Product product)
        {
            if (id != product.Id) return NotFound();

            // 確保仍屬於本人
            if (product.SellerId != User.FindFirstValue(ClaimTypes.NameIdentifier))
                return Forbid();

            if (!ModelState.IsValid) return View(product);

            _context.Update(product);
            _context.SaveChanges();
            return RedirectToAction("MyStore");
        }

        [Authorize]
        public IActionResult Delete(int id)
        {
            var product = _context.Product.FirstOrDefault(p => p.Id == id);
            if (product == null) return NotFound();

            // 權限檢查：只能刪自己的商品
            if (product.SellerId != User.FindFirstValue(ClaimTypes.NameIdentifier))
                return Forbid();                 // 403

            return View(product);                // Views/Product/Delete.cshtml
        }

        [Authorize]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        //真正刪除並導回 MyStore
        public IActionResult DeleteConfirmed(int id)
        {
            var product = _context.Product.Find(id);
            if (product == null) return NotFound();

            // 再次檢查是否本人
            if (product.SellerId != User.FindFirstValue(ClaimTypes.NameIdentifier))
                return Forbid();

            _context.Product.Remove(product);
            _context.SaveChanges();

            return RedirectToAction("MyStore");
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

        [Authorize]
        public IActionResult Create()
        {
            return View();   // 之後會建 Views/Product/Create.cshtml 表單頁
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Product product)
        {
            if (!ModelState.IsValid) return View(product);

            // 取得目前登入者的 Id
            string currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            // 自動寫入 SellerId
            product.SellerId = currentUserId;

            // 存進資料庫
            _context.Product.Add(product);
            _context.SaveChanges();

            // 轉回「我的賣場」頁
            return RedirectToAction("MyStore");
        }

    }
}
