using Microsoft.EntityFrameworkCore;

namespace InventoryApi.Context
{
    public class Inventory
    {
        public Guid Id { get; set; }

        public DateTime RecordDate { get; set; }

        public string Name { get; set; }
    }
}
