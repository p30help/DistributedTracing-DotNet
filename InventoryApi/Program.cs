using InventoryApi.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<MainContext>(options =>
{
    options.UseSqlServer("Initial Catalog=Test;Data Source=.;TrustServerCertificate=Yes;Integrated Security=SSPI;Persist Security Info=true;");
});

// Configure OpenTelemetry with tracing and auto-start.
builder.Services.AddOpenTelemetry()
    .ConfigureResource(builder => builder
    .AddService(serviceName: "Inventory Api"))
    .WithTracing(builder =>
    {
        builder.AddAspNetCoreInstrumentation(options =>
        {
            options.Filter = context =>
            {
                var ignoredUrls = new List<string>()
                {
                   "/swagger",
                   "/_framework/aspnetcore-browser-refresh.js",
                };
                if (context.Request.Path.Value != null &&
                    ignoredUrls.Any(url => context.Request.Path.Value.StartsWith(url))
                )
                {
                    return false;
                }

                return true;
            };

            options.EnrichWithHttpRequest = (activity, httpRequest) =>
            {
                activity.SetTag("requestProtocol", httpRequest.Protocol);
            };
            options.EnrichWithHttpResponse = (activity, httpResponse) =>
            {
                activity.SetTag("responseLength", httpResponse.ContentLength);
            };
            options.EnrichWithException = (activity, exception) =>
            {
                activity.SetTag("exceptionType", exception.GetType().ToString());
            };
        });
        builder.AddHttpClientInstrumentation(options => { });
        builder.AddSqlClientInstrumentation(options =>
        {
            options.SetDbStatementForText = true;
            options.SetDbStatementForStoredProcedure = true;
        });

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
