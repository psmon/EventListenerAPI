
using EventListenerAPI.Models;
using EventListenerAPI.Services;

using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var postgreConnectionString =builder.Configuration["ConnectionStrings:Postgre"];


builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql( postgreConnectionString, o => o.SetPostgresVersion(9, 6) ));

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
