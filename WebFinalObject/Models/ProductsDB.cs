using Microsoft.EntityFrameworkCore;

namespace WebFinalExam.Models
{
    public class ProductsDB : DbContext
    {
        public ProductsDB(DbContextOptions<ProductsDB> options) : base(options)
        {
        }

        public DbSet<Product> Product { get; set; }
    }
}

