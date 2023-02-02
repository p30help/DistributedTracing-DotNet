namespace OrderApi.Messages
{
    public class OrderSubmitted
    {
        public Guid OrderId { get; set; }
        public Guid UserId { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
