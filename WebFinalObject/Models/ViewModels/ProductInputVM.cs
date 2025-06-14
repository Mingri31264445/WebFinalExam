using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using WebFinalExam.Models;

namespace WebFinalExam.Models.ViewModels
{
    public class ProductInputVM
    {
        public int Id { get; set; }                     // 編輯時用
        [Required] public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public string Category { get; set; } = "";

        // 原先圖片路徑（編輯時顯示）
        public string? ImageUrl { get; set; }

        // ← 新增：上傳檔案欄位
        [Display(Name = "圖片檔")]
        public IFormFile? ImageFile { get; set; }

        // 把 VM 轉回 Product 實體
        public Product ToProduct(string? imageUrl = null) => new()
        {
            Id = Id,
            Name = Name,
            Description = Description,
            Price = Price,
            Stock = Stock,
            Category = Category,
            ImageUrl = imageUrl ?? ImageUrl ?? ""
        };
    }
}
