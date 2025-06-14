using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebFinalExam.Models
{
    public class Order
    {
        [Key]                           
        public int OrderId { get; set; }
        [Required]
        public string BuyerId { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        [Range(0, double.MaxValue)]
        public decimal TotalAmount { get; set; }

        // 一張訂單有多筆明細
        public ICollection<OrderDetail> Details { get; set; } = new List<OrderDetail>();
    }
}
