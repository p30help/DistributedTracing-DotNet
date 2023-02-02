using MassTransit;
using Newtonsoft.Json;
using System.Diagnostics;

namespace OrderApi.Messages
{
    public class MessagePublisher
    {

        private readonly IPublishEndpoint _publishEndpoint;

        public MessagePublisher(IPublishEndpoint publishEndpoint)
        {
            _publishEndpoint = publishEndpoint;
        }

        public async Task PublishAsync<T>(T message) where T : class
        {
            using var activity = AppTelemetry.ActivitySource.StartActivity("Message Send", ActivityKind.Producer);

            activity?.SetTag("MessageBody", JsonConvert.SerializeObject(message));

            await _publishEndpoint.Publish<T>(message);
        }
    }
}