using Microsoft.EntityFrameworkCore;
using Models.DataAccess;
using Models;
using Serilog;
using Serilog.Formatting.Elasticsearch;
using Prometheus;
using Elastic.Apm.NetCoreAll;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
var logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .WriteTo.Console(new ElasticsearchJsonFormatter() { })
            .Enrich.FromLogContext()
            .Enrich.WithEnvironmentName()
            .Enrich.WithMachineName()
            .Enrich.WithEnvironmentUserName()
            .CreateLogger();
builder.Logging.ClearProviders();
builder.Logging.AddSerilog(logger);

// Add services to the container.
builder.Services.AddControllersWithViews();


Console.WriteLine(builder.Environment.EnvironmentName);

string dbName = "InMemDevDb";

var logger1 = builder.Services.BuildServiceProvider().GetRequiredService<ILogger<Program>>();

logger1.LogInformation(builder.Configuration.GetConnectionString("StudentAppContext"));


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

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// For Elastic APM
// app.UseAllElasticApm(configuration);

app.UseHttpsRedirection();
app.UseStaticFiles();

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
