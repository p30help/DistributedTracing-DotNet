using MassTransit;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OrderApi.Messages;
using OrderApi.Tools;
using System.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<MessagePublisher, MessagePublisher>();

builder.Services.AddHttpClient("InventoryClient", configure =>
{
    configure.BaseAddress = new Uri("https://localhost:7171");
    configure.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));//ACCEPT header
    configure.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json; charset=utf-8");
});

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = $"localhost:6380";
});

builder.Services.AddMassTransit(cfg =>
{
    //cfg.AddConsumer<StartOrderConsumer>();

    // read more on https://masstransit-project.com/quick-starts/rabbitmq.html
    cfg.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("amqp://guest:guest@localhost:5672");
        cfg.ConfigureEndpoints(context);
    });

});

// Configure OpenTelemetry with tracing and auto-start.
builder.Services.AddOpenTelemetry()
    .ConfigureResource(builder => builder
    .AddService(serviceName: "Order Api"))
    .WithTracing(builder =>
    {
        builder.AddAspNetCoreInstrumentation(options =>
        {
            options.Filter = context =>
            {
                var ignoredUrls = new List<string>()
                {
                   "/swagger",
                   "/_framework/aspnetcore-browser-refresh.js"
                };
                if (context.Request.Path.Value != null &&
                    ignoredUrls.Any(url => context.Request.Path.Value.StartsWith(url))
                )
                {
                    return false;
                }

                return true;
            };

            options.EnrichWithHttpRequest = async (activity, httpRequest) =>
            {
                activity.SetTag("requestProtocol", httpRequest.Protocol);

                if (httpRequest.Method is "POST" or "PUT")
                {
                    activity.SetTag("requestBody", await Utils.GetRawBodyAsync(httpRequest));
                }
            };
            options.EnrichWithHttpResponse = async (activity, httpResponse) =>
            {
                activity.SetTag("responseLength", httpResponse.ContentLength);

                //activity.SetTag("responseBody", await Utils.GetRawBodyAsync(httpResponse));
            };
            options.EnrichWithException = (activity, exception) =>
            {
                activity.SetTag("exceptionType", exception.GetType().ToString());
            };
        });
        builder.AddHttpClientInstrumentation(options => { });
        builder.ConfigureBuilder((sp, builder) =>
        {
            RedisCache cache = (RedisCache)sp.GetRequiredService<IDistributedCache>();
            builder.AddRedisInstrumentation(cache.GetConnection(), options =>
            {
                options.SetVerboseDatabaseStatements = true;
                options.EnrichActivityWithTimingEvents = true;
            });
        });

        builder.AddSource("MassTransit");
        builder.AddSource("OrderApp");
        builder.AddSource("MessagePublisher");
        //builder.AddMassTransitInstrumentation(options => {  });

        builder.AddJaegerExporter(options => { });
    })
    .StartWithHost();



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
