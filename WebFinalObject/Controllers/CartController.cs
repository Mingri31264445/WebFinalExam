using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebFinalExam.Extensions;
using WebFinalExam.Models;
using WebFinalObject.Data;


namespace WebFinalObject.Controllers
{
    public class CartController : Controller
    {
        private readonly ProductsDB _prdCtx;
        private readonly ApplicationDbContext _appCtx;
        public CartController(ProductsDB prdCtx, ApplicationDbContext appCtx)
        {
            _prdCtx = prdCtx;     // 操作 Product、扣庫存
            _appCtx = appCtx;     // 寫入 Order / OrderDetail
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
            var product = _prdCtx.Product.FirstOrDefault(p => p.Id == id);
            if (product == null)
                return NotFound();

            var cartKey = GetCartKey();

            // 從 Session 取得購物車（如果沒有，就建立一個新的）
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>(cartKey) ?? new List<CartItem>();

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
            HttpContext.Session.SetObjectAsJson(cartKey, cart);

            return RedirectToAction("Index", "Home");
        }

        [Authorize]                               // 確保登入才可結帳
        [HttpPost]                                // 表單或按鈕以 POST 呼叫
        public IActionResult Checkout()
        {
            // 取出 Session 購物車
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>(GetCartKey())
                       ?? new List<CartItem>();

            if (!cart.Any())
            {
                TempData["Message"] = "購物車是空的！";
                return RedirectToAction("ViewCart");
            }

            // 建立訂單主檔
            var order = new Order
            {
                BuyerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!,
                OrderDate = DateTime.UtcNow,
                TotalAmount = cart.Sum(c => c.Price * c.Quantity)
            };

            // 逐筆處理
            foreach (var item in cart)
            {
                var product = _prdCtx.Product.FirstOrDefault(p => p.Id == item.ProductId);
                if (product == null)
                {
                    TempData["Message"] = $"找不到商品 {item.ProductId}";
                    return RedirectToAction("ViewCart");
                }

                // 檢查庫存
                if (product.Stock < item.Quantity)
                {
                    TempData["Message"] = $"{product.Name} 庫存不足（剩 {product.Stock} 件）";
                    return RedirectToAction("ViewCart");
                }

                // 扣庫存
                product.Stock -= item.Quantity;

                // 庫存歸零就自動下架
                if (product.Stock == 0)
                    product.IsActive = false;

                order.Details.Add(new OrderDetail
                {
                    ProductId = product.Id,
                    Quantity = item.Quantity,
                    UnitPrice = product.Price
                });
            }

            // 寫入 Order & OrderDetail，Product 庫存已在 _prdCtx 上修改
            _appCtx.Order.Add(order);
            _appCtx.SaveChanges();   // 寫 Order / OrderDetail
            _prdCtx.SaveChanges();   // 寫 Product 庫存

            // 清空購物車並回首頁
            HttpContext.Session.Remove(GetCartKey());
            TempData["Message"] = "結帳成功，感謝您的購買！";
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        public IActionResult RemoveFromCart(int id)
        {
            var cartKey = GetCartKey();

            // 讀購物車 (若 Session 沒東西就給空清單)
            var cart = HttpContext.Session
                       .GetObjectFromJson<List<CartItem>>(cartKey)
                       ?? new List<CartItem>();

            // 找目標商品
            var item = cart.FirstOrDefault(c => c.ProductId == id);
            if (item != null)
            {
                // 減量或刪除
                if (item.Quantity > 1)
                {
                    item.Quantity--;
                }
                else
                {
                    cart.Remove(item);
                }

                // 寫回 Session
                HttpContext.Session.SetObjectAsJson(cartKey, cart);
            }

            TempData["Message"] = "已從購物車移除！";
            return RedirectToAction("ViewCart");
        }

        private string GetCartKey()
        {
            return User.Identity!.IsAuthenticated
                ? $"Cart_{User.FindFirstValue(ClaimTypes.NameIdentifier)}"
                : "Cart_Guest";
        }

        public IActionResult ViewCart()
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>(GetCartKey()) ?? new List<CartItem>();
            return View(cart);
        }
    }
}
