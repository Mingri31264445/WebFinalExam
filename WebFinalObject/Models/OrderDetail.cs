using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebFinalExam.Models
{
    public class OrderDetail
    {
        [Key]
        public int DetailId { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }

        // 這筆商品的單價
        [Range(0, double.MaxValue)]
        public decimal UnitPrice { get; set; }

        // 購買數量
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        public Order? Order { get; set; }
        public Product? Product { get; set; }
    }
}
