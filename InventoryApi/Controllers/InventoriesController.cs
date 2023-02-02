using InventoryApi.Context;
using Microsoft.AspNetCore.Mvc;

namespace InventoryApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class InventoriesController : ControllerBase
    {
        private readonly ILogger<InventoriesController> _logger;
        private readonly MainContext context;

        public InventoriesController(ILogger<InventoriesController> logger, MainContext context)
        {
            _logger = logger;
            this.context = context;
        }

        [HttpPost("DecreasingFromStock")]
        public async Task<IActionResult> DecreasingFromStock(List<Stock> stocks)
        {
            if(stocks == null || stocks.Any(x => x.ProductName == "null"))
            {
                throw new ApplicationException("Stock data is empty");
            }

            var items = new List<Inventory>();
            foreach (var item in stocks)
            {
                items.Add(new Inventory()
                {
                    Id = item.ProductId,
                    RecordDate = DateTime.Now,
                    Name = item.ProductName,
                });
            }

            await context.AddRangeAsync(items);
            await context.SaveChangesAsync();

            return Ok("Data saved");
        }
    }

    public class Stock
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
    }

}