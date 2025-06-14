using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebFinalExam.Models;          // Order / OrderDetail / Product
using WebFinalObject.Data;           // ApplicationDbContext 的 namespace
using WebFinalExam.Models.ViewModels;  // SellerOrderVM

namespace WebFinalExam.Controllers
{
    [Authorize]                      // 兩個 Action 都需登入
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _ctx;

        public OrderController(ApplicationDbContext ctx)
        {
            _ctx = ctx;
        }

        //MyOrders買家角度：只看自己的訂單主檔＋明細
        public IActionResult MyOrders()
        {
            string uid = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            // 取自己下過的訂單，連同明細和商品名稱
            var orders = _ctx.Order
                             .Where(o => o.BuyerId == uid)//BuyerId是現在的登入者
                             .Include(o => o.Details)          // 明細
                                 .ThenInclude(d => d.Product)  // 商品
                             .OrderByDescending(o => o.OrderDate)
                             .ToList();

            return View(orders);   // -> Views/Order/MyOrders.cshtml
        }

        //SellerOrders賣家角度：列出含自己商品的訂單明細
        public IActionResult SellerOrders()
        {
            string sellerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var grouped = _ctx.OrderDetail
        .Include(d => d.Order)
        .Include(d => d.Product)
        .Where(d => d.Product!.SellerId == sellerId)
        .AsEnumerable()                            // 轉成 LINQ to Objects 以便 GroupBy
        .GroupBy(d => d.Order!)                    // 依訂單主檔分群
        .Select(g => new SellerOrderVM
        {
            Order = g.Key,
            Details = g.ToList()
        })
        .OrderByDescending(vm => vm.Order.OrderDate)
        .ToList();

            return View(grouped);   // -> Views/Order/SellerOrders.cshtml
        }
    }
}
