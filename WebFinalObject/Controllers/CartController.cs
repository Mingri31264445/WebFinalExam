using Microsoft.AspNetCore.Mvc;
using WebFinalExam.Models;
using WebFinalExam.Extensions;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;


namespace WebFinalObject.Controllers
{
    public class CartController : Controller
    {
        private readonly ProductsDB _context;
        public CartController(ProductsDB context)
        {
            _context = context;
        }

        public IActionResult Test()
        {
            return Content("Test OK");
        }

        [Authorize]
        [HttpGet]
        public IActionResult AddToCart(int id)
        {
            // 從資料庫找出商品
            var product = _context.Product.FirstOrDefault(p => p.Id == id);
            if (product == null)
                return NotFound();

            // 從 Session 取得購物車（如果沒有，就建立一個新的）
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();

            // 如果商品已在購物車中，則增加數量；否則新增一筆
            var item = cart.FirstOrDefault(c => c.ProductId == id);
            if (item != null)
            {
                item.Quantity++;
            }
            else
            {
                cart.Add(new CartItem
                {
                    ProductId = product.Id,
                    Name = product.Name,
                    Price = product.Price,
                    ImageUrl = product.ImageUrl,
                    Quantity = 1
                });
            }

            // 儲存回 Session
            HttpContext.Session.SetObjectAsJson("Cart", cart);

            return RedirectToAction("Index", "Home");
        }

        [Authorize]                               // 確保登入才可結帳
        [HttpPost]                                // 表單或按鈕以 POST 呼叫
        public IActionResult Checkout()
        {
            // 1. 取出 Session 購物車
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart")
                       ?? new List<CartItem>();

            if (!cart.Any())
            {
                TempData["Message"] = "購物車是空的！";
                return RedirectToAction("Index", "Home");
            }

            // 2. 逐筆處理
            foreach (var item in cart)
            {
                var product = _context.Product.FirstOrDefault(p => p.Id == item.ProductId);
                if (product == null)
                {
                    TempData["Message"] = $"找不到商品 {item.ProductId}";
                    return RedirectToAction("ViewCart");
                }

                // 3. 檢查庫存
                if (product.Stock < item.Quantity)
                {
                    TempData["Message"] = $"{product.Name} 庫存不足（剩 {product.Stock} 件）";
                    return RedirectToAction("ViewCart");
                }

                // 4. 扣庫存
                product.Stock -= item.Quantity;
            }

            // 5. 寫入資料庫
            _context.SaveChanges();

            // 6. 清空購物車並回首頁
            HttpContext.Session.Remove("Cart");
            TempData["Message"] = "結帳成功，感謝您的購買！";
            return RedirectToAction("Index", "Home");
        }


        public IActionResult ViewCart()
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
            return View(cart);
        }
    }
}
