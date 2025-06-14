using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebFinalExam.Models;

namespace WebFinalObject.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Product> Product { get; set; }   // 讓 EF Core 會去查表 Product
        public DbSet<Order> Order { get; set; }
        public DbSet<OrderDetail> OrderDetail { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // fluent API：Product.SellerId → AspNetUsers.Id
            builder.Entity<Product>()
                   .HasOne<IdentityUser>(p => p.Seller!)//HasOne / HasForeignKey 告訴 EF 「SellerId 是外鍵，對應 AspNetUsers(Id)」
                   .WithMany()                     // 我們不在 IdentityUser 反向宣告集合也行
                   .HasForeignKey(p => p.SellerId)
                   .OnDelete(DeleteBehavior.Cascade);

            // OrderDetail ↔ Order (1:N)
            builder.Entity<OrderDetail>()
                   .HasOne(d => d.Order)
                   .WithMany(o => o.Details)
                   .HasForeignKey(d => d.OrderId);

            // OrderDetail ↔ Product (N:1)
            builder.Entity<OrderDetail>()
                   .HasOne(d => d.Product)
                   .WithMany()                 // Product 不需要反向集合也行
                   .HasForeignKey(d => d.ProductId);

            // Order ↔ IdentityUser (N:1)  (可選，若想讓 EF 能 Include Buyer)
            builder.Entity<Order>()
                   .HasOne<IdentityUser>()     // 不宣告導航屬性也 OK
                   .WithMany()
                   .HasForeignKey(o => o.BuyerId);
        }
    }
}
