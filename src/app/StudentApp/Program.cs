using Microsoft.EntityFrameworkCore;
using Models.DataAccess;
using Models;
using Serilog;
using Serilog.Formatting.Elasticsearch;
using Prometheus;
using Elastic.Apm.NetCoreAll;
using Serilog.Events;
using Microsoft.AspNetCore.HttpLogging;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
var logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
             .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                       .MinimumLevel.Override("Worker", LogEventLevel.Warning)
                       .MinimumLevel.Override("System", LogEventLevel.Warning)
                       .MinimumLevel.Override("Host", LogEventLevel.Warning)
                       .MinimumLevel.Override("Host.Aggregator", LogEventLevel.Warning)
                       .MinimumLevel.Override("Host.Results", LogEventLevel.Warning)
                       .MinimumLevel.Override("Function", LogEventLevel.Warning)
                       .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                       .MinimumLevel.Override("System.Net.Http.HttpClient", LogEventLevel.Warning)
                       .MinimumLevel.Override("Microsoft.Azure.WebJobs", LogEventLevel.Warning)
                       .MinimumLevel.Override("Azure.Messaging.ServiceBus", LogEventLevel.Warning)
                       .MinimumLevel.Override("Microsoft.Azure.WebJobs.ServiceBus", LogEventLevel.Warning)
                       .MinimumLevel.Override("Azure.Storage.Blobs", LogEventLevel.Warning)
                       .MinimumLevel.Override("Microsoft.Azure.WebJobs.Storage.Blobs", LogEventLevel.Warning)
            .WriteTo.Console(new ElasticsearchJsonFormatter() { })
            .Enrich.FromLogContext()
            .Enrich.WithEnvironmentName()
            .Enrich.WithMachineName()
            .Enrich.WithEnvironmentUserName()
            .CreateLogger(); ;
builder.Logging.ClearProviders();
builder.Logging.AddSerilog(logger);


builder.Services.AddW3CLogging(logging =>
{
    // Log all W3C fields
    logging.LoggingFields = W3CLoggingFields.All;
    logging.FlushInterval = TimeSpan.FromSeconds(2);
});

// Add services to the container.
builder.Services.AddControllersWithViews();


Console.WriteLine(builder.Environment.EnvironmentName);

string dbName = "InMemDevDb";

builder.Services.AddDbContext<StudentAppContext>(options =>
  {
      options.UseInMemoryDatabase(dbName);
  });

var app = builder.Build();


//Seed Dummy Data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<StudentAppContext>();
        DbInitializer.Initialize(context);
    }
    catch (Exception ex)
    {
        var appLogger = services.GetRequiredService<ILogger<Program>>();
        appLogger.LogError(ex, "An error occurred while seeding the database.");
    }
}

// Configure the HTTP request pipeline.
//app.UseW3CLogging();
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();

    // For Elastic APM
    //We want APM only in PROD hence added this line here to Register our Agent
    app.UseAllElasticApm(configuration);
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseHttpLogging();

app.UseRouting();

//For Prometheus
//app.UseHttpMetrics();

app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    //Endpoint For Prometheus
    //  endpoints.MapMetrics();
    endpoints.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
});

app.Run();
