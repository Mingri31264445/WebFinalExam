using System.Collections.Generic;

namespace WebFinalExam.Models.ViewModels   // ← 跟 Controller 的 using 對應
{
    /// <summary>
    /// 賣家查詢訂單時，一張訂單 + 該賣家明細列
    /// </summary>
    public class SellerOrderVM
    {
        public Order Order { get; set; } = default!;
        public List<OrderDetail> Details { get; set; } = new();

        // 方便 View 顯示「我的小計」
        public decimal MySubTotal => Details.Sum(d => d.UnitPrice * d.Quantity);
    }
}
