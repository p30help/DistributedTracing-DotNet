using System.Diagnostics;

namespace OrderApi
{
    public class AppTelemetry
    {
        public static ActivitySource ActivitySource { get; private set; } = new("OrderApp");

    }
}
