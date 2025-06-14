using Microsoft.AspNetCore.Mvc;
using WebFinalExam.Models;
using Microsoft.AspNetCore.Authorization;   // Authorize 讓只有「已登入」的人才能新增商品。
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.AspNetCore.Hosting;
using WebFinalExam.Models.ViewModels;


namespace WebFinalExam.Controllers
{
    public class ProductController : Controller
    {
        private readonly ProductsDB _context;
        private readonly IWebHostEnvironment _env;//用來取得 wwwroot 路徑

        public ProductController(ProductsDB ctx, IWebHostEnvironment env)
        {
            _context = ctx;
            _env = env;
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
            var p = _context.Product.FirstOrDefault(p => p.Id == id);
            if (p == null || p.SellerId != User.FindFirstValue(ClaimTypes.NameIdentifier)) return Forbid();
            var vm = new ProductInputVM
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                Stock = p.Stock,
                Category = p.Category,
                ImageUrl = p.ImageUrl
            };
            return View(vm);
        }

        [Authorize, HttpPost, ValidateAntiForgeryToken]//// 只有登入的使用者，透過表單 POST 且有驗證碼，才能執行這段程式
        public IActionResult Edit(int id, ProductInputVM vm)
        {
            if (id != vm.Id) return NotFound();
            var product = _context.Product.Find(id);
            if (product == null || product.SellerId != User.FindFirstValue(ClaimTypes.NameIdentifier)) return Forbid();

            if (!ModelState.IsValid) return View(vm);

            // 若有新圖片→存檔→覆蓋 ImageUrl
            if (vm.ImageFile is { Length: > 0 })
            {
                string ext = Path.GetExtension(vm.ImageFile.FileName).ToLower();
                if (ext is not (".jpg" or ".jpeg" or ".png"))
                {
                    ModelState.AddModelError("ImageFile", "僅允許 JPG / PNG");
                    return View(vm);
                }
                string fileName = $"{Guid.NewGuid()}{ext}";
                string savePath = Path.Combine(_env.WebRootPath, "images", fileName);
                Directory.CreateDirectory(Path.GetDirectoryName(savePath)!);
                using var fs = System.IO.File.Create(savePath);
                vm.ImageFile.CopyTo(fs);
                product.ImageUrl = $"/images/{fileName}";
            }

            // 更新其他欄位
            product.Name = vm.Name;
            product.Description = vm.Description;
            product.Price = vm.Price;
            product.Stock = vm.Stock;
            product.Category = vm.Category;

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
            return View(new ProductInputVM());   
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ProductInputVM vm)
        {
            if (!ModelState.IsValid) return View(vm);

            string? imgUrl = null;
            if (vm.ImageFile is { Length: > 0 })
            {
                string ext = Path.GetExtension(vm.ImageFile.FileName).ToLower();
                if (ext is not (".jpg" or ".jpeg" or ".png"))
                {
                    ModelState.AddModelError("ImageFile", "僅允許 JPG / PNG");
                    return View(vm);
                }
                string fileName = $"{Guid.NewGuid()}{ext}";
                string savePath = Path.Combine(_env.WebRootPath, "images", fileName);
                Directory.CreateDirectory(Path.GetDirectoryName(savePath)!);
                using var fs = System.IO.File.Create(savePath);
                vm.ImageFile.CopyTo(fs);
                imgUrl = $"/images/{fileName}";
            }

            var product = vm.ToProduct(imgUrl);
            product.SellerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            _context.Product.Add(product);
            _context.SaveChanges();
            return RedirectToAction("MyStore");
        }

    }
}
