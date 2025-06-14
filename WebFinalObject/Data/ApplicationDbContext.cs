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

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // fluent API：Product.SellerId → AspNetUsers.Id
            builder.Entity<Product>()
                   .HasOne<IdentityUser>(p => p.Seller!)//HasOne / HasForeignKey 告訴 EF 「SellerId 是外鍵，對應 AspNetUsers(Id)」
                   .WithMany()                     // 我們不在 IdentityUser 反向宣告集合也行
                   .HasForeignKey(p => p.SellerId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
