using Microsoft.EntityFrameworkCore;

namespace InventoryApi.Context
{
    public class MainContext : DbContext
    {
        public MainContext(DbContextOptions<MainContext> options) : base(options)
        {
        }

        public DbSet<Inventory> Inventories { get; set; }
    }
}
