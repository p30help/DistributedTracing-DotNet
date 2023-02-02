using System.Diagnostics;

namespace OrderApi
{
    public class OrderPriceCalculator
    {
        public (decimal, decimal) Calculate()
        {
            using var parentActivity = AppTelemetry.ActivitySource.StartActivity("OrderCalculator");

            parentActivity.AddEvent(new ("Calculation Total Price", DateTimeOffset.Now));

            var totalPrice = new Random().Next(40000, 800000);

            parentActivity.AddEvent(new("Calculation Discount Price"));

            var discount = CalculateDiscount();

            parentActivity.AddEvent(new("Calculation Finished", DateTimeOffset.Now));

            parentActivity?.SetTag("TotalPrice", totalPrice);

            return (totalPrice, discount);
        }

        public decimal CalculateDiscount() {

            using var childActivity = AppTelemetry.ActivitySource.StartActivity("OrderCalculator");

            var discount = new Random().Next(0, 400000);

            childActivity?.SetTag("Discount", discount);

            return discount;
        }
    }
}
