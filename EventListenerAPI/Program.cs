
using EventListenerAPI.Models;
using EventListenerAPI.Services;

using Microsoft.EntityFrameworkCore;

using NLog;
using NLog.Web;

var logger = NLog.LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    // Add services to the container.
    builder.Services.AddControllers();

    // NLog: Setup NLog for Dependency injection
    builder.Logging.ClearProviders();
    builder.Logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
    builder.Host.UseNLog();

    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();


    var postgreConnectionString =builder.Configuration["ConnectionStrings:Postgre"];


    builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql( postgreConnectionString, o => o.SetPostgresVersion(9, 6) ));


    builder.Services.AddSingleton(typeof(IElasticEngine), typeof(ElasticEngine));

    builder.Services.AddScoped<IndexService>();

    builder.Services.AddScoped<EventService>();

    builder.Services.AddSingleton<IActorBridge, AkkaService>();

    // starts the IHostedService, which creates the ActorSystem and actors
    builder.Services.AddHostedService<AkkaService>(sp => (AkkaService)sp.GetRequiredService<IActorBridge>());


    var app = builder.Build();


    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseAuthorization();

    app.MapControllers();

    app.Run();

    Console.WriteLine("Started Application....");
    

}
catch(Exception exception)
{
    // NLog: catch setup errors
    logger.Error(exception, "Stopped program because of exception");
    throw;

}
finally
{
    // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
    NLog.LogManager.Shutdown();
}
