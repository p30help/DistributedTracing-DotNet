using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using OrderApi.Messages;
using System.Text;

namespace OrderApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly ILogger<OrdersController> _logger;
        private readonly IHttpClientFactory _factory;
        private readonly IDistributedCache _cache;
        private readonly MessagePublisher _messagePublisher;

        public OrdersController(ILogger<OrdersController> logger,
            IHttpClientFactory factory,
            IDistributedCache cache,
            MessagePublisher messagePublisher)
        {
            _logger = logger;
            _factory = factory;
            _cache = cache;
            _messagePublisher = messagePublisher;
        }

        [HttpGet("Get/{name}")]
        public IActionResult Get(string name)
        {
            if (name.Length < 3)
            {
                throw new Exception("name is short");
            }

            return Ok("Everything is Okay");
        }


        [HttpPost("SubmitOrder")]
        public async Task<IActionResult> SubmitOrder(Order order)
        {
            _logger.LogTrace("This is Trace log");
            _logger.LogDebug("This is Debug log");
            _logger.LogInformation("This is Information log");
            _logger.LogWarning("This is Warning log");
            _logger.LogError("This is Error log");
            _logger.LogCritical("This is Critical log");

            if (order == null || order.ProductNames == null || order.ProductNames.Length == 0)
            {
                throw new Exception("Products names is empty");
            }

            var supplier = await _cache.GetStringAsync("Supplier");
            if (supplier == null)
            {
                supplier = "Jackson";
                await _cache.SetStringAsync("Supplier", supplier);
            }

            var http = _factory.CreateClient("InventoryClient");
            var inputs = order.ProductNames.ToList().Select(name => new Stock(Guid.NewGuid(), name));
            var content = new StringContent(JsonConvert.SerializeObject(inputs), Encoding.UTF8, "application/json");
            var result = await http.PostAsync("/Inventories/DecreasingFromStock", content);

            var calculator = new OrderPriceCalculator();
            var (price, dscount) = calculator.Calculate();

            await _messagePublisher.PublishAsync(new OrderSubmitted
            {
                OrderId = order.OrderId,
                UserId = order.UserId,
                TotalPrice = price
            });

            return Ok("Everything is Okay");
        }

    }

    public record Stock(Guid ProductId, string ProductName);

    public class Order
    {
        public Guid OrderId { get; set; }
        public Guid UserId { get; set; }

        public string[] ProductNames { get; set; }
    }
}